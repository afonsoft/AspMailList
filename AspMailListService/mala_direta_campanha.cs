//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AspMailList.Service
{
    using System;
    using System.Collections.Generic;
    
    public partial class mala_direta_campanha
    {
        public int id { get; set; }
        public string DisplayName { get; set; }
        public string ReplyTo { get; set; }
        public string From { get; set; }
        public string SmtpServer { get; set; }
        public bool EnableSsl { get; set; }
        public int SmtpPort { get; set; }
        public int PopPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }
        public string Subject { get; set; }
        public string BodyHtml { get; set; }
        public string AttachmentPath { get; set; }
        public bool Enabled { get; set; }
    }
}
