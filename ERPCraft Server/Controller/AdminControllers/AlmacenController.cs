using ERPCraft_Server.Models.DB.Almacen;
using ERPCraft_Server.Storage;
using Newtonsoft.Json;
using System;

namespace ERPCraft_Server.Controller.AdminControllers
{
    public static class AlmacenController
    {
        public static string almacenCommand(DBStorage db, string command, string message)
        {
            switch (command)
            {
                case "get":
                    {
                        return getAlmacenes(db);
                    }
                case "add":
                    {
                        return addAlmacen(db, message);
                    }
                case "edit":
                    {
                        return editAlmacen(db, message);
                    }
                case "delete":
                    {
                        return deleteAlmacen(db, message);
                    }
                case "inventario":
                    {
                        return getInventario(db, message);
                    }
                case "name":
                    {
                        return nameAlmacen(db, message);
                    }
                case "localizar":
                    {
                        return localizarAlmacen(db);
                    }
                case "getNotificaciones":
                    {
                        return getNotificaciones(message, db);
                    }
                case "addNotificacion":
                    {
                        return addNotificaciones(message, db);
                    }
                case "deleteNotificacion":
                    {
                        return deleteNotificaciones(message, db);
                    }
                case "getAE2StorageCells":
                    {
                        return getAE2StorageCells(db, message);
                    }
                case "addAE2StorageCell":
                    {
                        return addAE2StorageCell(db, message);
                    }
                case "deleteAE2StorageCell":
                    {
                        return deleteAE2StorageCell(db, message);
                    }
            }

            return "ERR";
        }

        private static string getAlmacenes(DBStorage db)
        {
            return JsonConvert.SerializeObject(db.getAlmacenes());
        }

        private static string addAlmacen(DBStorage db, string message)
        {
            Almacen almacen;
            try
            {
                almacen = (Almacen)JsonConvert.DeserializeObject(message, typeof(Almacen));
            }
            catch (Exception) { return "ERR"; }
            if (almacen == null || !almacen.isValid())
                return "ERR";

            return db.addAlmacen(almacen) ? "OK" : "ERR";
        }

        private static string editAlmacen(DBStorage db, string message)
        {
            Almacen almacen;
            try
            {
                almacen = (Almacen)JsonConvert.DeserializeObject(message, typeof(Almacen));
            }
            catch (Exception) { return "ERR"; }
            if (almacen == null || almacen.id <= 0 || !almacen.isValid())
                return "ERR";

            return db.updateAlmacen(almacen) ? "OK" : "ERR";
        }

        private static string deleteAlmacen(DBStorage db, string message)
        {
            short id;
            try
            {
                id = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (id <= 0)
                return "ERR";

            return db.deleteAlmacen(id) ? "OK" : "ERR";
        }

        private static string getInventario(DBStorage db, string message)
        {
            short idAlmacen;
            try
            {
                idAlmacen = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (idAlmacen <= 0)
                return "ERR";

            return JsonConvert.SerializeObject(db.getInventarioAlmacen(idAlmacen));
        }

        private static string nameAlmacen(DBStorage db, string message)
        {
            short id;
            try
            {
                id = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (id <= 0)
                return "ERR";

            return db.getAlmacenName(id);
        }

        private static string localizarAlmacen(DBStorage db)
        {
            return JsonConvert.SerializeObject(db.localizarAlmacenes());
        }

        private static string getNotificaciones(string message, DBStorage db)
        {
            short idAlmacen;
            try
            {
                idAlmacen = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (idAlmacen <= 0)
                return "ERR";

            return JsonConvert.SerializeObject(db.getNotificacionesAlmacen(idAlmacen));
        }

        private static string addNotificaciones(string message, DBStorage db)
        {
            AlmacenInventarioNotificacion notificacion;
            try
            {
                notificacion = (AlmacenInventarioNotificacion)JsonConvert.DeserializeObject(message, typeof(AlmacenInventarioNotificacion));
            }
            catch (Exception) { return "ERR"; }
            if (notificacion == null || !notificacion.isValid())
                return "ERR";

            return db.addNotificacionAlmacen(notificacion) ? "OK" : "ERR";
        }

        private static string deleteNotificaciones(string message, DBStorage db)
        {
            AlmacenInventarioNotificacionDelete notificacion;
            try
            {
                notificacion = (AlmacenInventarioNotificacionDelete)JsonConvert.DeserializeObject(message, typeof(AlmacenInventarioNotificacionDelete));
            }
            catch (Exception) { return "ERR"; }
            if (notificacion == null || !notificacion.isValid())
                return "ERR";

            return db.deleteNotificacionAlmacen(notificacion.idAlmacen, notificacion.id) ? "OK" : "ERR";
        }

        private static string getAE2StorageCells(DBStorage db, string message)
        {
            short id;
            try
            {
                id = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (id <= 0)
                return "ERR";

            return JsonConvert.SerializeObject(db.getStrorageCells(id));
        }

        private static string addAE2StorageCell(DBStorage db, string message)
        {
            AE2StorageCell storageCell;
            try
            {
                storageCell = (AE2StorageCell)JsonConvert.DeserializeObject(message, typeof(AE2StorageCell));
            }
            catch (Exception) { return "ERR"; }
            if (storageCell == null || storageCell.idAlmacen <= 0 || storageCell.tier <= 0)
                return "ERR";

            return db.addStorageCell(storageCell) ? "OK" : "ERR";
        }

        private static string deleteAE2StorageCell(DBStorage db, string message)
        {
            AE2StorageCellDelete storageCell;
            try
            {
                storageCell = (AE2StorageCellDelete)JsonConvert.DeserializeObject(message, typeof(AE2StorageCellDelete));
            }
            catch (Exception) { return "ERR"; }
            if (storageCell == null || storageCell.idAlmacen <= 0 || storageCell.id <= 0)
                return "ERR";

            return db.deleteStorageCell(storageCell) ? "OK" : "ERR";
        }
    }
}
