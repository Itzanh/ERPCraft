﻿using Npgsql;
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
        public int cantidad;

        public AlmacenInventarioSet(string articulo, int cantidad)
        {
            this.articulo = articulo;
            this.cantidad = cantidad;
        }
    }
}
