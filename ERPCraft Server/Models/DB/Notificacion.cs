using Npgsql;
using System;

namespace ERPCraft_Server.Models.DB
{
    public class Notificacion
    {
        public DateTime id;
        public string name;
        public string descripcion;
        public bool leido;
        public NotificacionOrigen origen;

        public Notificacion(string name, string descripcion, NotificacionOrigen origen)
        {
            this.name = name;
            this.descripcion = descripcion;
            this.leido = false;
            this.origen = origen;
        }

        public Notificacion(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetDateTime(0);
            this.name = rdr.GetString(1);
            this.descripcion = rdr.GetString(2);
            this.leido = rdr.GetBoolean(3);
            this.origen = (NotificacionOrigen)rdr.GetInt16(4);
        }
    }

    public enum NotificacionOrigen
    {
        SinOrigen,
        RobotConectado,
        RobotDesconectado,
        RobotBateriaBaja,
        DroneConectado,
        DroneDesconectado,
        DroneBateriaBaja,
        OrdenMinadoDone,
        ServerOnline,
        ServerOffline,
        AlmacenInventario,
        BateriaLevel,
        GeneradorEstado
    }

}
