using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
