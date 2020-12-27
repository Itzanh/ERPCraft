using Npgsql;
using System.Collections.Generic;

namespace ERPCraft_Server.Models.DB.Electrico
{
    public class RedElectrica
    {
        public short id;
        public string name;
        public long capacidadElectrica;
        public long energiaActual;
        public string descripcion;
        public List<Generador> generadores;
        public List<Bateria> baterias;

        public RedElectrica()
        {

        }

        public RedElectrica(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt16(0);
            this.name = rdr.GetString(1);
            this.capacidadElectrica = rdr.GetInt64(2);
            this.energiaActual = rdr.GetInt64(3);
            this.descripcion = rdr.GetString(4);
        }

        public bool isValid()
        {
            return !(this.name == null || this.name.Length == 0 || this.capacidadElectrica < 0 || this.energiaActual < 0 || this.descripcion == null);
        }

    }
}