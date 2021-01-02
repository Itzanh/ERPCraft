using Npgsql;
using System;

namespace ERPCraft_Server.Models.DB.Robots
{
    public class RobotLog
    {
        public DateTime id;
        public string name;
        public string mensaje;

        public RobotLog(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetDateTime(0);
            this.name = rdr.GetString(1);
            this.mensaje = rdr.GetString(2);
        }

        public RobotLog(string name, string mensaje)
        {
            this.id = DateTime.Now;
            this.name = name;
            this.mensaje = mensaje;
        }
    }

    public class RobotLogQuery
    {
        public short idRobot;
        public DateTime start;
        public DateTime end;
        public int limit;
        public int offset;

        public RobotLogQuery()
        {
            this.idRobot = 0;
            this.start = DateTime.MinValue;
            this.end = DateTime.MaxValue;
            this.limit = 0;
            this.offset = 0;
        }
    }
}
