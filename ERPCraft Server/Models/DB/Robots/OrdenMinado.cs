using Npgsql;
using System;

namespace ERPCraft_Server.Models.DB.Robots
{
    public class OrdenMinado
    {
        public int id;
        public string name;
        public short size;
        public short? robot;
        public short posX;
        public short posY;
        public short posZ;
        public short facing;
        public short gpsX;
        public short gpsY;
        public short gpsZ;
        public int numeroItems;
        public DateTime dateAdd;
        public DateTime dateUpd;
        public DateTime? dateInicio;
        public DateTime? dateFin;
        public string descripcion;
        public char estado;
        public char unidadRecarga;
        public short energiaRecarga;
        public char modoMinado;
        public bool shutdown;
        public bool notificacion;

        public OrdenMinado()
        {
        }

        public OrdenMinado(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt32(0);
            this.name = rdr.GetString(1);
            this.size = rdr.GetInt16(2);
            if (rdr.IsDBNull(3))
                robot = null;
            else
                this.robot = rdr.GetInt16(3);
            this.posX = rdr.GetInt16(4);
            this.posY = rdr.GetInt16(5);
            this.posZ = rdr.GetInt16(6);
            this.facing = rdr.GetInt16(7);
            this.gpsX = rdr.GetInt16(8);
            this.gpsY = rdr.GetInt16(9);
            this.gpsZ = rdr.GetInt16(10);
            this.numeroItems = rdr.GetInt32(11);
            this.dateAdd = rdr.GetDateTime(12);
            this.dateUpd = rdr.GetDateTime(13);
            if (rdr.IsDBNull(14))
                this.dateInicio = null;
            else
                this.dateInicio = rdr.GetDateTime(14);
            if (rdr.IsDBNull(15))
                this.dateFin = null;
            else
                this.dateFin = rdr.GetDateTime(15);
            this.descripcion = rdr.GetString(16);
            this.estado = rdr.GetChar(17);
            this.unidadRecarga = rdr.GetChar(18);
            this.energiaRecarga = rdr.GetInt16(19);
            this.modoMinado = rdr.GetChar(20);
            this.shutdown = rdr.GetBoolean(21);
            this.notificacion = rdr.GetBoolean(22);
        }

        /// <summary>
        /// Ya hay un constructor que tome 'NpgsqlDataReader', para el front-end, pero no necesitamos todos los campos para los enviar a los robots
        /// Este metodo construye solo con los campos necesarios para los robots
        /// </summary>
        /// <param name="rdr"></param>
        public void robotParse(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt32(0);
            this.size = rdr.GetInt16(1);
            this.posX = rdr.GetInt16(2);
            this.posY = rdr.GetInt16(3);
            this.posZ = rdr.GetInt16(4);
            this.facing = rdr.GetInt16(5);
            this.gpsX = rdr.GetInt16(6);
            this.gpsY = rdr.GetInt16(7);
            this.gpsZ = rdr.GetInt16(8);
            this.unidadRecarga = rdr.GetChar(9);
            this.energiaRecarga = rdr.GetInt16(10);
            this.modoMinado = rdr.GetChar(11);
            this.shutdown = rdr.GetBoolean(12);
        }

        public bool isValid()
        {
            if (unidadRecarga != '%' && unidadRecarga != '=')
                return false;

            if (modoMinado != 'O' && modoMinado != 'E')
                return false;

            return !(this.name == null || this.name.Length == 0 || this.size <= 0 || (this.robot != null && this.robot <= 0) || this.facing < 0 || this.facing > 4
                || this.numeroItems < 0 || this.descripcion == null || this.energiaRecarga <= 0);
        }
    }

    public class OrdenMinadoQuery
    {
        public char[] estado;
        public short robot;

        public OrdenMinadoQuery()
        {
        }

        public bool isValid()
        {
            if (robot < 0 || estado == null)
                return false;

            foreach (char estado in this.estado)
            {
                if (estado != 'Q' && estado != 'R' && estado != 'E' && estado != 'O')
                    return false;
            }
            return true;
        }
    }

    public class OrdenMinadoInventario
    {
        public short numeroSlot;
        public short cant;
        public ArticuloSlot articulo;

        public OrdenMinadoInventario(short numeroSlot, short cant, ArticuloSlot articulo)
        {
            this.numeroSlot = numeroSlot;
            this.cant = cant;
            this.articulo = articulo;
        }
    }

    public class OrdenMinadoInventarioGet
    {
        public short numeroSlot;
        public short cant;
        public short articulo;

        public OrdenMinadoInventarioGet(NpgsqlDataReader rdr)
        {
            this.numeroSlot = rdr.GetInt16(0);
            if (rdr.IsDBNull(1))
                this.articulo = 0;
            else
                this.articulo = rdr.GetInt16(1);
            this.cant = rdr.GetInt16(2);
        }
    }

    public class OrdenMinadoInventarioSet
    {
        public string idArticulo;
        public short cant;

        public OrdenMinadoInventarioSet(string idArticulo, short cant)
        {
            this.idArticulo = idArticulo;
            this.cant = cant;
        }
    }
}
