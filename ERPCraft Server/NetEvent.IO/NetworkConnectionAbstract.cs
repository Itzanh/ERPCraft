using System.Net.Sockets;

namespace serverHashes
{
    public abstract class NetworkConnectionAbstract
    {
        public abstract string addr { get; }
        public abstract bool isSecure { get; }

        public abstract bool isConnected();
        public abstract bool receiveData(out byte[] receivedData, out bool isBinary);
        public abstract bool sendData(byte[] dataToSend, bool isBinary);
        public abstract void disconnect();
    }
}
