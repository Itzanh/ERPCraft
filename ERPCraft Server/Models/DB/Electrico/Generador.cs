using Npgsql;
using System;

namespace ERPCraft_Server.Models.DB.Electrico
{
    public class Generador
    {
        public short redElectrica;
        public short id;
        public string name;
        public Guid uuid;
        public int euTick;
        public bool activado;
        public char tipo;
        public string descripcion;

        public Generador()
        {
        }

        public Generador(NpgsqlDataReader rdr)
        {
            this.redElectrica = rdr.GetInt16(0);
            this.id = rdr.GetInt16(1);
            this.name = rdr.GetString(2);
            this.uuid = rdr.GetGuid(3);
            this.euTick = rdr.GetInt32(4);
            this.activado = rdr.GetBoolean(5);
            this.tipo = rdr.GetChar(6);
            this.descripcion = rdr.GetString(7);
        }

        public bool isValid()
        {
            if (tipo != 'G' && tipo != 'S' && tipo != 'R' && tipo != 'T' && tipo != 'O')
                return false;

            return !(this.redElectrica <= 0 || this.name == null || this.name.Length == 0 || this.euTick <= 0 || this.descripcion == null);
        }
    }

    class GeneradorQuery
    {
        public short redElectrica;
        public short id;

        public GeneradorQuery()
        {
        }

        public GeneradorQuery(short redElectrica, short id)
        {
            this.redElectrica = redElectrica;
            this.id = id;
        }
    }
}

