using System;

namespace serverHashes
{
    public class OnMessageEventArgs : EventArgs
    {
        public Message message;

        public OnMessageEventArgs()
        {
            this.message = null;
        }

        public OnMessageEventArgs(Message message)
        {
            this.message = message;
        }
    }
}
