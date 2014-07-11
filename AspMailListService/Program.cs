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
                    WriteLine("Lendo database...");
                    //Emails = (from m in db.Mala_Diretas
                    //          select m.email).ToList();
                    WriteLine("Localizado " + Emails.Count() + " registro.");
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
                    }
                    while (isRunnig)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Clear();
                isRunnig = false;
                WriteLine("Ocorreu um erro!");
                WriteLine("Erro: " + ex.Message);
                WriteLine("StackTrace: " + ex.StackTrace);
                WriteLine("Precione um tecla para sair.");
                Console.ReadKey();
            }
        }

        private static void ExecutarCampanhaHelps(object _myThreadCampanha)
        {

            while (isRunnig)
            {
                try
                {
                    myThreadCampanha threadCampanha = (myThreadCampanha)_myThreadCampanha;
                    WriteLine("Iniciando (ExecutarCampanhaHelps): " + threadCampanha.Campanha.DisplayName);
                    threadCampanha.ProcessarHelps(threadCampanha.Campanha.SmtpServer, threadCampanha.Campanha.PopPort, threadCampanha.Campanha.SmtpPort, threadCampanha.Campanha.EnableSsl, threadCampanha.Campanha.SmtpUser, threadCampanha.Campanha.SmtpPassword, threadCampanha.Campanha.DisplayName);
                    Thread.Sleep(60000); //1 Minutos
                }
                catch (Exception ex)
                {
                    WriteLine("(ExecutarCampanhaHelps) Erros: " + ex.Message);
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
                    WriteLine("Iniciando (ExecutarCampanhaErros): " + threadCampanha.Campanha.DisplayName);
                    threadCampanha.ProcessarErros(threadCampanha.Campanha.SmtpServer, threadCampanha.Campanha.PopPort, threadCampanha.Campanha.EnableSsl, threadCampanha.Campanha.SmtpUser, threadCampanha.Campanha.SmtpPassword);
                    Thread.Sleep(60000 * 10); //10 Minutos
                }
                catch (Exception ex)
                {
                    WriteLine("(ExecutarCampanhaErros) Erros: " + ex.Message);
                    Thread.Sleep(60000 * 60); //60 Minutos
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
                    WriteLine("Iniciando (ExecutarCampanhaUnsubscribeAndSubscribe): " + threadCampanha.Campanha.DisplayName);
                    threadCampanha.ProcessarUnsubscribeAndSubscribe(threadCampanha.Campanha.SmtpServer, threadCampanha.Campanha.PopPort, threadCampanha.Campanha.SmtpPort, threadCampanha.Campanha.EnableSsl, threadCampanha.Campanha.SmtpUser, threadCampanha.Campanha.SmtpPassword, threadCampanha.Campanha.DisplayName);
                    Thread.Sleep(60000); //1 Minutos
                }
                catch (Exception ex)
                {
                    WriteLine("(ExecutarCampanhaUnsubscribeAndSubscribe) Erros: " + ex.Message);
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

        public void ProcessarUnsubscribeAndSubscribe(string host, int popPort, int smtpPort, bool ssl, string user, string pass, string displayName)
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
                                db.Mala_Diretas.DeleteAllOnSubmit(optdelete);
                                db.SubmitChanges();
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
            sb.AppendLine("<br>List-Help: " + mail + "?subject=Help");
            sb.AppendLine("<br>List-Unsubscribe: " + mail + "?subject=Unsubscribe");
            sb.AppendLine("<br>List-Subscribe: " + mail + "?subject=Subscribe");
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
            sb.AppendLine("<br>List-Help: " + mail + "?subject=Help");
            if (Subscribe)
                sb.AppendLine("<br>List-Unsubscribe: " + mail + "?subject=Unsubscribe");
            else
                sb.AppendLine("<br>List-Subscribe: " + mail + "?subject=Subscribe");
            sb.AppendLine("<br>");
            sb.AppendLine("<br>Obrigado.<br>");
            sb.AppendLine(displayName);
            sb.AppendLine("<br>");
            return sb.ToString();
        }
    }
}
