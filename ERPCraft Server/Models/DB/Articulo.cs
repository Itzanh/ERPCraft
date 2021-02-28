using Npgsql;

namespace ERPCraft_Server.Models.DB
{
    public class Articulo
    {
        public short id;
        public string name;
        public string minecraftID;
        public int cantidad;
        public string descripcion;
        public string oreName;

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
            this.oreName = rdr.GetString(5);
        }

        private static bool minecraftIDisValid(string minecraftID)
        {
            if (minecraftID.IndexOf(':') <= 0 || minecraftID.LastIndexOf(':') == minecraftID.Length - 1)
                return false;

            const string charset = "abcdefghijklmnopqrstuvwxyz0123456789_:";
            for (int i = 0; i < minecraftID.Length; i++)
                if (charset.IndexOf(minecraftID[i]) < 0)
                    return false;

            return true;
        }

        internal bool isValid()
        {
            if (this.name == null || this.name.Equals(string.Empty) || this.minecraftID == null || this.minecraftID.Length == 0 || !minecraftIDisValid(this.minecraftID)
                || cantidad < 0 || this.descripcion == null || oreName == null)
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

        public ArticuloHead(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt16(0);
            this.name = rdr.GetString(1);
            this.minecraftID = rdr.GetString(2);
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
