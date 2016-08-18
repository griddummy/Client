

public class CreateRoomResultSerializer : Serializer{

    public bool Serialize(CreateRoomResultData data)
    {
        bool ret = true;
        ret &= Serialize(data.result);
        return ret;
    }
    public bool Deserialize(ref CreateRoomResultData element)
    {
        if (GetDataSize() == 0)
        {
            // 데이터가 설정되지 않았다.
            return false;
        }

        bool ret = true;
        byte result = 0;
        ret &= Deserialize(ref result);
        element.result = result;
        return ret;
    }
}
