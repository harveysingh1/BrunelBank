using CommonComponents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BankServer
{
    class BankServer
    {
        // Specify ports to listen and send on, and IP here
        private const int senderPort = 8887;
        private const int listenerPort = 8888;
        private const string IP = "127.0.0.1";

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Server...");
            Server serverWorker = new Server(listenerPort, senderPort);
            serverWorker.Start();
            Console.WriteLine("Server started, waiting for packets...");

            // Event handler for when we receive an input. If the header of the message is of type L
            // this means that the input received is a login of a user.
            #region Message Received Event Handler
            serverWorker.InputReceived += (s, e) =>
            {
                switch (e.Header)
                {
                    case ("L"):
                        Console.WriteLine("{0} has logged into the banking system.", e.AccountNumber);
                        string[] loginPacket = serverWorker.GetPacketFromArrayList();
                        string logPacket = string.Join("~", loginPacket);
                        SendClientsMessage(IP, System.Convert.ToInt32(e.SenderPort), logPacket);
                        break;

                    case ("M"):
                        //Console.WriteLine("{0}: {1}", e.Username, e.Message);
                        string[] packetReceivedFromClient = serverWorker.GetPacketFromArrayList();
                        string stringPacket = string.Join("~", packetReceivedFromClient);
                        SendClientsMessage(IP, senderPort, stringPacket);
                        break;

                    default:
                        break;
                }
            };
            #endregion
        }

        private static void SendClientsMessage(string IP, int port, string packet)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(IP, port);

            // Checks if the client has connected successfully
            if (tcpClient.Connected)
            {
                // Sends the packet through the stream
                StreamWriter sw = new StreamWriter(tcpClient.GetStream());
                sw.Write(packet);
                sw.Flush();
                sw.Close();
            }
            tcpClient.Close();
        }
    }
}
