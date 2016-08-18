
public class EnterRoomPacket :IPacket<EnterRoomData> {

    EnterRoomData m_data;

    public EnterRoomPacket(EnterRoomData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public EnterRoomPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        EnterRoomSerializer serializer = new EnterRoomSerializer();
        serializer.SetDeserializedData(data);
        m_data = new EnterRoomData();
        serializer.Deserialize(ref m_data);
    }

    public byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        EnterRoomSerializer serializer = new EnterRoomSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }

    public EnterRoomData GetData() // 데이터 얻기(수신용)
    {
        return m_data;
    }

    public int GetPacketId()
    {
        return (int)ClientPacketId.EnterRoom;
    }
}
