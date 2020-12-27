namespace serverHashes
{
    public enum PacketType
    {
        S_initOk,
        C_init,
        S_initErr,
        C_authSend,
        S_authOk,
        C_sendEvent,
        S_authErr,
        C_sendEventForCallback,
        S_sendEvent,
        C_sendCallbackForEvent,
        S_sendEventForCallback,
        C_subscribe,
        S_sendCallbackForEvent,
        C_unsubscribe,
        S_subscriptionPush,
        C_unsubscribeAll,
        S_binaryEvent,
        C_binaryEvent
    }
}
