using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using envio_mensagens_api_c_.Repositories;
using Newtonsoft.Json;

namespace envio_mensagens_api_c_.Services
{
    public class RabbitMQService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageRepository _messageRepository;

        public RabbitMQService(IServiceProvider serviceProvider, IMessageRepository messageRepository)
        {
            _serviceProvider = serviceProvider;
            _messageRepository = messageRepository;
        }

        protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
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
                    consumer.Received += async (model, args) =>
                    {
                        var message = Encoding.UTF8.GetString(args.Body.ToArray());
                        Console.WriteLine($"Mensagem recebida: {message}");

                        var novaMensagem = new Mensagem { Id = (int)args.DeliveryTag, Text = message };

                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
                            await messageRepository.AddMessageAsync(novaMensagem);
                        }
                    };

                    channel.BasicConsume(queue: "FILA",
                                         autoAck: true,
                                         consumer: consumer);

                await System.Threading.Tasks.Task.Delay(Timeout.Infinite, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao conectar ao RabbitMQ: " + ex.ToString());
                throw; // Re-throw a exceção para interromper a execução do serviço
            }
        }
    }
}
