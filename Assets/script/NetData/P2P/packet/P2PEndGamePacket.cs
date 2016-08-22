using UnityEngine;
using System.Collections;

public class P2PEndGamePacket : IPacket<NullData>
{
    public byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        return new byte[0];
    }

    public NullData GetData() // 데이터 얻기(수신용)
    {
        return null;
    }

    public int GetPacketId()
    {
        return (int)P2PPacketType.EndGame;
    }
}
