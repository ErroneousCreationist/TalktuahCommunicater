using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Tmds.DBus.Protocol;

namespace TalktuahCommunicater1925
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

        private Socket? CLIENT_SOCKET;
        private static readonly byte[] _magicNumber = new byte[] [0xCA, 0xFE, 0xBA, 0xBE];
        private static int MAX_MESSAGE_LEN;

        public event Action<string, string> TextMessageRecieved; // Event for incoming messages
        public event Action<string, byte[]> ImageMessageRecieved;
        public event Action OnConnected;
        public event Action OnDisconnected;

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

        public NetworkManager(string ip, int port, string username)
        {
            CLIENT_SOCKET = null;
            MAX_MESSAGE_LEN = 26214400 + _magicNumber.Length + 2; //like 25mb i think 
            Connect(ip, port, username);
        }

        private void Connect(string ip, int port, string un)
        {
            IPAddress? address = GetLocalIPAddress();
            //just keep using our own address if we put 'localhost'
            if(ip != "localhost") {
                bool valid = IPAddress.TryParse(ip, out IPAddress? tempaddress);
                if (!valid) { return; }
                else { address = tempaddress; }

                Ping ping = new();
                var pr = ping.Send(address);
                if (pr.Status != IPStatus.Success) { return; }
            }

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
                Array.Copy(username, 0, message, 1+_magicNumber.Length, username.Length);
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

            byte[] buffer = new byte[MAX_MESSAGE_LEN];
            int totallen = 0;
            while (true)
            {
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
                switch (buffer[0]) //first byte of any message is the identifier
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
                        TextMessageRecieved?.Invoke("Server", Encoding.Unicode.GetString(buffer, _magicNumber.Length + 1, totallen - 2) + " Connected");
                        break;
                    case RECIEVE_CLIENT_LEFT_CODE:
                        TextMessageRecieved?.Invoke("Server", Encoding.Unicode.GetString(buffer, _magicNumber.Length + 1, totallen - 2) + " Disconnected");
                        break;
                    case RECIEVE_MESSAGE_CODE:
                        var messagesender = Encoding.Unicode.GetString(buffer, _magicNumber.Length + 1, totallen - 2).Split('\0'); //sender and message delineated by \0
                        TextMessageRecieved?.Invoke(messagesender[0], messagesender[1]);
                        break;
                    case RECIEVE_IMAGE_CODE:
                        byte[] image = new byte[];
                        string un = "";
                        for (int i = _magicNumber.Length+1; i < buffer.Length; i++)
                        {
                            var s = Encoding.Unicode.GetString(new byte[] [buffer[i]]);
                            if (s == "\0") { image = new byte[buffer.Length - i - 2]; Array.Copy(buffer, i + 1, image, 0, buffer.Length - i - 2); break; }
                            else { un += s; }
                        }
                        ImageMessageRecieved?.Invoke(un, image);
                        break;
                    case RECIEVE_CLIENT_LIST_CODE:
                        string list = Encoding.Unicode.GetString(buffer, 1 + _magicNumber.Length, totallen - 2).Replace("\\", ", ");
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
