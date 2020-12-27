﻿using ERPCraft_Server.Models.DB.Almacen;
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
    }
}
