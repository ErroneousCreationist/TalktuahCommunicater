using System.Net.Sockets;
using System.Net;

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
            if (args.Length !=1) { Console.WriteLine("Error: Args must be of length 1, in format \"server.exe port\""); return; }
            if (!int.TryParse(args[0], out int value)) { Console.WriteLine("Error: Arg 0 must be a valid integer port"); return; }


        }

        private const byte CLIENT_CONNECTED_CODE = 0;
        private const byte MESSAGE_SENT_CODE = 1;
        private const byte CLIENT_LEFT_CODE = 2;
        private const byte RECIEVE_MESSAGE_CODE = 3;
        private const byte RECIEVE_CLIENT_JOIN_CODE = 4;
        private const byte RECIEVE_CLIENT_LEFT_CODE = 5;
        private const byte LIST_CLIENTS_CODE = 6;
        private const byte RECIEVE_CLIENT_LIST_CODE = 7;

        private byte[] _magicNumber = { 0xCA, 0xFE, 0xBA, 0xBE };
        private static Socket? SERVER_SOCKET, CLIENT_SOCKET;
        private readonly List<ConnectedClient> CLIENTS = new();
        private readonly object _lock = new(); // Prevent race conditions
        private bool _isRunning;

        private bool ValidateMagicNumber(NetworkStream stream)
        {
            byte[] receivedMagic = new byte[4];
            stream.Read(receivedMagic, 0, 4);

            for (int i = 0; i < _magicNumber.Length; i++)
            {
                if (receivedMagic[i] != _magicNumber[i])
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsClientConnected(Socket client)
        {
            if (CLIENTS == null) { return false; }
            foreach (var item in CLIENTS)
            {
                if (item.socket == client) { return true; }
            }
            return false;
        }

        void Host_ListenForConnection()
        {
            try
            {
                //start the host here
                IPEndPoint localEndPoint = new(IPAddress.Any, 11000);
                SERVER_SOCKET = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                SERVER_SOCKET.Bind(localEndPoint);
                SERVER_SOCKET.Listen(100);

                //begin listening for messages
                while (true)
                {
                    if (SERVER_SOCKET == null) { return; }
                    var client = SERVER_SOCKET.Accept();
                    //if we have a suspicious amount of connections from the same ip address, stop connections from it! (make it 2 to allow me to test my own program lmao)
                    if (client != null && !IsClientConnected(client))
                    {
                        //Thread thread = new(new ParameterizedThreadStart(ListenForMessage));
                        //CLIENTS.Add(new ConnectedClient(client, thread, ""));
                        Thread.Sleep(10);
                        //thread.Start(CLIENTS.Count - 1);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                if (e is SocketException) { Console.WriteLine("ERRORCODE=" + (e as SocketException).ErrorCode); }
            }
        }
}
