using UnityEngine;
using System.Collections;
using System.Net.Sockets;

// 로딩이 끝났으면 불리는 이벤트
public class LoadComplete : MonoBehaviour {

    MainManager main;
    NetManager net;
    
    void Awake()
    {
        //메인 매니저, 넷 매니저, 게임매니저 가져오기
        main = MainManager.instance;
        net = main.netManager;

        // 리시버 등록
        net.RegisterReceiveNotificationP2P((int)P2PPacketType.LoadComplete, OnReceiveLoadComplete);
        net.RegisterReceiveNotificationP2P((int)P2PPacketType.StartGame, OnReceiveStartGame);

    }

    void OnDestroy()
    {
        // 리시버 해제
        net.UnRegisterReceiveNotificationP2P((int)P2PPacketType.LoadComplete);
        net.UnRegisterReceiveNotificationP2P((int)P2PPacketType.StartGame);
    }

    void Start()
    {           

        //TODO
        // 게임매니저.Init?
       
        // 로딩끝 패킷 전송

    }

    void Update()
    {
        // 모두 로딩이 끝났는지 확인
        // 모두 로딩이 끝났으면 게임 시작 패킷 전송
    }
    
    
    // 게임 시작 패킷 수신 메서드 [호스트->게스트]
    void OnReceiveStartGame(Socket client, byte[] data)
    {
        //TODO
        // 카메라 이벤트 실행
    }

    // 로딩 끝 패킷 수신 메서드 [ 게스트 -> 호스트 ]
    void OnReceiveLoadComplete(Socket client, byte[] data)
    {
        //TODO
        // 카운트 증가        
    }    
}
