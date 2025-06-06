using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace Datos
{
    public class D_EmailSender
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly bool _enableSsl;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _fromName;
        private readonly string _fromAddress;

        public D_EmailSender()
        {
            // Lee desde Web.config → <appSettings>
            _smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            _smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            _enableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSsl"]);
            _userName = ConfigurationManager.AppSettings["EmailUser"];
            _password = ConfigurationManager.AppSettings["EmailPass"];
            _fromName = ConfigurationManager.AppSettings["FromName"];
            _fromAddress = ConfigurationManager.AppSettings["FromAddress"];
        }

        public void EnviarCorreo(
            string destinatario,
            string asunto,
            string cuerpoHtml,
            byte[] pdfBytes,
            string nombrePdf,
            string jsonContenido,
            string nombreJson)
        {
            var mail = new MailMessage();
            mail.From = new MailAddress(_fromAddress, _fromName);
            mail.To.Add(destinatario);
            mail.Subject = asunto;
            mail.Body = cuerpoHtml;
            mail.IsBodyHtml = true;

            // Adjuntar PDF
            using (var msPdf = new MemoryStream(pdfBytes))
            {
                var attachmentPdf = new Attachment(msPdf, nombrePdf, "application/pdf");
                mail.Attachments.Add(attachmentPdf);

                // Adjuntar JSON
                var jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonContenido);
                using (var msJson = new MemoryStream(jsonBytes))
                {
                    var attachmentJson = new Attachment(msJson, nombreJson, "application/json");
                    mail.Attachments.Add(attachmentJson);

                    // Configurar y enviar via SMTP
                    using (var client = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        client.EnableSsl = _enableSsl;
                        client.Credentials = new NetworkCredential(_userName, _password);
                        client.Send(mail);
                    }
                }
            }
        }
    }
}
