namespace serverHashes
{
    public class Message
    {
        public string id;
        public PacketType packetType;
        public string eventName;
        public string command;
        public string message;

        public Message()
        {
        }

        public Message(PacketType packetType)
        {
            this.packetType = packetType;
            this.eventName = "";
            this.command = "";
            this.message = "";
        }

        public Message(PacketType packetType, string eventName, string command, string message)
        {
            this.id = System.Guid.NewGuid().ToString();
            this.packetType = packetType;
            this.eventName = eventName;
            this.command = command;
            this.message = message;
        }

        public Message(string id, PacketType packetType, string eventName, string command, string message)
        {
            this.id = id;
            this.packetType = packetType;
            this.eventName = eventName;
            this.command = command;
            this.message = message;
        }
    }
}
