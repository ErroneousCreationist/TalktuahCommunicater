using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalktuahCommunicater1925
{
    public partial class ChatWindowViewModel : ViewModelBase
    {
        public ObservableCollection<ChatMessage> Messages { get; set; }
        public ChatWindowViewModel()
        {
            Messages = new ObservableCollection<ChatMessage>
            {
                new ChatMessage("Alice", "Hello!"),
                new ChatMessage("Bob", ConvertImageToByteArray("C:\\Users\\sam\\Downloads\\flower.jpg")),
            };
        }

        private byte[] ConvertImageToByteArray(string path)
        {
            return System.IO.File.ReadAllBytes(path);
        }
    }
}
