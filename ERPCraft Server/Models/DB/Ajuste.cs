using Npgsql;

namespace ERPCraft_Server.Models.DB
{
    public class Ajuste
    {
        public short id;
        public string name;
        public bool activado;
        public bool limpiarRobotGps;
        public short diasRobotGps;
        public bool limpiarRobotLogs;
        public short diasRobotLogs;
        public bool limpiarDroneGps;
        public short diasDroneGps;
        public bool limpiarDroneLogs;
        public short diasDroneLogs;
        public bool limpiarBateriaHistorial;
        public short horasBateriaHistorial;
        public bool vacuumLimpiar;
        public bool reindexLimpiar;
        public short pingInterval;
        public short timeout;
        public int puertoWeb;
        public int puertoOC;
        public int hashIteraciones;
        public bool limpiarNotificaciones;
        public short horasNotificaciones;

        public Ajuste()
        {

        }

        public Ajuste(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt16(0);
            this.name = rdr.GetString(1);
            this.activado = rdr.GetBoolean(2);
            this.limpiarRobotGps = rdr.GetBoolean(3);
            this.diasRobotGps = rdr.GetInt16(4);
            this.limpiarRobotLogs = rdr.GetBoolean(5);
            this.diasRobotLogs = rdr.GetInt16(6);
            this.limpiarDroneGps = rdr.GetBoolean(7);
            this.diasDroneGps = rdr.GetInt16(8);
            this.limpiarDroneLogs = rdr.GetBoolean(9);
            this.diasDroneLogs = rdr.GetInt16(10);
            this.limpiarBateriaHistorial = rdr.GetBoolean(11);
            this.horasBateriaHistorial = rdr.GetInt16(12);
            this.vacuumLimpiar = rdr.GetBoolean(13);
            this.reindexLimpiar = rdr.GetBoolean(14);
            this.pingInterval = rdr.GetInt16(15);
            this.timeout = rdr.GetInt16(16);
            this.puertoWeb = rdr.GetInt32(17);
            this.puertoOC = rdr.GetInt32(18);
            this.hashIteraciones = rdr.GetInt32(19);
            this.limpiarNotificaciones = rdr.GetBoolean(20);
            this.horasNotificaciones = rdr.GetInt16(21);
        }

        public bool isValid()
        {
            if (this.name == null || this.name.Length == 0)
                return false;
            if (this.diasRobotGps < 0 || this.diasRobotLogs < 0 || this.diasDroneGps < 0 || this.diasDroneLogs < 0
                || this.horasBateriaHistorial < 0 || this.puertoWeb < 1 || this.puertoWeb > 65535 || this.puertoOC < 1
                || this.puertoOC > 65535 || this.hashIteraciones < 1 || this.hashIteraciones > 500000)
                return false;
            return true;
        }

        /// <summary>
        /// Establecer los parámetros por defecto para los ajustes iniciales del software.
        /// Al iniciarse el servidor, de no encontrar ajustes, guardará y utilizará estos ajustes.
        /// No establecer en el constructor sin parametros para evitar conflictos con el serializador JSON.
        /// </summary>
        public void defaultAjustes()
        {
            this.id = 1;
            this.name = "Configuración general";
            this.activado = true;
            this.vacuumLimpiar = true;
            this.reindexLimpiar = true;
            this.pingInterval = 5;
            this.timeout = 30;
            this.puertoWeb = 32324;
            this.puertoOC = 32325;
            this.hashIteraciones = 250000;
            this.limpiarNotificaciones = true;
            this.horasNotificaciones = 48;
        }
    }
}
