namespace LLNet
{
    public enum NetMessageType : byte
    {
        CONNECTION_ACK,
        USER_INFO,
        CHAT_WHISPER,
        CHAT_BROADCAST,
        CHAT_TEAM_MESSAGE,
        DISCONNECTION_ACK,
        MOVEMENT
    }

}// Namescape