
public class P2PLoadingPacket : IPacket<P2PLoadingData>
{
    public class P2PLoadingSerializer : Serializer
    {
        public bool Serialize(P2PLoadingData data)
        {
            bool ret = true;
            ret &= Serialize(data.guestIndex);
            ret &= Serialize(data.percent);
            return ret;
        }

        public bool Deserialize(ref P2PLoadingData element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;
            ret &= Deserialize(ref element.guestIndex);
            ret &= Deserialize(ref element.percent);

            return ret;
        }
    }
    P2PLoadingData m_data;

    public P2PLoadingPacket(P2PLoadingData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public P2PLoadingPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        P2PLoadingSerializer serializer = new P2PLoadingSerializer();
        serializer.SetDeserializedData(data);
        m_data = new P2PLoadingData();
        serializer.Deserialize(ref m_data);
    }

    public byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        P2PLoadingSerializer serializer = new P2PLoadingSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }

    public P2PLoadingData GetData() // 데이터 얻기(수신용)
    {
        return m_data;
    }

    public int GetPacketId()
    {
        return (int)P2PPacketType.Loading;
    }
}
