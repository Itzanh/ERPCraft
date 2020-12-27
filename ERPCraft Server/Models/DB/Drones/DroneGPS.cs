using Npgsql;
using System;

namespace ERPCraft_Server.Models.DB.Drones
{
    public class DroneGPS
    {
        public DateTime tiempo;
        public short posX;
        public short posY;
        public short posZ;

        public DroneGPS(short posX, short posY, short posZ)
        {
            this.tiempo = DateTime.Now;
            this.posX = posX;
            this.posY = posY;
            this.posZ = posZ;
        }

        public DroneGPS(NpgsqlDataReader rdr)
        {
            this.tiempo = rdr.GetDateTime(0);
            this.posX = rdr.GetInt16(1);
            this.posY = rdr.GetInt16(2);
            this.posZ = rdr.GetInt16(3);
        }
    }

    public class DroneGpsQuery
    {
        public short droneId;
        public int offset;
        public int limit;

        public DroneGpsQuery()
        {
        }

        public bool isValid()
        {
            return !(droneId <= 0 || offset < 0 || limit <= 0);
        }

    }
}
