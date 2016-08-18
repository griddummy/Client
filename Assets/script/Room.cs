using System.Net.Sockets;
using System.Collections.Generic;

public class Room
{
    public enum RoomState
    {
        empty,
        waiting,
        playing,
    }
    public enum MapType
    {
        Basic = 1
    }
    public int roomNum;
    public string roomName;

    //public Socket host;
    public string hostId;

    public int mapType;

    public RoomState roomState;
    public List<PlayerInfo> listPlayer;
    public int userNum;
    public const int maxUserNum = 4;
    public enum Player
    {
        Host, Guest
    }
    public Player playerMode;
    //초기화용
    public Room()
    {
        roomNum = 0;
        roomName = "";
        //host = null;
        hostId = "";
        mapType = (int)MapType.Basic;
        roomState = RoomState.empty;
        userNum = 0;
        listPlayer = new List<PlayerInfo>();
    }
    public Room(string newHostId, string newRoomName, int newMapType, RoomState newRoomState)
    {
        roomName = newRoomName;
        //host = newHost;
        hostId = newHostId;
        mapType = newMapType;
        roomState = newRoomState;
        userNum = 1;
        playerMode = Player.Host;
        listPlayer = new List<PlayerInfo>();
    }
    //생성용
    //public Room(Socket newHost, string newHostId, string newRoomName, int newMapType, RoomState newRoomState)
    //{
    //    roomName = newRoomName;
    //    host = newHost;
    //    hostId = newHostId;
    //    mapType = newMapType;
    //    roomState = newRoomState;
    //    userNum = 1;
    //}
}