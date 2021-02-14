using ERPCraft_Server.Models.DB.Robots;
using System;
using Newtonsoft.Json;
using ERPCraft_Server.Storage;

namespace ERPCraft_Server.Controller.AdminControllers
{
    public static class RobotController
    {
        public static string robotCommands(DBStorage db, string command, string message)
        {
            switch (command)
            {
                case "get":
                    {
                        return getRobots(db, message);
                    }
                case "add":
                    {
                        return addRobot(db, message);
                    }
                case "update":
                    {
                        return updateRobot(db, message);
                    }
                case "delete":
                    {
                        return deleteRobot(db, message);
                    }
                case "getGPS":
                    {
                        return getRobotGPS(db, message);
                    }
                case "getInventario":
                    {
                        return getRobotInventario(db, message);
                    }
                case "getLog":
                    {
                        return getRobotLogs(db, message);
                    }
                case "clearLogs":
                    {
                        return clearLogs(db, message);
                    }
                case "clearLogsBetween":
                    {
                        return clearLogsBetween(db, message);
                    }
                case "localizar":
                    {
                        return getLocalizarRobots(db);
                    }
                case "name":
                    {
                        return getRobotName(db, message);
                    }
                case "command":
                    {
                        return execRobotCommand(db, message);
                    }
                case "referencias":
                    {
                        return robotLoadReferences(db, message);
                    }
                case "assemblerGet":
                    {
                        return getEnsambladoRobot(db, message);
                    }
                case "assemblerSet":
                    {
                        return setEnsambladoRobot(db, message);
                    }
                case "hasInventoryController":
                    {
                        return robotHasInventoryController(db, message);
                    }
                case "hasGeolyzer":
                    {
                        return robotHasGeolyzer(db, message);
                    }
            }

            return "ERR";
        }

        private static string getRobots(DBStorage db, string message)
        {
            RobotQuery query;
            if (message.Equals(string.Empty))
            {
                query = new RobotQuery();
            }
            else
            {
                try
                {
                    query = (RobotQuery)JsonConvert.DeserializeObject(message, typeof(RobotQuery));
                }
                catch (Exception) { return "ERR"; }
                if (query == null || query.text == null)
                    return "ERR";
            }

            return JsonConvert.SerializeObject(db.getRobots(query));
        }

        private static string addRobot(DBStorage db, string message)
        {
            Robot robot;
            try
            {
                robot = (Robot)JsonConvert.DeserializeObject(message, typeof(Robot));
            }
            catch (Exception) { return "ERR"; }
            if (robot == null || !robot.isValid())
                return "ERR";

            return "" + db.addRobot(robot);
        }

        private static string updateRobot(DBStorage db, string message)
        {
            Robot robot;
            try
            {
                robot = (Robot)JsonConvert.DeserializeObject(message, typeof(Robot));
            }
            catch (Exception) { return "ERR"; }
            if (robot == null || robot.id <= 0 || !robot.isValid())
                return "ERR";

            return db.updateRobot(robot) ? "OK" : "ERR";
        }

