using Npgsql;

namespace ERPCraft_Server.Models.DB.Robots
{
    public class RobotInventario
    {
        public short numeroSlot;
        public short cant;
        public ArticuloSlot articulo;

        public RobotInventario(short numeroSlot, short cant, ArticuloSlot articulo)
        {
            this.numeroSlot = numeroSlot;
            this.cant = cant;
            this.articulo = articulo;
        }
    }

    public class RobotInventarioGet
    {
        public short numeroSlot;
        public short cant;
        public short articulo;

        public RobotInventarioGet(NpgsqlDataReader rdr)
        {
            this.numeroSlot = rdr.GetInt16(0);
            this.cant = rdr.GetInt16(1);
            if (rdr.IsDBNull(2))
                this.articulo = 0;
            else
                this.articulo = rdr.GetInt16(2);
        }
    }

    public class RobotInventarioSet
    {
        public string idArticulo;
        public short cant;
        public string oreName;

        public RobotInventarioSet(string idArticulo, short cant, string oreName)
        {
            this.idArticulo = idArticulo;
            this.cant = cant;
            this.oreName = oreName;
        }
    }

}


