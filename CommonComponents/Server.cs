using BrunelBank;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonComponents
{
    public delegate void InputHandler(object sender, InputEventArgs e);

    public class Server : IListener
    {

        public event InputHandler InputReceived;

        // Create our TCPListener object
        TcpListener serverListener;
        // Create our port to listen on and send packets on
        private int listenerPort;
        private int senderPort;

        // List of packets
        private ArrayList listOfPackets;

        // Create a Semaphore to handle threads
        private Semaphore locker = new Semaphore(1, 1);

        protected virtual void OnInputReceived(string[] packet)
        {
            var del = InputReceived;
            if (del != null)
            {
                del(this, new InputEventArgs(packet));
            }
        }

        // Specify the ports used for the server for sending and receiving packets
        public Server(int listenerPort)
        {
            this.listenerPort = listenerPort;
        }

        public void ListenForInput()
        {
            serverListener.Start();
            while (true)
            {
                // Check if there are any pending connection requests
                if (serverListener.Pending())
                {
                    string[] acceptedPackets = AcceptPendingPackets();

                    // Add the received message to a list
                    locker.WaitOne();
                    listOfPackets.Add(acceptedPackets);
                    locker.Release();

                    // Event handler for when we receive a message
                    OnInputReceived(acceptedPackets);
                }
            }
        }

        private string[] AcceptPendingPackets()
        {
            // If there are pending requests, create a new connection
            TcpClient clientConnection = serverListener.AcceptTcpClient();

            // Get any messages waiting in the stream
            StreamReader sr = new StreamReader(clientConnection.GetStream());
            string str = sr.ReadToEnd();

            // Turn the incoming message into a string array.
            var messageReceived = str.Split('~');

            sr.Close();
            clientConnection.Close();

            return messageReceived;
        }

        public string[] GetPacketFromArrayList()
        {
            locker.WaitOne();
            string[] packet = (string[])listOfPackets[0];
            listOfPackets.RemoveAt(0);
            locker.Release();
            return packet;
        }

        public void Start()
        {
            listOfPackets = new ArrayList();
            serverListener = new TcpListener(IPAddress.Any, listenerPort);
            Thread threadListen = new Thread(new ThreadStart(ListenForInput));
            threadListen.Start();
        }

    }
}
