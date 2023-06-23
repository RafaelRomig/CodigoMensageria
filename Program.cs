using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using envio_mensagens_api_c_.Services;
using envio_mensagens_api_c_.Repositories;

namespace envio_mensagem_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<RabbitMQService>();
                    services.AddSingleton<IMessageRepository, MessageRepository>();
                });
    }
}