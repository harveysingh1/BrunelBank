using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonComponents.Packets
{
    public class LoginPacket : IPacket
    {
        public string Body { get { return _body; } }

        public string Header { get { return "L"; } }

        public void SetValue(string accountNumber, string port)
        {
            _body = Header + split + port + split + accountNumber + split + _balance;
        }

        private string _body;
        private string _balance;
        private string split = "~";
    }
}
