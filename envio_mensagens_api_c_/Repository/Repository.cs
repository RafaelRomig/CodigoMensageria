using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace envio_mensagens_api_c_.Repositories
{
    public interface IMessageRepository
    {
        Task AddMessageAsync(Mensagem mensagem);
        IEnumerable<Mensagem> GetMessages();
        Mensagem? GetMessageById(int id);
    }

    public class MessageRepository : IMessageRepository
    {
        private readonly List<Mensagem> _messages;

        public MessageRepository()
        {
            _messages = new List<Mensagem>();
        }

        public async Task AddMessageAsync(Mensagem mensagem)
        {
            if (mensagem != null)
            {
                _messages.Add(mensagem);
            }
            await Task.CompletedTask;
        }

        public IEnumerable<Mensagem> GetMessages()
        {
            return _messages.ToList();
        }

        public Mensagem? GetMessageById(int id)
        {
            return _messages.FirstOrDefault(m => m.Id == id);
        }
    }
}
