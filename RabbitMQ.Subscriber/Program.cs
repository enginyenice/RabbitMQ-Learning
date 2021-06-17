using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;


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
            // Kuyruk oluşturuyoruz. Eğer oluşturmazsak publisher böyle bir kuyruk oluşturmadığı durumda hata
            // mesajı alırız.
            //Hem publisher hemde subscriber tarafında oluşturuyorsak eğer parametrelerin aynı olduğundan emin olmalıyız.
            channel.QueueDeclare(
                queue: "hello-queue",
                durable: true,
                exclusive: false,
                autoDelete: false);
            var subscriber = new EventingBasicConsumer(model: channel);
            //autoAck : true yapılırsa mesaj doğruda işlense yanlış da işelense kuyruktan siler
            //autoAck : false yapılırsa mesaj kuyruktan silinmez.
            channel.BasicConsume(
                queue: "hello-queue",
                autoAck: true,
                consumer: subscriber);
            //Received : rabbitmq bir mesaj gönderdiğinde bu event tetiklenir
            subscriber.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                //Gelen mesajı string formatına çevirdik.
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                Console.WriteLine($"Gelen mesaj : {message}");
            };

            Console.ReadLine();
        }

    }
}
