using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChessLite.Network
{
    class Server
    {
        public readonly int Port;
        private IPEndPoint endPoint;
        public string MyName { get; private set; }
        public string OpponentName { get; private set; }
        private Socket tcpSocket;

        public Server(string IP, int port, string name)
        {
            Port = port;
            endPoint = new IPEndPoint(IPAddress.Any, Port);
            tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            MyName = name;
        }

        public void SetName()
        {
            var listener = tcpSocket.Accept();
            var buffer = new byte[64];
            var data = new StringBuilder();
            int size;
            size = listener.Receive(buffer);
            data.Append(Encoding.UTF8.GetString(buffer, 0, size));
            OpponentName = data.ToString();

            buffer = Encoding.UTF8.GetBytes(MyName);
            listener.Send(buffer);
            listener.Shutdown(SocketShutdown.Both);
            listener.Close();
        }

        public void InitServer()
        {
            tcpSocket.Bind(endPoint);
            tcpSocket.Listen(1);
            SetName();
        }

        public event EventHandler<byte[]> AnsweredEvent;
        public Func<byte[]> Answer;
        public void StartServer()
        {
            while (true)
            {
                var listener = tcpSocket.Accept();
                var buffer = new byte[4];
                listener.Receive(buffer);
                AnsweredEvent(this, buffer);
                listener.Send(Answer?.Invoke());
                listener.Shutdown(SocketShutdown.Both);
                listener.Close();
            }
        }
    }
}
