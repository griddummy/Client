using UnityEngine;
using System.Collections;

public class EnterRoomResultPacket : IPacket<EnterRoomResultData> {

    EnterRoomResultData m_data;

    public EnterRoomResultPacket(EnterRoomResultData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public EnterRoomResultPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        EnterRoomResultSerializer serializer = new EnterRoomResultSerializer();
        serializer.SetDeserializedData(data);
        m_data = new EnterRoomResultData();
        serializer.Deserialize(ref m_data);
    }

    public byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        EnterRoomResultSerializer serializer = new EnterRoomResultSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }

    public EnterRoomResultData GetData() // 데이터 얻기(수신용)
    {
        return m_data;
    }

    public int GetPacketId()
    {
        return (int)ServerPacketId.EnterRoomResult;
    }
}
