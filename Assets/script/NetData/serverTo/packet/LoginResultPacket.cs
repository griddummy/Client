

public class LoginResultPacket : IPacket<LoginResultData>
{
    LoginResultData m_data;

    public LoginResultPacket(LoginResultData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public LoginResultPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        LoginResultSerializer serializer = new LoginResultSerializer();
        serializer.SetDeserializedData(data);
        m_data = new LoginResultData();
        serializer.Deserialize(ref m_data);
    }

    public byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        LoginResultSerializer serializer = new LoginResultSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }

    public LoginResultData GetData() // 데이터 얻기(수신용)
    {
        return m_data;
    }

    public int GetPacketId()
    {
        return (int)ServerPacketId.LoginResult;
    }
}

