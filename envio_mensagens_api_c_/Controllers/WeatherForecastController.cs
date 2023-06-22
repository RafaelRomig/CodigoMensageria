using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using envio_mensagens_api_c_.Services;

namespace envio_mensagens_api_c_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return _messageService.GetMessages();
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var message = _messageService.GetMessageById(id);
            if (message == null)
                return NotFound();

            return Ok(message);
        }

        [HttpPost]
        public IActionResult Post([FromBody] string message)
        {
            _messageService.SendMessage(message);
            return Ok($"Mensagem enviada: {message}");
        }
    }
}