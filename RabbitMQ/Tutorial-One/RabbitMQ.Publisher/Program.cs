using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;
//Publisher yerine Producer da kullanılır.
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
            //exchange: adını istiyor. İsmi logs-fanout olsun. (Farketmez)
            //durable: kuyruklar fiziksel olarak kaydedilsin mi? (Restart attığında kuyruk silinmez : true) / (Restart attığında kuyruk silinir: false)
            //type: ExchangeType'ını belirtiyoruz.
            channel.ExchangeDeclare(exchange: "logs-fanout", durable: true, type: ExchangeType.Fanout);

            //Mesaj sayısını arttırmak için 50 tane mesaj gönderdik.
            Enumerable.Range(1, 50).ToList().ForEach(x =>
            {
                string message = $"Log: {x}";
                //RabbitMQ mesajlar byte dizisi olarak gönderilir.
                var messageBody = Encoding.UTF8.GetBytes(message);
                //Bu çalışmada exchange kullanmıyoruz bu yüzden String.Empty gönderdik. Bu işleme default exchange denir.
                //Default Exchange kullanıyorsak eğer Route keyimize kuyruğumuzun ismini vermemiz gerekiyor.
                //Exchange verdiğimizde Exchane ismini vermemiz gerekiyor. Bu sefer de kuyruk ismini boş göndermemiz gerekiyor.
                channel.BasicPublish(exchange: "logs-fanout",routingKey:"", basicProperties: null, body: messageBody);

                Console.WriteLine($"Mesaj gönderildi. Giden Mesaj: {message}");
            });
            
            Console.ReadLine();
        }
    }
}
