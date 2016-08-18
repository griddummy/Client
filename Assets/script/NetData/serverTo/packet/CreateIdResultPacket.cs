using UnityEngine;
using System.Collections;

public class CreateIdResultPacket : IPacket<CreateIdResultData>
{
    CreateIdResultData m_data;

    public CreateIdResultPacket(CreateIdResultData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public CreateIdResultPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        CreateIdResultSerializer serializer = new CreateIdResultSerializer();
        serializer.SetDeserializedData(data);
        m_data = new CreateIdResultData();
        serializer.Deserialize(ref m_data);
    }

    public byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        CreateIdResultSerializer serializer = new CreateIdResultSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }

    public CreateIdResultData GetData() // 데이터 얻기(수신용)
    {
        return m_data;
    }

    public int GetPacketId()
    {
        return (int)ServerPacketId.CreateIdResult;
    }
}
