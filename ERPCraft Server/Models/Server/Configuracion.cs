using Newtonsoft.Json;
using System;
using System.IO;

namespace ERPCraft_Server.Models.Server
{
    public class Configuracion
    {
        public string dbhost;
        public string dbname;
        public string dbuser;
        public string dbpassword;

        public Configuracion()
        {
            this.dbhost = "127.0.0.1";
            this.dbname = "ERPCraft";
            this.dbuser = "erpcraft";
            this.dbpassword = "erpcraft";
        }

        public static Configuracion readConfiguracion()
        {
            if (File.Exists("config.json"))
            {
                try
                {
                    string json = File.ReadAllText("config.json");
                    return (Configuracion)JsonConvert.DeserializeObject(json, typeof(Configuracion));
                }
                catch (Exception) { return new Configuracion(); }

            }
            else
            {
                try
                {
                    File.WriteAllText("config.json", JsonConvert.SerializeObject(new Configuracion()));
                }
                catch (Exception) { }

                return new Configuracion();
            }
        }
    }
}
