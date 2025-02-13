using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;

namespace TalktuahCommunicater1925
{
    public partial class MainWindow : Window
    {
        public string Username, IP, Port;
        public MainWindow()
        {
            Username = "";
            IP = "";
            Port = "";
            InitializeComponent();
        }

        public void JoinGeneralChat(object sender, RoutedEventArgs args)
        {
            
        }

        public void JoinPoliticalChat(object sender, RoutedEventArgs args)
        {
            
        }

        public void JoinRandomChat(object sender, RoutedEventArgs args)
        {
            
        }

        public void JoinCustomIP(object? sender, RoutedEventArgs args)
        {
            
        }
    }
}