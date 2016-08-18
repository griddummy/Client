using UnityEngine;
using System.Collections;

public class SystemMessage  {
    
    // Login    
    public const string FailedLogin = "아이디나 암호가 일치하지 않습니다";

    // 가입
    public const string NotEqualPw1Pw2 = "두 비밀번호가 일치하지 않습니다";
    public const string EmptyInfo = "모든 정보를 입력하세요";
    public const string AleadyExistID = "존재하는 ID 입니다";

    // MessageBox
    public const string ConnectToServer = "서버에 접속 중입니다";
}
