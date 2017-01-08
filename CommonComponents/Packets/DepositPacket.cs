namespace CommonComponents.Packets
{
    public class DepositPacket : IPacket
    {
        public string Header { get { return "D"; } }
        public string Body { get { return _body; } }

        public DepositPacket(string deposit)
        {
            _amount = deposit;
        }

        public void SetValue(string account, string port)
        {
            _body = Header + split + port + split + account + split + _amount;
        }

        private string _amount;
        private string _body;
        string split = "~";
    }
}
