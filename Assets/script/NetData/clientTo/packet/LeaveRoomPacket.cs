using UnityEngine;
using System.Collections;

public class LeaveRoomPacket : IPacket<LeaveRoomData>
{
    LeaveRoomData m_data;

    public LeaveRoomPacket(LeaveRoomData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public LeaveRoomPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        LeaveRoomSerializer serializer = new LeaveRoomSerializer();
        serializer.SetDeserializedData(data);
        m_data = new LeaveRoomData();
        serializer.Deserialize(ref m_data);
    }

    public byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        LeaveRoomSerializer serializer = new LeaveRoomSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }

    public LeaveRoomData GetData() // 데이터 얻기(수신용)
    {
        return m_data;
    }

    public int GetPacketId()
    {
        return (int)ClientPacketId.LeaveRoom;
    }
}

