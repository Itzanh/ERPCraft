using Npgsql;
using System;

namespace ERPCraft_Server.Models.DB.Almacen
{
    public class Almacen
    {
        public short id;
        public string name;
        public string descripcion;
        public short slots;
        public int items;
        public bool off;
        public Guid uuid;
        public DateTime dateAdd;
        public DateTime dateLastUpdate;

        public Almacen()
        {
        }

        public Almacen(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt16(0);
            this.name = rdr.GetString(1);
            this.descripcion = rdr.GetString(2);
            this.slots = rdr.GetInt16(3);
            this.items = rdr.GetInt32(4);
            this.off = rdr.GetBoolean(5);
            this.uuid = rdr.GetGuid(6);
            this.dateAdd = rdr.GetDateTime(7);
            this.dateLastUpdate = rdr.GetDateTime(8);
        }

        public bool isValid()
        {
            return !(name == null || name.Length == 0 || descripcion == null);
        }
    }

    public class AlmacenInventario
    {
        public short almacen;
        public short id;
        public ArticuloSlot articulo;
        public int cantidad;

        public AlmacenInventario(short almacen, short id, ArticuloSlot articulo, int cantidad)
        {
            this.almacen = almacen;
            this.id = id;
            this.articulo = articulo;
            this.cantidad = cantidad;
        }
    }

    public class AlmacenInventarioGet
    {
        public short almacen;
        public short id;
        public short articulo;
        public int cantidad;

        public AlmacenInventarioGet(NpgsqlDataReader rdr)
        {
            this.almacen = rdr.GetInt16(0);
            this.id = rdr.GetInt16(1);
            this.articulo = rdr.GetInt16(2);
            this.cantidad = rdr.GetInt32(3);
        }
    }

    public class AlmacenInventarioSet
    {
        public string articulo;
        public short articuloId;
        public int cantidad;

        public AlmacenInventarioSet(string articulo, int cantidad)
        {
            this.articulo = articulo;
            this.cantidad = cantidad;
        }

        public AlmacenInventarioSet(short articuloId, int cantidad)
        {
            this.articuloId = articuloId;
            this.cantidad = cantidad;
        }
    }

    public class AlmacenHead
    {
        public short id;
        public string name;
        public Guid uuid;

        public AlmacenHead(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt16(0);
            this.name = rdr.GetString(1);
            this.uuid = rdr.GetGuid(2);
        }
    }

    public class AlmacenInventarioNotificacion
    {
        public short idAlmacen;
        public short id;
        public string name;
        public short idArticulo;
        /// <summary>
        /// <,>,=,-,+
        /// < menor que
        /// > mayor que
        /// = igual
        /// - menor o igual que
        /// + mayor o igual que
        /// </summary>
        public char modo;
        public int cantidad;

        public AlmacenInventarioNotificacion()
        {
        }

        public AlmacenInventarioNotificacion(NpgsqlDataReader rdr)
        {
            this.idAlmacen = rdr.GetInt16(0);
            this.id = rdr.GetInt16(1);
            this.name = rdr.GetString(2);
            this.idArticulo = rdr.GetInt16(3);
            this.modo = rdr.GetChar(4);
            this.cantidad = rdr.GetInt32(5);
        }

        public bool isValid()
        {
            if (modo != '<' && modo != '>' && modo != '=' && modo != '-' && modo != '+')
                return false;

            return !(idAlmacen <= 0 || name == null || name.Length == 0 || idArticulo <= 0 || cantidad < 0);
        }
    }

    public class AlmacenInventarioNotificacionDelete
    {
        public short idAlmacen;
        public short id;

        public bool isValid()
        {
            return !(idAlmacen <= 0 || id <= 0);
        }
    }
}
