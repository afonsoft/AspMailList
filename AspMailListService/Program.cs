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
        private static string PathLog
        {
            get
            {
                string directory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "log");
                if (!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);
                return directory;
            }
        }

        private static object lockObject = new object();
        public static void WriteLine(string value)
        {
            lock (lockObject)  // all other threads will wait for y
            {
                Console.WriteLine(value);
                using (var lockStreamWriter = new StreamWriter(Path.Combine(PathLog, "log-" + DateTime.Now.ToString("yyyyMMdd") + ".txt"), true))
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
                using (var lockStreamWriter = new StreamWriter(Path.Combine(PathLog, "log-" + DateTime.Now.ToString("yyyyMMdd") + ".txt"), true))
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
                WriteLine("Sistema: " + (CoreAssembly.IsRunningOnMono() ? "Mono" : "CLR"));
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
                        myThreadCampanha threadcamp = new myThreadCampanha(campanha, PathLog);
                        threadcamp.WriteLine("Iniciando a Campanha " + campanha.DisplayName);
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

    public class SmtpMails
    {
        public int idCamapnha { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }
        public bool isEror { get; set; }
        public int Timeout { get; set; }
        public int Errocount { get; set; }
    }
    public class myThreadCampanha
    {
        #region Private var
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
        private List<string> Erros = new List<string>();
        private static object lockObject = new object();
        private string PathLogExecutable = "C:\\";
        private long CountErroTotalErrosLimite = 0;
        private string PathLog
        {
            get
            {
                if (!System.IO.Directory.Exists(System.IO.Path.Combine(PathLogExecutable, IdCampanha.ToString())))
                    System.IO.Directory.CreateDirectory(System.IO.Path.Combine(PathLogExecutable, IdCampanha.ToString()));
                return System.IO.Path.Combine(PathLogExecutable, IdCampanha.ToString());
            }
        }
        private int IdCampanha
        {
            get
            {
                if (Campanha != null)
                    return Campanha.id;
                else
                    return 0;
            }
        }
        #endregion
        /// <summary>
        /// Recuperar o log de erros.
        /// </summary>
        public string ErrosCampanhaHoje {
            get
            {
                lock (lockObject)  // all other threads will wait for y
                {
                    using (var lockStreamReader = new StreamReader(Path.Combine(PathLog,  "log-" + IdCampanha + "-" + DateTime.Now.ToString("yyyyMMdd") + ".txt"), true))
                    {
                        return lockStreamReader.ReadToEnd().Replace(Environment.NewLine, "<br/>" + Environment.NewLine);
                    }
                }
            }
        } 
        public string ErrosCampanhaTodos
        {
            get
            {
                lock (lockObject)  // all other threads will wait for y
                {

                    string[] files = System.IO.Directory.GetFiles(PathLog, "*.txt");
                    StringBuilder sb = new StringBuilder();
                    foreach (string file in files)
                    {
                        using (var lockStreamReader = new StreamReader(file, true))
                        {
                            sb.AppendLine(lockStreamReader.ReadToEnd().Replace(Environment.NewLine, "<br/>"));
                        }
                    }
                    return sb.ToString();
                }
            }
        }
        private AspMailList.library.Pop3 pop { get; set; }
        public void WriteLine(string value)
        {
            lock (lockObject)  // all other threads will wait for y
            {
                if (Debug)
                    Console.WriteLine(value);

                using (var lockStreamWriter = new StreamWriter(Path.Combine(PathLog, "log-" + IdCampanha + "-" + DateTime.Now.ToString("yyyyMMdd") + ".txt"), true))
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

                using (var lockStreamWriter = new StreamWriter(Path.Combine(PathLog, "log-" + IdCampanha + "-" + DateTime.Now.ToString("yyyyMMdd") + ".txt"), true))
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
        public long CountTotalEnviados
        {
            get
            {
                using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
                {
                    db.DeferredLoadingEnabled = false;
                    return (from b in db.Mala_Direta_Campanha_Enviados
                            where b.idCampanha == Campanha.id
                            select b).Count();
                }
            }
        } 
        public int TimeSleep { get; set; }
        public Mala_Direta_Campanha Campanha { get; set; }
        public List<SmtpMails> lstSmtpMails { get; set; }
        private SmtpMails getDisponivel()
        {
            SmtpMails item = (from s in lstSmtpMails
                              orderby s.Timeout descending
                              where s.isEror == false
                              select s).FirstOrDefault();

            if (item == null)
            {
                item = (from s in lstSmtpMails
                        orderby s.Timeout descending, s.Errocount ascending
                        select s).FirstOrDefault();
            }

            return item;
        }
        public myThreadCampanha(Mala_Direta_Campanha campamha, string pathExecutableLog) 
        { 
            Campanha = campamha;
            PathLogExecutable = pathExecutableLog;
            TimeSleep = 60000; 
            pop = new AspMailList.library.Pop3(); 
            Debug = false;

            using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
            {
                lstSmtpMails = (from s in db.Mala_Direta_Campanha_Smtp_Mails
                                where s.idCamapnha == campamha.id
                                select new SmtpMails
                                {
                                    idCamapnha = s.idCamapnha,
                                    isEror = false,
                                    Timeout = 60000,
                                    SmtpUser = s.SmtpUser,
                                    SmtpPassword = s.SmtpPassword
                                }).ToList();

                lstSmtpMails.Add(new SmtpMails()
                {
                    idCamapnha = campamha.id,
                    isEror = false,
                    Timeout = 60000,
                    SmtpUser = campamha.SmtpUser,
                    SmtpPassword = campamha.SmtpPassword
                });
            }

        }
        public void ProcessarEmail()
        {

            try
            {
                int enviado = 0;
                int totalEnvio = 0;

                List<Sp_camanha_email_nao_enviadoResult> Emails = new List<Sp_camanha_email_nao_enviadoResult>();
                using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
                {
                    db.ObjectTrackingEnabled = false;
                    if (!CoreAssembly.IsRunningOnMono())
                        db.CommandTimeout = db.Connection.ConnectionTimeout;
                    //LER DE 2000 EM 2000 E-MAILS
                    Emails = db.Sp_camanha_email_nao_enviado(Campanha.id).ToList();
                }
                CountEnvioTotal += Emails.Count;

                if (Debug)
                    WriteLine("ID " + Campanha.id + " - Total de registros: " + Emails.Count);

                Smtp mail = new Smtp();
                mail.Body = System.Web.HttpUtility.HtmlDecode(Campanha.BodyHtml);
                mail.EnableSsl = Campanha.EnableSsl;
                mail.DisplayName = Campanha.DisplayName;
                mail.From = Campanha.SmtpUser;
                mail.Port = Campanha.SmtpPort.ToString();
                mail.SmtpServer = Campanha.SmtpServer;
                mail.UseCredentials = true;

                foreach (Sp_camanha_email_nao_enviadoResult md in Emails)
                {
                    totalEnvio++;

                    mail.To = md.email.Replace("\t", "").Replace("\r", "").Replace("\n", "");
                    SmtpMails smtpserver = getDisponivel();
                    try
                    {
                        mail.User = smtpserver.SmtpUser;
                        mail.Password = smtpserver.SmtpPassword;
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
                        smtpserver.isEror = false;
                        smtpserver.Errocount = 0;
                        if (enviado >= 200)
                        {
                            enviado = 0;
                            System.Threading.Thread.Sleep(5000); //Esperar 5 seg. apos o envio de 200;
                        }
                    }

                    catch (Exception ex)
                    {
                        WriteLine("ID " + Campanha.id + " - Enviados " + totalEnvio + " emails.");
                        CountEnvioErro++;
                        smtpserver.isEror = true;
                        if (ex.Message.Contains("too many messages"))
                        {
                            WriteLine(string.Format("ID {2} - Destino: {0} - Erro: {1}", md.email, ex.Message, Campanha.id));
                            smtpserver.Errocount++;
                            smtpserver.Timeout = 900000 * smtpserver.Errocount; //Esperar (15 * erros) minutos antes de enviar o proximo.
                        }
                        else if (ex.Message.Contains("correio não disponível") || ex.Message.Contains("A valid address is required")
                                || ex.Message.Contains("caractere inválido") || ex.Message.Contains("cabeçalho do email")
                                || ex.Message.Contains("inválido"))
                        {
                            smtpserver.Timeout = 5000; //5 Segundos para processar o proximo e-mail.
                            WriteLine("ID " + Campanha.id + " - Removido o e-mail " + md.email + " Erro: " + ex.Message);
                            try
                            {
                                using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
                                {
                                    if (!CoreAssembly.IsRunningOnMono())
                                        db.CommandTimeout = db.Connection.ConnectionTimeout;

                                    var optDelete = (from m in db.Mala_Diretas
                                                     where m.email.Contains(md.email)
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
                            }
                            catch (Exception)
                            {
                                //Ignorar o erro para deletar o e-mail.
                            }
                        }
                        else
                        {
                            WriteLine(string.Format("ID {2} - Destino: {0} - Erro: {1}", md.email, ex.Message, Campanha.id), ex);
                        }

                        TimeSleep = smtpserver.Timeout;
                        System.Threading.Thread.Sleep(smtpserver.Timeout);
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
                string server = Campanha.SmtpServer;
                if (server == "127.0.0.1")
                    server = Campanha.SmtpUser.Substring(Campanha.SmtpUser.IndexOf('@') + 1, Campanha.SmtpUser.Length - (Campanha.SmtpUser.IndexOf('@') + 1));
                using (Pop3Client client = pop.pop3Client(server, Campanha.PopPort, Campanha.EnableSsl, Campanha.SmtpUser, Campanha.SmtpPassword))
                {

                    int messageCount = client.GetMessageCount();
                    if (messageCount == 0)
                        Thread.Sleep(TimeSleep);

                    for (int i = messageCount; i > 0; i--)
                    {
                        Message msg = client.GetMessage(i);
                        if (msg.Headers != null && !string.IsNullOrEmpty(msg.Headers.Subject))
                        {
                            if (msg.Headers.Subject.ToLower().Trim().IndexOf("mail delivery") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("delivery delayed")>=0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("undelivered mail") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("returning message") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("failed") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("undelivered") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("postmaster") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("failure") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("undeliverable") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("não entregue") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("delivery failure") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("falha na entrega") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("não foi possível enviar") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("Returned mail") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("delivery problems") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("retorno de mensagem") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("returned mail") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("não é possível entregar") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("nao foi entregue") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("automática") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("automático") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("rejected") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("falha ao entregar") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("rejeitado") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("delivery status") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("devolvida") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("automatic") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("ausência") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("out of office") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("non remis") >= 0
                                || msg.Headers.Subject.ToLower().Trim().IndexOf("not found") >= 0)
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

                                if (msg.Headers.Subject.ToLower().Trim().IndexOf("automática") < 0
                                    && msg.Headers.Subject.ToLower().Trim().IndexOf("automático") < 0
                                    && msg.Headers.Subject.ToLower().Trim().IndexOf("spam") < 0
                                    && msg.Headers.Subject.ToLower().Trim().IndexOf("delivery status") < 0
                                    && msg.Headers.Subject.ToLower().Trim().IndexOf("ausência") < 0
                                    && msg.Headers.Subject.ToLower().Trim().IndexOf("automatic") < 0
                                    && msg.Headers.Subject.ToLower().Trim().IndexOf("out of office") < 0
                                    && emails.Count() > 0)
                                {
                                    using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
                                    {
                                        if (!CoreAssembly.IsRunningOnMono())
                                            db.CommandTimeout = db.Connection.ConnectionTimeout;

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
                                }
                                CountErroTotal++;
                                WriteLine("ID " + Campanha.id + " - Removido o e-mail " + string.Join(";", emails));
                                break;
                            }
                        }
                        else
                        {
                            pop.DeleteMessageByMessageId(client, msg.Headers.MessageId);
                            break;
                        }
                    }
                }
                Thread.Sleep(5000);
            }
            catch (Exception ex)
            {
                CountErroTotalErros++;
                if (ex.Message.IndexOf("-ERR") >= 0)
                {
                    Thread.Sleep(5000);
                    WriteLine("ID " + Campanha.id + " - Tratamentos - Erros: " + ex.Message);
                }
                else
                {
                    CountErroTotalErrosLimite++;
                    WriteLine("ID " + Campanha.id + " - Tratamentos - Erros: " + ex.Message, ex);
                    Thread.Sleep(30000);
                    if (CountErroTotalErrosLimite >= 10)
                    {
                        WriteLine("ID " + Campanha.id + " - Tratamentos - Erros: Muitos erros, aguardando " + TimeSleep + " segundos para o proximo processamento.");
                        CountErroTotalErrosLimite = 0;
                        Thread.Sleep(TimeSleep);
                    }
                }
            }
        }
        public void ProcessarHelps()
        {
            try
            {
                string server = Campanha.SmtpServer;
                if (server == "127.0.0.1")
                    server = Campanha.SmtpUser.Substring(Campanha.SmtpUser.IndexOf('@') + 1, Campanha.SmtpUser.Length - (Campanha.SmtpUser.IndexOf('@') + 1));
                using (Pop3Client client = pop.pop3Client(server, Campanha.PopPort, Campanha.EnableSsl, Campanha.SmtpUser, Campanha.SmtpPassword))
                {
                    int messageCount = client.GetMessageCount();
                    if (messageCount == 0)
                        Thread.Sleep(TimeSleep);

                    for (int i = messageCount; i > 0; i--)
                    {
                        Message msg = client.GetMessage(i);

                        if (msg.Headers!= null && !string.IsNullOrEmpty(msg.Headers.Subject) && msg.Headers.Subject.ToLower().Trim().IndexOf("help") >= 0)
                        {
                            string Subject = msg.Headers.Subject.ToLower().Trim();
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
                Thread.Sleep(5000);
            }
            catch (Exception ex)
            {
                WriteLine("ID " + Campanha.id + " - Help - Erros: " + ex.Message, ex);
                CountHelpTotalErros++;
                Thread.Sleep(TimeSleep);
            }
        }
        public void ProcessarUnsubscribeAndSubscribe()
        {
            try
            {
                string server = Campanha.SmtpServer;
                if (server == "127.0.0.1")
                    server = Campanha.SmtpUser.Substring(Campanha.SmtpUser.IndexOf('@') + 1, Campanha.SmtpUser.Length - (Campanha.SmtpUser.IndexOf('@') + 1));
                using (Pop3Client client = pop.pop3Client(server, Campanha.PopPort, Campanha.EnableSsl, Campanha.SmtpUser, Campanha.SmtpPassword))
                {
                    int messageCount = client.GetMessageCount();
                    if (messageCount == 0)
                        Thread.Sleep(TimeSleep);

                    for (int i = messageCount; i > 0; i--)
                    {
                        Message msg = client.GetMessage(i);
                        if (msg.Headers != null && !string.IsNullOrEmpty(msg.Headers.Subject))
                        {
                            if (msg.Headers.Subject.ToLower().Trim().IndexOf("subscribe") >= 0
                            || msg.Headers.Subject.ToLower().Trim().IndexOf("unsubscribe") >= 0
                            || msg.Headers.Subject.ToLower().Trim().IndexOf("add") >= 0
                            || msg.Headers.Subject.ToLower().Trim().IndexOf("remove") >= 0
                            || msg.Headers.Subject.ToLower().Trim().IndexOf("count") >= 0
                            || msg.Headers.Subject.ToLower().Trim().IndexOf("info") >= 0
                            || msg.Headers.Subject.ToLower().Trim().IndexOf("command") >= 0
                            || msg.Headers.Subject.ToLower().Trim().IndexOf("error") >= 0
                            || msg.Headers.Subject.ToLower().Trim().IndexOf("erros") >= 0)
                            {
                                string Subject = msg.Headers.Subject.ToLower().Trim();
                                MessagePart msgpart = msg.FindFirstHtmlVersion();

                                if (msgpart == null)
                                    msgpart = msg.FindFirstPlainTextVersion();
                                if (msgpart == null)
                                    msgpart = msg.MessagePart;

                                if (Subject.IndexOf("subscribe") >= 0 || Subject.IndexOf("unsubscribe") >= 0)
                                {
                                    #region subscribe unsubscribe
                                    bool subscribe = Subject.IndexOf("unsubscribe") < 0 && Subject.IndexOf("subscribe") > 0;
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
                                            if (!CoreAssembly.IsRunningOnMono())
                                                db.CommandTimeout = db.Connection.ConnectionTimeout;

                                            db.Mala_Direta_add_Email(sfrom);
                                            db.SubmitChanges();
                                        }
                                    }
                                    else
                                    {
                                        using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
                                        {
                                            if (!CoreAssembly.IsRunningOnMono())
                                                db.CommandTimeout = db.Connection.ConnectionTimeout;

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
                                    #endregion
                                }
                                else if (Subject.IndexOf("add") >= 0)
                                {
                                    #region add
                                    string Body = msgpart.GetBodyAsText();
                                    string[] femails = AspMailList.library.ValidEmail.getListMail(Body);
                                    string[] emails = (from e in femails
                                                       where !e.Contains(Campanha.SmtpServer)
                                                       && !e.Contains("=")
                                                       select e).ToArray();

                                    pop.DeleteMessageByMessageId(client, msg.Headers.MessageId);

                                    foreach (string sfrom in emails)
                                    {
                                        try
                                        {
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
                                            smtp.Body = getHelpBodyUnsubscribeAndSubscribe(true);
                                            smtp.EnviarEmail();
                                        }
                                        catch { }

                                        CountSubscribeTotal++;
                                        using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
                                        {
                                            if (!CoreAssembly.IsRunningOnMono())
                                                db.CommandTimeout = db.Connection.ConnectionTimeout;

                                            db.Mala_Direta_add_Email(sfrom);
                                            db.SubmitChanges();
                                        }

                                        WriteLine("ID " + Campanha.id + " - Adicionado o e-mail " + sfrom);
                                    }
                                    #endregion
                                }
                                else if (Subject.IndexOf("remove") >= 0)
                                {
                                    #region remove
                                    string Body = msgpart.GetBodyAsText();
                                    string[] femails = AspMailList.library.ValidEmail.getListMail(Body);
                                    string[] emails = (from e in femails
                                                       where !e.Contains(Campanha.SmtpServer)
                                                       && !e.Contains("=")
                                                       select e).ToArray();

                                    using (dbMalaDiretaDataContext db = new dbMalaDiretaDataContext())
                                    {
                                        if (!CoreAssembly.IsRunningOnMono())
                                            db.CommandTimeout = db.Connection.ConnectionTimeout;

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

                                    WriteLine("ID " + Campanha.id + " - Removido o e-mail " + string.Join(";", emails));
                                    pop.DeleteMessageByMessageId(client, msg.Headers.MessageId);
                                    #endregion
                                }
                                else if (Subject.IndexOf("count") >= 0 || Subject.IndexOf("info") >= 0)
                                {
                                    #region info
                                    string sfrom = msg.Headers.From.Address.ToString().ToLower().Trim();

                                    AspMailList.library.Smtp smtp = new library.Smtp();
                                    smtp.Subject = "Informações sobre a Lista";
                                    smtp.To = sfrom;
                                    smtp.EnableSsl = Campanha.EnableSsl;
                                    smtp.From = Campanha.SmtpUser;
                                    smtp.Password = Campanha.SmtpPassword;
                                    smtp.User = Campanha.SmtpUser;
                                    smtp.Port = Campanha.SmtpPort.ToString();
                                    smtp.SmtpServer = Campanha.SmtpServer;
                                    smtp.DisplayName = Campanha.DisplayName + " - Info";
                                    smtp.Body = this.ToString().Replace(Environment.NewLine, "<br/>");
                                    smtp.EnviarEmail();

                                    pop.DeleteMessageByMessageId(client, msg.Headers.MessageId);
                                    WriteLine("ID " + Campanha.id + " - Solicitou as informações para: " + sfrom);
                                    #endregion
                                }
                                else if (Subject.IndexOf("command") >= 0)
                                {
                                    #region command
                                    string sfrom = msg.Headers.From.Address.ToString().ToLower().Trim();

                                    AspMailList.library.Smtp smtp = new library.Smtp();
                                    smtp.Subject = "Informações sobre os comando da Lista";
                                    smtp.To = sfrom;
                                    smtp.EnableSsl = Campanha.EnableSsl;
                                    smtp.From = Campanha.SmtpUser;
                                    smtp.Password = Campanha.SmtpPassword;
                                    smtp.User = Campanha.SmtpUser;
                                    smtp.Port = Campanha.SmtpPort.ToString();
                                    smtp.SmtpServer = Campanha.SmtpServer;
                                    smtp.DisplayName = Campanha.DisplayName + " - Command";
                                    smtp.Body = getHelpBodyUnsubscribeAndSubscribe(true, true);
                                    smtp.EnviarEmail();

                                    pop.DeleteMessageByMessageId(client, msg.Headers.MessageId);
                                    WriteLine("ID " + Campanha.id + " - Solicitou os comandos para: " + sfrom);

                                    #endregion
                                }
                                else if (Subject.IndexOf("error") >= 0 || Subject.IndexOf("erros") >= 0)
                                {
                                    #region Erros
                                    string sfrom = msg.Headers.From.Address.ToString().ToLower().Trim();

                                    AspMailList.library.Smtp smtp = new library.Smtp();
                                    smtp.Subject = "Informações sobre os erros desta lista";
                                    smtp.To = sfrom;
                                    smtp.EnableSsl = Campanha.EnableSsl;
                                    smtp.From = Campanha.SmtpUser;
                                    smtp.Password = Campanha.SmtpPassword;
                                    smtp.User = Campanha.SmtpUser;
                                    smtp.Port = Campanha.SmtpPort.ToString();
                                    smtp.SmtpServer = Campanha.SmtpServer;
                                    smtp.DisplayName = Campanha.DisplayName + " - Info";
                                    if (Subject.IndexOf("todos") >= 0 || Subject.IndexOf("all") >= 0)
                                        smtp.Body = this.ErrosCampanhaTodos;
                                    else
                                        smtp.Body = this.ErrosCampanhaHoje;
                                    smtp.EnviarEmail();

                                    pop.DeleteMessageByMessageId(client, msg.Headers.MessageId);
                                    WriteLine("ID " + Campanha.id + " - Solicitou as informações de erros para: " + sfrom);
                                    #endregion
                                }
                                break;
                            }
                        }
                        else
                        {
                            pop.DeleteMessageByMessageId(client, msg.Headers.MessageId);
                            break;
                        }
                    }
                }
                Thread.Sleep(5000);
            }
            catch (Exception ex)
            {
                WriteLine("ID " + Campanha.id + " - Unsubscribe And Subscribe - Erros: " + ex.Message, ex);
                CountSubscribeTotalErros++;
                Thread.Sleep(TimeSleep);
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
            return getHelpBodyUnsubscribeAndSubscribe(Subscribe, false);
        }
        public string getHelpBodyUnsubscribeAndSubscribe(bool Subscribe, bool command)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<br>");
            sb.AppendLine("<b>Informações sobre a sua subscrição.</b>");
            sb.AppendLine("<br>");
            if (!command)
            {
                if (Subscribe)
                    sb.AppendLine("<br>Você acabou de entrar no grupo da lista.");
                else
                    sb.AppendLine("<br>Você acabou de sair no grupo da lista.");
            }
            else
                sb.AppendLine("<br>Comandos da Lista.");
            sb.AppendLine("<br>");
            sb.AppendLine("<br>List-Help: <a href='mailto:" + Campanha.SmtpUser + "?subject=Help'>" + Campanha.SmtpUser + "?subject=Help</a>");
            if (Subscribe)
                sb.AppendLine("<br>List-Unsubscribe: <a href='mailto:" + Campanha.SmtpUser + "?subject=Unsubscribe'>" + Campanha.SmtpUser + "?subject=Unsubscribe</a>");
            else
                sb.AppendLine("<br>List-Subscribe: <a href='mailto:" + Campanha.SmtpUser + "?subject=Subscribe'>" + Campanha.SmtpUser + "?subject=Subscribe</a>");

            if (command)
            {
                sb.AppendLine("<br>List-Subscribe: <a href='mailto:" + Campanha.SmtpUser + "?subject=info'>" + Campanha.SmtpUser + "?subject=info</a> Recupera as informações de Status.");
                sb.AppendLine("<br>List-Subscribe: <a href='mailto:" + Campanha.SmtpUser + "?subject=add'>" + Campanha.SmtpUser + "?subject=add</a> Adiciona os email do corpo na lista.");
                sb.AppendLine("<br>List-Subscribe: <a href='mailto:" + Campanha.SmtpUser + "?subject=remove'>" + Campanha.SmtpUser + "?subject=remove</a> Remova os email do corpo na lista.");
                sb.AppendLine("<br>List-Subscribe: <a href='mailto:" + Campanha.SmtpUser + "?subject=error'>" + Campanha.SmtpUser + "?subject=error</a> Informações sobre os erros ocorridos.");
            }
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
            sb.AppendFormat(" Total Enviados      : ->{0}{1}", CountTotalEnviados, Environment.NewLine);
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
