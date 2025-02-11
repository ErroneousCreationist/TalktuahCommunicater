using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalktuahCommunicater1925
{
    public class ChatViewModel : ViewModelBase
    {
        public ObservableCollection<ChatMessage> Messages { get; } = new();

        public void AddTextMessage(string sender, string text)
        {
            Messages.Add(new ChatMessage(sender, text));
        }

        public void AddImageMessage(string sender, byte[] imageData)
        {
            Messages.Add(new ChatMessage(sender, imageData));
        }
    }
}
