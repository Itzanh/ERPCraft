using Npgsql;
using System;

namespace ERPCraft_Server.Models.DB.Robots
{
    public class RobotGPS
    {
		public DateTime tiempo;
        public short posX;
        public short posY;
        public short posZ;

        public RobotGPS(short posX, short posY, short posZ)
        {
            this.tiempo = DateTime.Now;
            this.posX = posX;
            this.posY = posY;
            this.posZ = posZ;
        }

        public RobotGPS(NpgsqlDataReader rdr)
        {
            this.tiempo = rdr.GetDateTime(0);
            this.posX = rdr.GetInt16(1);
            this.posY = rdr.GetInt16(2);
            this.posZ = rdr.GetInt16(3);
        }
    }

    public class RobotGpsQuery
    {
        public short robotId;
        public int offset;
        public int limit;

        public RobotGpsQuery()
        {
        }

    }
}
