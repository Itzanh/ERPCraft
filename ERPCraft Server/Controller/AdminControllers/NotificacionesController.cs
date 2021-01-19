using ERPCraft_Server.Models.DB.Almacen;
using ERPCraft_Server.Storage;
using Newtonsoft.Json;
using System;

namespace ERPCraft_Server.Controller.AdminControllers
{
    public static class NotificacionesController
    {
        public static string notificacionesCommands(DBStorage db, string command, string message)
        {
            switch (command)
            {
                case "count":
                    {
                        return countNotificaciones(db);
                    }
                case "get":
                    {
                        return getNotificaciones(db);
                    }
                case "leidas":
                    {
                        return notificacionesLeidas(db);
                    }
            }

            return "ERR";
        }

        private static string countNotificaciones(DBStorage db)
        {
            return "" + db.countNotificaciones();
        }

        private static string getNotificaciones(DBStorage db)
        {
            return JsonConvert.SerializeObject(db.getNotificaciones());
        }

        private static string notificacionesLeidas(DBStorage db)
        {
            return db.notificacionesLeidas() ? "OK" : "ERR";
        }
    }
}
