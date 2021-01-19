using Npgsql;
using System;

namespace ERPCraft_Server.Models.DB.Electrico
{
    public class Bateria
    {
        public short redElectrica;
        public short id;
        public string name;
        public Guid uuid;
        public long capacidadElectrica;
        public long cargaActual;
        public string descripcion;
        public char tipo;
        public bool notificacion;
        public long cargaNotificacion;

        public Bateria()
        {
        }

        public Bateria(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt16(0);
            this.redElectrica = rdr.GetInt16(1);
            this.name = rdr.GetString(2);
            this.uuid = rdr.GetGuid(3);
            this.capacidadElectrica = rdr.GetInt64(4);
            this.cargaActual = rdr.GetInt64(5);
            this.descripcion = rdr.GetString(6);
            this.tipo = rdr.GetChar(7);
            this.notificacion = rdr.GetBoolean(8);
            this.cargaNotificacion = rdr.GetInt64(9);
        }

        public bool isValid()
        {
            if (tipo != 'B' && tipo != 'C' && tipo != 'M' && tipo != 'F' && tipo != 'O')
                return false;

            return !(this.redElectrica <= 0 || this.name == null || this.name.Length == 0 || this.capacidadElectrica <= 0 || this.cargaActual < 0
                || this.descripcion == null);
        }
    }

    public class BateriaQuery
    {
        public short redElectrica;
        public short id;

        public BateriaQuery()
        {
        }

        public BateriaQuery(short redElectrica, short id)
        {
            this.redElectrica = redElectrica;
            this.id = id;
        }
    }

    public class BateriaHistorial
    {
        public short redElectrica;
        public short bateria;
        public short id;
        public DateTime tiempo;
        public long cargaActual;

        public BateriaHistorial(NpgsqlDataReader rdr)
        {
            this.redElectrica = rdr.GetInt16(0);
            this.bateria = rdr.GetInt16(1);
            this.id = rdr.GetInt16(2);
            this.tiempo = rdr.GetDateTime(3);
            this.cargaActual = rdr.GetInt64(4);
        }
    }

    public class BateriaHistorialQuery
    {
        public short redElectrica;
        public short bateria;
        public int offset;
        public int limit;

        public bool isValid()
        {
            return !(redElectrica <= 0 || bateria <= 0 || offset < 0 || limit <= 0);
        }
    }
}