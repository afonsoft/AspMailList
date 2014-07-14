using AspMailList.library;
using OpenPop.Mime;
using OpenPop.Pop3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AspMailList.Service
{
    class Program
    {
        private static object lockObject = new object();
        public static void WriteLine(string value)
        {
            lock (lockObject)  // all other threads will wait for y
            {
                Console.WriteLine(value);
                using (var lockStreamWriter = new StreamWriter("log-" + DateTime.Now.ToString("yyyyMMdd") + ".txt", true))
                {
                    lockStreamWriter.Write(DateTime.Now.ToString("HH:mm:ss") + ": " + value);
                    lockStreamWriter.Write(Environment.NewLine);
                }
            }
        }
        public static void WriteLine(string value, Exception ex)
        {
            lock (lockObject)  // all other threads will wait for y
            {
                Console.WriteLine(value);
                using (var lockStreamWriter = new StreamWriter("log-" + DateTime.Now.ToString("yyyyMMdd") + ".txt", true))
                {
                    lockStreamWriter.Write(DateTime.Now.ToString("HH:mm:ss") + ": " + value);
                    lockStreamWriter.Write(Environment.NewLine);
                    if (ex != null)
                    {
                        lockStreamWriter.Write(DateTime.Now.ToString("HH:mm:ss") + ": Erro: " + ex.Message);
                        lockStreamWriter.Write(Environment.NewLine);
                        lockStreamWriter.Write(DateTime.Now.ToString("HH:mm:ss") + ": StackTrace: " + ex.StackTrace);
                        lockStreamWriter.Write(Environment.NewLine);
                        if (ex.InnerException != null)
                        {
                            lockStreamWriter.Write(DateTime.Now.ToString("HH:mm:ss") + ": InnerException: " + ex.InnerException.Message);
                            lockStreamWriter.Write(Environment.NewLine);
                        }
                    }
                }
            }
        }
        public static List<Thread >Threads { get; set; }
        
        private static List<string> _emails = null;
        public static List<string> Emails
        {
            get
            {
                if (_emails == null) _emails = new List<string>();
                return _emails;
            }
            set { _emails = value; }
        }

        public static Boolean isRunnig = true;

        static void OnProcessExit(object sender, EventArgs e)
        {
            WriteLine("Finalizando AspMailList");
            isRunnig = false;
            foreach (Thread t in Threads)
            {
                try
                {
                    WriteLine("Finalizando as thread " + t.Name);
                    t.Join();
                    t.Abort();
                }
                catch { isRunnig = false; }
            }
        }

        static void Main(string[] args)
        {
            try
            {
                WriteLine("Iniciando AspMailList");
                WriteLine("Versão Aplicação: " + FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).FileVersion + " - " + typeof(Program).Assembly.GetName().Version.ToString());
                WriteLine("Versão biblioteca: " + CoreAssembly.getFileVersion + " - " + CoreAssembly.getVersion);
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit); 
                WriteLine("Recuperando a lista de Campanhas.");
                using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
                {
                    var listCampanha = (from c in db.Mala_Direta_Campanhas
                                        where c.Enabled == true
                                        select c).ToList();

                    WriteLine("Total de Campanhas: " + listCampanha.Count);
                    WriteLine("Iniciando as Threads das campanhas");
                    Threads = new List<Thread>();
                    foreach (var campanha in listCampanha)
                    {
                        myThreadCampanha threadcamp = new myThreadCampanha(campanha);
                        Thread threadCampanhaHelp = new Thread(new ParameterizedThreadStart(ExecutarCampanhaHelps));
                        threadCampanhaHelp.IsBackground = true;
                        threadCampanhaHelp.Name = "ExecutarCampanhaHelps";
                        threadCampanhaHelp.Priority = ThreadPriority.Lowest;
                        threadCampanhaHelp.Start(threadcamp);
                        Threads.Add(threadCampanhaHelp);
                        Thread.Sleep(1000);

                        Thread threadCampanhaErros = new Thread(new ParameterizedThreadStart(ExecutarCampanhaErros));
                        threadCampanhaErros.IsBackground = true;
                        threadCampanhaErros.Name = "ExecutarCampanhaErros";
                        threadCampanhaErros.Priority = ThreadPriority.Lowest;
                        threadCampanhaErros.Start(threadcamp);
                        Threads.Add(threadCampanhaErros);
                        Thread.Sleep(1000);

                        Thread threadCampanhaUnsubscribeAndSubscribe = new Thread(new ParameterizedThreadStart(ExecutarCampanhaUnsubscribeAndSubscribe));
                        threadCampanhaUnsubscribeAndSubscribe.IsBackground = true;
                        threadCampanhaUnsubscribeAndSubscribe.Name = "ExecutarCampanhaUnsubscribeAndSubscribe";
                        threadCampanhaUnsubscribeAndSubscribe.Priority = ThreadPriority.Lowest;
                        threadCampanhaUnsubscribeAndSubscribe.Start(threadcamp);
                        Threads.Add(threadCampanhaUnsubscribeAndSubscribe);
                        Thread.Sleep(1000);

                        Thread threadCampanhaEnvioEmail = new Thread(new ParameterizedThreadStart(ExecutarCampanhaEnvioEmail));
                        threadCampanhaEnvioEmail.IsBackground = true;
                        threadCampanhaEnvioEmail.Name = "ExecutarCampanhaEnvioEmail";
                        threadCampanhaEnvioEmail.Priority = ThreadPriority.Lowest;
                        threadCampanhaEnvioEmail.Start(threadcamp);
                        Threads.Add(threadCampanhaEnvioEmail);
                        Thread.Sleep(1000);

                    }
                    while (isRunnig)
                    {
                        Thread.Sleep(500);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Clear();
                isRunnig = false;
                WriteLine("Ocorreu um erro!", ex);
                WriteLine("Precione um tecla para sair.");
                Console.ReadKey();
            }
            finally
            {
                OnProcessExit(null, null);
            }
        }

        private static void ExecutarCampanhaEnvioEmail(object _myThreadCampanha)
        {
            while (isRunnig)
            {
                myThreadCampanha threadCampanha = (myThreadCampanha)_myThreadCampanha;
                int enviado = 0;
                int totalEnvio = 0;
                int Errocount = 0;
                long CountTotal = 0;
                long CountError = 0;

                List<Sp_camanha_email_nao_enviadoResult> Emails = new List<Sp_camanha_email_nao_enviadoResult>();
                WriteLine("Lendo database...");
                using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
                {
                    db.ObjectTrackingEnabled = false;
                    if (!CoreAssembly.IsRunningOnMono())
                        db.CommandTimeout = db.Connection.ConnectionTimeout;
                    Emails = db.Sp_camanha_email_nao_enviado(threadCampanha.Campanha.id).ToList();
                    //LER DE 5000 EM 5000 E-MAILS
                }
                WriteLine("Localizado " + Emails.Count() + " registro.");
                Smtp mail = new Smtp();
                mail.Body = System.Web.HttpUtility.HtmlDecode(threadCampanha.Campanha.BodyHtml);
                mail.EnableSsl = threadCampanha.Campanha.EnableSsl;
                mail.DisplayName = threadCampanha.Campanha.DisplayName;
                mail.From = threadCampanha.Campanha.SmtpUser;
                mail.Password = threadCampanha.Campanha.SmtpPassword;
                mail.Port = threadCampanha.Campanha.SmtpPort.ToString();
                mail.SmtpServer = threadCampanha.Campanha.SmtpServer;
                mail.UseCredentials = true;
                mail.User = threadCampanha.Campanha.SmtpUser;

                foreach (Sp_camanha_email_nao_enviadoResult md in Emails)
                {
                    totalEnvio++;

                    if (!isRunnig)
                        return;

                    mail.To = md.email.Replace("\t", "").Replace("\r", "").Replace("\n", "");
                    try
                    {
                        mail.EnviarEmail();
                        enviado++;

                        System.Threading.Thread.Sleep(500);
                        using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
                        {
                            if (!CoreAssembly.IsRunningOnMono())
                                db.CommandTimeout = db.Connection.ConnectionTimeout;

                            Mala_Direta_Campanha_Enviado menivado = new Mala_Direta_Campanha_Enviado();
                            menivado.dtEnvio = DateTime.Now;
                            menivado.idCampanha = threadCampanha.Campanha.id;
                            menivado.idMail = md.id;
                            db.Mala_Direta_Campanha_Enviados.InsertOnSubmit(menivado);
                            db.SubmitChanges();
                        }

                        CountTotal++;
                        Errocount = 0;

                        if (enviado >= 25)
                        {
                            enviado = 0;
                            System.Threading.Thread.Sleep(10000); //Esperar 10 seg. apos o envio de 25;
                        }
                    }

                    catch (Exception ex)
                    {
                        CountError++;
                        WriteLine(string.Format("Destino: {0} - Erro: {1} ", md.email, ex.Message), ex);

                        int time = 60000;
                        if (ex.Message.Contains("too many messages"))
                        {
                            Errocount++;
                            time = 300000 * Errocount; //Esperar (5 * erros) minutos antes de enviar o proximo.
                        }
                        else
                            time = 60000; //Qualquer erro esperar 1 minuto.

                        for (int i = 0; i <= time; i++)
                        {
                            if (!isRunnig)
                                return;

                            System.Threading.Thread.Sleep(1);
                        }
                    }
                }
            }
        }
        private static void ExecutarCampanhaHelps(object _myThreadCampanha)
        {

            while (isRunnig)
            {
                try
                {
                    myThreadCampanha threadCampanha = (myThreadCampanha)_myThreadCampanha;
                    WriteLine("Iniciando (Campanha Help): " + threadCampanha.Campanha.DisplayName);
                    threadCampanha.ProcessarHelps(threadCampanha.Campanha.SmtpServer, threadCampanha.Campanha.PopPort, threadCampanha.Campanha.SmtpPort, threadCampanha.Campanha.EnableSsl, threadCampanha.Campanha.SmtpUser, threadCampanha.Campanha.SmtpPassword, threadCampanha.Campanha.DisplayName);
                    Thread.Sleep(60010); //1 Minutos
                }
                catch (Exception ex)
                {
                    WriteLine("(Camapnha Help) Erros: " + ex.Message, ex);
                    if (ex.Message.IndexOf("Server not found") >= 0)
                        Thread.Sleep(60000); //1 Minutos
                    else
                        Thread.Sleep(60000 * 30); //30 Minutos
                }
            }
        }

        private static void ExecutarCampanhaErros(object _myThreadCampanha)
        {

            while (isRunnig)
            {
                try
                {
                    myThreadCampanha threadCampanha = (myThreadCampanha)_myThreadCampanha;
                    WriteLine("Iniciando (Campanha Erros): " + threadCampanha.Campanha.DisplayName);
                    threadCampanha.ProcessarErros(threadCampanha.Campanha.SmtpServer, threadCampanha.Campanha.PopPort, threadCampanha.Campanha.EnableSsl, threadCampanha.Campanha.SmtpUser, threadCampanha.Campanha.SmtpPassword);
                    Thread.Sleep(60100 * 10); //10 Minutos
                }
                catch (Exception ex)
                {
                    WriteLine("(Campanha Erros) Erros: " + ex.Message, ex);
                    if (ex.Message.IndexOf("Server not found") >= 0)
                        Thread.Sleep(60000); //1 Minutos
                    else
                        Thread.Sleep(60000 * 30); //30 Minutos
                }
            }
        }
        private static void ExecutarCampanhaUnsubscribeAndSubscribe(object _myThreadCampanha)
        {

            while (isRunnig)
            {
                try
                {
                    myThreadCampanha threadCampanha = (myThreadCampanha)_myThreadCampanha;
                    WriteLine("Iniciando (Unsubscribe And Subscribe): " + threadCampanha.Campanha.DisplayName);
                    threadCampanha.ProcessarUnsubscribeAndSubscribe(threadCampanha.Campanha.SmtpServer, threadCampanha.Campanha.PopPort, threadCampanha.Campanha.SmtpPort, threadCampanha.Campanha.EnableSsl, threadCampanha.Campanha.SmtpUser, threadCampanha.Campanha.SmtpPassword, threadCampanha.Campanha.DisplayName, threadCampanha.Campanha.id);
                    Thread.Sleep(60005); //1 Minutos
                }
                catch (Exception ex)
                {
                    WriteLine("(Unsubscribe And Subscribe) Erros: " + ex.Message, ex);
                    if (ex.Message.IndexOf("Server not found") >= 0)
                        Thread.Sleep(60000); //1 Minutos
                    else
                        Thread.Sleep(60000 * 30); //30 Minutos
                }
            }
        }
    }
    public class myThreadCampanha
    {
        private static object lockObject = new object();
        public static void WriteLine(string value)
        {
            lock (lockObject)  // all other threads will wait for y
            {
                Console.WriteLine(value);
                using (var lockStreamWriter = new StreamWriter("log-" + DateTime.Now.ToString("yyyyMMdd") + ".txt", true))
                {
                    lockStreamWriter.Write(DateTime.Now.ToString("HH:mm:ss") + ": " + value);
                    lockStreamWriter.Write(Environment.NewLine);
                }
            }
        }

        public static void WriteLine(string value, Exception ex)
        {
            lock (lockObject)  // all other threads will wait for y
            {
                Console.WriteLine(value);
                using (var lockStreamWriter = new StreamWriter("log-" + DateTime.Now.ToString("yyyyMMdd") + ".txt", true))
                {
                    lockStreamWriter.Write(DateTime.Now.ToString("HH:mm:ss") + ": " + value);
                    lockStreamWriter.Write(Environment.NewLine);
                    if (ex != null)
                    {
                        lockStreamWriter.Write(DateTime.Now.ToString("HH:mm:ss") + ": Erro: " + ex.Message);
                        lockStreamWriter.Write(Environment.NewLine);
                        lockStreamWriter.Write(DateTime.Now.ToString("HH:mm:ss") + ": StackTrace: " + ex.StackTrace);
                        lockStreamWriter.Write(Environment.NewLine);
                        if (ex.InnerException != null)
                        {
                            lockStreamWriter.Write(DateTime.Now.ToString("HH:mm:ss") + ": InnerException: " + ex.InnerException.Message);
                            lockStreamWriter.Write(Environment.NewLine);
                        }
                    }
                }
            }
        }

        public Mala_Direta_Campanha Campanha { get; set; }
        private myThreadCampanha() { }
        public myThreadCampanha(Mala_Direta_Campanha campamha) { Campanha = campamha; }
        public void ProcessarErros(string host, int port, bool ssl, string user, string pass)
        {
            int count = 0;
            AspMailList.library.Pop3 pop = new AspMailList.library.Pop3();
            using (Pop3Client client = pop.pop3Client(host, port, ssl, user, pass))
            {
                List<Message> lst = pop.FetchAllMessages(client);
                foreach (Message msg in lst)
                {
                    string Subject = msg.Headers.Subject.ToLower().Trim();

                    if (Subject.IndexOf("mail delivery") >= 0 || 
                        Subject.IndexOf("failed") >= 0 || 
                        Subject.IndexOf("undelivered") >= 0 || 
                        Subject.IndexOf("não entregue") >= 0 || 
                        Subject.IndexOf("postmaster") >= 0 ||
                        Subject.IndexOf("delivery failure") >= 0 ||
                        Subject.IndexOf("undeliverable") >= 0)
                    {
                        count++;
                        MessagePart msgpart = msg.FindFirstHtmlVersion();
                        if (msgpart == null)
                            msgpart = msg.FindFirstPlainTextVersion();
                        if (msgpart == null)
                            msgpart = msg.MessagePart;

                        string Body = msgpart.GetBodyAsText();

                        string[] femails = AspMailList.library.ValidEmail.getListMail(Body);
                        string[] emails = (from e in femails
                                           where !e.Contains(host)
                                           select e).ToArray();

                        using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
                        {
                            var optDelete = (from m in db.Mala_Diretas
                                             where emails.Contains(m.email)
                                             select m);

                            db.Mala_Diretas.DeleteAllOnSubmit(optDelete);
                            db.SubmitChanges();
                        }
                        pop.DeleteMessageByMessageId(client, msg.Headers.MessageId);
                    }
                }
                WriteLine("Removido " + count + " e-mails com erros.");
            }
        }
        public void ProcessarHelps(string host, int popPort, int smtpPort, bool ssl, string user, string pass, string displayName)
        {
            AspMailList.library.Pop3 pop = new AspMailList.library.Pop3();
            using (Pop3Client client = pop.pop3Client(host, popPort, ssl, user, pass))
            {
                List<Message> lst = pop.FetchAllMessages(client);
                foreach (Message msg in lst)
                {
                    string Subject = msg.Headers.Subject.ToLower().Trim();
                    if (Subject.IndexOf("help") >= 0)
                    {
                        string from = msg.Headers.From.Address.ToString().ToLower().Trim();
                        WriteLine("Enviando e-mail de help para " + from);
                        AspMailList.library.Smtp smtp = new library.Smtp();
                        smtp.Subject = "Informações sobre a subscrição";
                        smtp.To = from;
                        smtp.From = user;
                        smtp.Password = pass;
                        smtp.User = user;
                        smtp.Port = smtpPort.ToString();
                        smtp.SmtpServer = host;
                        smtp.DisplayName = displayName;
                        smtp.Body = getHelpBody(user, displayName);
                        smtp.EnviarEmail();

                        pop.DeleteMessageByMessageId(client, msg.Headers.MessageId);
                    }
                }
            }
        }
        public void ProcessarUnsubscribeAndSubscribe(string host, int popPort, int smtpPort, bool ssl, string user, string pass, string displayName, int idCampanha)
        {
            AspMailList.library.Pop3 pop = new AspMailList.library.Pop3();
            using (Pop3Client client = pop.pop3Client(host, popPort, ssl, user, pass))
            {
                List<Message> lst = pop.FetchAllMessages(client);
                foreach (Message msg in lst)
                {
                    string Subject = msg.Headers.Subject.ToLower().Trim();
                    if (Subject.IndexOf("subscribe") >= 0 || Subject.IndexOf("unsubscribe") >= 0)
                    {
                        bool subscribe = Subject.IndexOf("unsubscribe") < 0;
                        string sfrom = msg.Headers.From.Address.ToString().ToLower().Trim();
                        WriteLine("Enviando e-mail de help para " + sfrom + " com objetivo " + Subject);
                        AspMailList.library.Smtp smtp = new library.Smtp();
                        smtp.Subject = "Informações sobre a subscrição";
                        smtp.To = sfrom;
                        smtp.From = user;
                        smtp.Password = pass;
                        smtp.User = user;
                        smtp.Port = smtpPort.ToString();
                        smtp.SmtpServer = host;
                        smtp.DisplayName = displayName;
                        smtp.Body = getHelpBodyUnsubscribeAndSubscribe(user, displayName, subscribe);
                        smtp.EnviarEmail();

                        if (subscribe)
                        {
                            using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
                            {
                                Mala_Direta mala = new Mala_Direta();
                                mala.email = sfrom;
                                mala.dtCadastro = DateTime.Now;
                                db.Mala_Diretas.InsertOnSubmit(mala);
                                db.SubmitChanges();
                            }
                        }
                        else
                        {
                            using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
                            {
                                var optdelete = (from m in db.Mala_Diretas
                                                 where m.email == sfrom
                                                 select m);

                                foreach (var opt in optdelete)
                                {
                                    Mala_Direta_Campanha_Unsubscribe m = new Mala_Direta_Campanha_Unsubscribe();
                                    m.dtUnsubscribe = DateTime.Now;
                                    m.idCampanha = idCampanha;
                                    m.idMail = opt.id;
                                    db.Mala_Direta_Campanha_Unsubscribes.InsertOnSubmit(m);
                                    db.SubmitChanges();
                                }
                            }
                        }

                        pop.DeleteMessageByMessageId(client, msg.Headers.MessageId);
                    }
                }
            }
        }
        public string getHelpBody(string mail, string displayName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<br>");
            sb.AppendLine("<b>Informações sobre a lista.</b><br>");
            sb.AppendLine("<br>");
            sb.AppendLine("<br>List-Help: <a href='mailto:" + mail + "?subject=Help'>" + mail + "?subject=Help</a>");
            sb.AppendLine("<br>List-Unsubscribe: <a href='mailto:" + mail + "?subject=Unsubscribe'>" + mail + "?subject=Unsubscribe</a>");
            sb.AppendLine("<br>List-Subscribe: <a href='mailto:" + mail + "?subject=Subscribe'>" + mail + "?subject=Subscribe</a>");
            sb.AppendLine("<br>");
            sb.AppendLine("<br>Obrigado.<br>");
            sb.AppendLine(displayName);
            sb.AppendLine("<br>");
            return sb.ToString();
        }
        public string getHelpBodyUnsubscribeAndSubscribe(string mail, string displayName, bool Subscribe)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<br>");
            sb.AppendLine("<b>Informações sobre a sua subscrição.</b>");
            sb.AppendLine("<br>");
            if (Subscribe)
                sb.AppendLine("<br>Você acabou de entrar no grupo da lista.");
            else
                sb.AppendLine("<br>Você acabou de sair no grupo da lista.");
            sb.AppendLine("<br>");
            sb.AppendLine("<br>List-Help: <a href='mailto:" + mail + "?subject=Help'>" + mail + "?subject=Help</a>");
            if (Subscribe)
                sb.AppendLine("<br>List-Unsubscribe: <a href='mailto:" + mail + "?subject=Unsubscribe'>" + mail + "?subject=Unsubscribe</a>");
            else
                sb.AppendLine("<br>List-Subscribe: <a href='mailto:" + mail + "?subject=Subscribe'>" + mail + "?subject=Subscribe</a>");
            sb.AppendLine("<br>");
            sb.AppendLine("<br>Obrigado.<br>");
            sb.AppendLine(displayName);
            sb.AppendLine("<br>");
            return sb.ToString();
        }
    }
}
