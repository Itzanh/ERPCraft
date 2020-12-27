using System;
using ERPCraft_Server.Models.DB.Drones;
using ERPCraft_Server.Storage;
using Newtonsoft.Json;

namespace ERPCraft_Server.Controller.AdminControllers
{
    public static class DroneController
    {
        public static string droneCommands(DBStorage db, string command, string message)
        {
            switch (command)
            {
                case "get":
                    {
                        return getDrones(db);
                    }
                case "search":
                    {
                        return searchDrones(db, message);
                    }
                case "add":
                    {
                        return addDrone(db, message);
                    }
                case "update":
                    {
                        return updateDrone(db, message);
                    }
                case "delete":
                    {
                        return deleteDrone(db, message);
                    }
                case "getGPS":
                    {
                        return getDroneGPS(db, message);
                    }
                case "getInventario":
                    {
                        return getDroneInventario(db, message);
                    }
                case "getLog":
                    {
                        return getDroneLogs(db, message);
                    }
                case "clearLogs":
                    {
                        return clearLogs(db, message);
                    }
                case "clearLogsBetween":
                    {
                        return clearLogsBetween(db, message);
                    }
            }

            return "ERR";
        }

        private static string getDrones(DBStorage db)
        {
            return JsonConvert.SerializeObject(db.getDrones());
        }

        private static string searchDrones(DBStorage db, string message)
        {
            return JsonConvert.SerializeObject(db.searchDrones(message));
        }

        private static string addDrone(DBStorage db, string message)
        {
            Drone drone;
            try
            {
                drone = (Drone)JsonConvert.DeserializeObject(message, typeof(Drone));
            }
            catch (Exception) { return "ERR"; }
            if (drone == null || !drone.isValid())
                return "ERR";

            return "" + db.addDrone(drone);
        }

        private static string updateDrone(DBStorage db, string message)
        {
            Drone drone;
            try
            {
                drone = (Drone)JsonConvert.DeserializeObject(message, typeof(Drone));
            }
            catch (Exception) { return "ERR"; }
            if (drone == null || drone.id <= 0 || !drone.isValid())
                return "ERR";

            return db.updateDrone(drone) ? "OK" : "ERR";
        }

        private static string deleteDrone(DBStorage db, string message)
        {
            short droneId;
            try
            {
                droneId = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (droneId <= 0)
                return "ERR";

            return db.deleteDrone(droneId) ? "OK" : "ERR";
        }

        private static string getDroneGPS(DBStorage db, string message)
        {
            // parsear JSON con offset y limit
            DroneGpsQuery query;
            try
            {
                query = (DroneGpsQuery)JsonConvert.DeserializeObject(message, typeof(DroneGpsQuery));
            }
            catch (Exception) { return "ERR"; }
            if (query == null || !query.isValid())
                return "ERR";

            return JsonConvert.SerializeObject(db.getDroneGPS(query.droneId, query.offset, query.limit));
        }

        private static string getDroneInventario(DBStorage db, string message)
        {
            short droneId;
            try
            {
                droneId = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (droneId <= 0)
                return "ERR";

            return JsonConvert.SerializeObject(db.getDroneInventario(droneId));
        }

        private static string getDroneLogs(DBStorage db, string message)
        {
            DroneLogQuery query;
            try
            {
                query = (DroneLogQuery)JsonConvert.DeserializeObject(message, typeof(DroneLogQuery));
            }
            catch (Exception) { return "ERR"; }
            if (query == null || !query.isValid())
                return "ERR";

            return JsonConvert.SerializeObject(db.getDroneLogs(query));
        }

        private static string clearLogs(DBStorage db, string message)
        {
            short idDrone;
            try
            {
                idDrone = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (idDrone <= 0)
                return "ERR";

            return db.clearDroneLogs(idDrone) ? "OK" : "ERR";
        }

        private static string clearLogsBetween(DBStorage db, string message)
        {
            DroneLogQuery query;
            try
            {
                query = (DroneLogQuery)JsonConvert.DeserializeObject(message, typeof(DroneLogQuery));
            }
            catch (Exception) { return "ERR"; }
            if (query == null || query.idDrone <= 0) // no hacer validaciones, solo se utiliza el ID del drone y las fechas (sin offset ni limit)
                return "ERR";

            return db.clearDroneLogs(query.idDrone, query.start, query.end) ? "OK" : "ERR";
        }

    }
}
