using Npgsql;
using System;

namespace ERPCraft_Server.Models.DB.Almacen
{
    public class MovimientoAlmacen
    {
        public short almacen;
        public long id;
        public short articulo;
        public int cantidad;
        public char origen;
        public DateTime dateAdd;
        public string descripcion;

        public MovimientoAlmacen()
        {
        }

        public MovimientoAlmacen(NpgsqlDataReader rdr)
        {
            this.almacen = rdr.GetInt16(0);
            this.id = rdr.GetInt64(1);
            this.articulo = rdr.GetInt16(2);
            this.cantidad = rdr.GetInt32(3);
            this.origen = rdr.GetChar(4);
            this.dateAdd = rdr.GetDateTime(5);
            this.descripcion = rdr.GetString(6);
        }

        public bool isValid()
        {
            return !(almacen <= 0 || articulo <= 0 || cantidad == 0 || descripcion == null);
        }
    }

    public class MovimientoAlmacenQuery
    {
        public short almacen;
        public short articulo;
        public DateTime dateInicio;
        public DateTime dateFin;

        public MovimientoAlmacenQuery()
        {
            this.dateInicio = DateTime.MinValue;
            this.dateFin = DateTime.MinValue;
        }

        public bool isDefault()
        {
            return (almacen == 0 && articulo == 0 && dateInicio <= DateTime.MinValue && dateFin <= DateTime.MinValue);
        }
    }

    public class MovimientoAlmacenDelete
    {
        public short almacen;
        public long id;

        public MovimientoAlmacenDelete()
        {
        }

        public bool isValid()
        {
            return !(almacen <= 0 || id <= 0);
        }
    }
}
