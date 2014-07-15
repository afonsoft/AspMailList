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
        }

        static void Main(string[] args)
        {
            try
            {
                WriteLine("Iniciando AspMailList");
                WriteLine("Versão Aplicação: " + FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).FileVersion + " - " + typeof(Program).Assembly.GetName().Version.ToString());
                WriteLine("Versão biblioteca: " + CoreAssembly.getFileVersion + " - " + CoreAssembly.getVersion);
                WriteLine("Pressine Q para Sair. (Press Q for Exit)");
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit); 
                WriteLine("Recuperando a lista de Campanhas.");
                using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
                {
                    var listCampanha = (from c in db.Mala_Direta_Campanhas
                                        where c.Enabled == true
                                        select c).ToList();

                    WriteLine("Total de Campanhas: " + listCampanha.Count);
                    WriteLine("Iniciando as Threads das campanhas.");
                    Threads = new List<Thread>();
                    foreach (var campanha in listCampanha)
                    {
                        myThreadCampanha threadcamp = new myThreadCampanha(campanha);
                        WriteLine("Iniciando a Campanha: " + threadcamp.Campanha.DisplayName + " - ID: " + threadcamp.Campanha.id);
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
                        ConsoleKeyInfo keyinfo = Console.ReadKey();
                        if (keyinfo.Key == ConsoleKey.Q)
                        {
                            isRunnig = false;
                        }
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
            myThreadCampanha threadCampanha = (myThreadCampanha)_myThreadCampanha;
            WriteLine("ID " + threadCampanha.Campanha.id + " - Iniciando EnvioEmail");
            while (isRunnig)
            {
                try
                {
                    threadCampanha.ProcessarEmail();
                    Thread.Sleep(threadCampanha.TimeSleep);
                }
                catch (Exception ex)
                {
                    WriteLine("ID " + threadCampanha.Campanha.id + " - EnvioEmail - Erros: " + ex.Message, ex);
                    Thread.Sleep(threadCampanha.TimeSleep);
                }
            }
        }
        private static void ExecutarCampanhaHelps(object _myThreadCampanha)
        {
            myThreadCampanha threadCampanha = (myThreadCampanha)_myThreadCampanha;
            WriteLine("ID " + threadCampanha.Campanha.id + " - Iniciando Help");
            while (isRunnig)
            {
                try
                {
                    threadCampanha.ProcessarHelps();
                    Thread.Sleep(threadCampanha.TimeSleep); 
                }
                catch (Exception ex)
                {
                    WriteLine("ID " + threadCampanha.Campanha.id + " - Help - Erros: " + ex.Message, ex);
                    Thread.Sleep(threadCampanha.TimeSleep); 
                }
            }
        }
        private static void ExecutarCampanhaErros(object _myThreadCampanha)
        {
            myThreadCampanha threadCampanha = (myThreadCampanha)_myThreadCampanha;
            WriteLine("ID " + threadCampanha.Campanha.id + " - Iniciando Tratamentos");
            while (isRunnig)
            {
                try
                {       
                    threadCampanha.ProcessarErros();
                    Thread.Sleep(threadCampanha.TimeSleep); 
                }
                catch (Exception ex)
                {
                    WriteLine("ID " + threadCampanha.Campanha.id + " - Tratamentos - Erros: " + ex.Message, ex);
                    Thread.Sleep(threadCampanha.TimeSleep); 
                }
            }
        }
        private static void ExecutarCampanhaUnsubscribeAndSubscribe(object _myThreadCampanha)
        {
            myThreadCampanha threadCampanha = (myThreadCampanha)_myThreadCampanha;
            WriteLine("ID " + threadCampanha.Campanha.id + " - Iniciando Unsubscribe And Subscribe");
            while (isRunnig)
            {
                try
                {
                    threadCampanha.ProcessarUnsubscribeAndSubscribe();
                    Thread.Sleep(threadCampanha.TimeSleep);
                }
                catch (Exception ex)
                {
                    WriteLine("ID " + threadCampanha.Campanha.id + " - Unsubscribe And Subscribe - Erros: " + ex.Message, ex);
                    Thread.Sleep(threadCampanha.TimeSleep);
                }
            }
        }
    }
    public class myThreadCampanha
    {
        private static object lockObject = new object();
        private AspMailList.library.Pop3 pop { get; set; }
        private void WriteLine(string value)
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
        private void WriteLine(string value, Exception ex)
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
        private myThreadCampanha() { }
        public int TimeSleep { get; set; }
        public Mala_Direta_Campanha Campanha { get; set; }
        public myThreadCampanha(Mala_Direta_Campanha campamha) { Campanha = campamha; TimeSleep = 60000; pop = new AspMailList.library.Pop3(); }
        public void ProcessarEmail() 
        {
            int enviado = 0;
            int totalEnvio = 0;
            int Errocount = 0;
            long CountTotal = 0;
            long CountError = 0;

            List<Sp_camanha_email_nao_enviadoResult> Emails = new List<Sp_camanha_email_nao_enviadoResult>();
            using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
            {
                db.ObjectTrackingEnabled = false;
                if (!CoreAssembly.IsRunningOnMono())
                    db.CommandTimeout = db.Connection.ConnectionTimeout;
                Emails = db.Sp_camanha_email_nao_enviado(Campanha.id).ToList();
                //LER DE 5000 EM 5000 E-MAILS
            }
            Smtp mail = new Smtp();
            mail.Body = System.Web.HttpUtility.HtmlDecode(Campanha.BodyHtml);
            mail.EnableSsl = Campanha.EnableSsl;
            mail.DisplayName = Campanha.DisplayName;
            mail.From = Campanha.SmtpUser;
            mail.Password = Campanha.SmtpPassword;
            mail.Port = Campanha.SmtpPort.ToString();
            mail.SmtpServer = Campanha.SmtpServer;
            mail.UseCredentials = true;
            mail.User = Campanha.SmtpUser;

            foreach (Sp_camanha_email_nao_enviadoResult md in Emails)
            {
                totalEnvio++;

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
                        menivado.idCampanha = Campanha.id;
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
                    WriteLine("ID " + Campanha.id + " - Enviados " + totalEnvio + " emails.");
                    CountError++;
                    WriteLine(string.Format("ID {2} - Destino: {0} - Erro: {1}", md.email, ex.Message, Campanha.id), ex);

                    TimeSleep = 60000;
                    if (ex.Message.Contains("too many messages"))
                    {
                        Errocount++;
                        TimeSleep = 900000 * Errocount; //Esperar (15 * erros) minutos antes de enviar o proximo.
                    }
                    else
                        TimeSleep = 60000; //Qualquer erro esperar 1 minuto.

                    WriteLine(string.Format("ID {1} - Esperando {0} segundos para tentar novamente.", TimeSleep, Campanha.id));
                    System.Threading.Thread.Sleep(TimeSleep);
                }
            }
        }
        public void ProcessarErros()
        {
            int count = 0;
            string[] emails = new string[0];
            using (Pop3Client client = pop.pop3Client(Campanha.SmtpServer, Campanha.PopPort, Campanha.EnableSsl, Campanha.SmtpUser, Campanha.SmtpPassword))
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
                        Subject.IndexOf("failure") >= 0 ||
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
                        emails = (from e in femails
                                  where !e.Contains(Campanha.SmtpServer)
                                  select e).ToArray();

                        using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
                        {
                            var optDelete = (from m in db.Mala_Diretas
                                             where emails.Contains(m.email)
                                             select m);

                            foreach (var opt in optDelete)
                            {
                                var optdelEnvio = (from m in db.Mala_Direta_Campanha_Enviados
                                                   where m.idMail == opt.id
                                                   select m);

                                db.Mala_Direta_Campanha_Enviados.DeleteAllOnSubmit(optdelEnvio);
                                db.SubmitChanges();

                                var optdelUnsubscribes = (from m in db.Mala_Direta_Campanha_Unsubscribes
                                                          where m.idMail == opt.id
                                                          select m);

                                db.Mala_Direta_Campanha_Unsubscribes.DeleteAllOnSubmit(optdelUnsubscribes);
                                db.SubmitChanges();
                            }

                            db.Mala_Diretas.DeleteAllOnSubmit(optDelete);
                            db.SubmitChanges();
                        }
                        pop.DeleteMessageByMessageId(client, msg.Headers.MessageId);
                        WriteLine("ID " + Campanha.id + " - Removido o e-mail " + string.Join(";", emails) + " - Subject: " + Subject);
                        return;
                    }
                }
            }
        }
        public void ProcessarHelps()
        {
            using (Pop3Client client = pop.pop3Client(Campanha.SmtpServer, Campanha.PopPort, Campanha.EnableSsl, Campanha.SmtpUser, Campanha.SmtpPassword))
            {
                List<Message> lst = pop.FetchAllMessages(client);
                foreach (Message msg in lst)
                {
                    string Subject = msg.Headers.Subject.ToLower().Trim();
                    if (Subject.IndexOf("help") >= 0)
                    {
                        string from = msg.Headers.From.Address.ToString().ToLower().Trim();
                        AspMailList.library.Smtp smtp = new library.Smtp();
                        smtp.Subject = "Informações sobre a subscrição";
                        smtp.To = from;
                        smtp.EnableSsl = Campanha.EnableSsl;
                        smtp.From = Campanha.SmtpUser;
                        smtp.Password = Campanha.SmtpPassword;
                        smtp.User = Campanha.SmtpUser;
                        smtp.Port = Campanha.SmtpPort.ToString();
                        smtp.SmtpServer = Campanha.SmtpServer;
                        smtp.DisplayName = Campanha.DisplayName;
                        smtp.Body = getHelpBody();
                        smtp.EnviarEmail();

                        pop.DeleteMessageByMessageId(client, msg.Headers.MessageId);
                        WriteLine("ID " + Campanha.id + " - Enviando e-mail de help para " + from);
                        return;
                    }
                }
            }
        }
        public void ProcessarUnsubscribeAndSubscribe()
        {
            using (Pop3Client client = pop.pop3Client(Campanha.SmtpServer, Campanha.PopPort, Campanha.EnableSsl, Campanha.SmtpUser, Campanha.SmtpPassword))
            {
                List<Message> lst = pop.FetchAllMessages(client);
                foreach (Message msg in lst)
                {
                    string Subject = msg.Headers.Subject.ToLower().Trim();
                    if (Subject.IndexOf("subscribe") >= 0 || Subject.IndexOf("unsubscribe") >= 0)
                    {
                        bool subscribe = Subject.IndexOf("unsubscribe") < 0;
                        string sfrom = msg.Headers.From.Address.ToString().ToLower().Trim();
                        AspMailList.library.Smtp smtp = new library.Smtp();
                        smtp.Subject = "Informações sobre a subscrição";
                        smtp.To = sfrom;
                        smtp.EnableSsl = Campanha.EnableSsl;
                        smtp.From = Campanha.SmtpUser;
                        smtp.Password = Campanha.SmtpPassword;
                        smtp.User = Campanha.SmtpUser;
                        smtp.Port = Campanha.SmtpPort.ToString();
                        smtp.SmtpServer = Campanha.SmtpServer;
                        smtp.DisplayName = Campanha.DisplayName;
                        smtp.Body = getHelpBodyUnsubscribeAndSubscribe(subscribe);
                        smtp.EnviarEmail();

                        if (subscribe)
                        {
                            using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
                            {
                                db.Mala_Direta_add_Email(sfrom);
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
                                    m.idCampanha = Campanha.id;
                                    m.idMail = opt.id;
                                    db.Mala_Direta_Campanha_Unsubscribes.InsertOnSubmit(m);
                                    db.SubmitChanges();
                                }
                            }
                        }

                        pop.DeleteMessageByMessageId(client, msg.Headers.MessageId);
                        WriteLine("ID " + Campanha.id + " - Enviando e-mail para " + sfrom + " com objetivo " + Subject);
                        return;
                    }
                }
            }
        }
        public string getHelpBody()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<br>");
            sb.AppendLine("<b>Informações sobre a lista.</b><br>");
            sb.AppendLine("<br>");
            sb.AppendLine("<br>List-Help: <a href='mailto:" + Campanha.SmtpUser + "?subject=Help'>" + Campanha.SmtpUser + "?subject=Help</a>");
            sb.AppendLine("<br>List-Unsubscribe: <a href='mailto:" + Campanha.SmtpUser + "?subject=Unsubscribe'>" + Campanha.SmtpUser + "?subject=Unsubscribe</a>");
            sb.AppendLine("<br>List-Subscribe: <a href='mailto:" + Campanha.SmtpUser + "?subject=Subscribe'>" + Campanha.SmtpUser + "?subject=Subscribe</a>");
            sb.AppendLine("<br>");
            sb.AppendLine("<br>Obrigado.<br>");
            sb.AppendLine(Campanha.DisplayName);
            sb.AppendLine("<br>");
            return sb.ToString();
        }
        public string getHelpBodyUnsubscribeAndSubscribe(bool Subscribe)
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
            sb.AppendLine("<br>List-Help: <a href='mailto:" + Campanha.SmtpUser + "?subject=Help'>" + Campanha.SmtpUser + "?subject=Help</a>");
            if (Subscribe)
                sb.AppendLine("<br>List-Unsubscribe: <a href='mailto:" + Campanha.SmtpUser + "?subject=Unsubscribe'>" + Campanha.SmtpUser + "?subject=Unsubscribe</a>");
            else
                sb.AppendLine("<br>List-Subscribe: <a href='mailto:" + Campanha.SmtpUser + "?subject=Subscribe'>" + Campanha.SmtpUser + "?subject=Subscribe</a>");
            sb.AppendLine("<br>");
            sb.AppendLine("<br>Obrigado.<br>");
            sb.AppendLine(Campanha.DisplayName);
            sb.AppendLine("<br>");
            return sb.ToString();
        }
    }
}
