

public class CreateRoomResultPacket : IPacket<CreateRoomResultData>
{
    CreateRoomResultData m_data;

    public CreateRoomResultPacket(CreateRoomResultData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public CreateRoomResultPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        CreateRoomResultSerializer serializer = new CreateRoomResultSerializer();
        serializer.SetDeserializedData(data);
        m_data = new CreateRoomResultData();
        serializer.Deserialize(ref m_data);
    }

    public byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        CreateRoomResultSerializer serializer = new CreateRoomResultSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }

    public CreateRoomResultData GetData() // 데이터 얻기(수신용)
    {
        return m_data;
    }

    public int GetPacketId()
    {
        return (int)ServerPacketId.CreateRoomResult;
    }
}