namespace serverHashes
{
    public class BinaryMessage
    {
        public string id;
        public PacketType packetType;
        public string eventName;
        public string command;
        public byte[] message;

        public BinaryMessage()
        {
        }

        public BinaryMessage(PacketType packetType, string eventName, string command, byte[] message)
        {
            this.id = System.Guid.NewGuid().ToString();
            this.packetType = packetType;
            this.eventName = eventName;
            this.command = command;
            this.message = message;
        }

        public BinaryMessage(string id, PacketType packetType, string eventName, string command, byte[] message) : this(packetType, eventName, command, message)
        {
            this.id = id;
        }
    }
}
