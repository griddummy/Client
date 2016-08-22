using UnityEngine;
using System.Collections;

public class P2PChatPacket : IPacket<P2PChatData>
{
    public class P2PChatSerializer : Serializer
    {
        public bool Serialize(P2PChatData data)
        {
            bool ret = true;
            ret &= Serialize(data.guestIndex);
            ret &= Serialize(data.chat);
            return ret;
        }

        public bool Deserialize(ref P2PChatData element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;
            ret &= Deserialize(ref element.guestIndex);
            ret &= Deserialize(out element.chat, (int)GetDataSize()-1);            
            return ret;
        }
    }
    P2PChatData m_data;

    public P2PChatPacket(P2PChatData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public P2PChatPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        P2PChatSerializer serializer = new P2PChatSerializer();
        serializer.SetDeserializedData(data);
        m_data = new P2PChatData();
        serializer.Deserialize(ref m_data);
    }

    public byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        P2PChatSerializer serializer = new P2PChatSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }

    public P2PChatData GetData() // 데이터 얻기(수신용)
    {
        return m_data;
    }

    public int GetPacketId()
    {
        return (int)P2PPacketType.Chat;
    }
}
