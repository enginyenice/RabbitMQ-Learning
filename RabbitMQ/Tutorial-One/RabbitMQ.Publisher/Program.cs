using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;
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

            channel.ExchangeDeclare(exchange: "logs-direct", durable: true, type: ExchangeType.Direct);

            Enum.GetNames(typeof(LogNames)).ToList().ForEach(x =>
            {
                var routeKey = $"route-{x}";
                var queueName = $"direct-queue-{x}";
                channel.QueueDeclare(queueName, true, false, false);
                channel.QueueBind(queueName, "logs-direct",routeKey, null);
            });


            //Mesaj sayısını arttırmak için 50 tane mesaj gönderdik.
            Enumerable.Range(1, 50).ToList().ForEach(x =>
            {
                LogNames log=  (LogNames)new Random().Next(1, 5);
                string message = $"Log Type: {log} Log: {x}";
                var messageBody = Encoding.UTF8.GetBytes(message);
                var routeKey = $"route-{log}";

                channel.BasicPublish("logs-direct",routeKey,null,messageBody);

                Console.WriteLine($"Log gönderildi. Giden Log: {message}");
            });
            
            Console.ReadLine();
        }
    }
}
