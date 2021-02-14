using System;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using ERPCraft_Server.Models.DB.Robots;
using ERPCraft_Server.Models.DB.Almacen;
using ERPCraft_Server.Storage;
using ERPCraft_Server.Models.DB.Drones;
using ERPCraft_Server.Models.DB;

namespace ERPCraft_Server.Controller
{
    /// <summary>
    /// Esta clase permite comunicarse con los servidores del juego para controlar los componentes de la partida.
    /// Este servidor será capaz de conectarse al computer servidor de la partida, intercambiar datos a través de la tarjeta Internet, haciendo a su vez 
    /// que el servidor se comunique con los componentes OpenComputers de su misma red, de forma que parezca que todos los componentes tienen acceso a
    /// Internet, o que este servidor está integrado con todos los componentes del juego.
    /// </summary>
    public static class GameController
    {
        public class ServerController
        {
            public TcpClient server;
            public Guid uuid;
            public List<DeviceController> robots;
            public DBStorage db;
            public DateTime lastPong;

            public ServerController(TcpClient server)
            {
                this.server = server;
                this.robots = new List<DeviceController>();
                this.db = new DBStorage(Program.config);
                this.lastPong = DateTime.Now;
            }

            public bool existeDevice(string uuid)
            {
                for (int i = 0; i < this.robots.Count; i++)
                    if (this.robots[i].uuid.Equals(uuid))
                        return true;

                return false;
            }

            public bool pongDevice(string uuid)
            {
                for (int i = 0; i < this.robots.Count; i++)
                {
                    if (this.robots[i].uuid.Equals(uuid))
                    {
                        this.robots[i].lastConnection = DateTime.Now;
                        return true;
                    }
                }

                return false;
            }
        }

        public class DeviceController
        {
            public string uuid;
            public DateTime lastConnection;

            public DeviceController(string uuid)
            {
                this.uuid = uuid;
                this.lastConnection = DateTime.Now;
            }
        }

        private static List<ServerController> servers = new List<ServerController>();

