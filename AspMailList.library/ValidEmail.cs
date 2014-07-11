using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AspMailList.library
{
    public static class ValidEmail
    {
        /// <summary>
        /// Verificar se é um e-mail valido.
        /// </summary>
        public static bool isValidMail(string p)
        {
            if (!string.IsNullOrEmpty(p))
                if (p.IndexOf('@') > 0)
                    if (p.IndexOf('.') > 0)
                        if (p.LastIndexOf('.') != (p.Length - 1))
                            return (p.Length < 250);
                        else
                            return false;
                    else
                        return false;
                else
                    return false;
            else
                return false;
        }

        /// <summary>
        /// Recuperar os e-mails de uma lista
        /// </summary>
        public static string[] getListMail(List<string> sb)
        {
            List<string> lstStr = new List<string>();
            foreach (string p in sb)
            {
                foreach (string a in p.Split(';'))
                    foreach (string b in a.Split(':'))
                        foreach (string c in b.Split('('))
                            foreach (string d in c.Split(')'))
                                foreach (string e in d.Split('<'))
                                    foreach (string f in e.Split('>'))
                                        foreach (string g in f.Split(' '))
                                            foreach (string h in g.Split('\n'))
                                                foreach (string i in h.Split('\t'))
                                                    foreach (string j in i.Split('['))
                                                        foreach (string l in j.Split(']'))
                                                            foreach (string m in l.Split(','))
                                                            {
                                                                string item = m.ToLower().Trim().Replace("\"", "").Replace("'", "").Replace("/", "");
                                                                if (!string.IsNullOrEmpty(item))
                                                                {
                                                                    if (isValidMail(item))
                                                                    {
                                                                        if (!lstStr.Contains(item))
                                                                        {
                                                                            lstStr.Add(item);
                                                                        }
                                                                    }
                                                                }
                                                            }
            }

            return lstStr.ToArray();
        }

        /// <summary>
        /// Recupera os e-mails de um texto
        /// </summary>
        public static string[] getListMail(string p)
        {
            List<string> lstStr = new List<string>();
            foreach (string a in p.Split(';'))
                foreach (string b in a.Split(':'))
                    foreach (string c in b.Split('('))
                        foreach (string d in c.Split(')'))
                            foreach (string e in d.Split('<'))
                                foreach (string f in e.Split('>'))
                                    foreach (string g in f.Split(' '))
                                        foreach (string h in g.Split('\n'))
                                            foreach (string i in h.Split('\t'))
                                                foreach (string j in i.Split('['))
                                                    foreach (string l in j.Split(']'))
                                                        foreach (string m in l.Split(','))
                                                        {
                                                            string item = m.ToLower().Trim().Replace("\"", "").Replace("'", "").Replace("/", "");
                                                            if (!string.IsNullOrEmpty(item))
                                                            {
                                                                if (isValidMail(item))
                                                                {
                                                                    if (!lstStr.Contains(item))
                                                                    {
                                                                        lstStr.Add(item);
                                                                    }
                                                                }
                                                            }
                                                        }


            return lstStr.ToArray();
        }
    }
}
