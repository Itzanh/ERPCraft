using Npgsql;
using System;

namespace ERPCraft_Server.Models.DB.Almacen
{
    public class Almacen
    {
        public short id;
        public string name;
        public string descripcion;
        public short tipos;
        public int items;
        public bool off;
        public Guid uuid;
        public DateTime dateAdd;
        public DateTime dateLastUpdate;
        public short stacks;
        /// <summary>
        /// C = Cofre, D = Cofre Doble, M = Applied Energistics 2, O = Otro
        /// </summary>
        public char almacenamiento;
        public short maximoStacks;
        public short maximoTipos;
        public int maximoItems;

        public Almacen()
        {
        }

        public Almacen(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt16(0);
            this.name = rdr.GetString(1);
            this.descripcion = rdr.GetString(2);
            this.tipos = rdr.GetInt16(3);
            this.items = rdr.GetInt32(4);
            this.off = rdr.GetBoolean(5);
            this.uuid = rdr.GetGuid(6);
            this.dateAdd = rdr.GetDateTime(7);
            this.dateLastUpdate = rdr.GetDateTime(8);
            this.stacks = rdr.GetInt16(9);
            this.almacenamiento = rdr.GetChar(10);
            this.maximoStacks = rdr.GetInt16(11);
            this.maximoTipos = rdr.GetInt16(12);
            this.maximoItems = rdr.GetInt32(13);
        }

        public bool isValid()
        {
            if (almacenamiento != 'C' && almacenamiento != 'D' && almacenamiento != 'M' && almacenamiento != 'O')
                return false;

            return !(name == null || name.Length == 0 || descripcion == null || maximoItems < 0 || maximoTipos < 0 || maximoStacks < 0);
        }
    }

    public class AlmacenInventario
    {
        public short almacen;
        public short id;
        public ArticuloSlot articulo;
        public int cantidad;
        public int cantidadDisponible;

        public AlmacenInventario(short almacen, short id, ArticuloSlot articulo, int cantidad, int cantidadDisponible)
        {
            this.almacen = almacen;
            this.id = id;
            this.articulo = articulo;
            this.cantidad = cantidad;
            this.cantidadDisponible = cantidadDisponible;
        }
    }

    public class AlmacenInventarioGet
    {
        public short almacen;
        public short id;
        public short articulo;
        public int cantidad;
        public int cantidadDisponible;

        public AlmacenInventarioGet(short almacen, short articulo, int cantidad)
        {
            this.almacen = almacen;
            this.articulo = articulo;
            this.cantidad = cantidad;
        }

        public AlmacenInventarioGet(NpgsqlDataReader rdr)
        {
            this.almacen = rdr.GetInt16(0);
            this.id = rdr.GetInt16(1);
            this.articulo = rdr.GetInt16(2);
            this.cantidad = rdr.GetInt32(3);
            this.cantidadDisponible = rdr.GetInt32(4);
        }
    }

    public class AlmacenInventarioSet
    {
        public string articulo;
        public short articuloId;
        public int cantidad;
        public string oreName;

        public AlmacenInventarioSet(string articulo, int cantidad, string oreName)
        {
            this.articulo = articulo;
            this.cantidad = cantidad;
            this.oreName = oreName;
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

    public class AE2StorageCell
    {
        public short idAlmacen;
        public short id;
        public short tier;
        public DateTime dateAdd;

        public AE2StorageCell()
        {
        }

        public AE2StorageCell(NpgsqlDataReader rdr)
        {
            this.idAlmacen = rdr.GetInt16(0);
            this.id = rdr.GetInt16(1);
            this.tier = rdr.GetInt16(2);
            this.dateAdd = rdr.GetDateTime(3);
        }
    }

    public class AE2StorageCellDelete
    {
        public short idAlmacen;
        public short id;

        public AE2StorageCellDelete()
        {
        }
    }
}
