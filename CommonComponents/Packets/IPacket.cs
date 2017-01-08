namespace CommonComponents.Packets
{
    public interface IPacket
    {
        string Header { get; }
        string Body { get; }
        string ToString();
        void SetValue(string value, string port);
    }
}
