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
        private int senderPort;
        private const int listenerPort = 8888;
        private const string IP = "127.0.0.1";

        // For every connection, we will store which ports have been used
        // by the client to connect to the server in the format AccountNumber, PortNumber
        
        static void Main(string[] args)
        {
            List<KeyValuePair<String, String>> portsAccounts = new List<KeyValuePair<String, String>>();

            Console.WriteLine("Starting Server...");
            Server serverWorker = new Server(listenerPort);
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
                        portsAccounts.Add(new KeyValuePair<String, String>(e.AccountNumber, e.SenderPort));
                        break;

                    case ("D"):
                        Console.WriteLine("Account {0} has deposited £{1} into their account.", e.AccountNumber, e.Amount);
                        string[] packetReceivedFromClient = serverWorker.GetPacketFromArrayList();
                        string stringPacket = string.Join("~", packetReceivedFromClient);
                        SendClientsMessage(IP, System.Convert.ToInt32(e.SenderPort), stringPacket);
                        break;

                    case ("T"):
                        Console.WriteLine("Account {0} has transferred £{1} into Account {2}", e.AccountNumber, e.Amount, e.ReceivingAccount);
                        string[] transferPacket = serverWorker.GetPacketFromArrayList();
                        string tranPacket = string.Join("~", transferPacket);
                        foreach (KeyValuePair<String, String> kvp in portsAccounts)
                        {
                            if(kvp.Key == e.AccountNumber)
                            {
                                InformSender(kvp.Key, kvp.Value, e.Amount);
                            }
                        }
                        break;

                    default:
                        break;
                }
            };
            #endregion
        }

        private static void InformSender(string account, string port, string amount)
        {
            
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

        private static void InformReceiver()
        {

        }

    }
}
