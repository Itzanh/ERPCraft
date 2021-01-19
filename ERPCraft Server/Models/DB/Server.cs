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
        public bool permitirAutoregistro;
        public string pwd;
        public string salt;
        public int iteraciones;
        public bool notificacionOnline;
        public bool notificacionOffline;
        public DateTime dateAdd;

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
            this.permitirAutoregistro = rdr.GetBoolean(5);
            this.pwd = rdr.GetString(6);
            this.salt = rdr.GetString(7);
            this.iteraciones = rdr.GetInt32(8);
            this.notificacionOnline = rdr.GetBoolean(9);
            this.notificacionOffline = rdr.GetBoolean(10);
            this.dateAdd = rdr.GetDateTime(11);
        }

        public bool isValid()
        {
            return !(this.name == null || this.name.Length == 0 || this.descripcion == null);
        }
    }

    public class ServerPwd
    {
        public Guid uuid;
        public string pwd;
    }
}
