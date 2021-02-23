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
                case "getExistencias":
                    {
                        return getExistencias(db, message);
                    }
                case "getCrafteosArticuloUso":
                    {
                        return getCrafteosArticuloUso(db, message);
                    }
                case "getCrafteosArticuloResult":
                    {
                        return getCrafteosArticuloResult(db, message);
                    }
                case "getSmeltingArticuloUso":
                    {
                        return getSmeltingArticuloUso(db, message);
                    }
                case "getSmeltingArticuloResult":
                    {
                        return getSmeltingArticuloResult(db, message);
                    }
                // CRAFTEOS
                case "getCrafts":
                    {
                        return getCrafts(db, message);
                    }
                case "addCraft":
                    {
                        return addCraft(db, message);
                    }
                case "editCraft":
                    {
                        return editCraft(db, message);
                    }
                case "deleteCraft":
                    {
                        return deleteCraft(db, message);
                    }
                case "localizarCraft":
                    {
                        return localizarCraft(db);
                    }
                // HORNEOS
                case "getSmelting":
                    {
                        return getSmelting(db);
                    }
                case "addSmelting":
                    {
                        return addSmelting(db, message);
                    }
                case "editSmelting":
                    {
                        return editSmelting(db, message);
                    }
                case "deleteSmelting":
                    {
                        return deleteSmelting(db, message);
                    }
                case "localizarSmelting":
                    {
                        return localizarSmelting(db);
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

        private static string getExistencias(DBStorage db, string message)
        {
            short id;
            try
            {
                id = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (id <= 0)
                return "ERR";

            return JsonConvert.SerializeObject(db.getInventarioArticulo(id));
        }

        private static string getCrafteosArticuloUso(DBStorage db, string message)
        {
            short id;
            try
            {
                id = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (id <= 0)
                return "ERR";

            return JsonConvert.SerializeObject(db.getCrafteosByComponent(id));
        }

        private static string getCrafteosArticuloResult(DBStorage db, string message)
        {
            short id;
            try
            {
                id = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (id <= 0)
                return "ERR";

            return JsonConvert.SerializeObject(db.getCrafteosByResult(id));
        }

        private static string getSmeltingArticuloUso(DBStorage db, string message)
        {
            short id;
            try
            {
                id = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (id <= 0)
                return "ERR";

            return JsonConvert.SerializeObject(db.getSmeltingByComponent(id));
        }

        private static string getSmeltingArticuloResult(DBStorage db, string message)
        {
            short id;
            try
            {
                id = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (id <= 0)
                return "ERR";

            return JsonConvert.SerializeObject(db.getSmeltingByResult(id));
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

        // CRAFTS

        private static string getCrafts(DBStorage db, string message)
        {
            CrafteoQuery query;
            try
            {
                query = (CrafteoQuery)JsonConvert.DeserializeObject(message, typeof(CrafteoQuery));
            }
            catch (Exception) { return "ERR"; }
            if (query == null || query.offset < 0 || query.limit <= 0)
                return "ERR";

            return JsonConvert.SerializeObject(db.getCrafteos(query.offset, query.limit));
        }

        private static string addCraft(DBStorage db, string message)
        {
            Crafteo crafteo;
            try
            {
                crafteo = (Crafteo)JsonConvert.DeserializeObject(message, typeof(Crafteo));
            }
            catch (Exception) { return "ERR"; }
            if (crafteo == null || !crafteo.isValid())
                return "ERR";

            return db.addCrafteo(crafteo) ? "OK" : "ERR";
        }

        private static string editCraft(DBStorage db, string message)
        {
            Crafteo crafteo;
            try
            {
                crafteo = (Crafteo)JsonConvert.DeserializeObject(message, typeof(Crafteo));
            }
            catch (Exception) { return "ERR"; }
            if (crafteo == null || crafteo.id <= 0 || !crafteo.isValid())
                return "ERR";

            return db.updateCrafteo(crafteo) ? "OK" : "ERR";
        }

        private static string deleteCraft(DBStorage db, string message)
        {
            int id;
            try
            {
                id = Int32.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (id <= 0)
                return "ERR";

            return db.deleteCrafteo(id) ? "OK" : "ERR";
        }

        private static string localizarCraft(DBStorage db)
        {
            return JsonConvert.SerializeObject(db.getCrafteosHead());
        }

        // SMELTING

        private static string getSmelting(DBStorage db)
        {
            return JsonConvert.SerializeObject(db.getSmelting());
        }

        private static string addSmelting(DBStorage db, string message)
        {
            Smelting smelting;
            try
            {
                smelting = (Smelting)JsonConvert.DeserializeObject(message, typeof(Smelting));
            }
            catch (Exception) { return "ERR"; }
            if (smelting == null || !smelting.isValid())
                return "ERR";

            return db.addSmelting(smelting) ? "OK" : "ERR";
        }

        private static string editSmelting(DBStorage db, string message)
        {
            Smelting smelting;
            try
            {
                smelting = (Smelting)JsonConvert.DeserializeObject(message, typeof(Smelting));
            }
            catch (Exception) { return "ERR"; }
            if (smelting == null || smelting.id <= 0 || !smelting.isValid())
                return "ERR";

            return db.updateSmelting(smelting) ? "OK" : "ERR";
        }

        private static string deleteSmelting(DBStorage db, string message)
        {
            int id;
            try
            {
                id = Int32.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (id <= 0)
                return "ERR";

            return db.deleteSmelting(id) ? "OK" : "ERR";
        }

        private static string localizarSmelting(DBStorage db)
        {
            return JsonConvert.SerializeObject(db.getSmeltingHead());
        }

    }
}
