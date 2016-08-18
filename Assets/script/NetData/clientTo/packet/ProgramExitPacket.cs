using UnityEngine;
using System.Collections;

public class ProgramExitPacket : IPacket<NullData>
{
    NullData m_data;

    public ProgramExitPacket(NullData data = null) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public ProgramExitPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        m_data = null;
    }

    public byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        return new byte[0];
    }

    public NullData GetData() // 데이터 얻기(수신용)
    {
        return m_data;
    }

    public int GetPacketId()
    {
        return (int)ClientPacketId.ProgramExit;
    }
}