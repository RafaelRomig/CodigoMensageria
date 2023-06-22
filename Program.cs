using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace envio_mensagem_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<BancoMensagem>();

                dbContext.SeedData();
            }

            host.Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<BancoMensagem>(options =>
                        options.UseSqlite("BancoMensagem.db"));

                    services.AddHostedService<RabbitMQService>();
                });
    }

    public class RabbitMQService : BackgroundService
    {
        private readonly BancoMensagem _dbContext;

        public RabbitMQService(BancoMensagem dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, message) =>
            {
                var body = message.Body.ToArray();
                var mensagem = Encoding.UTF8.GetString(body);

                _dbContext.Add(new Message { Text = mensagem });
                _dbContext.SaveChanges();

                Console.WriteLine($"Mensagem recebida e salva no banco de dados: {mensagem}");
            };

            channel.BasicConsume(
                queue: "FILA",
                autoAck: true,
                consumer: consumer
            );

            return Task.CompletedTask;
        }
    }

    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }

    public class BancoMensagem : DbContext
    {
        public DbSet<Message> Messages { get; set; }

        public BancoMensagem(DbContextOptions<BancoMensagem> options)
            : base(options)
        {
        }

        public void SeedData()
        {
            if (!Messages.Any())
            {
                var messages = new List<Message>
                {
                    new Message { Text = "Exemplo de mensagem 1" },
                    new Message { Text = "Exemplo de mensagem 2" },
                    new Message { Text = "Exemplo de mensagem 3" }
                };

                Messages.AddRange(messages);
                SaveChanges();
            }
        }
    }
}