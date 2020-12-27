using ERPCraft_Server.Storage;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace serverHashes
{

    public class NetEventIO
    {
        public const int CURRENT_VERSION = 1;

        public readonly string id;
        public NetworkConnectionAbstract client;
        private NetEventIOClientState state;
        private readonly NetEventIOServer_Options options;
        public DBStorage db;

        public OnDisconnect onDisconnect;

        private readonly ConcurrentDictionary<string, OnMessage> messageListeners;
        private readonly ConcurrentDictionary<string, OnCallback> callbackQueue;
        private readonly ConcurrentDictionary<string, OnMessageWithCallback> messageListenersWithCallback;
        private readonly ConcurrentDictionary<string, OnBinaryMessage> binaryMessageListeners;

        public delegate void OnMessage(NetEventIO sender, OnMessageEventArgs e);
        public delegate void OnBinaryMessage(NetEventIO sender, BinaryMessage message);
        public delegate void Callback(string messageToCallback);
        public delegate void OnCallback(NetEventIO sender, string response);
        public delegate void OnMessageWithCallback(NetEventIO sender, OnMessageEventArgs e, Callback callback);

        public delegate void OnDisconnect(NetEventIO sender);

        private NetEventIO()
        {
            this.messageListeners = new ConcurrentDictionary<string, OnMessage>();
            this.callbackQueue = new ConcurrentDictionary<string, OnCallback>();
            this.messageListenersWithCallback = new ConcurrentDictionary<string, OnMessageWithCallback>();
            this.binaryMessageListeners = new ConcurrentDictionary<string, OnBinaryMessage>();

            this.onDisconnect = null;
            state = NetEventIOClientState.Connected;
            this.id = Guid.NewGuid().ToString();
        }

        public NetEventIO(NetworkConnectionAbstract client, NetEventIOServer_Options options) : this()
        {
            this.client = client;
            this.options = options;
        }

        public void Run()
        {
            this.Run(false);
        }

        public void Run(bool debug)
        {
            byte[] message;
            bool isBinary;

            // get messages
            while (this.client.isConnected())
            {
                if (debug) Console.WriteLine("Esperant a rebre un missatge");
                if (!this.client.receiveData(out message, out isBinary) || message == null)
                {
                    Console.WriteLine("ERROR REBENT MISSATGE");
                    this.disconnected();
                    return;
                }
                if (isBinary)
                {
                    BinaryMessage binaryMessage = this.parseMessage(message);
                    if (binaryMessage != null)
                        this.handleBinaryEvent(binaryMessage);
                    continue;
                }
                Message newMessage;
                try
                {
                    newMessage = this.parseMessage(Encoding.UTF8.GetString(message));
                }
                catch (Exception)
                {
                    continue;
                }
                if (newMessage != null)
                {
                    if (debug) Console.WriteLine("rebre un missatge " + newMessage.eventName + " " + newMessage.packetType);
                    // switch by packet type on the initialization
                    switch (newMessage.packetType)
                    {
                        case PacketType.C_init:
                            {
                                this.handleInit(newMessage);
                                break;
                            }
                        case PacketType.C_authSend:
                            {
                                this.handleAuth(newMessage);
                                break;
                            }
                        case PacketType.C_sendEvent:
                            {
                                this.handleEvent(newMessage);
                                break;
                            }
                        case PacketType.C_sendEventForCallback:
                            {
                                this.handleEventWithCallback(newMessage);
                                break;
                            }
                        case PacketType.C_subscribe:
                            {
                                this.handleSubscription(newMessage.eventName);
                                break;
                            }
                        case PacketType.C_unsubscribe:
                            {
                                this.handleUnsubscription(newMessage.eventName);
                                break;
                            }
                        case PacketType.C_unsubscribeAll:
                            {
                                this.handleUnsubscriptionAll();
                                break;
                            }
                        case PacketType.C_sendCallbackForEvent:
                            {
                                this.handleCallbackResponse(newMessage);
                                break;
                            }
                        default:
                            {
                                Console.WriteLine("Possible communication error with the client, one message was dropped.");
                                break;
                            }
                    }
                }
            }

            Console.WriteLine("HE ACABAT");
            this.disconnected();
        }

        private void disconnected()
        {
            Console.WriteLine("connection was closed");
            if (this.state == NetEventIOClientState.Disconnected) return;
            this.state = NetEventIOClientState.Disconnected;
            this.client.disconnect();
            if (this.onDisconnect != null) onDisconnect(this);

            this.callbackQueue.Clear();
            this.messageListeners.Clear();
            this.binaryMessageListeners.Clear();
            this.messageListenersWithCallback.Clear();
        }

        private Message parseMessage(string message)
        {
            try
            {
                // initial check
                if (message[message.Length - 1] != '$') return null;

                // get UUID
                int uuidEnd = message.IndexOf('$');
                if (uuidEnd == -1) return null;
                string uuid = message.Substring(0, uuidEnd);

                message = message.Substring(uuidEnd + 1);

                // get metadata
                int packetInitializationEnd = message.IndexOf('$');
                if (packetInitializationEnd == -1) return null;
                string[] metadata = message.Substring(0, packetInitializationEnd).Split(':');
                if (metadata.Length != 3) return null;

                // get message
                message = message.Substring(packetInitializationEnd + 1, (message.Length - (packetInitializationEnd + 2)));

                Message newMessage = new Message();

                newMessage.id = uuid;

                newMessage.packetType = (PacketType)Int32.Parse(metadata[0]);
                newMessage.eventName = metadata[1];
                newMessage.command = metadata[2];

                newMessage.message = message;

                return newMessage;
            }
            catch (Exception) { return null; }
        }

        private BinaryMessage parseMessage(byte[] message)
        {
            try
            {
                // get the header size
                byte[] headerLengthInt = new byte[4];
                Array.Copy(message, 0, headerLengthInt, 0, headerLengthInt.Length);
                int headerLength = BitConverter.ToInt32(message, 0);

                // get the string header
                byte[] binaryHeader = new byte[headerLength];
                Array.Copy(message, 4, binaryHeader, 0, headerLength);
                string header = Encoding.UTF8.GetString(binaryHeader);

                // get the message length
                byte[] messageLengthInt = new byte[4];
                Array.Copy(message, headerLength + 4, messageLengthInt, 0, messageLengthInt.Length);
                int messageLength = BitConverter.ToInt32(messageLengthInt, 0);

                // get the binary message
                byte[] binaryMessage = new byte[messageLength];
                Array.Copy(message, headerLength + 8, binaryMessage, 0, binaryMessage.Length);

                // initial check
                if (header[header.Length - 1] != '$') return null;

                // get UUID
                int uuidEnd = header.IndexOf('$');
                if (uuidEnd == -1) return null;
                string uuid = header.Substring(0, uuidEnd);

                header = header.Substring(uuidEnd + 1);

                // get metadata
                int packetInitializationEnd = header.IndexOf('$');
                if (packetInitializationEnd == -1) return null;
                string[] metadata = header.Substring(0, packetInitializationEnd).Split(':');
                if (metadata.Length != 3) return null;

                BinaryMessage newMessage = new BinaryMessage();

                newMessage.id = uuid;

                newMessage.packetType = (PacketType)Int32.Parse(metadata[0]);
                newMessage.eventName = metadata[1];
                newMessage.command = metadata[2];

                newMessage.message = binaryMessage;

                return newMessage;
            }
            catch (Exception e) { Console.WriteLine("No se pot parsejar " + e.ToString()); return null; }
        }

        private string serializeMessage(Message m)
        {
            try
            {
                StringBuilder str = new StringBuilder();
                str
                    .Append(m.id)
                    .Append('$')
                    .Append((int)m.packetType).Append(':')
                    .Append(m.eventName).Append(':')
                    .Append(m.command)
                    .Append('$').Append(m.message).Append('$');
                return str.ToString();
            }
            catch (Exception) { return null; }
        }

        private byte[] serializeMessage(BinaryMessage m)
        {
            try
            {
                MemoryStream ms = new MemoryStream();

                StringBuilder str = new StringBuilder();
                str
                    .Append(m.id)
                    .Append('$')
                    .Append((int)m.packetType).Append(':')
                    .Append(m.eventName).Append(':')
                    .Append(m.command)
                    .Append('$');

                // write header
                byte[] header = Encoding.UTF8.GetBytes(str.ToString());
                ms.Write(BitConverter.GetBytes(header.Length), 0, 4);
                ms.Write(header, 0, header.Length);

                // write message
                ms.Write(BitConverter.GetBytes(m.message.Length), 0, 4);
                ms.Write(m.message, 0, m.message.Length);

                // write verification char
                ms.Write(Encoding.UTF8.GetBytes("$"), 0, 1);
                return ms.ToArray();
            }
            catch (Exception) { return null; }
        }

        private bool sendMessage(Message m)
        {
            string serializedMessage = this.serializeMessage(m);
            return this.client.sendData(Encoding.UTF8.GetBytes(serializedMessage), false);
        }

        private bool sendMessage(BinaryMessage bm)
        {
            byte[] serializedMessage = this.serializeMessage(bm);
            return this.client.sendData(serializedMessage, true);
        }

        private void handleInit(Message m)
        {
            if (m.packetType != PacketType.C_init) return;
            string version = m.message;

            int versionNumber;
            if (!Int32.TryParse(version, out versionNumber)) return;

            if (versionNumber == CURRENT_VERSION)
            {
                this.sendMessage(new Message(PacketType.S_initOk, "", "", this.id));
                this.state = NetEventIOClientState.Initialized;
            }
            else
            {
                this.sendMessage(new Message(PacketType.S_initErr));
            }
        }

        private void handleAuth(Message m)
        {
            if (this.state == NetEventIOClientState.Authenticated)
            {
                this.state = NetEventIOClientState.Authenticated;
                this.sendMessage(new Message(PacketType.S_authOk));
                return;
            }

            if (m.command.Equals("token"))
            {
                if (this.options.onTokenLogin(this, m.message, this.client.addr))
                {
                    this.state = NetEventIOClientState.Authenticated;
                    this.sendMessage(new Message(PacketType.S_authOk));
                }
                else
                {
                    this.sendMessage(new Message(PacketType.S_authErr));
                }
            }
            else
            {
                if (this.options.onPasswordLogin(this, m.message))
                {
                    this.state = NetEventIOClientState.Authenticated;
                    this.sendMessage(new Message(PacketType.S_authOk));
                }
                else
                {
                    this.sendMessage(new Message(PacketType.S_authErr));
                }
            }
        }

        private void handleEvent(Message m)
        {
            if (this.state != NetEventIOClientState.Authenticated) return;
            // search event by name in the dictionary
            OnMessage listener = null;
            try
            {
                listener = this.messageListeners[m.eventName];
            }
            catch (Exception) { return; }
            if (listener == null) return;

            // emit event
            listener(this, new OnMessageEventArgs(m));
        }

        public bool on(string eventName, OnMessage listener)
        {
            if (eventName.Contains(':') || eventName.Contains('$') || listener == null) return false;
            try
            {
                return this.messageListeners.TryAdd(eventName, listener);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool on(string eventName, OnBinaryMessage listener)
        {
            if (eventName.Contains(':') || eventName.Contains('$') || listener == null) return false;
            try
            {
                this.binaryMessageListeners[eventName] = listener;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool on(string eventName, OnMessageWithCallback listenerWithCallback)
        {
            if (eventName.Contains(':') || eventName.Contains('$') || listenerWithCallback == null) return false;
            try
            {
                return this.messageListenersWithCallback.TryAdd(eventName, listenerWithCallback);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool handleEventWithCallback(Message m)
        {
            if (this.state != NetEventIOClientState.Authenticated) return false;
            OnMessageWithCallback listener = null;
            try
            {
                listener = this.messageListenersWithCallback[m.eventName];
            }
            catch (Exception) { return false; }
            if (listener == null) return false;

            // emit event
            listener(this, new OnMessageEventArgs(m), (string messageToCallback) =>
            {
                this.sendMessage(new Message(m.id, PacketType.S_sendCallbackForEvent, m.eventName, m.command, messageToCallback));
            });
            return true;
        }

        public bool handleBinaryEvent(BinaryMessage m)
        {
            if (this.state != NetEventIOClientState.Authenticated || m.packetType != PacketType.C_binaryEvent) return false;
            OnBinaryMessage listener = null;
            try
            {
                listener = this.binaryMessageListeners[m.eventName];
            }
            catch (Exception) { return false; }
            if (listener == null) { return false; }

            // emit event
            listener(this, m);
            return true;
        }

        public bool removeEvent(string eventName)
        {
            if (eventName.Contains(':') || eventName.Contains('$')) return false;
            try
            {
                return this.messageListeners.TryRemove(eventName, out _);
            }
            catch (Exception) { return false; }
        }

        public bool emit(Message m)
        {
            return this.emit(m.eventName, m.command, m.message);
        }

        public bool emit(string eventName)
        {
            return this.emit(eventName, "", "");
        }

        public bool emit(string eventName, string command)
        {
            return this.emit(eventName, command, "");
        }

        public bool emit(string eventName, string command, string message)
        {
            if (this.state != NetEventIOClientState.Authenticated) return false;

            Message m = new Message(PacketType.S_sendEvent, eventName, command, message);
            return this.sendMessage(m);
        }

        public bool emit(string eventName, string command, byte[] message)
        {
            if (this.state != NetEventIOClientState.Authenticated) return false;

            BinaryMessage m = new BinaryMessage(PacketType.S_binaryEvent, eventName, command, message);
            return this.sendMessage(m);
        }

        public bool handleCallbackResponse(Message m)
        {
            if (this.state != NetEventIOClientState.Authenticated) return false;
            try
            {
                if (!this.callbackQueue.TryGetValue(m.id, out OnCallback callback)) return false;
                new Thread(new ThreadStart(() =>
                {
                    callback(this, m.message);
                })).Start();
                return this.callbackQueue.TryRemove(m.id, out _);
            }
            catch (Exception) { return false; }
        }

        public bool emit(string eventName, OnCallback callback)
        {
            return this.emit(eventName, "", "", callback);
        }

        public bool emit(string eventName, string command, string message, OnCallback callback)
        {
            Message m = new Message(PacketType.S_sendEventForCallback, eventName, command, message);

            if (this.callbackQueue.TryAdd(m.id, callback))
                return this.sendMessage(m);
            else
                return false;
        }

        private void handleSubscription(string topicName)
        {
            if (this.state != NetEventIOClientState.Authenticated) return;

            this.options.onSubscribe(this, topicName);
        }

        private void handleUnsubscription(string topicName)
        {
            if (this.state != NetEventIOClientState.Authenticated) return;

            this.options.onUnsubscribe(this, topicName);
        }

        public bool subscriptionPush(string topicName, SubscriptionChangeType changeType, int position)
        {
            if (changeType != SubscriptionChangeType.delete) return false;
            return this.subscriptionPush(topicName, changeType, position, "");
        }

        public bool subscriptionPush(string topicName, SubscriptionChangeType changeType, int position, string newValue)
        {
            if (this.state != NetEventIOClientState.Authenticated) return false;

            Message m = new Message(PacketType.S_subscriptionPush, topicName, ((int)changeType) + "," + position, newValue);
            return this.sendMessage(m);
        }


        private void handleUnsubscriptionAll()
        {
            if (this.state != NetEventIOClientState.Authenticated) return;

            this.options.onUnsubscribeAll(this);
        }
    }
}
