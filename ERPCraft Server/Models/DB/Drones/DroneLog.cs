using Npgsql;
using System;

namespace ERPCraft_Server.Models.DB.Drones
{
    public class DroneLog
    {
        public DateTime id;
        public string name;
        public string mensaje;

        public DroneLog(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetDateTime(0);
            this.name = rdr.GetString(1);
            this.mensaje = rdr.GetString(2);
        }

        public DroneLog(string name, string mensaje)
        {
            this.id = DateTime.Now;
            this.name = name;
            this.mensaje = mensaje;
        }
    }

    public class DroneLogQuery
    {
        public short idDrone;
        public DateTime start;
        public DateTime end;
        public int limit;
        public int offset;

        public DroneLogQuery()
        {
            this.idDrone = 0;
            this.start = DateTime.MinValue;
            this.end = DateTime.MaxValue;
            this.limit = 0;
            this.offset = 0;
        }

        public bool isValid()
        {
            return !(idDrone <= 0 || offset < 0 || limit <= 0);
        }
    }
}
