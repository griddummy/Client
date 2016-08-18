
public class CreateRoomPacket : IPacket<CreateRoomData>
{
    CreateRoomData m_data;

    public CreateRoomPacket(CreateRoomData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public CreateRoomPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        CreateRoomSerializer serializer = new CreateRoomSerializer();
        serializer.SetDeserializedData(data);
        m_data = new CreateRoomData();
        serializer.Deserialize(ref m_data);
    }

    public byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        CreateRoomSerializer serializer = new CreateRoomSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }

    public CreateRoomData GetData() // 데이터 얻기(수신용)
    {
        return m_data;
    }

    public int GetPacketId()
    {
        return (int)ClientPacketId.CreateRoom;
    }
}
