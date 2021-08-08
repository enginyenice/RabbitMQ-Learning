using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.WaterMarkWeb.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.WaterMarkWeb.BackgroundServices
{
    public class ImageWaterMarkProcessBackgroundService : BackgroundService
    {
        private readonly RabbitMQClientService _rabbitMQClientService;
        private readonly ILogger<ImageWaterMarkProcessBackgroundService> _logger;
        private IModel _channel; // farklı bir methodda set edeceğiz o yüzden readonly demedik
        public ImageWaterMarkProcessBackgroundService(ILogger<ImageWaterMarkProcessBackgroundService> logger, RabbitMQClientService rabbitMQClientService)
        {
            _logger = logger;
            _rabbitMQClientService = rabbitMQClientService;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMQClientService.Connect();
            _channel.BasicQos(0, 1, false);

            return base.StartAsync(cancellationToken);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var consumer = new AsyncEventingBasicConsumer(_channel);
            _channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);
            consumer.Received += Consumer_Received;

            return Task.CompletedTask;

        }

        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            //Çok hızlı gerçekleşiyor görmek için yavaşlattık.
            Task.Delay(5000).Wait();

            try
            {
                //Gelen mesajı aldık
                var productImageCreatedEvent = JsonSerializer.Deserialize<ProductImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));
                //Resmin yolunu aldık
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/", productImageCreatedEvent.ImageName);

                //Resmi aldık
                using var img = Image.FromFile(path);
                //Grafik oluşturduk
                using var graphic = Graphics.FromImage(img);

                var font = new Font(FontFamily.GenericSansSerif, 40, FontStyle.Bold, GraphicsUnit.Pixel);

                var text = "enginyenice.com";
                //Yazılacak yazının boyutunu alacağız.
                var textSize = graphic.MeasureString(text, font);

                var color = Color.FromArgb(128, 255, 255);
                var brush = new SolidBrush(color);
                var position = new Point(img.Width - ((int)textSize.Width + 30), img.Height - ((int)textSize.Height + 30));

                graphic.DrawString(text, font, brush, position);
                img.Save("wwwroot/images/watermarks/" + productImageCreatedEvent.ImageName);
                img.Dispose();
                graphic.Dispose();
                _channel.BasicAck(@event.DeliveryTag,false);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.Message);
            }
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            //RabbitMQClientService tarafında dispose yazdığımız için burada _channel kapatmamıza gerek yok.
            return base.StopAsync(cancellationToken);
        }
    }
}
