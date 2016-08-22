using UnityEngine;
using System.Collections;

public class P2PLoadCompletePacket : IPacket<P2PLoadCompleteData>
{

    public class P2PLoadCompleteSerializer : Serializer
    {
        public bool Serialize(P2PLoadCompleteData data)
        {
            bool ret = true;
            ret &= Serialize(data.playerIndex);
            return ret;
        }

        public bool Deserialize(ref P2PLoadCompleteData element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;

            ret &= Deserialize(ref element.playerIndex);
            return ret;
        }
    }
    P2PLoadCompleteData m_data;

    public P2PLoadCompletePacket(P2PLoadCompleteData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public P2PLoadCompletePacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        P2PLoadCompleteSerializer serializer = new P2PLoadCompleteSerializer();
        serializer.SetDeserializedData(data);
        m_data = new P2PLoadCompleteData();
        serializer.Deserialize(ref m_data);
    }

    public byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        P2PLoadCompleteSerializer serializer = new P2PLoadCompleteSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }

    public P2PLoadCompleteData GetData() // 데이터 얻기(수신용)
    {
        return m_data;
    }

    public int GetPacketId()
    {
        return (int)P2PPacketType.LoadComplete;
    }
}
