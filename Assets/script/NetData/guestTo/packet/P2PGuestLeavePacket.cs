

public class P2PGuestLeavePacket : IPacket<P2PGuestLeaveData>
{
    public class P2PGuestLeaveSerializer : Serializer
    {
        public bool Serialize(P2PGuestLeaveData data)
        {
            bool ret = true;
            ret &= Serialize(data.guestIndex);
            return ret;
        }

        public bool Deserialize(ref P2PGuestLeaveData element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;
            ret &= Deserialize(ref element.guestIndex);
            return ret;
        }
    }

    P2PGuestLeaveData m_data;

    public P2PGuestLeavePacket(P2PGuestLeaveData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public P2PGuestLeavePacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        P2PGuestLeaveSerializer serializer = new P2PGuestLeaveSerializer();
        serializer.SetDeserializedData(data);
        m_data = new P2PGuestLeaveData();
        serializer.Deserialize(ref m_data);
    }

    public byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        P2PGuestLeaveSerializer serializer = new P2PGuestLeaveSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }

    public P2PGuestLeaveData GetData() // 데이터 얻기(수신용)
    {
        return m_data;
    }

    public int GetPacketId()
    {
        return (int)P2PPacketType.GuestLeave;
    }
}
