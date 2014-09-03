using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace AspMailList.library
{
    public class Smtp
    {
        #region private
        private string _From = string.Empty;
        private string _To = string.Empty;
        private string _CC = string.Empty;
        private string _CCo = string.Empty;
        private string _Subject = string.Empty;
        private string _Body = string.Empty;
        private string _Smtp = string.Empty;
        private bool _UseDefaultCredentials = false;
        private string _User = string.Empty;
        private string _Pass = string.Empty;
        private string _DisplayName = string.Empty;
        private string _Port = "25";
        private bool _EnableSsl = false;
        private List<string> LstFile = new List<string>();
        #endregion

        #region Get and Set
        public NetworkCredential Credential
        {
            get
            {
                return new NetworkCredential(User, Password);
            }
        }

        public string Port
        {
            get { return _Port; }
            set { _Port = value; }
        }

        public bool EnableSsl
        {
            get { return _EnableSsl; }
            set { _EnableSsl = value; }
        }

        public string DisplayName
        {
            get { return (string.IsNullOrEmpty(_DisplayName) ? From : _DisplayName); }
            set { _DisplayName = value; }
        }

        public string CCo
        {
            get { return _CCo; }
            set { _CCo = value; }
        }

        public string CC
        {
            get { return _CC; }
            set { _CC = value; }
        }
        public String Password
        {
            get { return _Pass; }
            set { _Pass = value; }
        }
        public String User
        {
            get { return _User; }
            set { _User = value; }
        }
        public String To
        {
            get { return _To; }
            set { _To = value; }
        }
        public String From
        {
            get { return _From; }
            set { _From = value; }
        }
        public String Subject
        {
            get { return _Subject; }
            set { _Subject = value; }
        }
        public String Body
        {
            get { return _Body; }
            set { _Body = value; }
        }
        public String SmtpServer
        {
            get { return _Smtp; }
            set { _Smtp = value; }
        }
        public Boolean UseCredentials
        {
            get { return _UseDefaultCredentials; }
            set { _UseDefaultCredentials = value; }
        }
        public void AddFile(string path)
        {
            if (!LstFile.Contains(path))
                LstFile.Add(path);
        }

        public int CountFile()
        {
            return LstFile.Count;
        }
        #endregion

        public string GetUniqueKey(int Size)
        {
            char[] chars = "0123456789".ToCharArray();
            byte[] data = new byte[Size];
            StringBuilder result = new StringBuilder(Size);
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            foreach (byte b in data)
                result.Append(chars[b % (chars.Length - 1)]);

            return result.ToString().ToUpper();
        }

        /// <summary>
        /// Enviar o e-mail
        /// </summary>
        public void EnviarEmail(string sServer, string sPort, string sUser, string sPassword, bool bEnableSsl, string sDisplayName, string sSubject, string sTo, string sFrom, string sBody)
        {
            SmtpServer = sServer;
            Port = sPort;
            User = sUser;
            Password = sPassword;
            EnableSsl = bEnableSsl;
            DisplayName = sDisplayName;
            Subject = sSubject;
            To = sTo;
            From = sFrom;
            Body = sBody;

            EnviarEmail();
        }

        /// <summary>
        /// Eniviar o e-mail
        /// </summary>
        public void EnviarEmail()
        {
            using (SmtpClient smtp = new SmtpClient(SmtpServer, int.Parse(Port)))
            {
                smtp.Timeout = 1800000; // 30 Minutos

                UseCredentials = true;
                if (string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(User))
                    UseCredentials = false;
                
                if (!CoreAssembly.IsRunningOnMono())
                {
                    if (UseCredentials)
                        smtp.UseDefaultCredentials = false;
                    else
                        smtp.UseDefaultCredentials = true;
                }

                if (UseCredentials)
                    smtp.Credentials = Credential;

                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.EnableSsl = EnableSsl;

                MailAddress from = new MailAddress(From.Trim().ToLower(), DisplayName);
                using (MailMessage mail = new MailMessage())
                {
                    mail.Subject = DisplayName;
                    mail.Body = Body + getRodape(User);
                    mail.IsBodyHtml = true;
                    mail.Priority = MailPriority.Normal;

                    mail.BodyEncoding = Encoding.GetEncoding("ISO-8859-1");
                    mail.SubjectEncoding = Encoding.GetEncoding("ISO-8859-1");
                    //Preventing gmail to mark our mails as spam
                    mail.Headers.Add("X-Spam-Status", "No, score=1.3");
                    mail.Headers.Add("X-Spam-Flag", "No");
                    mail.Headers.Add("X-Spam-Score", "-12");
                    mail.Headers.Add("X-Spam-Level", "---");
                    mail.Headers.Add("X-Spam-Bar", "-");
                    mail.Headers.Add("X-Company", DisplayName);
                    mail.Headers.Add("X-Location", "Brazil");
                    mail.Headers.Add("X-No-Archive", "YES");
                    mail.Headers.Add("X-Organization", DisplayName);
                    mail.Headers.Add("X-Unique-Id", GetUniqueKey(8));
                    mail.Headers.Add("X-Application-name", "AspMailList");
                    mail.Headers.Add("X-AspMailList-File-Version", CoreAssembly.getFileVersion);
                    mail.Headers.Add("X-AspMailList-Version", CoreAssembly.getVersion);
                    mail.Headers.Add("Mailing-List", "contact " + User + "?subject=Help; run by AspMailList");
                    mail.Headers.Add("List-Help", "<mailto:" + User + "?subject=Help>");
                    mail.Headers.Add("List-Unsubscribe", "<mailto:" + User + "?subject=Unsubscribe>");
                    mail.Headers.Add("List-Subscribe", "<mailto:" + User + "?subject=Subscribe>");

                    string dominio = From.Trim().ToLower().Split('@')[1];
                    mail.Headers.Add("Message-Id", String.Concat("<", DateTime.Now.ToString("yyyyMMdd"), ".", DateTime.Now.ToString("HHmmss"), "@", dominio, ">"));

                    mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(Body, Encoding.GetEncoding("ISO-8859-1"), "text/html"));

                    foreach (string file in LstFile)
                    {
                        if (!string.IsNullOrEmpty(file))
                            mail.Attachments.Add(new Attachment(file));
                    }

                    #region Tratar To CC CCo
                    mail.To.Clear();
                    mail.CC.Clear();
                    mail.Bcc.Clear();

                    if (To.IndexOf(";") >= 0)
                    {
                        string[] para = To.Split(';');
                        for (int i = 0; i < para.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(para[i].Trim()))
                                mail.To.Add(para[i].Trim().ToLower());
                        }
                    }
                    else if (To.IndexOf(",") >= 0)
                    {
                        string[] para = To.Split(',');
                        for (int i = 0; i < para.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(para[i].Trim()))
                                mail.To.Add(para[i].Trim().ToLower());
                        }
                    }
                    else
                    {
                        mail.To.Add(To.Trim().ToLower());
                    }
                    if (!(CC.Equals("")))
                    {
                        if (CC.IndexOf(";") >= 0)
                        {
                            string[] cc = CC.Split(';');
                            for (int i = 0; i < cc.Length; i++)
                            {
                                if (!string.IsNullOrEmpty(cc[i].Trim()))
                                    mail.CC.Add(cc[i].Trim().ToLower());
                            }
                        }
                        else if (CC.IndexOf(",") >= 0)
                        {
                            string[] cc = CC.Split(',');
                            for (int i = 0; i < cc.Length; i++)
                            {
                                if (!string.IsNullOrEmpty(cc[i].Trim()))
                                    mail.CC.Add(cc[i].Trim().ToLower());
                            }
                        }
                        else
                        {
                            mail.CC.Add(CC.Trim().ToLower());
                        }
                    }
                    if (!(CCo.Equals("")))
                    {
                        if (CCo.IndexOf(";") >= 0)
                        {
                            string[] Bcc = CCo.Split(';');
                            for (int i = 0; i < Bcc.Length; i++)
                            {
                                if (!string.IsNullOrEmpty(Bcc[i].Trim()))
                                    mail.Bcc.Add(Bcc[i].Trim().ToLower());
                            }
                        }
                        else if (CCo.IndexOf(",") >= 0)
                        {
                            string[] Bcc = CCo.Split(',');
                            for (int i = 0; i < Bcc.Length; i++)
                            {
                                if (!string.IsNullOrEmpty(Bcc[i].Trim()))
                                    mail.Bcc.Add(Bcc[i].Trim().ToLower());
                            }
                        }
                        else
                        {
                            mail.Bcc.Add(CCo.Trim().ToLower());
                        }
                    }
                    #endregion

                    mail.From = from;
                    mail.ReplyToList.Add(from);
                    smtp.Send(mail);
                }
            }
        }

        private string getRodape(string email)
        {
            return "<br/><span style=\"color:#CFCFCF;font-size:8pt;text-decoration: none;\"><a href='mailto:" + email + "?subject=Help'  style=\"color:#CFCFCF;font-size:8pt;text-decoration: none;\">Informações sobre a lista de distribuição</a><br/><a href='mailto:" + email + "?subject=Unsubscribe'  style=\"color:#CFCFCF;font-size:8pt;text-decoration: none;\">Sair desta lista de distribuição</a></span>";
        }
    }
}


