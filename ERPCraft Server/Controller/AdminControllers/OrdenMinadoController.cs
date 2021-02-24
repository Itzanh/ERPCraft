using ERPCraft_Server.Models.DB.Robots;
using ERPCraft_Server.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ERPCraft_Server.Controller.AdminControllers
{
    public static class OrdenMinadoController
    {
        public static string ordenesCommand(DBStorage db, string command, string message)
        {
            switch (command)
            {
                case "get":
                    {
                        return getOrdenesMinado(db, message);
                    }
                case "add":
                    {
                        return addOrdenMinado(db, message);
                    }
                case "addArray":
                    {
                        return addOrdenesMinado(db, message);
                    }
                case "update":
                    {
                        return updateOrdenesMinado(db, message);
                    }
                case "delete":
                    {
                        return deleteOrdenesMinado(db, message);
                    }
                case "getInventario":
                    {
                        return getOrdenMinadoInventario(db, message);
                    }
            }

            return "ERR";
        }

        private static string getOrdenesMinado(DBStorage db, string message)
        {
            OrdenMinadoQuery query;
            try
            {
                query = (OrdenMinadoQuery)JsonConvert.DeserializeObject(message, typeof(OrdenMinadoQuery));
            }
            catch (Exception) { return "ERR"; }
            if (query == null || !query.isValid())
                return "ERR";

            return JsonConvert.SerializeObject(db.getOrdenesDeMinado(query.estado, query.robot));
        }

        private static string addOrdenMinado(DBStorage db, string message)
        {
            OrdenMinado orden;
            try
            {
                orden = (OrdenMinado)JsonConvert.DeserializeObject(message, typeof(OrdenMinado));
            }
            catch (Exception) { return "ERR"; }
            if (orden == null || !orden.isValid())
                return "ERR";

            return db.addOrdenesDeMinado(orden) ? "OK" : "ERR";
        }

        private static string addOrdenesMinado(DBStorage db, string message)
        {
            List<OrdenMinado> ordenes;
            try
            {
                ordenes = (List<OrdenMinado>)JsonConvert.DeserializeObject(message, typeof(List<OrdenMinado>));
            }
            catch (Exception) { return "ERR"; }
            foreach (OrdenMinado orden in ordenes)
            {
                if (orden == null)
                    return "ERR";

                orden.descripcion = string.Empty;
                orden.unidadRecarga = '%';
                orden.energiaRecarga = 10;
                orden.chestSide = '1';

                if (!orden.isValid())
                    return "ERR";
            }

            return db.addOrdenesDeMinado(ordenes) ? "OK" : "ERR";
        }

        private static string updateOrdenesMinado(DBStorage db, string message)
        {
            OrdenMinado orden;
            try
            {
                orden = (OrdenMinado)JsonConvert.DeserializeObject(message, typeof(OrdenMinado));
            }
            catch (Exception) { return "ERR"; }
            if (orden == null || orden.id <= 0 || !orden.isValid())
                return "ERR";

            return db.updateOrdenesDeMinado(orden) ? "OK" : "ERR";
        }

        private static string deleteOrdenesMinado(DBStorage db, string message)
        {
            int idOrden;
            try
            {
                idOrden = Int32.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (idOrden <= 0)
                return "ERR";

            return db.deleteOrdenesDeMinado(idOrden) ? "OK" : "ERR";
        }

        private static string getOrdenMinadoInventario(DBStorage db, string message)
        {
            int idOrden;
            try
            {
                idOrden = Int32.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (idOrden <= 0)
                return "ERR";

            return JsonConvert.SerializeObject(db.getOrdenMinadoInventario(idOrden));
        }
    }
}
