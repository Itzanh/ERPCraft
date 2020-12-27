using Npgsql;
using System;

namespace ERPCraft_Server.Models.DB
{
    public class ClaveDeAPI
    {
        public short id;
        public string name;
        public Guid uuid;
        public DateTime ultimaConexion;

        public ClaveDeAPI(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt16(0);
            this.name = rdr.GetString(1);
            this.uuid = rdr.GetGuid(2);
            this.ultimaConexion = rdr.GetDateTime(3);
        }

        public ClaveDeAPI(short id, string name, Guid uuid)
        {
            this.id = id;
            this.name = name;
            this.uuid = uuid;
        }
    }
}
