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

        public ClientTcp()
        {
            this.tcpclnt = new TcpClient();
            connect = false;
        }

        public string Connect(string ip, int port)
        {
            if (IsConnect()) { return "Ok"; }
            try
            {
                tcpclnt.Connect(ip, port);
                this.stm = this.tcpclnt.GetStream();
                Write("data\n");
                connect = true;
            }
            catch (Exception)
            {
                connect = false;
                return "There is a problem with connecting to the server";
            }
            return "Ok";
        }
        public string Disconnect()
        {
            if (!IsConnect()) { return "Ok"; }
            try
            {
                // Close networkStream.
                tcpclnt.GetStream().Close();
            }// The networkStream was already closed or something else happen.
            catch (Exception) 
            {
                return "There is a problen of closing NetworkStream";
            }
            try
            {
                // Close tcpClient.
                tcpclnt.Close();
            }// The tcpclnt was already closed or something else happen.
            catch (Exception) 
            {
                return "There is a problen of closing TcpClient";
            }
            connect = false;
            tcpclnt = null;
            return "Ok";
        }

        public string Read()
        {
            if (tcpclnt == null)
            {
                return "ERR";
            }
            // Time out of 10 seconds.
            tcpclnt.ReceiveTimeout = 10000;
            this.stm.ReadTimeout = 10000;
            // Only if the ReceiveBufferSize not empty so we want to convert the message to string and return it.
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
            return "ERR";
        }
        public void Write(string command)
        {
            try
            {
                this.stm = this.tcpclnt.GetStream();
                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(command);
                stm.Write(ba, 0, ba.Length);
            }
            catch (Exception)
            {
                Console.WriteLine("The sever is stoped");
                Thread.Sleep(2000);
            }
        }
        public bool IsConnect()
        {
            return connect;
        }
    }
}