using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using zkemkeeper;

namespace crashio
{
    class Crashio
    {
        private const int _BUFFER_SIZE = 2048;
        private const string _IP_BIOMETRICO = "192.168.1.201";
        private const string _IP_WS = "127.0.0.1";
        private const int _PORT = 8080;

        private CZKEM _reader = new CZKEM();
        private static Socket _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        private static readonly List<Socket> _clients = new List<Socket>();
        private static readonly byte[] _buffer = new byte[_BUFFER_SIZE];

        #region Dispositivo Biometrico
        private bool ConnectBiometric(string ip)
        {
            bool connection = _reader.Connect_Net(ip, Convert.ToInt32(4370));

            if (connection)
            {
                bool events = _reader.RegEvent(1, 65535);

                if (events)
                {
                    _reader.OnVerify += new _IZKEMEvents_OnVerifyEventHandler(GetUser);
                    _reader.OnHIDNum += new _IZKEMEvents_OnHIDNumEventHandler(GetCard);
                    _reader.OnAttTransactionEx += new _IZKEMEvents_OnAttTransactionExEventHandler(GetType);
                }
            }

            return connection;
        }

        private void GetUser(int user)
        {

        }

        private void GetCard(int card)
        {

        }

        private void GetType(string a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k)
        {
            // (d == 1 ? "HUELLA" : "TARJETA"); d=tipo, a=usuario
        }

        #endregion

        #region Servidor BootPark
        public static void server() {
            Crashio a = new Crashio();
            bool connect = a.ConnectBiometric(_IP_BIOMETRICO);

            if (connect)
            {
                Console.WriteLine("Configuración del servidor");
                Console.WriteLine("Conexión: ws://" + _IP_WS + ":" + _PORT + "/service");
                Console.WriteLine("Estado: " + connect);
                _server.Bind(new IPEndPoint(IPAddress.Any, _PORT));
                _server.Listen(128);
                _server.BeginAccept(null, 0, OnAccept, null);
                Console.WriteLine("Configuración del servidor completa");
                Console.Read();
            }
            else {
                Console.WriteLine("Dispositivo Biometrico sin conexión");
            }
        }

        private static void CloseAllSockets()
        {
            foreach (Socket socket in _clients)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            _server.Close();
        }

        private static void OnAccept(IAsyncResult result)
        {
            Socket _client = null;
            try
            {
                if (_server != null && _server.IsBound)
                {
                    _client = _server.EndAccept(result);
                }
                if (_client != null)
                {
                    /* Protocolo de sincronización y gestión del Cliente Socket */
                    _clients.Add(_client);
                    _client.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, OnRecieveData, _client);
                    Console.WriteLine("Cliente conectado, en espera de petición");
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                if (_server != null && _server.IsBound)
                {
                    _server.BeginAccept(null, 0, OnAccept, null);
                }
            }
        }

        private static void OnRecieveData(IAsyncResult res) {
            Socket current = (Socket)res.AsyncState;
            int received = current.EndReceive(res);
            byte[] recBuf = new byte[received];
            Array.Copy(_buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Console.WriteLine("Received Text: " + text);


        }
        #endregion

        public static void Main()
        {

            Console.Title = "Crash.io BootPark";
            server();
            Console.ReadLine();
            CloseAllSockets();

        }
    }
}



/*

    Socket current = null;
            int received;

            try
            {
                current = (Socket)res.AsyncState;
                received = current.EndReceive(res);

                byte[] recBuf = new byte[received];
                Array.Copy(_buffer, recBuf, received);
                string text = Encoding.ASCII.GetString(recBuf);

                Console.WriteLine("Texto recibido: " + text);

                // Apertura Validación -> Mensaje entrante
                if (text.ToLower() == "get time") // Hora de la solicitud
                {
                    Console.WriteLine("Obtención de tiempo");
                    byte[] data = Encoding.ASCII.GetBytes(DateTime.Now.ToLongTimeString());
                    current.Send(data);
                    Console.WriteLine("Hora enviada al cliente");
                }
                else if (text.ToLower() == "exit") // Cuando el cliente quiere terminar apropiadamente
                {
                    // Apagar siempre antes de cerrar
                    current.Shutdown(SocketShutdown.Both);
                    current.Close();
                    _clients.Remove(current);
                    Console.WriteLine("Cliente desconectado");
                    return;
                }
                else
                {
                    Console.WriteLine("El texto es una solicitud no válida");
                    byte[] data = Encoding.ASCII.GetBytes("Petición invalida");
                    current.Send(data);
                    Console.WriteLine("Advertencia al enviar");
                }
                // Cierre Validación
            }
            catch (SocketException e)
            {
                Console.WriteLine("El cliente se desconectó la fuerza: " + e);
                current.Close();
                _clients.Remove(current);
                return;
            }
            finally {
                current.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, OnRecieveData, current);
            }  

*/
