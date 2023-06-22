using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using envio_mensagens_api_c_.Data;

namespace envio_mensagens_api_c_.Services
{
    public class RabbitMQService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public RabbitMQService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "FILA",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, message) =>
                {
                    var body = message.Body.ToArray();
                    var mensagem = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"Mensagem recebida: {mensagem}");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
                        await messageRepository.AddMessageAsync(mensagem);
                    }
                };

                channel.BasicConsume(queue: "FILA",
                                     autoAck: true,
                                     consumer: consumer);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }
    }
}