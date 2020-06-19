using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightServer.Models
{
    public interface ITCPClient
    {
        string Connect(string ip, int port);
        void Write(string command);
        string Read(); // blocking call
        string Disconnect();
        bool IsConnect();
    }
}
