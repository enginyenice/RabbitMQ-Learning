using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Spire.Doc;
using System;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace RabbitMQ.Word_To_Pdf.Consumer
{
    class Program
    {
        public static bool EmailSend(string email, MemoryStream memoryStream, string fileName)
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
                smtpClient.Send(mailMessage);
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
            bool result = true;
            var factory = new ConnectionFactory();
            var uri = "amqps://gnhvkhqr:rrUuNbVKXKmpfcdYgb6Ua8WpnPTFZh3Z@snake.rmq2.cloudamqp.com/gnhvkhqr";
            factory.Uri = new Uri(uri);
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {

                    channel.ExchangeDeclare("convert-exchange", ExchangeType.Direct, true, false, null);
                    channel.QueueBind(queue: "file", exchange: "convert-exchange",routingKey: "wordToPdf",arguments:null);
                    channel.BasicQos(0, 1, false);
                    var consumer = new EventingBasicConsumer(channel);
                    channel.BasicConsume(queue: "file", autoAck: false, consumer);
                    consumer.Received += (sender,ea) =>
                    {
                        
                        try
                        {
                            Console.WriteLine("Kuyruktan bir mesaj alındı ve işleniyor...");
                            Document document = new Document();
                            var messageWordToPdf = JsonSerializer.Deserialize<MessageWordToPdf>(Encoding.UTF8.GetString(ea.Body.ToArray()));
                            document.LoadFromStream(new MemoryStream(messageWordToPdf.WordByte), FileFormat.Docx2013);
                            using (var memoryStream = new MemoryStream())
                            {
                                document.SaveToStream(memoryStream, FileFormat.PDF);
                                result = EmailSend(messageWordToPdf.Email, memoryStream, messageWordToPdf.FileName);

                            }
                        }
                        catch (Exception ex)
                        {

                            Console.WriteLine($"Hata meydana geldi Hata:{ex.InnerException}");
                        }

                        if (result)
                        {
                            Console.WriteLine("Kuyruktan mesaj başarıyla işlendi");
                            channel.BasicAck(ea.DeliveryTag, false);
                        }
                    };
                    Console.Write("Çıkmak için bir tuşa basınız");
                    Console.ReadLine();
                }
            }

        }


    }
}
