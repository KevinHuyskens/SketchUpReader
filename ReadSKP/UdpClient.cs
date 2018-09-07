using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ReadSKP
{
    class UdpClient
    {

        public bool DataSend;

        public void SendData(List<string>skpData)
        {
            byte[] data = new byte[1024];
            //string input, stringData;
            IPEndPoint ipep = new IPEndPoint(
                            IPAddress.Parse("127.0.0.1"), 9050);

            Socket server = new Socket(AddressFamily.InterNetwork,
                           SocketType.Dgram, ProtocolType.Udp);


            foreach (string s in skpData)
            {
                data = Encoding.ASCII.GetBytes(s);
                server.SendTo(data, data.Length, SocketFlags.None, ipep);
            }

            data = Encoding.ASCII.GetBytes("endInstance");
            server.SendTo(data, data.Length, SocketFlags.None, ipep);

            DataSend = true;
            //IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            //EndPoint Remote = (EndPoint)sender;

            // data = new byte[1024];
            // int recv = server.ReceiveFrom(data, ref Remote);

            /*Console.WriteLine("Message received from {0}:", Remote.ToString());
            Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));*/

            /*while (true)
            {
                input = Console.ReadLine();
                if (input == "exit")
                    break;
                server.SendTo(Encoding.ASCII.GetBytes(input), Remote);
                data = new byte[1024];
                recv = server.ReceiveFrom(data, ref Remote);
                stringData = Encoding.ASCII.GetString(data, 0, recv);
                Console.WriteLine(stringData);
            }
            Console.WriteLine("Stopping client");*/
            server.Close();
        }
    }
}
