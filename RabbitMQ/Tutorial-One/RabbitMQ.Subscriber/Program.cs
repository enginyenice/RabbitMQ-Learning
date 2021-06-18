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
            using var connection = factorcy.CreateConnection();
            var channel = connection.CreateModel();

            channel.BasicQos(0,1,false);
            var subscriber = new EventingBasicConsumer(channel);

            
            var queueName = channel.QueueDeclare().QueueName;
            //var routeKey = "*.Error.*"; // Sadece ortasında route olan başı ve sonu önemli değil. Bu kuyruğa gelsin.
            //var routeKey = "*.*.Warning"; //Sonu Warning olanlar bu kuyruğa gelsin
            var routeKey = "Info.#";//Başı Info olsun sonunda ne geldiği önemli değil.
            channel.QueueBind(queueName, "logs-topic", routeKey);
            channel.BasicConsume(queueName,false,subscriber);

            Console.WriteLine("Loglar dinleniyor...");
            subscriber.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                
                //Gelen mesajı string formatına çevirdik.
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                //Mesajlar çok hızlı geldiği için threadi 1 saniyelik uyutalım. Görsel amaçlı.
                Thread.Sleep(1000);

                Console.WriteLine($"Gelen mesaj : {message}");
                //File.AppendAllText("log-critical.txt", message+ "\n"); //Txt çıktı için örnek kod
                channel.BasicAck(e.DeliveryTag,false);


                
            };

            Console.ReadLine();
        }

    }
}
