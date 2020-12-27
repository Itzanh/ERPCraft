using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace serverHashes
{
    class WebSocketConnection : NetworkConnectionAbstract
    {
        /// <summary>
        /// IP address of the remote endpoint
        /// </summary>
        public override string addr
        {
            get { return _addr; }
        }
        private string _addr;
        /// <summary>
        /// TCP connection with a client.
        /// </summary>
        public TcpClient client;
        /// <summary>
        /// NetworkStream with the client.
        /// </summary>
        public NetworkStream stream;
        /// <summary>
        /// Sets whether if the client is currently connected
        /// </summary>
        private bool connected;

        /* SECURE CONNECTION (WSS) */

        /// <summary>
        /// Secure sockets layer connection with the client, if it's set to use it.
        /// </summary>
        private SslStream ssl;
        /// <summary>
        /// The NetworkStream or SslStream to send the information through, as the connection may be secure or insecure and may not change.
        /// </summary>
        public Stream s;
        /// <summary>
        /// Gets whether this connection is secured (by sending data over SSL) or not.
        /// </summary>
        public override bool isSecure
        {
            get { return this.ssl != null; }
        }
        /// <summary>
        /// object used to lock the receiveData method and use the socket thread-safely
        /// </summary>
        private readonly object lockReceiveData = new object();
        /// <summary>
        /// object used to lock the sendData method and use the socket thread-safely
        /// </summary>
        private readonly object lockSendData = new object();



        public WebSocketConnection()
        {
            this.connected = false;
        }

        public WebSocketConnection(TcpClient client, NetworkStream stream) : this()
        {
            this.client = client;
            this.stream = stream;

            this._addr = client.Client.RemoteEndPoint.ToString();
        }

        public override bool isConnected()
        {
            return this.connected && this.client.Connected;
        }

        public bool connect(bool isSecure = false)
        {
            try
            {
                this.client.NoDelay = true;

                if (isSecure)
                    this.s = ssl;
                else
                    this.s = stream;

                // read the request till finding out the 'Sec-WebSocket-Key' tag on the HTTP message
                byte[] bytes = new byte[3];
                int readData = 0;
                while (readData < 3)
                {
                    readData += this.s.Read(bytes, 0, bytes.Length);
                }
                if (!Regex.IsMatch(Encoding.UTF8.GetString(bytes), "^GET"))
                {
                    return false;
                }
                StringBuilder str = new StringBuilder(Encoding.UTF8.GetString(bytes));
                TextReader textReader = new StreamReader(this.s);
                string nextLine;
                do
                {
                    nextLine = textReader.ReadLine();
                    str.Append(nextLine + "\n");

                }
                while (nextLine == null || !nextLine.Equals(String.Empty));

                string secWebSocketKey = new Regex("Sec-WebSocket-Key: (.*)\n").Match(str.ToString()).Groups[1].Value.Trim();
                this.handShake(secWebSocketKey);
                this.connected = true;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public bool connect(X509Certificate2 certificate)
        {
            this.ssl = this.initializeSSL(this.client, certificate);
            if (this.ssl == null) return false;

            return this.connect(true);
        }

        private SslStream initializeSSL(TcpClient client, X509Certificate2 certificate)
        {
            SslStream sslStream = null;
            try
            {
                // A client has connected. Create the 
                // SslStream using the client's network stream.
                sslStream = new SslStream(
                    client.GetStream(), true);

                // Authenticate the server but don't require the client to authenticate.
                sslStream.AuthenticateAsServer(certificate, clientCertificateRequired: false, checkCertificateRevocation: true);

                // Set timeouts for the read and write to 5 seconds.
                sslStream.ReadTimeout = 60000;
                sslStream.WriteTimeout = 60000;
                // Read a message from the client.   
                return sslStream;
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine("Authentication failed - closing the connection.");
                if (sslStream != null)
                    sslStream.Close();
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        private byte[] readNetworkData(int length)
        {
            try
            {
                byte[] savedData = new byte[length];
                int read = 0;
                while (read < length)
                {
                    read += this.s.Read(savedData, read, (length - read));
                    if (read == 0)
                        return null;
                }
                return savedData;
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
            return null;
        }

        public override bool receiveData(out byte[] receivedData, out bool isBinary)
        {
            lock (lockReceiveData)
                try
                {
                    bool fin = false;

                    isBinary = false;
                    receivedData = null;

                    MemoryStream ms = new MemoryStream();

                    while (!fin)
                    {
                        // decodificar missatge
                        // vore si es text, sabent si el primer bit es 1 (te que ser igual o superior a 128 per a estar activat)
                        byte[] header = this.readNetworkData(1);
                        if (header == null)
                            return false;
                        int opcode = BitConverter.ToUInt16(new byte[2] { header[0], new byte() }, 0);
                        if (opcode >= 128)
                        {
                            fin = true;
                        }
                        else if (opcode < 128 || opcode == 0)
                        {
                            fin = false;
                        }

                        if (opcode == 129 || opcode == 1)
                        {
                            isBinary = false;
                        }
                        else if (opcode == 130 || opcode == 2)
                        {
                            isBinary = true;
                        }
                        else if (opcode == 136)
                        {
                            Console.WriteLine("*** A WEBSOCKET CONNECTION WAS CLOSED ***");
                            this.connected = false;
                            return false;
                        }
                        else if (opcode != 128 && opcode != 0)
                        {
                            Console.WriteLine("Unknown WebSocket OPCODE " + opcode);
                            this.connected = false;
                            return false;
                        }

                        // vore la longitud i aon hi ha que començar a llegir
                        // llegim el segon byte menys 128
                        byte[] len = this.readNetworkData(1);
                        if (len == null)
                            return false;
                        ushort longitud = BitConverter.ToUInt16(new byte[2] { len[0], new byte() }, 0);
                        longitud -= 128;
                        ulong tamanyDades = 0;
                        // si es menor o igual a 125, ixa es la longitud de la informacio enviada. 
                        // Començarem llegint el key a una distancia de 2 byes (el seguent byte)
                        if (longitud >= 0 && longitud <= 125)
                        {
                            tamanyDades = (ulong)longitud;
                        }
                        // si es 126, significa que els proxims 2 bytes tambe representen la longitud
                        else if (longitud == 126)
                        {
                            byte[] size = this.readNetworkData(2);
                            if (size == null)
                                return false;
                            Array.Reverse(size);
                            tamanyDades = BitConverter.ToUInt16(size, 0);
                        }
                        // si es 126, significa que els proxims 8 bytes tambe representen la longitud
                        else if (longitud == 127)
                        {
                            byte[] size = this.readNetworkData(8);
                            if (size == null)
                                return false;
                            Array.Reverse(size);
                            tamanyDades = BitConverter.ToUInt64(size, 0);
                        }

                        byte[] key = this.readNetworkData(4);

                        // creem un array per a la informacio a decodificar i per a la informacio decodificada,
                        // el tamany es el tamany de les dades rebudes, menys la primera posicio que se llig mes 4 bytes de key que van primer
                        byte[] encoded = this.readNetworkData((int)tamanyDades);
                        byte[] decoded = new byte[tamanyDades];
                        // iterar el array fent el XOR de cada uno dels elements del array key sequencialment
                        for (int i = 0; i < encoded.Length; i++)
                        {
                            decoded[i] = (byte)(encoded[i] ^ key[i % 4]);
                        }

                        ms.Write(decoded, 0, decoded.Length);
                    }

                    receivedData = ms.ToArray();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    receivedData = null;
                    isBinary = false;
                    return false;
                }
        }

        public override bool sendData(byte[] msgCodificat, bool isBinary)
        {
            lock (lockSendData)
                try
                {
                    int longit = msgCodificat.Length;
                    // representa la longitud de la capçalera de abans del missatge
                    int començamentMissatge = 0;
                    // representa  la capçalera de abans del missatge
                    byte[] capcalera = new byte[10];

                    // activar el primer bit de FIN
                    if (isBinary) // el opcode del binari
                        capcalera[0] = (byte)130;
                    else // FIN of text message
                        capcalera[0] = (byte)129;

                    // representar la longitud de les dades a enviar segons els bytes de longitud que represente la longitud
                    if (longit <= 125)
                    {
                        capcalera[1] = (byte)longit;
                        començamentMissatge = 2;
                    }
                    else if (longit > 125 && longit <= 65535)
                    {
                        capcalera[1] = (byte)126;
                        començamentMissatge = 4;
                        capcalera[2] = (byte)((longit >> 8) & 255);
                        capcalera[3] = (byte)(longit & 255);
                    }
                    else
                    {
                        capcalera[1] = (byte)127;
                        començamentMissatge = 10;
                        int j = 2;
                        for (int i = 56; i >= 0; i -= 8)
                        {
                            capcalera[j] = (byte)((longit >> i) & 255);
                            j++;
                        }
                    }

                    this.s.Write(capcalera, 0, començamentMissatge);
                    this.s.Write(msgCodificat, 0, msgCodificat.Length);

                    return true;
                }
                catch (Exception) { return false; }
        }

        public override void disconnect()
        {
            // tancar la connexio
            if (this.isSecure)
                this.ssl.Dispose();
            this.stream.Dispose();
            this.client.Dispose();
        }

        private void handShake(string secWebSocketKey)
        {
            const string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            string secWebSocketAccept = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(secWebSocketKey + guid)));
            StringBuilder str = new StringBuilder();
            str
                .Append("HTTP/1.1 101 Switching Protocols").Append(Environment.NewLine)
                .Append("Connection: Upgrade").Append(Environment.NewLine)
                .Append("Upgrade: websocket").Append(Environment.NewLine)
                .Append("Sec-WebSocket-Accept: ").Append(secWebSocketAccept)
                .Append(Environment.NewLine).Append(Environment.NewLine);
            byte[] resposta = Encoding.UTF8.GetBytes(str.ToString());

            this.s.Write(resposta, 0, resposta.Length);
        }
    }
}
