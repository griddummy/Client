
public class P2PEnterRoomResultPacket : IPacket<P2PEnterRoomResultData>
{

    public class P2PEnterRoomResultSerializer : Serializer
    {
        public bool Serialize(P2PEnterRoomResultData data)
        {
            bool ret = true;
            ret &= Serialize(data.result);
            if(data.result == (byte)P2PEnterRoomResultData.RESULT.Success)
            {
                ret &= Serialize(data.myIndex);
                ret &= Serialize(data.otherGuestCount);
                for (int i = 0; i < data.otherGuestCount; i++)
                {
                    ret &= Serialize(data.otherGuestIndex[i]);
                }
                for (int i = 0; i < data.otherGuestCount; i++)
                {
                    ret &= Serialize(data.otherGuestID[i]);
                    if (i < data.otherGuestCount - 1)
                        ret &= Serialize(".");
                }
            }
            return ret;
        }

        public bool Deserialize(ref P2PEnterRoomResultData element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;
            
            ret &= Deserialize(ref element.result);
            if (element.result == (byte)P2PEnterRoomResultData.RESULT.Fail)
                return false;
            ret &= Deserialize(ref element.myIndex);
            ret &= Deserialize(ref element.otherGuestCount);
            element.otherGuestIndex = new byte[element.otherGuestCount];            
            for(int i = 0; i < element.otherGuestCount; i++)
            {
                ret &= Deserialize(ref element.otherGuestIndex[i]);
            }
            string userName;
            ret &= Deserialize(out userName, (int)GetDataSize() - 3 + element.otherGuestCount); // byte+byte+byte+count*byte
            element.otherGuestID = userName.Split('.');
            return ret;
        }
    }
    P2PEnterRoomResultData m_data;

    public P2PEnterRoomResultPacket(P2PEnterRoomResultData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public P2PEnterRoomResultPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        P2PEnterRoomResultSerializer serializer = new P2PEnterRoomResultSerializer();
        serializer.SetDeserializedData(data);
        m_data = new P2PEnterRoomResultData();
        serializer.Deserialize(ref m_data);
    }

    public byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        P2PEnterRoomResultSerializer serializer = new P2PEnterRoomResultSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }

    public P2PEnterRoomResultData GetData() // 데이터 얻기(수신용)
    {
        return m_data;
    }

    public int GetPacketId()
    {
        return (int)P2PPacketType.EnterRoomResult;
    }
}
