using System;
using ERPCraft_Server.Models.DB;
using ERPCraft_Server.Storage;
using Newtonsoft.Json;
using serverHashes;

namespace ERPCraft_Server.Controller.AdminControllers
{
    public static class ArticulosController
    {
        public static string articulosCommand(DBStorage db, string command, string message)
        {
            switch (command)
            {
                case "get":
                    {
                        return getArticulos(db);
                    }
                case "search":
                    {
                        return searchArticulos(db, message);
                    }
                case "add":
                    {
                        return addArticulo(db, message);
                    }
                case "edit":
                    {
                        return editArticulos(db, message);
                    }
                case "delete":
                    {
                        return deleteArticulo(db, message);
                    }
                case "deleteImg":
                    {
                        return deleteArticuloImg(db, message);
                    }
                case "name":
                    {
                        return nameArticulo(db, message);
                    }
                case "localizar":
                    {
                        return localizarArticulos(db);
                    }
            }

            return "ERR";
        }

        private static string searchArticulos(DBStorage db, string message)
        {
            return JsonConvert.SerializeObject(db.searchArticulos(message));
        }

        public static void setArticuloImg(DBStorage db, BinaryMessage msg)
        {
            short id;
            try
            {
                id = Int16.Parse(msg.command);
            }
            catch (Exception) { return; }
            if (id <= 0 || msg.message.Length >= 32767)
                return;

            db.setArticuloImg(id, msg.message);
        }

        private static string deleteArticuloImg(DBStorage db, string message)
        {
            short id;
            try
            {
                id = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (id <= 0)
                return "ERR";

            return db.deleteArticuloImg(id) ? "OK" : "ERR";
        }

        private static string nameArticulo(DBStorage db, string message)
        {
            short id;
            try
            {
                id = Int16.Parse(message);
            }
            catch (Exception) { return ""; }
            if (id <= 0)
                return "";

            return db.getArticuloName(id);
        }

        private static string localizarArticulos(DBStorage db)
        {
            return JsonConvert.SerializeObject(db.localizarArticulos());
        }

        public static void getArticuloImg(NetEventIO client, string message)
        {
            short id;
            try
            {
                id = Int16.Parse(message);
            }
            catch (Exception) { return; }
            if (id <= 0)
                return;

            byte[] img = client.db.getArticuloImg(id);

            client.emit("artImg", "" + id, img);
        }

        private static string getArticulos(DBStorage db)
        {
            return JsonConvert.SerializeObject(db.getArticulos());
        }

        private static string addArticulo(DBStorage db, string message)
        {
            Articulo articulo;
            try
            {
                articulo = (Articulo)JsonConvert.DeserializeObject(message, typeof(Articulo));
            }
            catch (Exception) { return "ERR"; }
            if (articulo == null || !articulo.isValid())
                return "ERR";

            return db.addArticulo(articulo) ? "OK" : "ERR";
        }

        private static string editArticulos(DBStorage db, string message)
        {
            Articulo articulo;
            try
            {
                articulo = (Articulo)JsonConvert.DeserializeObject(message, typeof(Articulo));
            }
            catch (Exception) { return "ERR"; }
            if (articulo == null || articulo.id <= 0 || !articulo.isValid())
                return "ERR";

            return db.updateArticulo(articulo) ? "OK" : "ERR";
        }

        private static string deleteArticulo(DBStorage db, string message)
        {
            short id;
            try
            {
                id = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (id <= 0)
                return "ERR";

            return db.deleteArticulo(id) ? "OK" : "ERR";
        }

    }
}
