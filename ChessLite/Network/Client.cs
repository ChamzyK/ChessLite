using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChessLite.Network
{
    class Client
    {
        private IPEndPoint endPoint;
        public string MyName { get; private set; }
        public string OpponentName { get; private set; }

        private Socket tcpSocket;
        public Client(string IP, int port, string name)
        {
            endPoint = new IPEndPoint(IPAddress.Parse(IP), port);
            tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            MyName = name;
        }

        public void ConnectToServer()
        {
            tcpSocket.Connect(endPoint);
            var data = Encoding.UTF8.GetBytes(MyName);
            tcpSocket.Send(data);

            var buffer = new byte[64];
            int size;
            var answer = new StringBuilder();

            do
            {
                size = tcpSocket.Receive(buffer);
                answer.Append(Encoding.UTF8.GetString(buffer, 0, size));
            }
            while (tcpSocket.Available > 0);

            OpponentName = answer.ToString();
            tcpSocket.Shutdown(SocketShutdown.Both);
            tcpSocket.Close();

        }

        public event EventHandler<byte[]> AnsweredEvent;
        public async void Send(byte[] message)
        {
            tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpSocket.Connect(endPoint);
            tcpSocket.Send(message);

            var buffer = new byte[4];
            await Task.Run(() => tcpSocket.Receive(buffer));
            AnsweredEvent?.Invoke(this, buffer); 
            tcpSocket.Shutdown(SocketShutdown.Both);
            tcpSocket.Close();
        }
    }
}
