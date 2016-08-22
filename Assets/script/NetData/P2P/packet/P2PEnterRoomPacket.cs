
// 방입장 요청 패킷
public class P2PEnterRoomPacket : IPacket<P2PEnterRoomData>
{

    public class P2PEnterRoomSerializer : Serializer
    {
        public bool Serialize(P2PEnterRoomData data)
        {
            bool ret = true;
            ret &= Serialize(data.userName);           
            return ret;
        }

        public bool Deserialize(ref P2PEnterRoomData element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;
            ret &= Deserialize(out element.userName, (int)GetDataSize());           
            return ret;
        }
    }
    P2PEnterRoomData m_data;

    public P2PEnterRoomPacket(P2PEnterRoomData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public P2PEnterRoomPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        P2PEnterRoomSerializer serializer = new P2PEnterRoomSerializer();
        serializer.SetDeserializedData(data);
        m_data = new P2PEnterRoomData();
        serializer.Deserialize(ref m_data);
    }

    public byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        P2PEnterRoomSerializer serializer = new P2PEnterRoomSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }

    public P2PEnterRoomData GetData() // 데이터 얻기(수신용)
    {
        return m_data;
    }

    public int GetPacketId()
    {
        return (int)P2PPacketType.EnterRoom;
    }
}