        public static void Run()
        {
            TcpListener listener = new TcpListener(System.Net.IPAddress.Any, Program.ajuste.puertoOC);
            listener.Start();

            Thread limpieza = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    pingRobots();
                }
            }));
            limpieza.Start();

            Console.WriteLine("Waiting connections from OpenComputers servers");
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    clientLoop(client);
                }));
                thread.Start();
            }
        }

        private static void clientLoop(TcpClient client)
        {
            ServerController server = new ServerController(client);
            Console.WriteLine("El client s'ha conectat");
            client.ReceiveTimeout = 60000;
            client.SendTimeout = 60000;

            if (!handshake(server, client))
            {
                client.Close();
                return;
            }
            servers.Add(server);
            server.db.updateServerOnline(server.uuid, true);

            while (client.Connected)
            {
                string mensaje = recibirMensaje(client);
                if (mensaje == null)
                    return;
                else
                    procesarComando(client, server, mensaje);
            }

            Console.WriteLine("El client s'ha desconectat");
        }

        private static bool handshake(ServerController srv, TcpClient client)
        {
            try
            {
                byte[] msgSrv = new byte[36];
                client.GetStream().Read(msgSrv, 0, 36);
                string server = Encoding.ASCII.GetString(msgSrv);

                byte[] msgKey = new byte[36];
                client.GetStream().Read(msgKey, 0, 36);
                string apiKey = Encoding.ASCII.GetString(msgKey);
                Guid key = Guid.Parse(apiKey);

                // funcionalidad de registrar un servidor automáticamente
                if (server.Equals("AUTOREGISTER                        "))
                {
                    byte[] password = new byte[36];
                    client.GetStream().Read(password, 0, 36);
                    if (!autoregisterServer(key, Encoding.ASCII.GetString(password), srv.db))
                        return false;
                    srv.uuid = key;
                    return true;
                }
                else
                {
                    srv.uuid = Guid.Parse(server);
                    return srv.db.serverExists(srv.uuid) && Program.db.existsApiKey(key) && Program.db.onlineApiKey(key);
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); return false; }
        }

        /// <summary>
        /// funcionalidad de registrar un servidor automáticamente
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static bool autoregisterServer(Guid srvId, string password, DBStorage db)
        {
            // comprobar que el registro automático está habilitado en este servidor
            if (!Program.ajuste.permitirAutoregistrar)
                return false;

            // buscar si la contraseña es correcta
            if (!Usuario.verifyHash(Program.ajuste.pwd, Program.ajuste.salt + password.Trim(), Program.ajuste.iteraciones))
                return false;

            // registrar el servidor en la base de datos
            return db.addServer(srvId);
        }

        private static string recibirMensaje(TcpClient client)
        {
            try
            {
                byte[] msgSize = new byte[2];
                if (client.GetStream().Read(msgSize, 0, 2) == 0)
                    return null;
                short size = BitConverter.ToInt16(msgSize, 0);

                byte[] msg = new byte[size];
                client.GetStream().Read(msg, 0, size);
                return Encoding.ASCII.GetString(msg);
            }
            catch (Exception) { return null; }
        }

        private static void pingRobots()
        {
            DateTime limit = DateTime.Now;

            foreach (ServerController server in servers)
            {
                enviarMensaje(server.server, "PING$$");
                foreach (DeviceController robot in server.robots)
                {
                    if ((limit - robot.lastConnection).TotalSeconds > Program.ajuste.pingInterval)
                    {
                        Console.WriteLine("ENVIANT PING...");
                        enviarMensaje(server.server, "MSG$$" + robot.uuid + "&&" + "PING");
                    }
                }
            }

            limit = DateTime.Now;
            Thread.Sleep(Program.ajuste.pingInterval * 1000);

            for (int i = servers.Count - 1; i >= 0; i--)
            {
                ServerController server = servers[i];

                // HANDLE SERVER TIMEOUT. DISCONNECT EVERYTHING!
                if ((limit - server.lastPong).TotalSeconds > Program.ajuste.timeout)
                {
                    Console.WriteLine("SERVER TIMEOUT!");
                    server.db.updateServerOnline(server.uuid, false);
                    for (int j = server.robots.Count - 1; j >= 0; j--)
                    {
                        DeviceController robot = server.robots[j];
                        server.db.updateRobotOffline(robot.uuid, true);
                        server.db.stopRobotOrdenMinado(Guid.Parse(robot.uuid));
                        server.robots.RemoveAt(j);
                    }
                    server.db.Dispose();
                    server.server.Close();
                    server.server.Dispose();
                    servers.RemoveAt(i);
                    break;
                }

                for (int j = server.robots.Count - 1; j >= 0; j--)
                {
                    DeviceController robot = server.robots[j];
                    if ((limit - robot.lastConnection).TotalSeconds > Program.ajuste.timeout)
                    {
                        Console.WriteLine("TIMEOUT!");
                        server.db.updateRobotOffline(robot.uuid);
                        server.db.stopRobotOrdenMinado(Guid.Parse(robot.uuid));
                        enviarMensaje(server.server, "DESC$$" + robot.uuid);
                        server.robots.RemoveAt(j);
                    }
                }
            }

        }

        private static bool enviarMensaje(TcpClient client, string mensaje)
        {
            try
            {
                byte[] msg = Encoding.ASCII.GetBytes(mensaje);
                client.GetStream().Write(BitConverter.GetBytes(msg.Length), 0, sizeof(byte));
                client.GetStream().Write(msg, 0, msg.Length);
                return true;
            }
            catch (Exception) { return false; }
        }

        private static void procesarComando(TcpClient client, ServerController server, string mensaje)
        {
            // CONN$$3e54c72a-c68d-4b31-9920-94de189bea63
            // MSG$$0c29a857-6269-4efa-b175-ba1059b277a0&&robOnline--
            Console.WriteLine(mensaje);
            // no se encuentra el separador, o no hay espacio para un UUID después del separador
            if (mensaje.IndexOf("$$") == -1 || (mensaje.IndexOf("$$") + 2 + 36 > mensaje.Length && (!mensaje.Equals("PONG$$"))))
                return;
            // cortar comando y UUID
            string cmd = mensaje.Substring(0, mensaje.IndexOf("$$"));
            string uuid = mensaje.Equals("PONG$$") ? string.Empty : mensaje.Substring(mensaje.IndexOf("$$") + 2, 36);
            switch (cmd)
            {
                case "CONN":
                    {
                        if (!server.existeDevice(uuid))
                            server.robots.Add(new DeviceController(uuid));

                        break;
                    }
                case "MSG":
                    {
                        if (mensaje.IndexOf("&&") == -1)
                            return;
                        string msg = mensaje.Substring(mensaje.IndexOf("&&") + 2);
                        procesarMensaje(client, server, uuid, msg);
                        break;
                    }
                case "PONG":
                    {
                        server.lastPong = DateTime.Now;
                        break;
                    }
            }
        }

        private static void procesarMensaje(TcpClient client, ServerController server, string uuid, string mensaje)
        {
            server.pongDevice(uuid);
            if (mensaje.IndexOf("--") == -1)
                return;

            string cmd = mensaje.Substring(0, mensaje.IndexOf("--"));
            string msg = mensaje.Substring(mensaje.IndexOf("--") + 2);
            switch (cmd)
            {
                case "PONG":
                    {
                        break;
                    }
                /*  ROBOTS */
                case "robRegister":
                    {
                        comandoRobRegister(server.uuid, uuid, msg, server.db);
                        break;
                    }
                case "robOnline":
                    {
                        comandoRobOnline(uuid, msg, server.db);
                        break;
                    }
                case "robInventario":
                    {
                        comandoRobInventario(uuid, msg, server.db);
                        break;
                    }
                case "robClearIneventario":
                    {
                        comandoRobClerInventario(uuid, server.db);
                        break;
                    }
                case "robLog":
                    {
                        comandoRobLog(uuid, msg, server.db);
                        break;
                    }
                case "robGPS":
                    {
                        comandoRobGps(uuid, msg, server.db);
                        break;
                    }
                case "robBat":
                    {
                        comandoRobBat(uuid, msg, server.db);
                        break;
                    }
                // ÓRDENES DE MINADO
                case "robOrdenMinado":
                    {
                        comandoOrdenMinado(client, uuid, server.db);
                        break;
                    }
                case "robOrdenMinadoUpdate":
                    {
                        comandoOrdenMinadoUpdate(uuid, msg, server.db);
                        break;
                    }
                case "robOrdenMinadoInv":
                    {
                        comandoOrdenMinadoInventario(uuid, msg, server.db);
                        break;
                    }
                case "robOrdenMinadoFin":
                    {
                        comandoOrdenMinadoFinalizar(uuid, server.db);
                        break;
                    }
                /* RED ELECTRICA */
                case "batInit":
                    {
                        comandoBateriaInit(uuid, msg, server.db);
                        break;
                    }
                case "batSet":
                    {
                        comandoBateriaSet(uuid, msg, server.db);
                        break;
                    }
                case "genSet":
                    {
                        comandoGeneradorSet(uuid, msg, server.db);
                        break;
                    }
                /* ALMACÉN */
                case "almacenSetInv":
                    {
                        comandoAlmacenSetInventario(uuid, msg, server.db);
                        break;
                    }
                /*  DRONES */
                case "drnOnline":
                    {
                        comandoDroneOnline(uuid, msg, server.db);
                        break;
                    }
                case "drnInventario":
                    {
                        comandoDroneInventario(uuid, msg, server.db);
                        break;
                    }
                case "drnClearIneventario":
                    {
                        comandoDroneClerInventario(uuid, server.db);
                        break;
                    }
                case "drnLog":
                    {
                        comandoDroneLog(uuid, msg, server.db);
                        break;
                    }
                case "drnGPS":
                    {
                        comandoDroneGps(uuid, msg, server.db);
                        break;
                    }
                /* FABRICACIÓN */
                case "getOrdFab":
                    {
                        comandoGetOrdenesFabricacion(uuid, server.db, server.server);
                        break;
                    }
                case "setOrdFab":
                    {
                        comandoSetOrdenFabricacion(uuid, server.db, msg);
                        break;
                    }
            }
        }

        /*  ROBOTS */

        private static void comandoRobRegister(Guid serveruuid, string uuid, string mensaje, DBStorage db)
        {
            // server_password;name;num_slots;energia_act;total_ene;generador_upgrade;generador_items;gps_upgrade;pos_x;pos_y;pos_z
            Console.WriteLine("uuid " + uuid + " msg " + mensaje);
            string[] data = mensaje.Split(';');
            if (data.Length != 11)
                return;

            try
            {
                db.autoRegisterRobot(serveruuid, data[0], uuid, data[1], Int16.Parse(data[2]), Int32.Parse(data[3]), Int32.Parse(data[4]), data[5].Equals("1"),
                    Int16.Parse(data[6]), data[7].Equals("1"), Int16.Parse(data[8]), Int16.Parse(data[9]), Int16.Parse(data[10]));
            }
            catch (Exception) { return; }
        }

        private static void comandoRobOnline(string uuid, string mensaje, DBStorage db)
        {
            // name;energia_act;total_ene;pos_x,pos_y;pos_z
            Console.WriteLine("uuid " + uuid + " msg " + mensaje);
            string[] data = mensaje.Split(';');
            if (data.Length != 6)
                return;

            try
            {
                db.updateRobotOnline(uuid, data[0],
                Int32.Parse(data[1]), Int32.Parse(data[2]), Int16.Parse(data[3]), Int16.Parse(data[4]), Int16.Parse(data[5]));
            }
            catch (Exception) { return; }
        }

        private static void comandoRobInventario(string uuid, string msg, DBStorage db)
        {
            // articulo:cantidad;articulo:cantidad;articulo:cantidad...
            string[] data = msg.Split(';');
            List<RobotInventarioSet> setInventario = new List<RobotInventarioSet>();

            foreach (string slot in data)
            {
                string[] slotInfo = slot.Split('@');
                if (slotInfo.Length == 2)
                {
                    short cantidad = Int16.Parse(slotInfo[1]);
                    if (slotInfo[0].Length == 0 || cantidad < 0)
                        continue;
                    try
                    {
                        setInventario.Add(new RobotInventarioSet(slotInfo[0], cantidad));
                    }
                    catch (Exception) { }
                }
            }

            db.setRobotInventario(Guid.Parse(uuid), setInventario);
        }

        private static void comandoRobClerInventario(string uuid, DBStorage db)
        {
            db.clearRobotInventario(Guid.Parse(uuid));
        }

        private static void comandoRobLog(string uuid, string msg, DBStorage db)
        {
            int separatorPos = msg.IndexOf("@@");
            if (separatorPos == -1)
                return;

            db.addRobotLog(Guid.Parse(uuid), new RobotLog(msg.Substring(0, separatorPos), msg.Substring(separatorPos + 2)));
        }

        private static void comandoRobGps(string uuid, string msg, DBStorage db)
        {
            string[] data = msg.Split('@');
            if (data.Length == 3)
            {
                try
                {
                    db.addRobotGps(Guid.Parse(uuid), Int16.Parse(data[0]), Int16.Parse(data[1]), Int16.Parse(data[2]));
                }
                catch (Exception) { return; }
            }
        }

        private static void comandoRobBat(string uuid, string msg, DBStorage db)
        {
            int energiaActual;
            try
            {
                energiaActual = Int32.Parse(msg);
            }
            catch (Exception) { return; }

            db.updateRobotBateria(uuid, energiaActual);
        }

        // ÓRDENES DE MINADO

        private static void comandoOrdenMinado(TcpClient client, string uuid, DBStorage db)
        {
            OrdenMinado orden = db.getRobotOrdenMinado(Guid.Parse(uuid));
            if (orden == null)
            {
                enviarMensaje(client, "IDLE");
                return;
            }
            short energiaRecarga;
            if (orden.unidadRecarga == '=') // la energía en la que el robot parará a recargar está en valor absoluto
                energiaRecarga = orden.energiaRecarga;
            else if (orden.unidadRecarga == '%') // la energía en la que el robot parará a recargar está en porcentaje. calcular en base a la energía total del robot
                energiaRecarga = (short)((db.getRobot(Guid.Parse(uuid)).totalEnergia / ((short)100)) * orden.energiaRecarga);
            else
                return;

            string str = "MSG$$" + uuid + "&&" + orden.size + ";" + orden.posX + ";" + orden.posY + ";" + orden.posZ + ";" + orden.facing + ";"
                + orden.gpsX + ";" + orden.gpsY + ";" + orden.gpsZ + ";" + energiaRecarga + ";" + orden.modoMinado + ";" + (orden.shutdown ? 1 : 0);
            enviarMensaje(client, str);
        }

        private static void comandoOrdenMinadoUpdate(string uuid, string msg, DBStorage db)
        {
            string[] data = msg.Split(';');
            if (data.Length != 4)
                return;

            try
            {
                db.updateProgresoOrdenDeMinado(Guid.Parse(uuid), Int16.Parse(data[0]), Int16.Parse(data[1]), Int16.Parse(data[2]), Int16.Parse(data[3]));
            }
            catch (Exception) { return; }
        }

        private static void comandoOrdenMinadoFinalizar(string uuid, DBStorage db)
        {
            db.finRobotOrdenMinado(Guid.Parse(uuid));
        }

        /* RED ELECTRICA */

        private static void comandoBateriaInit(string uuid, string msg, DBStorage db)
        {
            string[] data = msg.Split('@');
            if (data.Length != 2)
                return;

            try
            {
                db.updateBateriaRedElectrica(Guid.Parse(uuid), Int64.Parse(data[0]), Int64.Parse(data[1]));
            }
            catch (Exception) { return; }
        }

        private static void comandoBateriaSet(string uuid, string msg, DBStorage db)
        {
            try
            {
                db.updateBateriaRedElectrica(Guid.Parse(uuid), Int64.Parse(msg));
            }
            catch (Exception) { return; }
        }

        private static void comandoGeneradorSet(string uuid, string msg, DBStorage db)
        {
            bool activado;
            if (msg == "on")
                activado = true;
            else if (msg == "off")
                activado = false;
            else
                return;

            db.updateGeneradorRedElectrica(Guid.Parse(uuid), activado);
        }

        public static bool enviarComando(string uuid, string mensaje)
        {
            foreach (ServerController server in servers)
            {
                foreach (DeviceController device in server.robots)
                {
                    if (device.uuid.Equals(uuid))
                    {
                        enviarMensaje(server.server, "MSG$$" + device.uuid + "&&CMD--" + mensaje);
                        return true;
                    }
                }
            }
            return false;
        }

        private static void comandoOrdenMinadoInventario(string uuid, string msg, DBStorage db)
        {
            // articulo@cantidad;articulo@cantidad;articulo@cantidad...
            string[] data = msg.Split(';');
            List<OrdenMinadoInventarioSet> setInventario = new List<OrdenMinadoInventarioSet>();

            foreach (string slot in data)
            {
                string[] slotInfo = slot.Split('@');
                if (slotInfo.Length == 2)
                {
                    short cantidad = Int16.Parse(slotInfo[1]);
                    try
                    {
                        setInventario.Add(new OrdenMinadoInventarioSet(slotInfo[0], cantidad));
                    }
                    catch (Exception) { }
                }
            }

            db.setOrdenMinadoInventario(db.getRobotOrdenMinado(Guid.Parse(uuid)).id, setInventario);
        }

        private static void comandoAlmacenSetInventario(string uuid, string msg, DBStorage db)
        {
            // articulo@cantidad;articulo@cantidad;articulo@cantidad...
            string[] data = msg.Split(';');
            List<AlmacenInventarioSet> setInventario = new List<AlmacenInventarioSet>();

            foreach (string slot in data)
            {
                string[] slotInfo = slot.Split('@');
                if (slotInfo.Length == 2)
                {
                    short cantidad = Int16.Parse(slotInfo[1]);
                    if (slotInfo[0].Length == 0 || cantidad < 0)
                        continue;
                    try
                    {
                        setInventario.Add(new AlmacenInventarioSet(slotInfo[0], cantidad));
                    }
                    catch (Exception) { }
                }
            }

            db.setInventarioAlmacen(db.getAlmacenId(Guid.Parse(uuid)), setInventario);
        }

        /*  DRONES */

        private static void comandoDroneOnline(string uuid, string mensaje, DBStorage db)
        {
            // name;energia_act;total_ene;pos_x,pos_y;pos_z
            string[] data = mensaje.Split(';');
            if (data.Length != 6)
                return;

            try
            {
                db.updateDroneOnline(uuid, data[0],
                Int16.Parse(data[1]), Int16.Parse(data[2]), Int16.Parse(data[3]), Int16.Parse(data[4]), Int16.Parse(data[5]));
            }
            catch (Exception) { return; }
        }

        private static void comandoDroneInventario(string uuid, string msg, DBStorage db)
        {
            // articulo:cantidad;articulo:cantidad;articulo:cantidad...
            string[] data = msg.Split(';');
            List<DroneInventarioSet> setInventario = new List<DroneInventarioSet>();

            foreach (string slot in data)
            {
                string[] slotInfo = slot.Split('@');
                if (slotInfo.Length == 2)
                {
                    short cantidad = Int16.Parse(slotInfo[1]);
                    if (slotInfo[0].Length == 0 || cantidad < 0)
                        continue;
                    try
                    {
                        setInventario.Add(new DroneInventarioSet(slotInfo[0], cantidad));
                    }
                    catch (Exception) { }
                }
            }

            db.setDroneInventario(Guid.Parse(uuid), setInventario);
        }

        private static void comandoDroneClerInventario(string uuid, DBStorage db)
        {
            db.clearDroneInventario(Guid.Parse(uuid));
        }

        private static void comandoDroneLog(string uuid, string msg, DBStorage db)
        {
            int separatorPos = msg.IndexOf("@@");
            if (separatorPos == -1)
                return;

            db.addDroneLog(Guid.Parse(uuid), new DroneLog(msg.Substring(0, separatorPos), msg.Substring(separatorPos + 2)));
        }

        private static void comandoDroneGps(string uuid, string msg, DBStorage db)
        {
            string[] data = msg.Split('@');
            if (data.Length == 3)
            {
                try
                {
                    db.addDroneGps(Guid.Parse(uuid), Int16.Parse(data[0]), Int16.Parse(data[1]), Int16.Parse(data[2]));
                }
                catch (Exception) { return; }
            }
        }

        /* FABRICACIÓN */

        private static void comandoGetOrdenesFabricacion(string uuid, DBStorage db, TcpClient server)
        {
            Fabricacion fabricacion = db.getFabricacion(Guid.Parse(uuid));
            if (fabricacion == null)
                return;

            OrdenFabricacion ordenFabricacion = db.getNextOrdenFabricacion(fabricacion.idAlmacen, fabricacion.id);
            if (ordenFabricacion == null)
            {
                enviarMensaje(server, "MSG$$" + uuid + "&&IDLE");
                return;
            }

            if (fabricacion.tipo == 'R')
                robotGetOrdenesFabricacion(uuid, db, server, fabricacion, ordenFabricacion);
            else if (fabricacion.tipo == 'A')
                meGetOrdenesFabricacion(db, ordenFabricacion, uuid, server);
        }

        private static void robotGetOrdenesFabricacion(string uuid, DBStorage db, TcpClient server, Fabricacion fabricacion, OrdenFabricacion ordenFabricacion)
        {
            StringBuilder str = new StringBuilder();
            // id;cantidad;c=craft/s=smelt,
            str.Append("MSG$$").Append(uuid).Append("&&").Append(ordenFabricacion.id).Append(";").Append(ordenFabricacion.cantidad).Append(";");

            if (ordenFabricacion.idCrafteo != null)
            {
                str.Append("C");
                Crafteo crafteo = db.getCrafteo((int)ordenFabricacion.idCrafteo);
                // minecraft_id:cant;minecraft_id:cant;minecraft_id:cant;minecraft_id:cant;minecraft_id:cant;minecraft_id:cant... (x9)
                if (crafteo.idArticuloSlot1 != null)
                    str.Append(";").Append(db.getArticulo((short)crafteo.idArticuloSlot1).minecraftID).Append(";").Append(crafteo.cantidadArticuloSlot1);
                else
                    str.Append(";;").Append(0);
                if (crafteo.idArticuloSlot2 != null)
                    str.Append(";").Append(db.getArticulo((short)crafteo.idArticuloSlot2).minecraftID).Append(";").Append(crafteo.cantidadArticuloSlot2);
                else
                    str.Append(";;").Append(0);
                if (crafteo.idArticuloSlot3 != null)
                    str.Append(";").Append(db.getArticulo((short)crafteo.idArticuloSlot3).minecraftID).Append(";").Append(crafteo.cantidadArticuloSlot3);
                else
                    str.Append(";;").Append(0);
                if (crafteo.idArticuloSlot4 != null)
                    str.Append(";").Append(db.getArticulo((short)crafteo.idArticuloSlot4).minecraftID).Append(";").Append(crafteo.cantidadArticuloSlot4);
                else
                    str.Append(";;").Append(0);
                if (crafteo.idArticuloSlot5 != null)
                    str.Append(";").Append(db.getArticulo((short)crafteo.idArticuloSlot5).minecraftID).Append(";").Append(crafteo.cantidadArticuloSlot5);
                else
                    str.Append(";;").Append(0);
                if (crafteo.idArticuloSlot6 != null)
                    str.Append(";").Append(db.getArticulo((short)crafteo.idArticuloSlot6).minecraftID).Append(";").Append(crafteo.cantidadArticuloSlot6);
                else
                    str.Append(";;").Append(0);
                if (crafteo.idArticuloSlot7 != null)
                    str.Append(";").Append(db.getArticulo((short)crafteo.idArticuloSlot7).minecraftID).Append(";").Append(crafteo.cantidadArticuloSlot7);
                else
                    str.Append(";;").Append(0);
                if (crafteo.idArticuloSlot8 != null)
                    str.Append(";").Append(db.getArticulo((short)crafteo.idArticuloSlot8).minecraftID).Append(";").Append(crafteo.cantidadArticuloSlot8);
                else
                    str.Append(";;").Append(0);
                if (crafteo.idArticuloSlot9 != null)
                    str.Append(";").Append(db.getArticulo((short)crafteo.idArticuloSlot9).minecraftID).Append(";").Append(crafteo.cantidadArticuloSlot9);
                else
                    str.Append(";;").Append(0);
            }
            else if (ordenFabricacion.idSmelting != null)
            {
                str.Append("S;");
                Smelting smelting = db.getSmelting((int)ordenFabricacion.idSmelting);
                // minecraft_id;cant
                str.Append(db.getArticulo(smelting.idArticuloEntrada).minecraftID).Append(";").Append(fabricacion.hornoSide);
            }
            else
            {
                return;
            }

            str.Append(";" + fabricacion.cofreSide);
            db.setOrdenFabricacionReady(ordenFabricacion.id, ordenFabricacion.idAlmacen, ordenFabricacion.idFabricacion);
            Console.WriteLine(str.ToString());
            enviarMensaje(server, str.ToString());
        }

        private static void meGetOrdenesFabricacion(DBStorage db, OrdenFabricacion ordenFabricacion, string uuid, TcpClient server)
        {
            string articuloMinecraftID;
            if (ordenFabricacion.idCrafteo != null)
            {
                Crafteo crafteo = db.getCrafteo((int)ordenFabricacion.idCrafteo);
                if (crafteo == null)
                    return;
                Articulo articulo = db.getArticulo(crafteo.idArticuloResultado);
                if (articulo == null)
                    return;
                articuloMinecraftID = articulo.minecraftID;
            }
            else if (ordenFabricacion.idSmelting != null)
            {
                Smelting smelting = db.getSmelting((int)ordenFabricacion.idSmelting);
                if (smelting == null)
                    return;
                Articulo articulo = db.getArticulo(smelting.idArticuloResultado);
                if (articulo == null)
                    return;
                articuloMinecraftID = articulo.minecraftID;
            }
            else
            {
                return;
            }
            enviarMensaje(server, "MSG$$" + uuid + "&&" + ordenFabricacion.id + ";" + articuloMinecraftID + ";" + ordenFabricacion.cantidad);
        }

        private static void comandoSetOrdenFabricacion(string uuid, DBStorage db, string msg)
        {
            Fabricacion fabricacion = db.getFabricacion(Guid.Parse(uuid));
            if (fabricacion == null)
                return;

            string[] data = msg.Split(';');
            if (data.Length != 3)
                return;

            int id;
            try
            {
                id = Int32.Parse(data[0]);
            }
            catch (Exception) { return; }
            if (id <= 0)
                return;

            if (data[1].Equals("OK"))
            {
                db.setResultadoOrdenFabricaicon(id, fabricacion.idAlmacen, fabricacion.id, true);
            }
            else if (data[1].Equals("ERR"))
            {
                short errorCode;
                try
                {
                    errorCode = Int16.Parse(data[2]);
                }
                catch (Exception) { return; }
                if (errorCode <= 0)
                    return;

                db.setResultadoOrdenFabricaicon(id, fabricacion.idAlmacen, fabricacion.id, false, errorCode);
            }
            else
            {
                return;
            }
        }
    }
}
