
public class RoomInfo
{
    public enum State { Empty, Wait, Play } // 빈방, 대기, 게임중
    public const int MaxPlayer = 4;    // 최대 플레이어

    public int number;                  // 방번호
    public string strTitle;             // 방제목
    public string host;                 // 방장이름
    public string[] guest;              // 게스트 이름
    public int count;                   // 플레이어 수(방장포함)
    public State state;                 // 방 상태
}