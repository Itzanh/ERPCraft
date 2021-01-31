namespace ERPCraft_Server.Models.DB.Robots
{
    public class RobotEnsamblado
    {
        public short computerCase;
        public short upgrade1;
        public short upgrade2;
        public short upgrade3;
        public short upgrade4;
        public short upgrade5;
        public short upgrade6;
        public short upgrade7;
        public short upgrade8;
        public short upgrade9;
        public short container1;
        public short container2;
        public short container3;
        public short card1;
        public short card2;
        public short card3;
        public short cpu;
        public short ram1;
        public short ram2;
        public short eeprom;
        public short hdd;

        public RobotEnsamblado()
        {
        }

        /// <summary>
        /// COPY CONSTRUCTOR
        /// Esta clase llega de la web como un RobotEnsambladoSet para poder referirse al ID del robot para modificar, y se hace cast es esta clase 
        /// solo con datos para guardar en la base de datos. Se hace cast para quitar el atributo del ID de Robot. El problema es que a pesar del cast, el atributo
        /// siguie en memoria y se ve en la serialización JSON al guardar en base de datos:
        /// {"robotId":4,"computerCase":-1,"upgrade1":-1,"upgrade2":-1,"upgrade3":-1,"upgrade4":-1,"upgrade5":-1,"upgrade6":-1,"upgrade7":-1,"upgrade8":-1,"upgrade9":-1,"container1":-1,"container2":-1,"container3":-1,"card1":-1,"card2":-1,"card3":-1,"cpu":-1,"ram1":-1,"ram2":-1,"eeprom":-1,"hdd":-1}
        /// 
        /// Este constructor pretende copiar el objeto en memoria sin los atributos heredados.
        /// </summary>
        public RobotEnsamblado(RobotEnsamblado robotEnsamblado)
        {
            this.computerCase = robotEnsamblado.computerCase;
            this.upgrade1 = robotEnsamblado.upgrade1;
            this.upgrade2 = robotEnsamblado.upgrade2;
            this.upgrade3 = robotEnsamblado.upgrade3;
            this.upgrade4 = robotEnsamblado.upgrade4;
            this.upgrade5 = robotEnsamblado.upgrade5;
            this.upgrade6 = robotEnsamblado.upgrade6;
            this.upgrade7 = robotEnsamblado.upgrade7;
            this.upgrade8 = robotEnsamblado.upgrade8;
            this.upgrade9 = robotEnsamblado.upgrade9;
            this.container1 = robotEnsamblado.container1;
            this.container2 = robotEnsamblado.container2;
            this.container3 = robotEnsamblado.container3;
            this.card1 = robotEnsamblado.card1;
            this.card2 = robotEnsamblado.card2;
            this.card3 = robotEnsamblado.card3;
            this.cpu = robotEnsamblado.cpu;
            this.ram1 = robotEnsamblado.ram1;
            this.ram2 = robotEnsamblado.ram2;
            this.eeprom = robotEnsamblado.eeprom;
            this.hdd = robotEnsamblado.hdd;
        }
    }

    public class RobotEnsambladoSet : RobotEnsamblado
    {
        public short robotId;

        public RobotEnsambladoSet()
        {
        }

        public RobotEnsamblado toRobotEnsamblado()
        {
            return new RobotEnsamblado((RobotEnsamblado)this);
        }
    }
}
