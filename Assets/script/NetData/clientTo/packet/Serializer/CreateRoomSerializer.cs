
public class CreateRoomSerializer : Serializer
{
    public bool Serialize(CreateRoomData data)
    {
        bool ret = true;
        ret &= Serialize(data.map);
        ret &= Serialize(data.title);
        return ret;
    }

    public bool Deserialize(ref CreateRoomData element)
    {
        if (GetDataSize() == 0)
        {
            // 데이터가 설정되지 않았다.
            return false;
        }

        bool ret = true;
        byte mapType = 0;
        string title;
        ret &= Deserialize(ref mapType);
        ret &= Deserialize(out title, (int)GetDataSize());
        element.map = mapType;
        element.title = title; 
        return ret;
    }
}