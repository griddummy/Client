using UnityEngine;
using System.Collections;

public class LeaveRoomSerializer : Serializer
{
    public bool Serialize(LeaveRoomData data)
    {
        bool ret = true;
        ret &= Serialize(data.roomNum);
        return ret;
    }
    public bool Deserialize(ref LeaveRoomData element)
    {
        if (GetDataSize() == 0)
        {
            // 데이터가 설정되지 않았다.
            return false;
        }

        bool ret = true;
        byte roomNum = 0;
        ret &= Deserialize(ref roomNum);
        element.roomNum = roomNum;

        return ret;
    }
}

