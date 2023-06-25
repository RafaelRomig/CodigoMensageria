using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using envio_mensagens_api_c_.Repositories;
using RabbitMQ.Client;
using System;
using System.Text;

namespace envio_mensagem_c_
{
    class Program
    {
        private readonly IMessageRepository _messageRepository;

        public Program(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IMessageRepository, MessageRepository>();
                    services.AddTransient<Program>();
                })
                .UseConsoleLifetime();

        public void Run()
        {
            string exchangeName = "amq.default";
            string routingKey = "minha.routing.key";
            string message = "Minha mensagem de texto";

            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: exchangeName,
                             routingKey: routingKey,
                             basicProperties: null,
                             body: body);

                Console.WriteLine("Mensagem enviada com sucesso.");
            }

            Console.ReadLine();
        }
    }
}
