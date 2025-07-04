﻿using System;
using System.IO;
using System.Net;
//referencias
using System.Net.Mail;
using System.Security.Cryptography; //referencias
using System.Text;

namespace Negocios
{
    public class N_Recursos
    {
        //generar claves de 8 digitos
        public static string GenerarClave()
        {
            string clave = Guid.NewGuid().ToString("N").Substring(0, 8);
            return clave;
        }

        //encriptacion de texto SHA256
        public static string ConvertitSha256(string texto)
        {
            StringBuilder sb = new StringBuilder();
            //usamos la referencia de "system.Security.Cryptography"
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(texto));
                foreach (byte b in result)
                    sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        //enviar email
        public static bool EnviarEmail(string correo, string asunto, string mensaje)
        {
            bool resultado = false;
            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(correo);
                mail.From = new MailAddress("fabriziobarrios92@gmail.com");
                mail.Subject = asunto;
                mail.Body = mensaje;
                mail.IsBodyHtml = true;

                //servidor
                var smtp = new SmtpClient()
                {
                    Credentials = new NetworkCredential("fabriziobarrios92@gmail.com", "zhrpbownlpdjyair"),
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                };
                smtp.Send(mail);
                resultado = true;
            }
            catch (Exception ex)
            {
                resultado = false;
            }
            return resultado;
        }

        //imagenes
        public static string ConvertirBase64(string ruta, out bool conversion)
        {
            string textoBase64 = string.Empty;
            conversion = true;
            try
            {
                byte[] bytes = File.ReadAllBytes(ruta);
                textoBase64 = Convert.ToBase64String(bytes);
            }
            catch
            {
                conversion = false;
            }
            return textoBase64;
        }

    }
}
