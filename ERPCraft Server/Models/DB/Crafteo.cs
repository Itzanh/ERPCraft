using Npgsql;
using System;

namespace ERPCraft_Server.Models.DB
{
    public class Crafteo
    {
        public int id;
        public string name;
        public short idArticuloResultado;
        public short cantidadResultado;
        public short? idArticuloSlot1;
        public short cantidadArticuloSlot1;
        public short? idArticuloSlot2;
        public short cantidadArticuloSlot2;
        public short? idArticuloSlot3;
        public short cantidadArticuloSlot3;
        public short? idArticuloSlot4;
        public short cantidadArticuloSlot4;
        public short? idArticuloSlot5;
        public short cantidadArticuloSlot5;
        public short? idArticuloSlot6;
        public short cantidadArticuloSlot6;
        public short? idArticuloSlot7;
        public short cantidadArticuloSlot7;
        public short? idArticuloSlot8;
        public short cantidadArticuloSlot8;
        public short? idArticuloSlot9;
        public short cantidadArticuloSlot9;
        public DateTime dateAdd;
        public DateTime dateUpdate;
        public bool off;

        public Crafteo()
        {
        }

        public Crafteo(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt32(0);
            this.name = rdr.GetString(1);
            this.idArticuloResultado = rdr.GetInt16(2);
            this.cantidadResultado = rdr.GetInt16(3);
            if (rdr.IsDBNull(4))
                this.idArticuloSlot1 = null;
            else
                this.idArticuloSlot1 = rdr.GetInt16(4);
            this.cantidadArticuloSlot1 = rdr.GetInt16(5);
            if (rdr.IsDBNull(6))
                this.idArticuloSlot2 = null;
            else
                this.idArticuloSlot2 = rdr.GetInt16(6);
            this.cantidadArticuloSlot2 = rdr.GetInt16(7);
            if (rdr.IsDBNull(8))
                this.idArticuloSlot3 = null;
            else
                this.idArticuloSlot3 = rdr.GetInt16(8);
            this.cantidadArticuloSlot3 = rdr.GetInt16(9);
            if (rdr.IsDBNull(10))
                this.idArticuloSlot4 = null;
            else
                this.idArticuloSlot4 = rdr.GetInt16(10);
            this.cantidadArticuloSlot4 = rdr.GetInt16(11);
            if (rdr.IsDBNull(12))
                this.idArticuloSlot5 = null;
            else
                this.idArticuloSlot5 = rdr.GetInt16(12);
            this.cantidadArticuloSlot5 = rdr.GetInt16(13);
            if (rdr.IsDBNull(14))
                this.idArticuloSlot6 = null;
            else
                this.idArticuloSlot6 = rdr.GetInt16(14);
            this.cantidadArticuloSlot6 = rdr.GetInt16(15);
            if (rdr.IsDBNull(16))
                this.idArticuloSlot7 = null;
            else
                this.idArticuloSlot7 = rdr.GetInt16(16);
            this.cantidadArticuloSlot7 = rdr.GetInt16(17);
            if (rdr.IsDBNull(18))
                this.idArticuloSlot8 = null;
            else
                this.idArticuloSlot8 = rdr.GetInt16(18);
            this.cantidadArticuloSlot8 = rdr.GetInt16(19);
            if (rdr.IsDBNull(20))
                this.idArticuloSlot9 = null;
            else
                this.idArticuloSlot9 = rdr.GetInt16(20);
            this.cantidadArticuloSlot9 = rdr.GetInt16(21);
            this.dateAdd = rdr.GetDateTime(22);
            this.dateUpdate = rdr.GetDateTime(23);
            this.off = rdr.GetBoolean(24);
        }

        public bool isValid()
        {
            if (this.name == null || this.name.Length == 0 || this.idArticuloResultado <= 0 || this.cantidadResultado <= 0)
                return false;

            if (this.idArticuloSlot1 == null && this.idArticuloSlot2 == null && this.idArticuloSlot3 == null && this.idArticuloSlot4 == null &&
                this.idArticuloSlot5 == null && this.idArticuloSlot6 == null && this.idArticuloSlot7 == null && this.idArticuloSlot8 == null &&
                this.idArticuloSlot9 == null)
                return false;

            if (this.idArticuloSlot1 != null && (this.idArticuloSlot1 <= 0 || this.cantidadArticuloSlot1 <= 0))
                return false;
            if (this.idArticuloSlot2 != null && (this.idArticuloSlot2 <= 0 || this.cantidadArticuloSlot2 <= 0))
                return false;
            if (this.idArticuloSlot3 != null && (this.idArticuloSlot3 <= 0 || this.cantidadArticuloSlot3 <= 0))
                return false;
            if (this.idArticuloSlot4 != null && (this.idArticuloSlot4 <= 0 || this.cantidadArticuloSlot4 <= 0))
                return false;
            if (this.idArticuloSlot5 != null && (this.idArticuloSlot5 <= 0 || this.cantidadArticuloSlot5 <= 0))
                return false;
            if (this.idArticuloSlot6 != null && (this.idArticuloSlot6 <= 0 || this.cantidadArticuloSlot6 <= 0))
                return false;
            if (this.idArticuloSlot7 != null && (this.idArticuloSlot7 <= 0 || this.cantidadArticuloSlot7 <= 0))
                return false;
            if (this.idArticuloSlot8 != null && (this.idArticuloSlot8 <= 0 || this.cantidadArticuloSlot8 <= 0))
                return false;
            if (this.idArticuloSlot9 != null && (this.idArticuloSlot9 <= 0 || this.cantidadArticuloSlot9 <= 0))
                return false;

            return true;
        }
    }

    public class CrafteoHead
    {
        public int id;
        public string name;
        public string articuloResultadoName;

        public CrafteoHead(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt32(0);
            this.name = rdr.GetString(1);
            this.articuloResultadoName = rdr.GetString(2);
        }
    }

    public class CrafteoQuery
    {
        public int offset;
        public int limit;
    }

    public class Smelting
    {
        public int id;
        public string name;
        public short idArticuloResultado;
        public short cantidadResultado;
        public short idArticuloEntrada;
        public short cantidadEntrada;
        public DateTime dateAdd;
        public DateTime dateUpdate;
        public bool off;

        public Smelting()
        {
        }

        public Smelting(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt32(0);
            this.name = rdr.GetString(1);
            this.idArticuloResultado = rdr.GetInt16(2);
            this.cantidadResultado = rdr.GetInt16(3);
            this.idArticuloEntrada = rdr.GetInt16(4);
            this.cantidadEntrada = rdr.GetInt16(5);
            this.dateAdd = rdr.GetDateTime(6);
            this.dateUpdate = rdr.GetDateTime(7);
            this.off = rdr.GetBoolean(8);

        }

        public bool isValid()
        {
            return !(this.name == null || this.name.Length == 0 || this.idArticuloResultado <= 0 || this.cantidadResultado <= 0
                || this.idArticuloEntrada <= 0 || this.cantidadEntrada <= 0);
        }
    }

    public class SmeltingHead
    {
        public int id;
        public string name;
        public string articuloResultadoName;

        public SmeltingHead(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt32(0);
            this.name = rdr.GetString(1);
            this.articuloResultadoName = rdr.GetString(2);
        }
    }
}
