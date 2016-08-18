
public enum P2PPacketType
{
    EnterRoom = 1,
    EnterRoomResult,
    NewGuestEnter,
    GuestLeave,
    HostLeave,

    Chat,
    GameStartCount,
    StartLoadingScene,
    Loading,
    StartGameScene,

    EndGame
}
public class P2PEnterRoomData
{
    public string userName;
}
public class P2PEnterRoomResultData // Host To Guest 입장요청 결과
{
    public enum RESULT { Fail = 0, Success }
    public byte result; // 0이면 실패 1이면 성공
    public byte myIndex; // 룸에서 나의 번호
    public byte otherGuestCount; // 다른 게스트 명수
    public byte[] otherGuestIndex; // 다른 게스트 번호
    public string[] otherGuestID;  // 다른 게스트 아이디
}
public class P2PNewGuestEnterData // Host To All 게스트 입장 알림
{
    public byte guestIndex;
    public string userName;
}
public class P2PGuestLeaveData // Guest To Host + Host To All, 게스트 퇴장 알림
{
    public byte guestIndex;
}
public class P2PChatData // 채팅 데이터
{
    public byte guestIndex;
    public string chat;
}
public class P2PLoadingData // 로딩 데이터
{
    public byte guestIndex;
    public byte percent; //0~100
}