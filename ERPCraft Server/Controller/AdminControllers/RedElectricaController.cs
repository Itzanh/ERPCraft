using ERPCraft_Server.Models.DB.Electrico;
using ERPCraft_Server.Storage;
using Newtonsoft.Json;
using System;

namespace ERPCraft_Server.Controller.AdminControllers
{
    public static class RedElectricaController
    {
        public static string redElectricaCommand(DBStorage db, string command, string message)
        {
            switch (command)
            {
                // red eléctrica
                case "get":
                    {
                        return getRedesElectricas(db);
                    }
                case "search":
                    {
                        return searchRedesElectricas(db, message);
                    }
                case "add":
                    {
                        return addRedElectrica(db, message);
                    }
                case "update":
                    {
                        return updateRedElectrica(db, message);
                    }
                case "delete":
                    {
                        return deleteRedElectrica(db, message);
                    }
                // generadores
                case "addGenerador":
                    {
                        return addGenerador(db, message);
                    }
                case "updateGenerador":
                    {
                        return updateGenerador(db, message);
                    }
                case "deleteGenerador":
                    {
                        return deleteGenerador(db, message);
                    }
                // baterías
                case "addBateria":
                    {
                        return addBateria(db, message);
                    }
                case "updateBateria":
                    {
                        return updateBateria(db, message);
                    }
                case "deleteBateria":
                    {
                        return deleteBateria(db, message);
                    }
                // batería - historial
                case "getBateriaHist":
                    {
                        return getHistorialBateria(db, message);
                    }
            }

            return "ERR";
        }

        /* REDES ELECTRICAS */

        private static string getRedesElectricas(DBStorage db)
        {
            return JsonConvert.SerializeObject(db.getRedesElectricas());
        }

        private static string searchRedesElectricas(DBStorage db, string message)
        {
            return JsonConvert.SerializeObject(db.searchRedesElectricas(message));
        }

        private static string addRedElectrica(DBStorage db, string message)
        {
            RedElectrica red;
            try
            {
                red = (RedElectrica)JsonConvert.DeserializeObject(message, typeof(RedElectrica));
            }
            catch (Exception) { return "ERR"; }
            if (red == null || !red.isValid())
                return "ERR";

            return db.addRedElectrica(red) ? "OK" : "ERR";
        }

        private static string updateRedElectrica(DBStorage db, string message)
        {
            RedElectrica red;
            try
            {
                red = (RedElectrica)JsonConvert.DeserializeObject(message, typeof(RedElectrica));
            }
            catch (Exception) { return "ERR"; }
            if (red == null || red.id <= 0 || !red.isValid())
                return "ERR";

            return db.updateRedElectrica(red) ? "OK" : "ERR";
        }

        private static string deleteRedElectrica(DBStorage db, string message)
        {
            short id;
            try
            {
                id = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            if (id <= 0)
                return "ERR";

            return db.deleteRedElectrica(id) ? "OK" : "ERR";
        }

        /* GENERADOR */

        private static string addGenerador(DBStorage db, string message)
        {
            Generador generador;
            try
            {
                generador = (Generador)JsonConvert.DeserializeObject(message, typeof(Generador));
            }
            catch (Exception) { return "ERR"; }
            if (generador == null || !generador.isValid())
                return "ERR";

            return "" + db.addGeneradorRedElectrica(generador);
        }

        private static string updateGenerador(DBStorage db, string message)
        {
            Generador generador;
            try
            {
                generador = (Generador)JsonConvert.DeserializeObject(message, typeof(Generador));
            }
            catch (Exception) { return "ERR"; }
            if (generador == null || generador.id <= 0 || !generador.isValid())
                return "ERR";

            return db.updateGeneradorRedElectrica(generador) ? "OK" : "ERR";
        }

        private static string deleteGenerador(DBStorage db, string message)
        {
            GeneradorQuery query;
            try
            {
                query = (GeneradorQuery)JsonConvert.DeserializeObject(message, typeof(GeneradorQuery));
            }
            catch (Exception) { return "ERR"; }
            if (query == null || query.id <= 0 || query.redElectrica <= 0)
                return "ERR";

            return db.deleteGeneradorRedElectrica(query.redElectrica, query.id) ? "OK" : "ERR";
        }

        /* BATERÍA */

        private static string addBateria(DBStorage db, string message)
        {
            Bateria bateria;
            try
            {
                bateria = (Bateria)JsonConvert.DeserializeObject(message, typeof(Bateria));
            }
            catch (Exception) { return "ERR"; }
            if (bateria == null || !bateria.isValid())
                return "ERR";

            return "" + db.addBateriaRedElectrica(bateria);
        }

        private static string updateBateria(DBStorage db, string message)
        {
            Bateria bateria;
            try
            {
                bateria = (Bateria)JsonConvert.DeserializeObject(message, typeof(Bateria));
            }
            catch (Exception) { return "ERR"; }
            if (bateria == null || bateria.id <= 0 || !bateria.isValid())
                return "ERR";

            return db.updateBateriaRedElectrica(bateria) ? "OK" : "ERR";
        }

        private static string deleteBateria(DBStorage db, string message)
        {
            BateriaQuery query;
            try
            {
                query = (BateriaQuery)JsonConvert.DeserializeObject(message, typeof(BateriaQuery));
            }
            catch (Exception) { return "ERR"; }
            if (query == null || query.id <= 0 || query.redElectrica <= 0)
                return "ERR";

            return db.deleteBateriaRedElectrica(query.redElectrica, query.id) ? "OK" : "ERR";
        }

        /* BATERÍA - HISTORIAL */

        private static string getHistorialBateria(DBStorage db, string message)
        {
            BateriaHistorialQuery query;
            try
            {
                query = (BateriaHistorialQuery)JsonConvert.DeserializeObject(message, typeof(BateriaHistorialQuery));
            }
            catch (Exception) { return "ERR"; }
            if (query == null || !query.isValid())
                return "ERR";

            return JsonConvert.SerializeObject(db.getBateriaHistorial(query.redElectrica, query.bateria, query.offset, query.limit));
        }
    }
}
