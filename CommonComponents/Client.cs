using System.Net.Sockets;
using BrunelBank;
using System.IO;
using System.Net;
using System.Threading;
using CommonComponents.Packets;

namespace CommonComponents
{
    public class Client : IListener
    {
        public event InputHandler InputReceived;

        private TcpListener clientListener;

        private int listenerPort;
        private int senderPort;


        public int AccountNumber
        {
            get { return _accountNumber; }
            set { _accountNumber = value; }
        }

        public int Balance
        {
            get { return _balance; }
            set { _balance = value; }
        }


        public Client(int listenerPort, int senderPort)
        {
            this.listenerPort = listenerPort;
            this.senderPort = senderPort;
            Balance = 200;
        }

        protected virtual void OnMessageReceived(string[] packet)
        {
            var del = InputReceived;
            if (del != null)
            {
                del(this, new InputEventArgs(packet));
            }
        }

        public string CreateLoginPacket(string accountNumber)
        {
            IPacket packet = new LoginPacket();
            packet.SetValue(accountNumber, (listenerPort.ToString()));
            return packet.Body;

        }

        public string CreateDepositPacket(string accountNumber, string deposit)
        {
            IPacket packet = new DepositPacket(deposit);
            packet.SetValue(accountNumber, listenerPort.ToString());
            return packet.Body;
        }

        public string CreateTransferPacket(string accountNumber, string amount, string receivingAccount)
        {
            IPacket packet = new TransferPacket(amount, receivingAccount);
            packet.SetValue(accountNumber, listenerPort.ToString());
            return packet.Body;
        }

        public void ListenForInput()
        {
            clientListener.Start();
            while (true)
            {
                // Check if there are any pending connection requests
                if (clientListener.Pending())
                {
                    string[] messageReceived = AcceptPendingPackets();
                    OnMessageReceived(messageReceived);
                }
            }
        }

        private string[] AcceptPendingPackets()
        {
            // If there are pending requests, create a new connection
            TcpClient clientConnection = clientListener.AcceptTcpClient();

            // Get any messages waiting in the stream
            StreamReader sr = new StreamReader(clientConnection.GetStream());

            string str = sr.ReadToEnd();

            // Turn the incoming message into a string array.
            var messageReceived = str.Split('~');

            sr.Close();
            clientConnection.Close();

            return messageReceived;
        }

        public void SendPacket(string IP, string packet)
        {
            // Send data to client
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(IP, senderPort);

            StreamWriter sw = new StreamWriter(tcpClient.GetStream());
            sw.Write(packet);
            sw.Flush();
            sw.Close();
            tcpClient.Close();
        }

        public void Start()
        {
            // Start listening on the specified port for TCP connections
            clientListener = new TcpListener(IPAddress.Any, listenerPort);
            // Create a background thread to listen for messages
            Thread threadListen = new Thread(new ThreadStart(ListenForInput));
            threadListen.Start();
        }

        private int _accountNumber;
        private int _balance;
    }
}
