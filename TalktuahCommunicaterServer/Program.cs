using System.Net.Sockets;
using System.Net;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace TalktuahCommunicaterServer
{
    struct ConnectedClient
    {
        public Socket socket;
        public Thread myThread;
        public bool ClientDisconnecting;
        public string Username;

        public ConnectedClient(Socket socket, Thread myThread, string username, bool disconnecting = false)
        {
            this.socket = socket;
            this.myThread = myThread;
            Username = username;
            ClientDisconnecting = disconnecting;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            PORT = 11000;
            if (args.Length >= 1) { if (!int.TryParse(args[0], out int value)) { PORT = value; } }

            CLIENTS = new();

            MAX_MESSAGE_LEN = 26214400+_magicNumber.Length+2;
            
            Host_ListenForConnection();
        }

        private const byte CLIENT_CONNECTED_CODE = 0;
        private const byte MESSAGE_SENT_CODE = 1;
        private const byte CLIENT_LEFT_CODE = 2;
        private const byte RECIEVE_MESSAGE_CODE = 3;
        private const byte RECIEVE_CLIENT_JOIN_CODE = 4;
        private const byte RECIEVE_CLIENT_LEFT_CODE = 5;
        private const byte LIST_CLIENTS_CODE = 6;
        private const byte RECIEVE_CLIENT_LIST_CODE = 7;

        private static int MAX_MESSAGE_LEN;

        private static readonly byte[] _magicNumber = { 0xCA, 0xFE, 0xBA, 0xBE };
        private static Socket? SERVER_SOCKET;
        private static List<ConnectedClient>? CLIENTS = new();
        private static readonly object _lock = new(); // Prevent race conditions
        private static int PORT;

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

        static bool IsClientConnected(Socket client)
        {
            if (CLIENTS == null) { return false; }
            foreach (var item in CLIENTS)
            {
                if (item.socket == client) { return true; }
            }
            return false;
        }

        static void Host_ListenForConnection()
        {
            try
            {
                //start the host here
                IPEndPoint localEndPoint = new(IPAddress.Any, 11000);
                SERVER_SOCKET = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                SERVER_SOCKET.Bind(localEndPoint);
                SERVER_SOCKET.Listen(1000);

                //begin listening for messages
                while (true)
                {
                    if (SERVER_SOCKET == null) { return; }
                    var client = SERVER_SOCKET.Accept();
                    if (client != null && !IsClientConnected(client))
                    {
                        Thread thread = new(new ParameterizedThreadStart(ListenForMessage));
                        CLIENTS.Add(new ConnectedClient(client, thread, ""));
                        Thread.Sleep(10);
                        thread.Start(CLIENTS.Count - 1);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                if (e is SocketException) { Console.WriteLine(e.ToString()); }
            }
        }

        private static void ListenForMessage(object? data)
        {
            while(true){
                if (data == null || CLIENTS==null || CLIENTS.Count==0 || SERVER_SOCKET == null) { return; }
                int id = (int)data;
                if (CLIENTS[id].ClientDisconnecting || CLIENTS[id].socket==null) { return; } //end thread if we lose connection or something 🔥🇺🇦
                byte[] buffer = new byte[MAX_MESSAGE_LEN];
                int totallen = 0;
                while (true)
                {
                    try
                    {
                        if (CLIENTS[id].ClientDisconnecting) { return; }
                        if (SERVER_SOCKET == null) { return; }
                        if (!CLIENTS[id].socket.Connected) { return; }
                        var bytes = new byte[MAX_MESSAGE_LEN];
                        int bytesRec = CLIENTS[id].socket.Receive(bytes);
                        if (bytesRec <= 0) { break; }
                        Array.Copy(bytes, 0, buffer, totallen, bytesRec);
                        totallen += bytesRec;
                        if (buffer.Contains((byte)'\r') || totallen >= MAX_MESSAGE_LEN) { break; }//look for EOF
                        if(buffer.Length >= _magicNumber.Length && !ValidateMagicNumber(buffer)) { 
                            Console.WriteLine("Connection is erroneous, kicking client."); 
                            CLIENTS[id].socket.Disconnect(false); CLIENTS[id].socket.Close(); 
                            CLIENTS[id] = new(CLIENTS[id].socket, Thread.CurrentThread, CLIENTS[id].Username, true); 
                            return; 
                        }// well its already an invalid message so ohwell
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.WriteLine("Client "+CLIENTS[id].Username+" Disconnected with error. Press any key to continue.");
                        CLIENTS[id].socket.Shutdown(SocketShutdown.Both);
                        CLIENTS[id].socket.Disconnect(false);
                        return;
                    }
                }
                if (totallen <= 0) { //disconnect client due to loss of connection
                    Thread.Sleep(10);
                    CLIENTS[id] = new(CLIENTS[id].socket, Thread.CurrentThread, CLIENTS[id].Username, true);
                    Console.WriteLine(CLIENTS[id].Username + " Disconnected");
                    var username = Encoding.Unicode.GetBytes(CLIENTS[id].Username);
                    byte[] sent = new byte[username.Length+2+_magicNumber.Length];
                    Array.Copy(_magicNumber, 0, sent, 0, _magicNumber.Length);
                    sent[_magicNumber.Length] = RECIEVE_CLIENT_LEFT_CODE;
                    sent[^1] = (byte)'\r';
                    Array.Copy(username, 0, sent, _magicNumber.Length+1, username.Length);
                    CLIENTS[id] = new(CLIENTS[id].socket, Thread.CurrentThread, CLIENTS[id].Username, true); //mark as disconnecting
                    for (int i = 0; i < CLIENTS.Count; i++)
                    {
                        if (i == id) { continue; }
                        if (CLIENTS[i].ClientDisconnecting) { continue; }
                        CLIENTS[i].socket.Send(sent);
                    }

                    return;
                }

                if (buffer[_magicNumber.Length] != CLIENT_CONNECTED_CODE && CLIENTS[id].Username == "") { 
                    Console.WriteLine("Connection is erroneous, kicking client."); 
                    CLIENTS[id].socket.Disconnect(false); 
                    CLIENTS[id].socket.Close(); 
                    CLIENTS[id] = new(CLIENTS[id].socket, Thread.CurrentThread, CLIENTS[id].Username, true); 
                    return; 
                } //if our first message wasn't handing over username that we connected, then kick the connection

                switch (buffer[_magicNumber.Length]) //first byte of any message is the identifier
                {
                    case CLIENT_CONNECTED_CODE:
                        {
                            Console.WriteLine(Encoding.Unicode.GetString(buffer, 1+_magicNumber.Length, totallen - 2) + " Connected");
                            buffer[_magicNumber.Length] = RECIEVE_CLIENT_JOIN_CODE;
                            byte[] sent = new byte[totallen];
                            Array.Copy(buffer, sent, totallen);
                            CLIENTS[id] = new(CLIENTS[id].socket, Thread.CurrentThread, Encoding.Unicode.GetString(buffer, 1+_magicNumber.Length, totallen - 2), false); //mark as disconnecting
                            for (int i = 0; i < CLIENTS.Count; i++)
                            {
                                if (CLIENTS[i].ClientDisconnecting) { continue; }
                                CLIENTS[i].socket.Send(sent);
                            }
                            break;
                        }
                    case LIST_CLIENTS_CODE:
                        {
                            Console.WriteLine("List clients request recieved");
                            string fulllist = "";
                            for (int i = 0; i < CLIENTS.Count; i++)
                            {
                                if (CLIENTS[i].ClientDisconnecting) { continue; }//yeah well we can't just remove them from the list so im going to have to compromise 💀
                                fulllist += CLIENTS[i].Username + "\\";
                            }
                            fulllist = fulllist[..^1];
                            var listbytes = Encoding.Unicode.GetBytes(fulllist);
                            byte[] sent = new byte[listbytes.Length+_magicNumber.Length+2];
                            sent[_magicNumber.Length] = RECIEVE_CLIENT_LIST_CODE;
                            Array.Copy(_magicNumber, 0, sent, 0, _magicNumber.Length);
                            sent[^1] = (byte)'\r';
                            Array.Copy(listbytes, 0, sent, 1 + _magicNumber.Length, listbytes.Length);
                            CLIENTS[id].socket.Send(sent);
                            break;
                        }
                    case MESSAGE_SENT_CODE:
                        //return all messages to the clients
                        Console.WriteLine(Encoding.Unicode.GetString(buffer, 1+_magicNumber.Length, totallen - 2)); 
                        {
                            buffer[_magicNumber.Length] = RECIEVE_MESSAGE_CODE;
                            byte[] sent = new byte[totallen];
                            Array.Copy(buffer, sent, totallen);
                            for (int i = 0; i < CLIENTS.Count; i++)
                            {
                                if (CLIENTS[i].ClientDisconnecting) { continue; }
                                CLIENTS[i].socket.Send(sent);
                            }
                            break;
                        }
                    case CLIENT_LEFT_CODE:
                        Console.WriteLine(Encoding.Unicode.GetString(buffer, 1+_magicNumber.Length, totallen - 2) + " Disconnected");
                        {
                            buffer[_magicNumber.Length] = RECIEVE_CLIENT_LEFT_CODE;
                            byte[] sent = new byte[totallen];
                            Array.Copy(buffer, sent, totallen);
                            CLIENTS[id] = new(CLIENTS[id].socket, Thread.CurrentThread, CLIENTS[id].Username, true); //mark as disconnecting
                            for (int i = 0; i < CLIENTS.Count; i++)
                            {
                                if (i == id) { continue; }
                                if (CLIENTS[i].ClientDisconnecting) { continue; }
                                CLIENTS[i].socket.Send(sent);
                            }
                            break;
                        }
                    case RECIEVE_CLIENT_JOIN_CODE:
                        //server doesnt need to act on this
                        break;
                    case RECIEVE_MESSAGE_CODE:
                        //server doesn't need to act on this
                        break;
                    case RECIEVE_CLIENT_LIST_CODE:
                        //sevrer doesnt need to act pn this
                        break;
                    default:
                        Console.WriteLine("Recieved erroneous message with code " + buffer[_magicNumber.Length] + ", discarding");
                        //TERMINAL.Output("Recieved erroneous message with code " + buffer[0] + ", discarding");
                        break;
                }
            }
        }
    }
}
