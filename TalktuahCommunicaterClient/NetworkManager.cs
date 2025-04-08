using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TalktuahCommunicaterClient
{
    //handles the client sending and recieving information from server
    public class NetworkManager
    {
        private const byte CLIENT_CONNECTED_CODE = 0;
        private const byte MESSAGE_SENT_CODE = 1;
        private const byte CLIENT_LEFT_CODE = 2;
        private const byte RECIEVE_MESSAGE_CODE = 3;
        private const byte RECIEVE_CLIENT_JOIN_CODE = 4;
        private const byte RECIEVE_CLIENT_LEFT_CODE = 5;
        private const byte LIST_CLIENTS_CODE = 6;
        private const byte RECIEVE_CLIENT_LIST_CODE = 7;
        private const byte IMAGE_SENT_CODE = 8;
        private const byte RECIEVE_IMAGE_CODE = 9;
        public const byte PING_CODE = 10;

        private Socket? CLIENT_SOCKET;
        public static readonly byte[] _magicNumber = new byte[] { 0xCA, 0xFE, 0xBA, 0xBE };
        public static int MAX_MESSAGE_LEN;
        private string currUsername;
        private bool DISCONNECTED = false;

        public static Action<string, string> TextMessageRecieved; // Event for incoming messages
        public static Action<string, byte[]> ImageMessageRecieved;
        public static Action OnConnected;
        public static Action OnDisconnected;
        public static Action<string> SendMessage; //this is for networkmanager, called by program
        public static Action<byte[]> SendImage; //same as above
        public static Action Close;

        private static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private void sendMessage(string CurrText)
        {
            if (CurrText == "/list")
            {
                var message = Encoding.ASCII.GetBytes("yes");
                var final = new byte[_magicNumber.Length + 1 + message.Length + 1];
                Array.Copy(_magicNumber, 0, final, 0, _magicNumber.Length);
                final[_magicNumber.Length] = LIST_CLIENTS_CODE;
                Array.Copy(message, 0, final, _magicNumber.Length+1, message.Length);
                final[^1] = (byte)'\r';
                int _ = CLIENT_SOCKET.Send(final);
            }
            else if (CurrText.Split(' ')[0] == "/emoji")
            {
                if (CurrText.Split(' ')[1] == "list")
                {
                    string emojislist = "";
                    string currline = "";
                    foreach (var key in Program.EMOJIS.Keys)
                    {
                        emojislist += key + ", ";
                        currline += key + ", ";
                        if(currline.Length >= 110) { emojislist = emojislist[..^2]; emojislist += "\n"; currline = ""; }
                    }
                    if (emojislist[^1] == ' ') { emojislist = emojislist[..^2]; }
                    TextMessageRecieved?.Invoke("Client", emojislist);
                }
                else if (!Program.EMOJIS.ContainsKey(CurrText.Split(' ')[1])) { TextMessageRecieved?.Invoke("Client", "No emoji called " + CurrText.Split(' ')[1]); }
                else
                {
                    byte[] message = Encoding.Unicode.GetBytes(currUsername);
                    var emoji = Program.EMOJIS[CurrText.Split(' ')[1]];
                    var final = new List<byte>();
                    foreach (var byt in _magicNumber)
                    {
                        final.Add(byt);
                    }
                    final.Add(IMAGE_SENT_CODE);
                    final.Add((byte)(message.Length + _magicNumber.Length + 2));
                    foreach (var byt in message)
                    {
                        final.Add(byt);
                    }
                    
                    foreach (var byt in emoji)
                    {
                        final.Add(byt);
                    }
                    final.Add((byte)'\r');
                    int _ = CLIENT_SOCKET.Send(final.ToArray());
                }
            }
            else if (CurrText == "/help")
            {
                TextMessageRecieved?.Invoke("Server", "LIST OF COMMANDS: \n/list - lists connected clients\n/emoji <emoji> - sends an emoji, like 'fire' or 'nerd'\n/emoji list - lists all emojis (pretty long)");
            }
            //send a message
            else
            {
                byte[] message = Encoding.Unicode.GetBytes(currUsername + "\0" + CurrText);
                var final = new byte[_magicNumber.Length + 1 + message.Length + 1];
                Array.Copy(_magicNumber, 0, final, 0, _magicNumber.Length);
                final[_magicNumber.Length] = MESSAGE_SENT_CODE;
                Array.Copy(message, 0, final, _magicNumber.Length+1, message.Length);
                final[^1] = (byte)'\r';
                int _ = CLIENT_SOCKET.Send(final);
            }
        }

        private void sendImage(byte[] image)
        {
            byte[] message = Encoding.Unicode.GetBytes(currUsername);
            var final = new List<byte>();
            foreach (var byt in _magicNumber)
            {
                final.Add(byt);
            }
            final.Add(IMAGE_SENT_CODE);
            final.Add((byte)(message.Length + _magicNumber.Length + 2));
            foreach (var byt in message)
            {
                final.Add(byt);
            }
            foreach (var byt in image)
            {
                final.Add(byt);
            }
            final.Add((byte)'\r');
            int _ = CLIENT_SOCKET.Send(final.ToArray());
        }

        private void close()
        {
            DISCONNECTED = true;
            if (CLIENT_SOCKET == null) { return; }
            var message = Encoding.Unicode.GetBytes(currUsername);
            var final = new byte[_magicNumber.Length + message.Length + 2];
            Array.Copy(_magicNumber, 0, final, 0, _magicNumber.Length);
            final[_magicNumber.Length] = CLIENT_LEFT_CODE;
            final[^1] = (byte)'\r';
            Array.Copy(message, 0, final, _magicNumber.Length+1, message.Length);
            int _ = CLIENT_SOCKET.Send(final);
            CLIENT_SOCKET.Shutdown(SocketShutdown.Both);
            CLIENT_SOCKET.Close();
            CLIENT_SOCKET = null;
            OnDisconnected?.Invoke();
        }

        public NetworkManager(string ip, int port, string username)
        {
            SendMessage += sendMessage;
            SendImage += sendImage;
            Close += close;
            currUsername = username;
            CLIENT_SOCKET = null;
            MAX_MESSAGE_LEN = 26214400 + _magicNumber.Length + 2; //like 25mb i think 
            Thread thread = new(() => { Connect(ip, port, username); });
            thread.Start();
        }

        ~NetworkManager()
        {
            SendMessage -= sendMessage;
            SendImage -= sendImage;
            Close -= close;
        }

        private void Connect(string ip, int port, string un)
        {
            IPAddress? address = GetLocalIPAddress();
            //just keep using our own address if we put 'localhost'
            if (ip != "localhost")
            {
                bool valid = IPAddress.TryParse(ip, out IPAddress? tempaddress);
                if (!valid) { return; }
                else { address = tempaddress; }
            }

            Ping ping = new();
            var pr = ping.Send(address);
            if (pr.Status != IPStatus.Success) { return; }

            //try to open a connection to the target
            try
            {
                IPEndPoint localEndPoint = new(address, 11000);
                CLIENT_SOCKET = new(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                CLIENT_SOCKET.Connect(localEndPoint);
            }
            catch
            {
                return;
            }
            OnConnected?.Invoke();
            //send the initial message to the server containing our username
            {
                var username = Encoding.Unicode.GetBytes(un); //make sure to add the required EOF character (ascii 26)
                var message = new byte[username.Length + _magicNumber.Length + 2];
                message[_magicNumber.Length] = CLIENT_CONNECTED_CODE;
                Array.Copy(_magicNumber, 0, message, 0, _magicNumber.Length);
                message[^1] = (byte)'\r';
                Array.Copy(username, 0, message, 1 + _magicNumber.Length, username.Length);
                int _ = CLIENT_SOCKET.Send(message);
            }
            Thread.Sleep(10);
            Client_ListenForMessage(un);
        }

        static bool ValidateMagicNumber(byte[] receivedMagic)
        {
            for (int i = 0; i < _magicNumber.Length; i++)
            {
                if (receivedMagic[i] != _magicNumber[i])
                {
                    return false;
                }
            }
            return true;
        }

        private void Client_ListenForMessage(string username)
        {
            if (CLIENT_SOCKET == null || !CLIENT_SOCKET.Connected) { return; }
            if (DISCONNECTED) { return; }
            
            while (true)
            {
                byte[] buffer = new byte[MAX_MESSAGE_LEN];
                int totallen = 0;
                while (true)
                {
                    if (CLIENT_SOCKET == null || !CLIENT_SOCKET.Connected) { return; }
                    try
                    {
                        var bytes = new byte[MAX_MESSAGE_LEN];
                        int bytesRec = CLIENT_SOCKET.Receive(bytes);
                        if (bytesRec <= 0) { break; }
                        Array.Copy(bytes, 0, buffer, totallen, bytesRec);
                        totallen += bytesRec;
                        if (buffer.Contains((byte)'\r') || totallen >= MAX_MESSAGE_LEN) { break; }//look for EOF
                        if (buffer.Length >= _magicNumber.Length && !ValidateMagicNumber(buffer))
                        {
                            if (CLIENT_SOCKET.Connected) { CLIENT_SOCKET.Disconnect(false); }
                            CLIENT_SOCKET.Shutdown(SocketShutdown.Both);
                            CLIENT_SOCKET.Close();
                            CLIENT_SOCKET = null;
                            OnDisconnected?.Invoke();
                            return;
                        }// well its already an invalid message so ohwell
                    }
                    catch
                    {
                        if (CLIENT_SOCKET == null) { return; }
                        if (CLIENT_SOCKET.Connected) { CLIENT_SOCKET.Disconnect(false); }
                        CLIENT_SOCKET.Shutdown(SocketShutdown.Both);
                        CLIENT_SOCKET.Close();
                        CLIENT_SOCKET = null;
                        OnDisconnected?.Invoke();
                        return;
                    }
                }
                if (totallen <= 0)
                {
                    if (CLIENT_SOCKET.Connected) { CLIENT_SOCKET.Disconnect(false); }
                    CLIENT_SOCKET.Shutdown(SocketShutdown.Both);
                    CLIENT_SOCKET.Close();
                    CLIENT_SOCKET = null;
                    OnDisconnected?.Invoke();
                    return;
                }

                switch (buffer[_magicNumber.Length]) //first byte of any message is the identifier no its fucking not dipshit
                {
                    case CLIENT_CONNECTED_CODE:
                        //nothing here
                        break;
                    case MESSAGE_SENT_CODE:
                        //nothing here
                        break;
                    case CLIENT_LEFT_CODE:
                        //nothing here
                        break;
                    case LIST_CLIENTS_CODE:
                        //nothing here
                        break;
                    case IMAGE_SENT_CODE:
                        break;
                    case RECIEVE_CLIENT_JOIN_CODE:
                        TextMessageRecieved?.Invoke("Server", Encoding.Unicode.GetString(buffer, _magicNumber.Length + 1, totallen - 2 - _magicNumber.Length) + " Connected");
                        break;
                    case RECIEVE_CLIENT_LEFT_CODE:
                        TextMessageRecieved?.Invoke("Server", Encoding.Unicode.GetString(buffer, _magicNumber.Length + 1, totallen - 2 - _magicNumber.Length) + " Disconnected");
                        break;
                    case RECIEVE_MESSAGE_CODE:
                        var messagesender = Encoding.Unicode.GetString(buffer, _magicNumber.Length + 1, totallen - 2 - _magicNumber.Length).Split('\0'); //sender and message delineated by \0
                        TextMessageRecieved?.Invoke(messagesender[0], messagesender[1]);
                        break;
                    //this is FUCKING HORRIBLE BRO
                    case RECIEVE_IMAGE_CODE:
                        var index = _magicNumber.Length + 2;
                        // Find separator position
                        int sepIndex = buffer[_magicNumber.Length+1];

                        // Extract username
                        byte[] usernameBytes = new byte[sepIndex - _magicNumber.Length - 2];
                        Array.Copy(buffer, index, usernameBytes, 0, usernameBytes.Length);
                        string un = Encoding.Unicode.GetString(usernameBytes);

                        // Extract image data
                        int imgLength = totallen - sepIndex - 1; // Exclude end byte ('\r') correct

                        //Console.WriteLine("Sep index: " + sepIndex + " username length: "+usernameBytes.Length + " image length: "+imgLength);

                        byte[] imageData = new byte[imgLength];
                        Array.Copy(buffer, sepIndex, imageData, 0, imgLength);
                        ImageMessageRecieved?.Invoke(un, imageData);
                        break;
                    case RECIEVE_CLIENT_LIST_CODE:
                        string list = Encoding.Unicode.GetString(buffer, 1 + _magicNumber.Length, totallen - 2 - _magicNumber.Length).Replace("\\", ", ");
                        TextMessageRecieved?.Invoke("Server", "Connected Clients: " + list);
                        break;
                    default:
                        //TERMINAL.Output("Recieved erroneous message with code " + buffer[0] + ", discarding");
                        break;                    
                }
            }
        }
    }
}
