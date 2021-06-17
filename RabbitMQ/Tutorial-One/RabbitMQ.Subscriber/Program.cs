using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
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

            //exchange: adını istiyor. İsmi logs-fanout olsun. (Farketmez)
            //durable: kuyruklar fiziksel olarak kaydedilsin mi? (Restart attığında kuyruk silinmez : true) / (Restart attığında kuyruk silinir: false)
            //type: ExchangeType'ını belirtiyoruz.
            //subscriber tarafında bunu aynı ayarlarla oluşturmaya gerek yok. Oluştursakda hata almayız.
            //channel.ExchangeDeclare(exchange: "logs-fanout", durable: true, type: ExchangeType.Fanout);

            //Random kuyruk ismi oluşturduk. Elle de oluşturabilirdik ama kütüphane zaten oluşturduğu için 
            //onu kullandık
            var randomQueueName = channel.QueueDeclare().QueueName;


            //QueueBind => Uygulama ayağa kalktığında kuyruk oluşacak. Uygulama down olduğunda kuyruk silinecek.
            channel.QueueBind(queue: randomQueueName, exchange: "logs-fanout",routingKey:String.Empty,arguments:null);


            //prefetchSize: Hangi boyuttaki mesajlar gelsin 0 (herhangi bir boyutta ki mesajlar gelebilir) 
            //prefetchCount: Her bir subscriber kaç kaç mesaj gelsin 
            //global: Bu değer global olsun mu? 
            //false yaparsak: Her bir subscriber'a tek bir seferde beşer beşer mesaj gönderir.  Örnek: prefetchCount:5 kabul edersek.
            //true yaparsak: Her bir subscriber varsa toplam değeri 5 olacak şekilde ayarlar. Paydaş dağıtır. Örnek: prefetchCount:5 kabul edersek.
            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            var subscriber = new EventingBasicConsumer(model: channel);

            Console.WriteLine("Loglar dinleniyor...");

            //autoAck : true yapılırsa mesaj doğruda işlense yanlış da işelense kuyruktan siler
            //autoAck : false yapılırsa mesaj kuyruktan silinmez.
            channel.BasicConsume(
                queue: randomQueueName,
                autoAck: false,
                consumer: subscriber);
            //Received : rabbitmq bir mesaj gönderdiğinde bu event tetiklenir
            subscriber.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                //Gelen mesajı string formatına çevirdik.
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                Console.WriteLine($"Gelen mesaj : {message}");
                //Mesajı silme işlemi
                //deliveryTag: silinecek mesajın tagı. Parametre olarak gelen e içerisinde bulunan DeliveryTag propery si üzerinden ulaşılabilir.
                //multiple: true : İşlenmiş ama rabbitmq haberdar edilmemiş mesajlar varsa eğer onları da haberdar eder.
                //Haberdar edilmeyen mesajları belirli bir süre sonra başka bir subscriber (consumer) tekrar gönderir.
                channel.BasicAck(deliveryTag: e.DeliveryTag,multiple:false);


                //Mesajlar çok hızlı geldiği için threadi 1 saniyelik uyutalım. Görsel amaçlı.
                Thread.Sleep(1000);
            };

            Console.ReadLine();
        }

    }
}
