using ERPCraft_Server.Models.DB.Almacen;
using ERPCraft_Server.Storage;
using Newtonsoft.Json;
using System;

namespace ERPCraft_Server.Controller.AdminControllers
{
    public static class MovimientosAlmacenController
    {
        public static string movimientosAlmacenCommands(DBStorage db, string command, string message)
        {
            switch (command)
            {
                case "get":
                    {
                        return getMovimientosAlmacen(db, message);
                    }
                case "search":
                    {
                        return searchMovimientosAlmacen(db, message);
                    }
                case "add":
                    {
                        return addMovimientoAlmacen(db, message);
                    }
                case "update":
                    {
                        return updateMovimientoAlmacen(db, message);
                    }
            }

            return "ERR";
        }

        private static string getMovimientosAlmacen(DBStorage db, string message)
        {
            MovimientoAlmacenGet query;
            try
            {
                query = (MovimientoAlmacenGet)JsonConvert.DeserializeObject(message, typeof(MovimientoAlmacenGet));
            }
            catch (Exception) { return "ERR"; }
            if (query == null || query.offset < 0 || query.limit <= 0)
                return "ERR";

            return JsonConvert.SerializeObject(db.getMovimientosAlmacen(query.offset, query.limit));
        }

        private static string searchMovimientosAlmacen(DBStorage db, string message)
        {
            MovimientoAlmacenQuery query;
            try
            {
                query = (MovimientoAlmacenQuery)JsonConvert.DeserializeObject(message, typeof(MovimientoAlmacenQuery));
            }
            catch (Exception) { return "ERR"; }
            if (query == null || query.offset < 0 || query.limit <= 0)
                return "ERR";

            return JsonConvert.SerializeObject(db.getMovimientosAlmacen(query));
        }

        private static string addMovimientoAlmacen(DBStorage db, string message)
        {
            MovimientoAlmacen movimientoAlmacen;
            try
            {
                movimientoAlmacen = (MovimientoAlmacen)JsonConvert.DeserializeObject(message, typeof(MovimientoAlmacen));
            }
            catch (Exception) { return "ERR"; }
            if (movimientoAlmacen == null || !movimientoAlmacen.isValid())
                return "ERR";

            // inicializar movimiento de almacen
            movimientoAlmacen.dateAdd = DateTime.Now;
            movimientoAlmacen.origen = 'M';

            return db.addMovimientoAlmacen(movimientoAlmacen) ? "OK" : "ERR";
        }

        private static string updateMovimientoAlmacen(DBStorage db, string message)
        {
            MovimientoAlmacen movimiento;
            try
            {
                movimiento = (MovimientoAlmacen)JsonConvert.DeserializeObject(message, typeof(MovimientoAlmacen));
            }
            catch (Exception) { return "ERR"; }
            if (movimiento == null || movimiento.id <= 0 || movimiento.almacen <= 0 || movimiento.descripcion == null)
                return "ERR";

            return db.updateMovimientoAlmacen(movimiento) ? "OK" : "ERR";
        }
    }
}
