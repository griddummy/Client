
public class EnterRoomSerializer : Serializer {

    public bool Serialize(EnterRoomData data)
    {
        bool ret = true;
        ret &= Serialize(data.roomNumber);
        return ret;
    }

    public bool Deserialize(ref EnterRoomData element)
    {
        if (GetDataSize() == 0)
        {
            // 데이터가 설정되지 않았다.
            return false;
        }

        bool ret = true;
        byte roomNumber = 0;
        ret &= Deserialize(ref roomNumber);
        element.roomNumber = roomNumber;
        return ret;
    }
}
