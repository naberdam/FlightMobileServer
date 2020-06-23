using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Web;
using System.Text;
using System.Threading;

namespace FlightServer.Models
{
    public class ClientTcp : ITCPClient
    {
        TcpClient tcpclnt;
        NetworkStream stm;
        bool connect;
        // Const string that represents all exceptions that we want to send to client.
        private const string ConnectionMessage = "There is a problem with connecting" +
            " to the server";
        private const string TcpClientCloseMessage = "There is a problen of closing" +
            " TcpClient";
        private const string NetworStreamCloseMessage = "There is a problen of closing" +
            " NetworkStream";
        private const string SendTextToServerInBeginning = "data\n";
        private const string EverythingIsGood = "Ok";
        private const string ERRMessage = "ERR";


        public ClientTcp()
        {
            this.tcpclnt = new TcpClient();
            connect = false;
        }
        // Function that connects to server.
        public string Connect(string ip, int port)
        {
            if (IsConnect()) { return EverythingIsGood; }
            try
            {
                tcpclnt.Connect(ip, port);
                this.stm = this.tcpclnt.GetStream();
                Write(SendTextToServerInBeginning);
                connect = true;
            }
            catch (Exception)
            {
                connect = false;
                return ConnectionMessage;
            }
            return EverythingIsGood;
        }
        // Function that disconnect from server.
        public string Disconnect()
        {
            if (!IsConnect()) { return EverythingIsGood; }
            try
            {
                // Close networkStream.
                tcpclnt.GetStream().Close();
            }
            // The networkStream was already closed or something else happen.
            catch (Exception) 
            {
                return NetworStreamCloseMessage;
            }
            try
            {
                // Close tcpClient.
                tcpclnt.Close();
            }
            // The tcpclnt was already closed or something else happen.
            catch (Exception) 
            {
                return TcpClientCloseMessage;
            }
            connect = false;
            tcpclnt = null;
            return EverythingIsGood;
        }
        // Function that read from server.
        public string Read()
        {
            if (tcpclnt == null)
            {
                return ERRMessage;
            }
            // Time out of 10 seconds.
            tcpclnt.ReceiveTimeout = 10000;
            this.stm.ReadTimeout = 10000;
            // Only if the ReceiveBufferSize not empty so we want to convert the 
            // Message to string and return it.
            if (tcpclnt.ReceiveBufferSize > 0)
            {
                byte[] bb = new byte[tcpclnt.ReceiveBufferSize];
                int k = this.stm.Read(bb, 0, 100);
                string massage = "";
                for (int i = 0; i < k; i++)
                {
                    massage += (Convert.ToChar(bb[i]));
                }
                return massage;
            }
            return ERRMessage;
        }
        // Function that writes to server.
        public void Write(string command)
        {
            this.stm = this.tcpclnt.GetStream();
            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(command);
            stm.Write(ba, 0, ba.Length);
        }
        // Function that returns the status connection of server.
        public bool IsConnect()
        {
            return connect;
        }
    }
}