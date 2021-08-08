using RabbitMQ.Client;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
//Publisher yerine Producer da kullanılır.
namespace RabbitMQ.Publisher
{
    public enum LogNames
    {
        Critical = 1,
        Error = 2,
        Warning = 3,
        Info = 4
    };
    class Program
    {
        static void Main(string[] args)
        {
            var factorcy = new ConnectionFactory();
            factorcy.Uri = new Uri("amqps://gnhvkhqr:rrUuNbVKXKmpfcdYgb6Ua8WpnPTFZh3Z@snake.rmq2.cloudamqp.com/gnhvkhqr");
            //Bağlantı oluşturuyoruz
            using var connection = factorcy.CreateConnection();
            //Kanal oluşturuyoruz
            var channel = connection.CreateModel();

            channel.ExchangeDeclare("header-exchange", durable: true, type: ExchangeType.Headers);
            //Header belirliyoruz. value kısmına istediğin herşeyi gömebilirsin
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add("format", "pdf");
            headers.Add("shape", "a4");

            //channel üzerinden bir properties oluşturuyoruz.
            var properties = channel.CreateBasicProperties();
            //oluşturduğumuz properties in headersına kendi headers ımızı gömüyoruz.
            properties.Headers = headers;
            //Mesalar kalıcı hale gelmiş olur
            properties.Persistent = true;

            var product = new Product { Id = 1, Name = "Defter", Price = 12, Stock = 10 };
            var productJson = JsonSerializer.Serialize(product);
            //basicProperties içerisine oluşturduğumuz properties ekliyoruz.
            channel.BasicPublish("header-exchange", String.Empty, basicProperties: properties, Encoding.UTF8.GetBytes(productJson));
            Console.WriteLine("Mesaj gönderildi.");
            Console.ReadLine();
        }
    }
}
