using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using envio_mensagens_api_c_.Repositories;
using RabbitMQ.Client;

namespace envio_mensagens_api_c_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;
        private readonly ConnectionFactory _connectionFactory;

        public MessagesController(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;

            _connectionFactory = new ConnectionFactory
            {
                HostName = "localhost"
            };
        }

        [HttpPost]
        public IActionResult Post([FromBody] Mensagem mensagem)
        {
            // Verifica se o campo Text não está vazio ou nulo
            if (string.IsNullOrEmpty(mensagem.Text))
            {
                return BadRequest("O campo Text não pode estar vazio.");
            }

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "FILA",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var body = Encoding.UTF8.GetBytes(mensagem.Text);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "FILA",
                                         basicProperties: null,
                                         body: body);
                }

                return Ok($"Mensagem enviada: {mensagem.Text}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao conectar ao RabbitMQ: " + ex.ToString());
                return StatusCode(500, "Erro ao enviar a mensagem.");
            }
        }
    }
}
