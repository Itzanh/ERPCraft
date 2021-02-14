using Npgsql;
using System;

namespace ERPCraft_Server.Models.DB.Robots
{
    public class Robot
    {
        public short id;
        public string name;
        public Guid uuid;
        public short tier;
        public short numeroSlots;
        public short numeroStacks;
        public short numeroItems;
        public char estado;
        public int totalEnergia;
        public int energiaActual;
        public bool upgradeGenerador;
        public bool upgradeInventoryController;
        public bool upgradeGeolyzer;
        public short itemsGenerador;
        public DateTime fechaConexion;
        public DateTime fechaDesconexion;
        public string descripcion;
        public bool upgradeGps;
        public short posX;
        public short posY;
        public short posZ;
        public short complejidad;
        public DateTime dateAdd;
        public DateTime dateUpd;
        public bool off;
        public short offsetPosX;
        public short offsetPosY;
        public short offsetPosZ;
        public bool notificacionConexion;
        public bool notificacionDesconexion;
        public bool notificacionBateriaBaja;

        public Robot()
        {
            this.estado = 'F';
        }

        public Robot(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt16(0);
            this.name = rdr.GetString(1);
            this.uuid = rdr.GetGuid(2);
            this.tier = rdr.GetInt16(3);
            this.numeroSlots = rdr.GetInt16(4);
            this.numeroStacks = rdr.GetInt16(5);
            this.numeroItems = rdr.GetInt16(6);
            this.estado = rdr.GetChar(7);
            this.totalEnergia = rdr.GetInt32(8);
            this.energiaActual = rdr.GetInt32(9);
            this.upgradeGenerador = rdr.GetBoolean(10);
            this.itemsGenerador = rdr.GetInt16(11);
            this.fechaConexion = rdr.GetDateTime(12);
            this.fechaDesconexion = rdr.GetDateTime(13);
            this.descripcion = rdr.GetString(14);
            this.upgradeGps = rdr.GetBoolean(15);
            this.posX = rdr.GetInt16(16);
            this.posY = rdr.GetInt16(17);
            this.posZ = rdr.GetInt16(18);
            this.complejidad = rdr.GetInt16(19);
            this.dateAdd = rdr.GetDateTime(20);
            this.dateUpd = rdr.GetDateTime(21);
            this.off = rdr.GetBoolean(22);
            this.offsetPosX = rdr.GetInt16(23);
            this.offsetPosY = rdr.GetInt16(24);
            this.offsetPosZ = rdr.GetInt16(25);
            this.notificacionConexion = rdr.GetBoolean(26);
            this.notificacionDesconexion = rdr.GetBoolean(27);
            this.notificacionBateriaBaja = rdr.GetBoolean(28);
            this.upgradeInventoryController = rdr.GetBoolean(29);
            this.upgradeGeolyzer = rdr.GetBoolean(30);
        }

        public bool isValid()
        {
            if (this.estado != 'O' && this.estado != 'M' && this.estado != 'F' && this.estado != 'L' && this.estado != 'B')
                return false;

            return !(this.name == null || this.name.Length == 0 || this.tier < 1 || this.tier > 3 || this.numeroSlots < 0 || this.totalEnergia <= 0
                || this.energiaActual < 0 || this.itemsGenerador < 0 || this.itemsGenerador > 64 || this.descripcion == null || this.complejidad < 0);
        }

    }

    public class RobotHead
    {
        public short id;
        public string name;
        public Guid uuid;

        public RobotHead(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt16(0);
            this.name = rdr.GetString(1);
            this.uuid = rdr.GetGuid(2);
        }
    }

    public class RobotQuery
    {
        public string text;
        public bool off;

        public RobotQuery()
        {
            this.text = String.Empty;
            this.off = false;
        }
    }

    public class RobotCommand
    {
        public short robotId;
        public string command;

        public RobotCommand()
        {
        }
    }

    public enum RobotReferencesTable
    {
        RobotInventario,
        RobotGPS,
        RobotLogs,
        RobotOrdenesMinado
    }

    public class RobotReferences
    {
        public RobotReferencesTable reference;
        public short[] ids;
    }


}
