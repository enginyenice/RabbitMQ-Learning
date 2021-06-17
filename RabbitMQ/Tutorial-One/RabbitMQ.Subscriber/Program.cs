using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Text;
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
            //Bağlantı oluşturuyoruz
            using var connection = factorcy.CreateConnection();
            //Kanal oluşturuyoruz
            var channel = connection.CreateModel();


            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            var subscriber = new EventingBasicConsumer(model: channel);

            Console.WriteLine("Loglar dinleniyor...");
            var queueName = "direct-queue-Critical";
            channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: subscriber);
            //Received : rabbitmq bir mesaj gönderdiğinde bu event tetiklenir
            subscriber.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                //Gelen mesajı string formatına çevirdik.
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                Console.WriteLine($"Gelen mesaj : {message}");
                //File.AppendAllText("log-critical.txt", message+ "\n");
                channel.BasicAck(deliveryTag: e.DeliveryTag,multiple:false);


                //Mesajlar çok hızlı geldiği için threadi 1 saniyelik uyutalım. Görsel amaçlı.
                Thread.Sleep(1000);
            };

            Console.ReadLine();
        }

    }
}
