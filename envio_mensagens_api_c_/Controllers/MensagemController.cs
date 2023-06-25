using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using envio_mensagens_api_c_.Repositories;
using RabbitMQ.Client;
using MySql.Data.MySqlClient;

namespace envio_mensagens_api_c_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;
        private readonly RabbitMQ.Client.ConnectionFactory _connectionFactory;
        private readonly string _connectionString;

        public MessagesController(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;

            _connectionFactory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            _connectionString = "Server=localhost;Port=3306;User ID=root;Password=Elisangela1;Database=chamadodb";
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

                // Grava a mensagem no banco de dados MySQL
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var comm = new MySqlCommand())
                    {
                        comm.Connection = conn;
                        comm.CommandType = System.Data.CommandType.Text;
                        comm.CommandText = "INSERT INTO mensagem VALUES (0, @mensagem)";
                        comm.Parameters.AddWithValue("@mensagem", mensagem.Text);
                        comm.ExecuteNonQuery();
                    }
                }

                return Ok($"Mensagem enviada e gravada no banco de dados: {mensagem.Text}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao conectar ao RabbitMQ: " + ex.ToString());
                return StatusCode(500, "Erro ao enviar a mensagem.");
            }
        }
    }
}
