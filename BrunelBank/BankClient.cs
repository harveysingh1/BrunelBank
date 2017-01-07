using CommonComponents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace BrunelBank
{
    public class BankClient
    {
        // Specify ports to listen and send on, and IP here
        private const int senderPort = 8888;
        private static int listenerPort;        

        private const string IP = "127.0.0.1";
        private static int accountNumber;

        static void Main(string[] args)
        {
            listenerPort = CheckOpenPort();

            // Create a new client instance to handle stuff and start it
            Client clientWorker = new Client(listenerPort, senderPort);
            clientWorker.Start();

            #region Message Received Event Handler
            clientWorker.InputReceived += (s, e) =>
            {
                switch (e.Header)
                {
                    case ("L"):
                        Console.WriteLine("Welcome to your banking system Account Number: {0}", e.AccountNumber);                        
                        break;

                    case ("D"):
                        //Console.WriteLine("{0}: {1}", e.Username, e.Message);
                        break;

                    default:
                        break;
                }
            };
            #endregion 

            string accountLoggedIn = Login(clientWorker);
            accountNumber = System.Convert.ToInt32(accountLoggedIn);
            clientWorker.AccountNumber = System.Convert.ToInt32(accountLoggedIn);

            while (true)
            {
                ProvideOptions(accountLoggedIn, clientWorker);
            }        
        }

        private static void ProvideOptions(string accountLoggedIn, Client clientWorker)
        {
            Console.WriteLine();
            Console.WriteLine("Please select from the following options:");
            Console.WriteLine();
            Console.WriteLine("1) Check balance on account");
            Console.WriteLine("2) Deposit money into the account");
            Console.WriteLine("3) Withdraw money");
            Console.WriteLine("4) Transfer money to another account");

            string option = Console.ReadLine();

            switch (System.Convert.ToInt32(option))
            {
                case (1):
                    Console.WriteLine();
                    Console.WriteLine("Your current balance is: £{0}", clientWorker.Balance);
                    break;

                case (2):
                    Console.WriteLine();
                    Console.WriteLine("How much would you like to deposit into your account?");
                    string deposit = Console.ReadLine();
                    int depositInt = System.Convert.ToInt32(deposit);
                    DepositMoney(depositInt, clientWorker);
                    Console.WriteLine("Your new balance is now: £{0}", clientWorker.Balance);
                    break;

                case (3):
                    Console.WriteLine();
                    Console.WriteLine("How much would you like to withdraw from your account?");
                    string withdraw = Console.ReadLine();
                    int withdrawInt = System.Convert.ToInt32(withdraw);
                    WithdrawMoney(withdrawInt, clientWorker);
                    Console.WriteLine("Your new balance is now £{0}", clientWorker.Balance);
                    break;

                case (4):
                    Console.WriteLine();
                    Console.WriteLine("Please enter the account that you would like to transfer money to, followed by the amount");
                    break;
                default:
            	    break;
            }

        }

        private static void WithdrawMoney(int withdraw, Client clientWorker)
        {
            clientWorker.Balance -= withdraw;
        }

        private static void DepositMoney(int deposit, Client clientWorker)
        {
            clientWorker.Balance += deposit;
            string DepositPacket = clientWorker.CreateDepositPacket(accountNumber.ToString(), deposit.ToString());
            clientWorker.SendPacket(IP, DepositPacket);
        }

        private static void TransferMoney(int transfer, Client clientWorker, string receivingAccount)
        {
            string TransferPacket = clientWorker.CreateTransferPacket(accountNumber.ToString(), transfer.ToString(), receivingAccount);
            clientWorker.SendPacket(IP, TransferPacket);
        }

        private static string Login(Client clientWorker)
        {
            Console.WriteLine("Please enter your account number: ");
            string accountNumber = Console.ReadLine();
            string LoginPacket = clientWorker.CreateLoginPacket(accountNumber);
            clientWorker.SendPacket(IP, LoginPacket);
            return accountNumber;
        }

        private static int CheckOpenPort()
        {
            Socket sock = new Socket(AddressFamily.InterNetwork,
                         SocketType.Stream, ProtocolType.Tcp);

            sock.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0)); // Pass 0 here.

            int port = ((IPEndPoint)sock.LocalEndPoint).Port;
            return port;
        }
    }
}
