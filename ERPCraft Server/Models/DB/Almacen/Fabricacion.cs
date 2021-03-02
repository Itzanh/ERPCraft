using Npgsql;
using System;
using System.Collections.Generic;

namespace ERPCraft_Server.Models.DB.Almacen
{
    public class Fabricacion
    {
        public short id;
        public short idAlmacen;
        public string name;
        /// <summary>
        /// R = Robot, A = AE2
        /// </summary>
        public char tipo;
        public DateTime dateAdd;
        public Guid uuid;
        public bool off;
        public short cofreSide;
        /// <summary>
        /// 0 = Izquierda, 1 = Derecha
        /// </summary>
        public short hornoSide;

        public Fabricacion()
        {
        }

        public Fabricacion(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt16(0);
            this.idAlmacen = rdr.GetInt16(1);
            this.name = rdr.GetString(2);
            this.tipo = rdr.GetChar(3);
            this.dateAdd = rdr.GetDateTime(4);
            this.uuid = rdr.GetGuid(5);
            this.off = rdr.GetBoolean(6);
            this.cofreSide = rdr.GetInt16(7);
            this.hornoSide = rdr.GetInt16(8);
        }

        public bool isValid()
        {
            return !(idAlmacen <= 0 || name == null || name.Length == 0 || (tipo != 'R' && tipo != 'A') || uuid == null || cofreSide < 0 || cofreSide > 5 || hornoSide < 0 || hornoSide > 1);
        }
    }

    public class FabricacionHead
    {
        public short id;
        public string name;

        public FabricacionHead(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt16(0);
            this.name = rdr.GetString(1);
        }
    }

    public class FabricacionDelete
    {
        public short id;
        public short idAlmacen;

        public FabricacionDelete()
        {
        }

        public bool isValid()
        {
            return !(id <= 0 || idAlmacen <= 0);
        }
    }

    public class FabricacionCrafteo
    {
        public short idArticulo;
        public short cantidadCrafteo;
        public int cantidadDisponible;

        public FabricacionCrafteo(short idArticulo, short cantidadCrafteo)
        {
            this.idArticulo = idArticulo;
            this.cantidadCrafteo = cantidadCrafteo;
        }

        public FabricacionCrafteo(short idArticulo, short cantidadCrafteo, int cantidadDisponible) : this(idArticulo, cantidadCrafteo)
        {
            this.cantidadDisponible = cantidadDisponible;
        }

        public static void agregarArticulo(ref List<FabricacionCrafteo> articulosCrafteo, short idArticulo, short cantidad)
        {
            for (int i = 0; i < articulosCrafteo.Count; i++)
            {
                if (articulosCrafteo[i].idArticulo == idArticulo)
                {
                    articulosCrafteo[i].cantidadCrafteo += cantidad;
                    return;
                }
            }

            articulosCrafteo.Add(new FabricacionCrafteo(idArticulo, cantidad));
        }
    }

    public class OrdenFabricacion
    {
        public int id;
        public short idAlmacen;
        public string name;
        public short idFabricacion;
        public int? idCrafteo;
        public int? idSmelting;
        public DateTime dateAdd;
        public DateTime? dateFinalizado;
        /// <summary>
        /// Q = Queued, R = Ready, D = Done, E = Error
        /// </summary>
        public char estado;
        public int cantidad;
        public int cantidadFabricado;
        public short errorCode;

        public OrdenFabricacion()
        {
        }

        public OrdenFabricacion(int? idCrafteo, int? idSmelting, int cantidad, string name)
        {
            this.idCrafteo = idCrafteo;
            this.idSmelting = idSmelting;
            this.cantidad = cantidad;
            this.name = generateOrderName(name);
        }

        public OrdenFabricacion(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt32(0);
            this.idAlmacen = rdr.GetInt16(1);
            this.name = rdr.GetString(2);
            this.idFabricacion = rdr.GetInt16(3);
            if (rdr.IsDBNull(4))
                this.idCrafteo = null;
            else
                this.idCrafteo = rdr.GetInt16(4);
            if (rdr.IsDBNull(5))
                this.idSmelting = null;
            else
                this.idSmelting = rdr.GetInt16(5);
            this.dateAdd = rdr.GetDateTime(6);
            if (rdr.IsDBNull(7))
                this.dateFinalizado = null;
            else
                this.dateFinalizado = rdr.GetDateTime(7);
            this.estado = rdr.GetChar(8);
            this.cantidad = rdr.GetInt32(9);
            this.cantidadFabricado = rdr.GetInt32(10);
            this.errorCode = rdr.GetInt16(11);
        }

