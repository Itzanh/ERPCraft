/**
MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using ERPCraft_Server.Controller;
using ERPCraft_Server.Models.DB;
using ERPCraft_Server.Models.Server;
using ERPCraft_Server.Storage;
using serverHashes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ERPCraft_Server
{
    class Program
    {
        /// <summary>
        /// Initial program configuration. Database connection parameters.
        /// The rest of the settings, are stored in the database.
        /// </summary>
        public static Configuracion config;
        /// <summary>
        /// Containts all the function to do CRUD operations on the database.
        /// </summary>
        public static DBStorage db;
        /// <summary>
        /// Current settings loaded from the database.
        /// </summary>
        public static Ajuste ajuste;
        /// <summary>
        /// Publication/Subscriptions system for the websocket client to be aware of new changes on the server without 
        /// needing to periodically or manually refresh.
        /// This object emits permits to send events to a topis by its topis name, and allows to NetEventIO clients to subscribe and unsubscribe to the topics,
        /// so they can be notified about the changes the need to be aware of.
        /// </summary>
        public static PubSub websocketPubSub;

        static void Main(string[] args)
        {
            Console.WriteLine("*** ERPCRAFT - Control your Minecraft world from the real world, anywhere, realtime. ***");
            Console.WriteLine("[[[ https://github.com/Itzanh/ERPCraft ]]]");
            Console.WriteLine("");
            config = Configuracion.readConfiguracion();
            db = new DBStorage(config);
            db.createDB();
            initDb();

            websocketPubSub = new PubSub(new string[] { "robots", "articulos", "articulosImg", "electrico", "generador", "bateria", "ordenMinado", "almacen", "drones", "usuarios", "config", "apiKeys", "servers" });

            // gestio administrativa remota. crear el objecte que controal la gestio remota, 
            // conectar el listener, i rebre les connexions el altre fil
            Thread threadWS = new Thread(new ThreadStart(WaitForWebSocket.Run));
            threadWS.Start();

            // accept OC communication
            GameController.Run();
        }

        private static void initDb()
        {
            // establecer como offline todos los servers y robots
            db.updateAllOffline();
            // intentar cargar ajustes, si no es posible, guardar unos nuevos ajustes y cargar
            ajuste = db.getAjuste();
            if (ajuste == null)
            {
                ajuste = new Ajuste();
                ajuste.defaultAjustes();
                short id = db.addAjuste(ajuste);
                db.activarAjuste(id);
                ajuste = db.getAjuste();
            }
            // si no existe el usuario con ID 1, crearlo con los datos por defecto
            db.initUsers();
        }
    }
}


