using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Word_To_Pdf_Producer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RabbitMQ.Word_To_Pdf_Producer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }



        public IActionResult WordToPdfPage()
        {
            return View();
        }

        [HttpPost]
        public IActionResult WordToPdfPage(WordToPdf wordToPdf)
        {
           
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(_configuration.GetConnectionString("RabbitMQ"));
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {

                    channel.ExchangeDeclare("convert-exchange", ExchangeType.Direct, true, false, null);
                    channel.QueueDeclare(queue: "file", durable: true, exclusive: false, autoDelete: false, arguments: null);
                    channel.QueueBind(queue: "file", exchange: "convert-exchange", routingKey: "wordToPdf", arguments: null);
                    MessageWordToPdf messageWordToPdf = new MessageWordToPdf();
                    using (var memoryStream = new MemoryStream())
                    {
                        wordToPdf.WordFile.CopyToAsync(memoryStream);
                        messageWordToPdf.WordByte = memoryStream.ToArray();
                    }

                    messageWordToPdf.Email = wordToPdf.Email;
                    messageWordToPdf.FileName = Path.GetFileNameWithoutExtension(wordToPdf.WordFile.FileName);
                    string searlizeMessage = JsonSerializer.Serialize(messageWordToPdf);
                    var byteMessage = Encoding.UTF8.GetBytes(searlizeMessage);
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    channel.BasicPublish(exchange: "convert-exchange", routingKey: "wordToPdf", basicProperties: properties, body: byteMessage);

                    ViewBag.result = "Dosyanız pdf dosyasına dönüştürüldükten sonra size email olarak gönderilecektir.";
                    return View();
                }
            }
        }

















        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
