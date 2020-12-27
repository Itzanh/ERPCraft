using Npgsql;
using System;

namespace ERPCraft_Server.Models.DB
{
    public class Server
    {
        public Guid uuid;
        public string name;
        public string descripcion;
        public bool online;
        public DateTime ultimaConexion;

        public Server()
        {
            this.online = false;
        }

        public Server(NpgsqlDataReader rdr)
        {
            this.uuid = rdr.GetGuid(0);
            this.name = rdr.GetString(1);
            this.descripcion = rdr.GetString(2);
            this.online = rdr.GetBoolean(3);
            this.ultimaConexion = rdr.GetDateTime(4);
        }

        public bool isValid()
        {
            return !(this.name == null || this.name.Length == 0 || this.descripcion == null);
        }
    }
}
