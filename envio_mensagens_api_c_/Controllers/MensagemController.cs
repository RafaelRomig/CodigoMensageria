using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using envio_mensagens_api_c_.Repositories;

namespace envio_mensagens_api_c_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;

        public MessagesController(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        [HttpGet]
        public IEnumerable<Mensagem> Get()
        {
            return _messageRepository.GetMessages();
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var message = _messageRepository.GetMessageById(id);
            if (message == null)
                return NotFound();

            return Ok(message);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Mensagem mensagem)
        {
            // Verifica se o campo Text não está vazio ou nulo
            if (string.IsNullOrEmpty(mensagem.Text))
            {
                return BadRequest("O campo Text não pode estar vazio.");
            }

            // Gera um ID exclusivo para a mensagem
            int id = GerarIdUnico();

            // Cria uma nova instância de Mensagem com o ID e o texto fornecido
            var novaMensagem = new Mensagem { Id = id, Text = mensagem.Text };

            // Envia a mensagem para o repositório ou faz o processamento necessário
            _messageRepository.AddMessageAsync(novaMensagem);

            return Ok($"Mensagem enviada: {novaMensagem.Text}");
        }

        private int GerarIdUnico()
        {
            // Aqui você pode implementar sua lógica para gerar um ID único,
            // como usar um contador, gerar um GUID, etc.
            return new Random().Next(1, 1000);
        }
    }
}
