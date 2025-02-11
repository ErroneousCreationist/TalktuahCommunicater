
using Avalonia.Media.Imaging;
using System.IO;

namespace TalktuahCommunicater1925
{
    public class ChatMessage
    {
        public string? Sender { get; set; }
        public string? Text { get; set; } // Null if it's an image message
        public byte[]? ImageData { get; set; } // Null if it's a text message

        public Bitmap BindingImage
        {
            get
            {
                if(ImageData == null) { return null; }
                using var stream = new MemoryStream(ImageData);
                return new Bitmap(stream);
            }
        }

        public bool IsImage => ImageData != null;
        public bool IsText => Text != null;
        public string BindingIsImage => ImageData != null ? "True" : "False";
        public string BindingIsText => Text != null ? "True" : "False";

        public ChatMessage(string sender, string text)
        {
            Sender = sender;
            Text = text;
            ImageData = null;
        }

        public ChatMessage(string sender, byte[] data)
        {
            Sender = sender;
            ImageData = data;
            Text = null;
        }
    }
}
