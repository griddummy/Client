
public class LoginResultSerializer : Serializer
{

    public bool Serialize(LoginResultData data)
    {
        bool ret = true;
        ret &= Serialize(data.result);
        return ret;
    }
    public bool Deserialize(ref LoginResultData element)
    {
        if (GetDataSize() == 0)
        {
            // 데이터가 설정되지 않았다.
            return false;
        }

        bool ret = true;
        string total;        
        ret &= Deserialize(out total, (int)GetDataSize());
        element.result = total;
        return ret;
    }
}

