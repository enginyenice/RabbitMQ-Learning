using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    // //if the queue is not found, create a queue.
                    // channel.QueueDeclare(queue: "task_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {

                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);



                        int dots = message.Split('.').Length - 1;

                        Console.WriteLine("[x] ({0} sec.) Received {1}", dots, message);
                        Thread.Sleep(dots * 1000);
                        Console.WriteLine("[x] Done");
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                    };
                    channel.BasicQos(prefetchSize: 0,
                                         prefetchCount: 1,
                                         global: false);
                    channel.BasicConsume(queue: "task_queue",
                                         autoAck: false,
                                         consumer: consumer);
                    Console.ReadLine();
                }
            }

        }
    }
}
