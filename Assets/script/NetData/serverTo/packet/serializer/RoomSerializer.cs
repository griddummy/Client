using System.Collections;
using UnityEngine;
public class RoomSerializer : Serializer
{
    public bool Serialize(Room[] data)
    {
        bool ret = true;
        byte roomDataLength;

        ret &= Serialize((byte)data.Length);

        for (int i = 0; i < data.Length; i++)
        {
            ret &= Serialize((byte)data[i].roomNum);
            ret &= Serialize((byte)data[i].mapType);

            roomDataLength = (byte)((data[i].hostId.Length + data[i].roomName.Length) + 1);
            ret &= Serialize(roomDataLength*2);
            ret &= Serialize(data[i].hostId);
            ret &= Serialize(".");
            ret &= Serialize(data[i].roomName);
        }

        return ret;
    }

    public bool Deserialize(ref Room[] element)
    {
        if (GetDataSize() == 0)
        {
            return false;
        }

        bool ret = true;

        byte roomCount = 0;
        byte roomNum = 0;
        byte mapType = 0;
        byte roomDataLength = 0;
        string data = "";
        string[] str;

        ret &= Deserialize(ref roomCount);        
        if (ret)
        {
            element = new Room[roomCount];
            Debug.Log("방 개수 : " + roomCount);
        }
        else
        {
            element = new Room[0];
            return false;
        }            
        for (int i = 0; i < roomCount; i++)
        {
            ret &= Deserialize(ref roomNum);
            ret &= Deserialize(ref mapType);
            ret &= Deserialize(ref roomDataLength);
            
            ret &= Deserialize(out data, (int)roomDataLength);
            str = data.Split('.');
            element[i] = new Room();
            element[i].roomNum = roomNum;
            element[i].mapType = mapType;
            Debug.Log(roomDataLength+":호스트이름.방이름:" + data);
            element[i].hostId = str[0];
            element[i].roomName = str[1];
        }

        return ret;
    }
}