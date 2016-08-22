// 새로운 입장 게스트 알림
public class P2PNewGuestEnterPacket : IPacket<P2PNewGuestEnterData>
{

    public class P2PNewGuestEnterSerializer : Serializer
    {
        public bool Serialize(P2PNewGuestEnterData data)
        {
            bool ret = true;
            ret &= Serialize(data.guestIndex);
            ret &= Serialize(data.userName);
            return ret;
        }

        public bool Deserialize(ref P2PNewGuestEnterData element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;
            ret &= Deserialize(ref element.guestIndex);
            ret &= Deserialize(out element.userName, (int)GetDataSize()-1);
            return ret;
        }
    }
    P2PNewGuestEnterData m_data;

    public P2PNewGuestEnterPacket(P2PNewGuestEnterData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public P2PNewGuestEnterPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        P2PNewGuestEnterSerializer serializer = new P2PNewGuestEnterSerializer();
        serializer.SetDeserializedData(data);
        m_data = new P2PNewGuestEnterData();
        serializer.Deserialize(ref m_data);
    }

    public byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        P2PNewGuestEnterSerializer serializer = new P2PNewGuestEnterSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }

    public P2PNewGuestEnterData GetData() // 데이터 얻기(수신용)
    {
        return m_data;
    }

    public int GetPacketId()
    {
        return (int)P2PPacketType.NewGuestEnter;
    }
}
