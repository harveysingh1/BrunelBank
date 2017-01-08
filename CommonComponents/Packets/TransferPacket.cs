using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonComponents.Packets
{
    class TransferPacket : IPacket
    {
        public string Header { get { return "T"; } }
        public string Body { get { return _body; } }

        public TransferPacket(string transfer, string receivingAccount)
        {
            _amount = transfer;
            _receivingAccount = receivingAccount;

        }

        public void SetValue(string account, string port)
        {
            _body = Header + split + port + split + account + split + _amount + split + _receivingAccount;
        }

        private string _amount;
        private string _body;
        private string _receivingAccount;
        string split = "~";
    }
}
