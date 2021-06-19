using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQ.WaterMarkWeb.Services
{
    public class RabbitMQClientService :IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        public static string ExchangeName = "ImageDirectExchange";
        public static string RoutingWaterMark = "watermark-route-image";
        public static string QueueName = "queue-watermark-image";
        private readonly ILogger<RabbitMQClientService> _logger;

        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public IModel Connect()
        {
            _connection = _connectionFactory.CreateConnection();
            //Bir kanal var ise
            if(_channel is { IsOpen:true})
            {
                return _channel;
            }

            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, true, false);
            _channel.QueueDeclare(QueueName, true, false, false,null);
            _channel.QueueBind(exchange: ExchangeName, queue: QueueName, routingKey: RoutingWaterMark);
            _logger.LogInformation("RabbitMQ ile bağlantı kuruldu...");
            return _channel;
        }

        public void Dispose()
        {
            _channel?.Close(); // ?. varsa
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("RabbitMQ ile bağlantı koptu...");
        }
    }
}
