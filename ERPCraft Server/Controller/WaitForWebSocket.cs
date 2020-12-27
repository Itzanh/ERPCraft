using ERPCraft_Server.Models.DB;
using Newtonsoft.Json;
using serverHashes;
using System;
using System.Net.Sockets;
using System.Threading;

namespace ERPCraft_Server.Controller
{
    public static class WaitForWebSocket
    {
        public static void Run()
        {
            // start listening connections
            TcpListener server = new TcpListener(System.Net.IPAddress.Any, Program.ajuste.puertoWeb);
            server.Start();

            // generate options for the future socket connections
            NetEventIOServer_Options options = new NetEventIOServer_Options(onWebSocketPasswordLogin, onWebSocketTokenLogin,
                Program.websocketPubSub.onSubscribe, Program.websocketPubSub.onUnsubscribe, Program.websocketPubSub.onUnsubscribeAll);

            Console.WriteLine("WebSocket server listening.");

            // wait for more TCP clients
            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                Console.WriteLine("WebSocket client connected :D");

                new Thread(new ThreadStart(() =>
                {
                    WebSocketConnection conn = new WebSocketConnection(client, stream);
                    if (!conn.connect())
                    {
                        return;
                    }

                    NetEventIO clientIO = new NetEventIO(conn, options);
                    new Thread(new ThreadStart(clientIO.Run)).Start();

                    clientIO.onDisconnect = onWebSocketDisconnect;
                    AdminController.addEventListeners(clientIO);
                })).Start();
            }
        }

        public static bool onWebSocketPasswordLogin(NetEventIO client, string passwd)
        {
            Console.WriteLine("processant login... ");

            // recuperar la informacio del hash enmagatzemat
            UsuarioLogin login;
            try
            {
                login = (UsuarioLogin)JsonConvert.DeserializeObject(passwd, typeof(UsuarioLogin));
            }
            catch (Exception) { return false; }
            if (login == null || !login.isValid()) return false;

            Usuario usuario = Program.db.getUsuario(login.name);
            if (usuario == null)
                return false;

            // comprovar si les contrasenyes coincideixen
            bool loggedIn = Usuario.verifyHash(usuario.pwd, (usuario.salt + login.pwd), usuario.iteraciones);

            if (loggedIn)
            {
                Console.WriteLine("login fet");
                client.db = new Storage.DBStorage(Program.config);
                client.db.updateUsuarioOnline(usuario.id);
            }
            else
            {
                Console.WriteLine("login no fet");

            }

            return loggedIn;
        }

        public static bool onWebSocketTokenLogin(NetEventIO client, string token, string addr)
        {
            Console.WriteLine("processant token... " + token + " " + addr);
            return false;
        }

        public static void onWebSocketDisconnect(NetEventIO sender)
        {
            Program.websocketPubSub.onUnsubscribeAll(sender);
            if (sender.db != null)
                sender.db.Dispose();
            Thread.CurrentThread.Abort();
        }
    }
}
