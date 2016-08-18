using UnityEngine;
using System.Collections;

public class EnterRoomResultSerializer : Serializer {

    public bool Serialize(EnterRoomResultData data)
    {
        bool ret = true;
        ret &= Serialize(data.result);
        ret &= Serialize(data.hostIP);
        return ret;
    }

    public bool Deserialize(ref EnterRoomResultData element)
    {
        if (GetDataSize() == 0)
        {
            // 데이터가 설정되지 않았다.
            return false;
        }

        bool ret = true;
        byte result = 0;
        string hostIP;
        ret &= Deserialize(ref result);
        ret &= Deserialize(out hostIP, (int)GetDataSize()-1);
        element.result = result;
        element.hostIP = hostIP;
        return ret;
    }
}
