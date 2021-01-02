using ERPCraft_Server.Models.DB;
using ERPCraft_Server.Storage;
using Newtonsoft.Json;
using System;

namespace ERPCraft_Server.Controller.AdminControllers
{
    public static class AjustesController
    {
        public static string ajustesCommand(DBStorage db, string command, string message)
        {
            switch (command)
            {
                case "get":
                    {
                        return getAjustes(db);
                    }
                case "add":
                    {
                        return addAjuste(db, message);
                    }
                case "update":
                    {
                        return updateAjuste(db, message);
                    }
                case "delete":
                    {
                        return deleteAjuste(db, message);
                    }
                case "activar":
                    {
                        return activarAjuste(db, message);
                    }
                case "limpiar":
                    {
                        return ejecutarLimpieza(db);
                    }
            }

            return "ERR";
        }

        private static string getAjustes(DBStorage db)
        {
            return JsonConvert.SerializeObject(db.getAjustes());
        }

        private static string addAjuste(DBStorage db, string message)
        {
            Ajuste ajuste;
            try
            {
                ajuste = (Ajuste)JsonConvert.DeserializeObject(message, typeof(Ajuste));
            }
            catch (Exception) { return "ERR"; }
            if (ajuste == null || !ajuste.isValid())
                return "ERR";

            return "" + db.addAjuste(ajuste);
        }

        private static string updateAjuste(DBStorage db, string message)
        {
            Ajuste ajuste;
            try
            {
                ajuste = (Ajuste)JsonConvert.DeserializeObject(message, typeof(Ajuste));
            }
            catch (Exception) { return "ERR"; }
            if (ajuste == null || ajuste.id <= 0 || !ajuste.isValid())
                return "ERR";

            return db.updateAjuste(ajuste) ? "OK" : "ERR";
        }

        private static string deleteAjuste(DBStorage db, string message)
        {
            short id;
            try
            {
                id = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (id <= 0)
                return "ERR";

            return db.deleteAjuste(id) ? "OK" : "ERR";
        }

        private static string activarAjuste(DBStorage db, string message)
        {
            short id;
            try
            {
                id = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (id <= 0)
                return "ERR";

            bool ok = db.activarAjuste(id);
            if (!ok)
                return "ERR";

            Program.ajuste = db.getAjuste();
            return "OK";
        }

        private static string ejecutarLimpieza(DBStorage db)
        {
            db.limpiar();
            return "OK";
        }

        // API KEY

        public static string apiKeyCommand(DBStorage db, string command, string message)
        {
            switch (command)
            {
                case "get":
                    {
                        return getApiKeys(db);
                    }
                case "add":
                    {
                        return addApiKeys(db, message);
                    }
                case "reset":
                    {
                        return resetApiKey(db, message);
                    }
                case "delete":
                    {
                        return deleteApiKey(db, message);
                    }
            }

            return "ERR";
        }

        private static string getApiKeys(DBStorage db)
        {
            return JsonConvert.SerializeObject(db.getApiKeys());
        }

        private static string addApiKeys(DBStorage db, string message)
        {
            if (message.Length == 0)
                return "ERR";
            db.addApiKey(message);
            return "OK";
        }

        private static string resetApiKey(DBStorage db, string message)
        {
            short id;
            try
            {
                id = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (id <= 0)
                return "ERR";

            return db.resetClaveDeApi(id) ? "OK" : "ERR";
        }

        private static string deleteApiKey(DBStorage db, string message)
        {
            short id;
            try
            {
                id = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (id <= 0)
                return "ERR";

            return db.deleteClaveDeApi(id) ? "OK" : "ERR";
        }

        // SERVERS

        public static string serverCommand(DBStorage db, string command, string message)
        {
            switch (command)
            {
                case "get":
                    {
                        return getServers(db);
                    }
                case "add":
                    {
                        return addServer(db, message);
                    }
                case "update":
                    {
                        return updateServer(db, message);
                    }
                case "delete":
                    {
                        return deleteServer(db, message);
                    }
            }

            return "ERR";
        }

        private static string getServers(DBStorage db)
        {
            return JsonConvert.SerializeObject(db.getServers());
        }

        private static string addServer(DBStorage db, string message)
        {
            Server server;
            try
            {
                server = (Server)JsonConvert.DeserializeObject(message, typeof(Server));
            }
            catch (Exception) { return "ERR"; }
            if (server == null || !server.isValid())
                return "ERR";

            return db.addServer(server) ? "OK" : "ERR";
        }

        private static string updateServer(DBStorage db, string message)
        {
            Server server;
            try
            {
                server = (Server)JsonConvert.DeserializeObject(message, typeof(Server));
            }
            catch (Exception) { return "ERR"; }
            if (server == null || !server.isValid())
                return "ERR";

            return db.updateServer(server) ? "OK" : "ERR";
        }

        private static string deleteServer(DBStorage db, string message)
        {
            Guid guid;
            try
            {
                guid = Guid.Parse(message);
            }
            catch (Exception) { return "ERR"; }

            return db.deleteServer(guid) ? "OK" : "ERR";
        }
    }
}
