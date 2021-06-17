using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace RabbitMQ.Publisher
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
            // Kuyruk oluşturuyoruz. Yoksa mesajlar boşa gider.
            channel.QueueDeclare(
                queue:"hello-queue",
                durable:true,
                exclusive:false,
                autoDelete:false);

            //Mesaj sayısını arttırmak için 50 tane mesaj gönderdik.
            Enumerable.Range(1, 50).ToList().ForEach(x =>
            {
                string message = $"Message: {x}";
                //RabbitMQ mesajlar byte dizisi olarak gönderilir.
                var messageBody = Encoding.UTF8.GetBytes(message);
                //Bu çalışmada exchange kullanmıyoruz bu yüzden String.Empty gönderdik. Bu işleme default exchange denir.
                //Default Exchange kullanıyorsak eğer Route keyimize kuyruğumuzun ismini vermemiz gerekiyor.
                channel.BasicPublish(exchange: String.Empty, routingKey: "hello-queue", basicProperties: null, body: messageBody);

                Console.WriteLine($"Mesaj gönderildi. Giden Mesaj: {message}");
            });
            
            Console.ReadLine();
        }
    }
}