        private static string deleteRobot(DBStorage db, string message)
        {
            short robotId;
            try
            {
                robotId = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (robotId <= 0)
                return "ERR";

            return db.deleteRobot(robotId) ? "OK" : "ERR";
        }

        private static string getRobotGPS(DBStorage db, string message)
        {
            // parsear JSON con offset y limit
            RobotGpsQuery query;
            try
            {
                query = (RobotGpsQuery)JsonConvert.DeserializeObject(message, typeof(RobotGpsQuery));
            }
            catch (Exception) { return "ERR"; }
            if (query == null || query.robotId == 0 || query.offset < 0 || query.limit <= 0)
                return "ERR";

            return JsonConvert.SerializeObject(db.getRobotGPS(query.robotId, query.offset, query.limit));
        }

        private static string getRobotInventario(DBStorage db, string message)
        {
            short robotId;
            try
            {
                robotId = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (robotId <= 0)
                return "ERR";

            return JsonConvert.SerializeObject(db.getRobotInventario(robotId));
        }

        private static string getRobotLogs(DBStorage db, string message)
        {
            RobotLogQuery query;
            try
            {
                query = (RobotLogQuery)JsonConvert.DeserializeObject(message, typeof(RobotLogQuery));
            }
            catch (Exception) { return "ERR"; }
            if (query == null || query.idRobot <= 0 || query.offset < 0 || query.limit <= 0)
                return "ERR";

            return JsonConvert.SerializeObject(db.getRobotLogs(query));
        }

        private static string clearLogs(DBStorage db, string message)
        {
            short robotId;
            try
            {
                robotId = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (robotId <= 0)
                return "ERR";

            return db.clearRobotLogs(robotId) ? "OK" : "ERR";
        }

        private static string clearLogsBetween(DBStorage db, string message)
        {
            RobotLogQuery query;
            try
            {
                query = (RobotLogQuery)JsonConvert.DeserializeObject(message, typeof(RobotLogQuery));
            }
            catch (Exception) { return "ERR"; }
            if (query == null || query.idRobot <= 0 || query.offset < 0 || query.limit <= 0)
                return "ERR";

            return db.clearRobotLogs(query.idRobot, query.start, query.end) ? "OK" : "ERR";
        }

        private static string getLocalizarRobots(DBStorage db)
        {
            return JsonConvert.SerializeObject(db.getRobotsHead());
        }

        private static string getRobotName(DBStorage db, string message)
        {
            short robotId;
            try
            {
                robotId = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (robotId <= 0)
                return "ERR";

            return db.getRobotName(robotId);
        }

        private static string execRobotCommand(DBStorage db, string message)
        {
            RobotCommand command;
            try
            {
                command = (RobotCommand)JsonConvert.DeserializeObject(message, typeof(RobotCommand));
            }
            catch (Exception) { return "ERR"; }
            if (command == null || command.command == null || command.command.Length == 0 || command.robotId < 0)
                return "ERR";
            Robot r = db.getRobot(command.robotId);
            if (r == null)
                return "ERR";

            return GameController.enviarComando(r.uuid.ToString(), command.command) ? "OK" : "ERR";
        }

        private static string robotLoadReferences(DBStorage db, string message)
        {
            RobotReferences references;
            try
            {
                references = (RobotReferences)JsonConvert.DeserializeObject(message, typeof(RobotReferences));
            }
            catch (Exception) { return "ERR"; }
            if (references == null || references.reference < 0 || references.ids == null || references.ids.Length == 0)
                return "ERR";

            switch (references.reference)
            {
                case RobotReferencesTable.RobotInventario:
                    {
                        return JsonConvert.SerializeObject(db.loadRobotReferenceInventario(references.ids));
                    }
                case RobotReferencesTable.RobotGPS:
                    {
                        return JsonConvert.SerializeObject(db.loadRobotReferenceGPS(references.ids));
                    }
                case RobotReferencesTable.RobotLogs:
                    {
                        return JsonConvert.SerializeObject(db.loadRobotReferenceLog(references.ids));
                    }
                case RobotReferencesTable.RobotOrdenesMinado:
                    {
                        break;
                    }
            }
            return null;
        }

        private static string getEnsambladoRobot(DBStorage db, string message)
        {
            short robotId;
            try
            {
                robotId = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (robotId <= 0)
                return "ERR";

            return db.getRobotEnsamblado(robotId);
        }

        private static string setEnsambladoRobot(DBStorage db, string message)
        {
            RobotEnsambladoSet ensamblado;
            try
            {
                ensamblado = (RobotEnsambladoSet)JsonConvert.DeserializeObject(message, typeof(RobotEnsambladoSet));
            }
            catch (Exception) { return "ERR"; }
            if (ensamblado.robotId <= 0)
                return "ERR";

            return db.setRobotEnsamblado(ensamblado.robotId, ensamblado.toRobotEnsamblado()) ? "OK" : "ERR";
        }

        private static string robotHasInventoryController(DBStorage db, string message)
        {
            short robotId;
            try
            {
                robotId = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (robotId <= 0)
                return "ERR";

            return db.getRobotInventoryController(robotId).ToString().ToLower();
        }

        private static string robotHasGeolyzer(DBStorage db, string message)
        {
            short robotId;
            try
            {
                robotId = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (robotId <= 0)
                return "ERR";

            return db.getRobotGeolyzer(robotId).ToString().ToLower();
        }


    }
}
