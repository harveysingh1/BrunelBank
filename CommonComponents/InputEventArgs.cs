using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrunelBank
{
    public class InputEventArgs : EventArgs
    {
        public InputEventArgs(string[] packet)
        {
            Header = packet[0];
            SenderPort = packet[1];
            AccountNumber = packet[2];
            Amount = packet[3];

            // If the packet the user is making is a transfer, specify
            // which account the money is being transferred to.
            if (packet.Length == 5)
            {
                ReceivingAccount = packet[4];
            }
        }

        public string Header { get; set; }
        public string SenderPort { get; set; }
        public string AccountNumber { get; set; }
        public string Amount { get; set; }
        public string ReceivingAccount { get; set; }
    }
}

