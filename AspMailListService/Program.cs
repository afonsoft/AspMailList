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
        public static List<Thread> Threads { get; set; }
        public static List<myThreadCampanha> ThreadCampanha { get; set; }

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
            WriteLine("");
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
                WriteLine("Pressine D para Debug. (Press D for Debug)");
                WriteLine("Pressine I para Informações. (Press I for information)");
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
                    ThreadCampanha = new List<myThreadCampanha>();
                    foreach (var campanha in listCampanha)
                    {
                        myThreadCampanha threadcamp = new myThreadCampanha(campanha);
                        ThreadCampanha.Add(threadcamp);

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

                    WriteLine("Total de Threads iniciadas " + Threads.Count);
                    while (isRunnig)
                    {
                        WriteLine("");
                        Console.Write("Command: ");
                        Thread.Sleep(500);
                        ConsoleKeyInfo keyinfo = Console.ReadKey();
                        if (keyinfo.Key == ConsoleKey.Q)
                        {
                            isRunnig = false;
                        }
                        else if (keyinfo.Key == ConsoleKey.D)
                        {
                            foreach (var t in ThreadCampanha)
                            {
                                t.Debug = !t.Debug;
                                WriteLine("");
                                WriteLine("ID " + t.Campanha.id + " - Debug Mode " + (t.Debug ? "On" : "Off"));
                            }

                        }
                        else if (keyinfo.Key == ConsoleKey.I)
                        {
                            foreach (var t in ThreadCampanha)
                            {
                                WriteLine(t.ToString());
                            }

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
        }

        private static void ExecutarCampanhaEnvioEmail(object _myThreadCampanha)
        {
            myThreadCampanha threadCampanha = (myThreadCampanha)_myThreadCampanha;
            WriteLine("ID " + threadCampanha.Campanha.id + " - Iniciando EnvioEmail");

            while (isRunnig)
            {
                threadCampanha.ProcessarEmail();
                Thread.Sleep(threadCampanha.TimeSleep);
            }
        }
        private static void ExecutarCampanhaHelps(object _myThreadCampanha)
        {
            myThreadCampanha threadCampanha = (myThreadCampanha)_myThreadCampanha;
            WriteLine("ID " + threadCampanha.Campanha.id + " - Iniciando Help");

            while (isRunnig)
            {
                threadCampanha.ProcessarHelps();
                Thread.Sleep(threadCampanha.TimeSleep);
            }
        }
        private static void ExecutarCampanhaErros(object _myThreadCampanha)
        {
            myThreadCampanha threadCampanha = (myThreadCampanha)_myThreadCampanha;
            WriteLine("ID " + threadCampanha.Campanha.id + " - Iniciando Tratamentos");

            while (isRunnig)
            {
                threadCampanha.ProcessarErros();
                Thread.Sleep(5000);
            }
        }
        private static void ExecutarCampanhaUnsubscribeAndSubscribe(object _myThreadCampanha)
        {
            myThreadCampanha threadCampanha = (myThreadCampanha)_myThreadCampanha;
            WriteLine("ID " + threadCampanha.Campanha.id + " - Iniciando Unsubscribe And Subscribe");

            while (isRunnig)
            {
                threadCampanha.ProcessarUnsubscribeAndSubscribe();
                Thread.Sleep(threadCampanha.TimeSleep);
            }
        }
    }
    public class myThreadCampanha
    {
        #region Provate var
        private long CountEnvioSucesso = 0;
        private long CountEnvioErro = 0;
        private long CountEnvioTotal = 0;
        private long CountHelpTotal = 0;
        private long CountHelpTotalErros = 0;
        private long CountErroTotal = 0;
        private long CountErroTotalErros = 0;
        private long CountSubscribeTotal = 0;
        private long CountUnsubscribeTotal = 0;
        private long CountSubscribeTotalErros = 0;
        private static object lockObject = new object();
        #endregion

        private AspMailList.library.Pop3 pop { get; set; }
        private void WriteLine(string value)
        {
            lock (lockObject)  // all other threads will wait for y
            {
                if (Debug)
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
                if (Debug)
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
        public bool Debug { get; set; }
        public int TimeSleep { get; set; }
        public Mala_Direta_Campanha Campanha { get; set; }
        public myThreadCampanha(Mala_Direta_Campanha campamha) { Campanha = campamha; TimeSleep = 60000; pop = new AspMailList.library.Pop3(); Debug = false; }
        public void ProcessarEmail()
        {
            try
            {
                int enviado = 0;
                int Errocount = 0;
                int totalEnvio = 0;

                List<Sp_camanha_email_nao_enviadoResult> Emails = new List<Sp_camanha_email_nao_enviadoResult>();
                using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
                {
                    db.ObjectTrackingEnabled = false;
                    if (!CoreAssembly.IsRunningOnMono())
                        db.CommandTimeout = db.Connection.ConnectionTimeout;
                    Emails = db.Sp_camanha_email_nao_enviado(Campanha.id).ToList();
                    //LER DE 5000 EM 5000 E-MAILS
                }
                CountEnvioTotal += Emails.Count;

                if (Debug)
                    WriteLine("ID " + Campanha.id + " - Total de registros: " + Emails.Count);

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

                        if (Debug)
                            WriteLine("ID " + Campanha.id + " - Enviados: " + totalEnvio + " de " + Emails.Count + " - Email enviado: " + mail.To);

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
                        CountEnvioSucesso++;
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
                        CountEnvioErro++;
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
            catch (Exception ex)
            {
                WriteLine("ID " + Campanha.id + " - EnvioEmail - Erros: " + ex.Message, ex);
            }
        }
        public void ProcessarErros()
        {
            try
            {
                string[] emails = new string[0];
                using (Pop3Client client = pop.pop3Client(Campanha.SmtpServer, Campanha.PopPort, Campanha.EnableSsl, Campanha.SmtpUser, Campanha.SmtpPassword))
                {
                    List<Message> lst = pop.FetchAllMessages(client);

                    lst = (from l in lst
                           where l.Headers.Subject.ToLower().Trim().IndexOf("mail delivery") >= 0
                           || l.Headers.Subject.ToLower().Trim().IndexOf("failed") >= 0
                           || l.Headers.Subject.ToLower().Trim().IndexOf("undelivered") >= 0
                           || l.Headers.Subject.ToLower().Trim().IndexOf("postmaster") >= 0
                           || l.Headers.Subject.ToLower().Trim().IndexOf("failure") >= 0
                           || l.Headers.Subject.ToLower().Trim().IndexOf("undeliverable") >= 0
                           || l.Headers.Subject.ToLower().Trim().IndexOf("não entregue") >= 0
                           || l.Headers.Subject.ToLower().Trim().IndexOf("delivery failure") >= 0
                           || l.Headers.Subject.ToLower().Trim().IndexOf("falha na entrega") >= 0
                           || l.Headers.Subject.ToLower().Trim().IndexOf("não foi possível enviar") >= 0
                           || l.Headers.Subject.ToLower().Trim().IndexOf("Returned mail") >= 0
                           || l.Headers.Subject.ToLower().Trim().IndexOf("delivery problems") >= 0
                           || l.Headers.Subject.ToLower().Trim().IndexOf("retorno de mensagem") >= 0
                           select l).ToList();

                    if (Debug)
                        WriteLine("ID " + Campanha.id + " - Total de e-mail para remover: " + lst.Count);

                    if (lst.Count == 0)
                        Thread.Sleep(60000);

                    foreach (Message msg in lst)
                    {
                        MessagePart msgpart = msg.FindFirstHtmlVersion();
                        if (msgpart == null)
                            msgpart = msg.FindFirstPlainTextVersion();
                        if (msgpart == null)
                            msgpart = msg.MessagePart;

                        string Body = msgpart.GetBodyAsText();

                        string[] femails = AspMailList.library.ValidEmail.getListMail(Body);
                        emails = (from e in femails
                                  where !e.Contains(Campanha.SmtpServer)
                                  && !e.Contains("=")
                                  select e).ToArray();

                        pop.DeleteMessageByMessageId(client, msg.Headers.MessageId);

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
                            CountErroTotal++;
                        }

                        WriteLine("ID " + Campanha.id + " - Removido o e-mail " + string.Join(";", emails));
                    }
                }
            }
            catch (Exception ex)
            {
                CountErroTotalErros++;
                if (ex.Message.IndexOf("-ERR") >= 0)
                    WriteLine("ID " + Campanha.id + " - Tratamentos - Erros: " + ex.Message);
                else
                {
                    WriteLine("ID " + Campanha.id + " - Tratamentos - Erros: " + ex.Message, ex);
                    Thread.Sleep(TimeSleep);
                }
            }
        }
        public void ProcessarHelps()
        {
            try
            {
                using (Pop3Client client = pop.pop3Client(Campanha.SmtpServer, Campanha.PopPort, Campanha.EnableSsl, Campanha.SmtpUser, Campanha.SmtpPassword))
                {
                    List<Message> lst = pop.FetchAllMessages(client);

                    lst = (from l in lst
                           where l.Headers.Subject.ToLower().Trim().IndexOf("help") >= 0
                           select l).ToList();

                    if (Debug)
                        WriteLine("ID " + Campanha.id + " - Total de e-mail para ajuda: " + lst.Count);

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
                            CountHelpTotal++;
                            WriteLine("ID " + Campanha.id + " - Enviando e-mail de help para " + from);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLine("ID " + Campanha.id + " - Help - Erros: " + ex.Message, ex);
                CountHelpTotalErros++;
            }
        }
        public void ProcessarUnsubscribeAndSubscribe()
        {
            try
            {
                using (Pop3Client client = pop.pop3Client(Campanha.SmtpServer, Campanha.PopPort, Campanha.EnableSsl, Campanha.SmtpUser, Campanha.SmtpPassword))
                {
                    List<Message> lst = pop.FetchAllMessages(client);

                    lst = (from l in lst
                           where l.Headers.Subject.ToLower().Trim().IndexOf("subscribe") >= 0
                           || l.Headers.Subject.ToLower().Trim().IndexOf("unsubscribe") >= 0
                           select l).ToList();

                    if (Debug)
                        WriteLine("ID " + Campanha.id + " - Total de e-mail para Subscribe: " + lst.Count);

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

                            pop.DeleteMessageByMessageId(client, msg.Headers.MessageId);

                            if (subscribe)
                            {
                                CountSubscribeTotal++;
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
                                CountUnsubscribeTotal++;
                            }
                            WriteLine("ID " + Campanha.id + " - Enviando e-mail para " + sfrom + " com objetivo " + Subject);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLine("ID " + Campanha.id + " - Unsubscribe And Subscribe - Erros: " + ex.Message, ex);
                CountSubscribeTotalErros++;
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
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("");
            sb.AppendFormat("ID: {0} - Camapnha: {1}{2}", Campanha.id, Campanha.DisplayName, Environment.NewLine);
            sb.AppendFormat(" Envios com Sucessos : ->{0}{1}", CountEnvioSucesso, Environment.NewLine);
            sb.AppendFormat(" Envios com Erros    : ->{0}{1}", CountEnvioErro, Environment.NewLine);
            sb.AppendFormat(" Total de Envios     : ->{0}{1}", CountEnvioTotal, Environment.NewLine);
            sb.AppendFormat(" Total de Ajudas     : ->{0}{1}", CountHelpTotal, Environment.NewLine);
            sb.AppendFormat(" Ajudas com Erros    : ->{0}{1}", CountHelpTotalErros, Environment.NewLine);
            sb.AppendFormat(" Total de Tratamentos: ->{0}{1}", CountErroTotal, Environment.NewLine);
            sb.AppendFormat(" Tratamentos Erros   : ->{0}{1}", CountErroTotalErros, Environment.NewLine);
            sb.AppendFormat(" Total de Subscribe  : ->{0}{1}", CountSubscribeTotal, Environment.NewLine);
            sb.AppendFormat(" Total de Unsubscribe: ->{0}{1}", CountUnsubscribeTotal, Environment.NewLine);
            sb.AppendFormat(" Subscribe Erros     : ->{0}{1}", CountSubscribeTotalErros, Environment.NewLine);
            sb.AppendFormat(" Tempo de Espera     : ->{0}{1}", TimeSleep, Environment.NewLine);
            sb.AppendFormat(" Debug Mode          : ->{0}{1}", Debug ? "Sim" : "Não", Environment.NewLine);
            sb.AppendFormat(" Informação Obtida   : ->{0}{1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), Environment.NewLine);
            sb.AppendLine("");
            return sb.ToString();
        }
    }
}
