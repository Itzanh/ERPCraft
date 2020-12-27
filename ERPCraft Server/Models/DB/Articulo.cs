using Npgsql;
using System;

namespace ERPCraft_Server.Models.DB
{
    public class Articulo
    {
        public short id;
        public string name;
        public string minecraftID;
        public int cantidad;
        public string descripcion;

        public Articulo()
        {
        }

        public Articulo(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt16(0);
            this.name = rdr.GetString(1);
            this.minecraftID = rdr.GetString(2);
            this.cantidad = rdr.GetInt32(3);
            this.descripcion = rdr.GetString(4);
        }

        internal bool isValid()
        {
            if (this.name == null || this.name.Equals(string.Empty))
                return false;

            if (this.minecraftID == null || (this.minecraftID.Length > 0 && this.minecraftID.IndexOf(':') < 0))
                return false;

            if (cantidad < 0)
                return false;

            if (this.descripcion == null)
                return false;

            return true;
        }
    }

    /// <summary>
    /// Artículo sin campos largos para mostrar en la tabla principal
    /// </summary>
    public class ArticuloHead
    {
        public short id;
        public string name;
        public string minecraftID;
        public short cantidad;

        public ArticuloHead(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt16(0);
            this.name = rdr.GetString(1);
            this.minecraftID = rdr.GetString(2);
            this.cantidad = rdr.GetInt16(3);
        }
    }

    public class ArticuloSlot
    {
        public short id;
        public string name;

        public ArticuloSlot(short id, NpgsqlDataReader rdr)
        {
            this.id = id;
            this.name = rdr.GetString(0);
        }
    }
}
