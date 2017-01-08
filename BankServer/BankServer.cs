using CommonComponents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BankServer
{
    class BankServer
    {
        // Specify ports to listen and send on, and IP here
        private int senderPort;
        private const int listenerPort = 8888;
        private const string IP = "127.0.0.1";
        private static Object thisLock = new Object();

        // For every connection, we will store which ports have been used
        // by the client to connect to the server in the format AccountNumber, PortNumber

        static void Main(string[] args)
        {
            // Create a keyvalue pair to show ports and their related accounts that are logged in
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
                        lock(serverWorker){
                            Console.WriteLine("{0} has logged into the banking system.", e.AccountNumber);
                            string[] loginPacket = serverWorker.GetPacketFromArrayList();
                            string logPacket = string.Join("~", loginPacket);
                            portsAccounts.Add(new KeyValuePair<String, String>(e.AccountNumber, e.SenderPort));
                        }
                        break;

                    case ("D"):
                        lock (serverWorker)
                        {
                            Console.WriteLine("Account {0} has deposited £{1} into their account.", e.AccountNumber, e.Amount);
                            string[] packetReceivedFromClient = serverWorker.GetPacketFromArrayList();
                            string stringPacket = string.Join("~", packetReceivedFromClient);
                            SendClientsMessage(IP, System.Convert.ToInt32(e.SenderPort), stringPacket);
                        }
                        break;

                    case ("T"):
                        string[] transferPacket = serverWorker.GetPacketFromArrayList();
                        string tranPacket = string.Join("~", transferPacket);


                        lock (serverWorker)
                        {
                            foreach (KeyValuePair<String, String> kvp in portsAccounts)
                            {
                                if (kvp.Key == e.AccountNumber)
                                {
                                    int senderPort = System.Convert.ToInt32(kvp.Value);
                                    {
                                        InformSender(tranPacket, senderPort);

                                        /************* The method below is to demonstrate a deadlock **************/
                                        //ThreadPool.QueueUserWorkItem(o => DeadlockInformSender(serverWorker, portsAccounts, e.AccountNumber, tranPacket));
                                    }
                                }
                                else if (kvp.Key == e.ReceivingAccount)
                                {
                                    int senderPort = System.Convert.ToInt32(kvp.Value);
                                    {
                                        InformReceiver(tranPacket, senderPort);

                                        /************* The method below is to demonstrate a deadlock **************/
                                        //ThreadPool.QueueUserWorkItem(o => DeadlockInformReceiver(serverWorker, portsAccounts, e.AccountNumber, tranPacket));
                                    }
                                }
                            }
                        }
                        Console.WriteLine();
                        Console.WriteLine("Account {0} has transferred £{1} into Account {2}", e.AccountNumber, e.Amount, e.ReceivingAccount);
                        break;

                    default:
                        break;
                }
            };
            #endregion
        }

        private static void InformSender(string tranPacket, int port)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(IP, port);

            // Checks if the client has connected successfully
            if (tcpClient.Connected)
            {
                // Sends the packet through the stream
                StreamWriter sw = new StreamWriter(tcpClient.GetStream());
                sw.Write(tranPacket);
                sw.Flush();
                sw.Close();
            }
            tcpClient.Close();
        }

        private static void InformReceiver(string tranPacket, int port)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(IP, port);

            // Checks if the client has connected successfully
            if (tcpClient.Connected)
            {
                // Sends the packet through the stream
                StreamWriter sw = new StreamWriter(tcpClient.GetStream());
                sw.Write(tranPacket);
                sw.Flush();
                sw.Close();
            }
            tcpClient.Close();
        }

        /*************************************** The below DeadlockInformSender and DeadlockInformReceiver methods are to demonstrate what would occur
        **************************************** during a deadlock. As stated below, the first thread would lock the serverWorker, and
        **************************************** then go on to attempt to lock the portsAccounts KeyValuePair. At the same time, the
        **************************************** second thread would attempt to lock the portsAccounts KeyvaluePair, and then try to lock
        **************************************** the serverWorker. As the two methods cannot fulfill their duties without full access of
        **************************************** each other, a deadlock occurs at the account level.
        */


        //private static void DeadlockInformSender(Server serverWorker, List<KeyValuePair<String, String>> portsAccounts, string account, string tranPacket)
        //{
        //    bool isLocked = false;
        //    lock (serverWorker)
        //    {
        //        Console.WriteLine("Sender has locked B");
        //        //string[] transferPacket = serverWorker.GetPacketFromArrayList();
        //        //string tranPacket = string.Join("~", transferPacket);
        //        Thread.Sleep(2000);

        //        Console.WriteLine("Sender attempting to lock A...");
        //        isLocked = Monitor.TryEnter(portsAccounts);
        //        if (!isLocked)
        //        {
        //            Console.WriteLine("Sender cannot access A as it is in use...");
        //        }
        //        else {
        //            lock (portsAccounts)
        //            {
        //                foreach (KeyValuePair<String, String> kvp in portsAccounts)
        //                {
        //                    if (kvp.Key == account)
        //                    {
        //                        Console.WriteLine("Sender has locked A");
        //                    }
        //                }
        //                Thread.Sleep(5000);
        //            }
        //        }

        //    }

        //}

        //private static void DeadlockInformReceiver(Server serverWorker, List<KeyValuePair<String, String>> portsAccounts, string account, string tranPacket)
        //{
        //    bool isLocked = false;
        //    lock (portsAccounts)
        //    {
        //        Console.WriteLine("Receiver has locked A");
        //        Thread.Sleep(2000);

        //        Console.WriteLine("Receiver attempting to lock B...");
        //        isLocked = Monitor.TryEnter(serverWorker);
        //        if (!isLocked)
        //        {
        //            Console.WriteLine("Receiver cannot access B as it is in use...");
        //        }
        //        else
        //        {
        //            lock (serverWorker)
        //            {
        //                //string[] transferPacket = serverWorker.GetPacketFromArrayList();
        //                //string tranPacket = string.Join("~", transferPacket);
        //                foreach (KeyValuePair<String, String> kvp in portsAccounts)
        //                {
        //                    if (kvp.Key == account)
        //                    {
        //                        Console.WriteLine("Receiver has locked B");
        //                    }
        //                }
        //                Thread.Sleep(5000);
        //            }
        //        }
        //    }
        //}


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