        private static string generateOrderName(string name)
        {
            name += " ";
            const string charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            Random r = new Random();
            for (int i = 0; i < 6; i++)
            {
                name += charset[(r.Next(0, charset.Length))];
            }

            return name;
        }

        public bool isValid()
        {
            return !(idAlmacen <= 0 || name == null || name.Length == 0 || idFabricacion <= 0 || (idCrafteo == null && idSmelting == null)
                || (idCrafteo != null && idSmelting != null) || (idCrafteo != null && idCrafteo <= 0) || (idSmelting != null && idSmelting <= 0) || cantidad <= 0);
        }
    }

    public class OrdenFabricacionSearch
    {
        public short idAlmacen;
        public short idFabricacion;
        /// <summary>
        /// C = Craftear, S = Smelting, T = Todo
        /// </summary>
        public char tipoReceta;
        public int idReceta;
        public DateTime inicio;
        public DateTime fin;
        /// <summary>
        /// 0 = En cola & pendiente, 1 = En cola, 2 = En curso, 3 = Finalizado, 4 = Todo
        /// </summary>
        public short estado;

        public OrdenFabricacionSearch()
        {
            this.inicio = DateTime.MinValue;
            this.fin = DateTime.MinValue;
        }

        public bool isValid()
        {
            if (tipoReceta != 'C' && tipoReceta != 'S' && tipoReceta != 'T')
                return false;
            return !(idAlmacen <= 0 || idFabricacion <= 0 || idReceta < 0 || estado < 0 || estado > 5);
        }

        public bool isDefault()
        {
            return (tipoReceta == 'T' && idReceta == 0 && inicio == DateTime.MinValue && fin == DateTime.MinValue && estado == '0');
        }
    }

    public class OrdenFabricacionQuery
    {
        public short idAlmacen;
        public short idFabricacion;

        public OrdenFabricacionQuery()
        {
        }

        public bool isValid()
        {
            return !(idAlmacen <= 0 || idFabricacion <= 0);
        }
    }

    public class OrdenFabricacionDelete
    {
        public int id;
        public short idAlmacen;
        public short idFabricacion;

        public OrdenFabricacionDelete()
        {
        }

        public bool isValid()
        {
            return !(id <= 0 || idAlmacen <= 0);
        }
    }

    public class OrdenFabricacionPreviewQuery
    {
        public short idAlmacen;
        public int idReceta;
        /// <summary>
        /// C = Craft, S = Smelt
        /// </summary>
        public char tipo;
        /// <summary>
        /// MultiCraft
        /// </summary>
        public bool avanzado;

        public OrdenFabricacionPreviewQuery()
        {
            this.avanzado = false;
        }

        public bool isValid()
        {
            if (tipo != 'C' && tipo != 'S')
                return false;

            return !(idAlmacen <= 0 || idReceta <= 0);
        }
    }

    public class OrdenFabricacionGenerateQuery
    {
        public short idAlmacen;
        public short idFabricacion;
        public int idReceta;
        /// <summary>
        /// C = Craft, S = Smelt
        /// </summary>
        public char tipo;
        public short cantidadPedida;

        public bool isValid()
        {
            if (tipo != 'C' && tipo != 'S')
                return false;

            return !(idAlmacen <= 0 || idReceta <= 0 || idFabricacion <= 0 || cantidadPedida <= 0);
        }
    }

    public class OrdenFabricacionPreview
    {
        public List<FabricacionCrafteo> articulosCrafteo;
        public short cantidadCrafteo;
        public short maxCantidadCrafteo;

        public OrdenFabricacionPreview(List<FabricacionCrafteo> articulosCrafteo, short cantidadCrafteo, short maxCantidadCrafteo)
        {
            this.articulosCrafteo = articulosCrafteo;
            this.cantidadCrafteo = cantidadCrafteo;
            this.maxCantidadCrafteo = maxCantidadCrafteo;
        }
    }

}
