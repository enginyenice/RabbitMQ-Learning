using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;


//Subscriber yerine Consumer da kullanılır.
namespace RabbitMQ.Subscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            var factorcy = new ConnectionFactory();
            factorcy.Uri = new Uri("amqps://gnhvkhqr:rrUuNbVKXKmpfcdYgb6Ua8WpnPTFZh3Z@snake.rmq2.cloudamqp.com/gnhvkhqr");
            using var connection = factorcy.CreateConnection();
            var channel = connection.CreateModel();

            channel.BasicQos(0,1,false);
            var subscriber = new EventingBasicConsumer(channel);

            
            var queueName = channel.QueueDeclare().QueueName;
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add("format", "pdf");
            headers.Add("shape", "a4");
            headers.Add("x-match", "all");//All dersem tüm key value çiftleri eşleşmeli
            //headers.Add("x-match", "any");//Any dersem bir tane key value çifti eşleşmesi yeterlidir.
            channel.ExchangeDeclare("header-exchange", durable: true, type: ExchangeType.Headers); // Yoksa exchange oluşsun (hata vermemesi için)
            channel.QueueBind(queueName, "header-exchange",String.Empty,headers);
            channel.BasicConsume(queueName,false,subscriber);

            Console.WriteLine("Loglar dinleniyor...");
            subscriber.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                
                //Gelen mesajı string formatına çevirdik.
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                var product = JsonSerializer.Deserialize<Product>(message);
                //Mesajlar çok hızlı geldiği için threadi 1 saniyelik uyutalım. Görsel amaçlı.
                Thread.Sleep(1000);

                Console.WriteLine($"Gelen mesaj : {product.Id} - {product.Name} - {product.Price} - {product.Stock}");
                channel.BasicAck(e.DeliveryTag,false);


                
            };

            Console.ReadLine();
        }

    }
}
