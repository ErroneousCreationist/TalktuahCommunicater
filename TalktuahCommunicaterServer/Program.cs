﻿using System.Net.Sockets;
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
            OPENPINGINGPORT = false;
            if (args.Length > 0) { if (int.TryParse(args[0], out int value)) { PORT = value; } }
            if (args.Length > 1) { if (args[1] == "true") { OPENPINGINGPORT = true; } }
            CLIENTS = new();

            MAX_MESSAGE_LEN = 26214400+_magicNumber.Length+2;

            Console.WriteLine($"Starting server on port {PORT} with the pinging port {PORT + 1}");
            if (OPENPINGINGPORT) { new Thread(Host_ListenForPings).Start(); }
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
        private const byte IMAGE_SENT_CODE = 8;
        private const byte RECIEVE_IMAGE_CODE = 9;
        private const byte PING_CODE = 10;

        private static int MAX_MESSAGE_LEN;

        private static readonly byte[] _magicNumber = { 0xCA, 0xFE, 0xBA, 0xBE };

        private static Socket? SERVER_SOCKET, PING_SOCKET;
        private static List<ConnectedClient>? CLIENTS = new();
        private static int PORT;
        private static bool OPENPINGINGPORT;

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

        static int ConnectedClientCount
        {
            get
            {
                int returned = 0;
                if (CLIENTS == null) { return returned; }
                foreach (var item in CLIENTS)
                {
                    if (!item.ClientDisconnecting) { returned++; }
                }
                return returned;
            }
        }

        static void Host_ListenForPings()
        {
            try
            {
                // Start the host here
                IPEndPoint localEndPoint = new(IPAddress.Any, PORT + 1);
                PING_SOCKET = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                PING_SOCKET.Bind(localEndPoint);
                PING_SOCKET.Listen();
                Console.WriteLine("Pinging endpoint is now operational");

                // Begin listening for connected client pings
                while (true)
                {
                    if (PING_SOCKET == null) { return; }
                    var client = PING_SOCKET.Accept();
                    if (client != null)
                    {
                        Task.Run(() => HandlePing(client)); // Handle the client in a separate thread
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                if (e is SocketException) { Console.WriteLine(e.ToString()); }
            }
        }

        // Function to handle client communication
        static void HandlePing(Socket client)
        {
            try
            {
                byte[] buffer = new byte[1024]; // Adjust buffer size as needed
                var totallen = 0;
                while (true)
                {
                    try
                    {
                        if (PING_SOCKET == null) { return; }
                        if (!client.Connected) { return; }
                        var bytes = new byte[1024];
                        int bytesRec = client.Receive(bytes);
                        if (bytesRec <= 0) { break; }
                        Array.Copy(bytes, 0, buffer, totallen, bytesRec);
                        totallen += bytesRec;
                        if (buffer.Contains((byte)'\r') || totallen >= 1024) { break; }//look for EOF
                        if (buffer.Length >= _magicNumber.Length && !ValidateMagicNumber(buffer))
                        {
                            Console.WriteLine("Received erroneous ping from " + IPAddress.Parse(((IPEndPoint)client.RemoteEndPoint).Address.ToString()));
                            client.Close();
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Ping disconnected with error: "+e.ToString());
                        client.Close();
                        return;
                    }
                }

                byte[] receivedData = buffer[..totallen];
                if (receivedData[_magicNumber.Length] != PING_CODE) {
                    Console.WriteLine("Received erroneous ping from " + IPAddress.Parse(((IPEndPoint)client.RemoteEndPoint).Address.ToString()));
                    client.Close();
                }

                // Process received data (implement your own message parsing here)
                Console.WriteLine("Received ping from " + IPAddress.Parse(((IPEndPoint)client.RemoteEndPoint).Address.ToString())+", responding...");

                // Prepare a response (implement your own response logic here)
                byte[] responseData = new byte[_magicNumber.Length + 2];
                Array.Copy(_magicNumber, 0, responseData, 0, _magicNumber.Length);
                responseData[_magicNumber.Length] = (byte)ConnectedClientCount;
                responseData[^1] = (byte)'\r';
                client.Send(responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while receiving ping: {ex}");
            }
            finally
            {
                client.Close(); // Discard the client after responding
            }
        }

        static void Host_ListenForConnection()
        {
            try
            {
                //start the host here
                IPEndPoint localEndPoint = new(IPAddress.Any, PORT);
                SERVER_SOCKET = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                SERVER_SOCKET.Bind(localEndPoint);
                SERVER_SOCKET.Listen();
                Console.WriteLine("Endpoint is now operational");

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
                        Console.WriteLine("Client "+CLIENTS[id].Username+" Disconnected with error.");
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
                            Console.WriteLine(Encoding.Unicode.GetString(buffer, 1+_magicNumber.Length, totallen - _magicNumber.Length - 2) + " Connected");
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
                        Console.WriteLine(Encoding.Unicode.GetString(buffer, 1+_magicNumber.Length, totallen - _magicNumber.Length - 2).Replace('\0', ':')); 
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
                    case IMAGE_SENT_CODE:
                        Console.WriteLine("Received image");
                        {
                            buffer[_magicNumber.Length] = RECIEVE_IMAGE_CODE;
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
                        Console.WriteLine(Encoding.Unicode.GetString(buffer, 1+_magicNumber.Length, totallen - _magicNumber.Length - 2) + " Disconnected");
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
                    case RECIEVE_IMAGE_CODE: break;
                    default:
                        Console.WriteLine("Recieved erroneous message with code " + buffer[_magicNumber.Length] + ", discarding");
                        //TERMINAL.Output("Recieved erroneous message with code " + buffer[0] + ", discarding");
                        break;
                }
            }
        }
    }
}
