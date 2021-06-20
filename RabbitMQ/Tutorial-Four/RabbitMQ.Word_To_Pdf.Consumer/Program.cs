using System;
using System.IO;
using System.Net.Mail;

namespace RabbitMQ.Word_To_Pdf.Consumer
{
    class Program
    {

        public static bool EmailSend(string email,MemoryStream memoryStream,string fileName)
        {

            try
            {
                memoryStream.Position = 0;
                System.Net.Mime.ContentType ct = new System.Net.Mime.ContentType(System.Net.Mime.MediaTypeNames.Application.Pdf);

                Attachment attach = new Attachment(memoryStream, ct);
                attach.ContentDisposition.FileName = $"{fileName}.pdf";
                MailMessage mailMessage = new MailMessage();
                SmtpClient smtpClient = new SmtpClient();
                mailMessage.From = new MailAddress("test@enginyenice.com");
                mailMessage.To.Add(email);
                mailMessage.Subject = "Dosyanız PDF olarak dönüştürüldü. | enginyenice.com";
                mailMessage.Body = "Word dosyanız pdf dosyasına dönüştürüldü. Dosyanız ektedir";
                mailMessage.IsBodyHtml = true;
                mailMessage.Attachments.Add(attach);
                smtpClient.Host = "mail.enginyenice.com";
                smtpClient.Port = 587;
                smtpClient.Credentials = new System.Net.NetworkCredential("test@enginyenice.com", "Qsj^ZTP0yWE6_*$%");
                Console.WriteLine($"{email} adresine gönderilmiştir");
                memoryStream.Close();
                memoryStream.Dispose();
                return true;
            }
            catch (Exception ex)
            {

                Console.WriteLine($"{email} adresine gönderilirken bir hata meydana geldi. Hata: {ex.InnerException}");
                return false;
            }


           


        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
