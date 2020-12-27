namespace serverHashes
{
    public class NetEventIOServer_Options
    {
        public OnPasswordLogin onPasswordLogin;
        public OnTokenLogin onTokenLogin;
        public OnSubscribe onSubscribe;
        public OnUnsubscribe onUnsubscribe;
        public OnUnsubscribeAll onUnsubscribeAll;

        public delegate bool OnPasswordLogin(NetEventIO client, string pwd);
        public delegate bool OnTokenLogin(NetEventIO client, string token, string addr);

        public delegate bool OnSubscribe(NetEventIO sender, string topicName);
        public delegate bool OnUnsubscribe(NetEventIO sender, string topicName);
        public delegate bool OnUnsubscribeAll(NetEventIO sender);

        public NetEventIOServer_Options()
        {
            onPasswordLogin = (NetEventIO client, string pwd) =>
            {
                return false;
            };
            onTokenLogin = (NetEventIO client, string token, string addr) =>
            {
                return false;
            };
            onSubscribe = (NetEventIO sender, string topicName) =>
            {
                return false;
            };
            onUnsubscribe = (NetEventIO sender, string topicName) =>
            {
                return false;
            };
        }

        public NetEventIOServer_Options(OnPasswordLogin onPasswordLogin, OnTokenLogin onTokenLogin) : this()
        {
            this.onPasswordLogin = onPasswordLogin;
            this.onTokenLogin = onTokenLogin;

            this.onSubscribe = (NetEventIO sender, string topicName) => { return false; };
            this.onUnsubscribe = (NetEventIO sender, string topicName) => { return false; };
            this.onUnsubscribeAll = (NetEventIO sender) => { return false; };
        }

        public NetEventIOServer_Options(OnPasswordLogin onPasswordLogin, OnTokenLogin onTokenLogin, OnSubscribe onSubscribe,
            OnUnsubscribe onUnsubscribe, OnUnsubscribeAll onUnsubscribeAll) : this(onPasswordLogin, onTokenLogin)
        {
            this.onSubscribe = onSubscribe;
            this.onUnsubscribe = onUnsubscribe;
            this.onUnsubscribeAll = onUnsubscribeAll;
        }
    }
}
