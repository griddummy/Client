using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour {

    public GameObject prefabLoadingBar;
    public Transform transGridLayout;

    List<PlayerLoadingBar> listBar = new List<PlayerLoadingBar>();
    RoomInfo curRoominfo;
    NetManager netManager;
    AsyncOperation ao;
    void Start()
    {
        netManager = GameManager.instance.netManager;
        curRoominfo = GameManager.instance.currentRoomInfo;

        // 방 인원수만큼 로딩바를 만든다.
        CreateLoadingBar(curRoominfo.PlayerCount);

        //TODO
        // 게임상태가 로딩이 아니라면 
        // 로그인씬으로 되돌아간다.

        // 로딩정보 패킷 리시버 등록
        netManager.RegisterReceiveNotificationP2P((int)P2PPacketType.Loading, OnReceiveGuestLoading);
        netManager.RegisterReceiveNotificationP2P((int)P2PPacketType.StartGameScene, OnReceiveStartGameScene);

        //TODO
        // 맵선택
        string mapScene = "GameScene";

        // 로딩
        ao = SceneManager.LoadSceneAsync(mapScene);
        StartCoroutine(StartLoadingGameScene(ao));
    }


    //로딩바를 만드는 메서드
    void CreateLoadingBar(int count) 
    {        
        for(int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefabLoadingBar); // 로딩바 오브젝트 생성
            PlayerLoadingBar bar = obj.AddComponent<PlayerLoadingBar>(); // 로딩바 스크립트 붙이기
            obj.transform.SetParent(transGridLayout); // 그리드오브젝트에 추가
            listBar.Add(bar); // 로딩바 저장
        }        
    }


    // [ 게스트, 호스트 모두 수신하는 패킷리시버 메서드] - 로딩진행값 수신
    void OnReceiveGuestLoading(Socket client, byte[] data)
    {
        P2PLoadingPacket packet = new P2PLoadingPacket(data);
        P2PLoadingData dataLoading = packet.GetData();
        
        // 자신이 호스트라면 다른 게스트들에게 브로드캐스트한다.
        // 자신이 호스트라면 자신과 모든 게스트들이 로딩 값이 100에 도달했으면 게임씬으로 전환한다.

        // 자신이 게스트라면 호스트에게 전송한다.
    }


    // [호스트가 게스트에게 보내는 패킷 리시버] - 게임씬 시작
    void OnReceiveStartGameScene(Socket client, byte[] data)
    {
        
    }
    

    // 씬 로딩을 하고, 1초마다 현재 로딩값을 전송하는 코루틴
    IEnumerator StartLoadingGameScene(AsyncOperation _ao) 
    {
        

        _ao.allowSceneActivation = false; // 로딩이 끝나도 씬 전환이 일어나지 않도록 막는다.

        while (!_ao.isDone) // 로딩이 완료되기 전까지 반복
        {
            yield return new WaitForSeconds(1f); // 1초마다 다음루틴 실행

            // 백분율로 변경
            int percent = (int)(_ao.progress * 100f);
            
            if (percent < 100) // 100% 로딩 되기 전까지
            {
                SetLoadingProgress(curRoominfo.myIndex, percent); // 화면 표시
                SendLoadingProgress(curRoominfo.myIndex, percent); // 백분율로 전환
            }
        }// end Of While

        // 화면에 100 표시
        SetLoadingProgress(curRoominfo.myIndex, 100);
        
        // 100%패킷전송
        SendLoadingProgress(curRoominfo.myIndex, 100);
    }
    void SendLoadingProgress(int indexPlayer, int percent)
    {
        P2PLoadingData dataLoading = new P2PLoadingData(); // 로딩 값을 담을 데이터 객체
        dataLoading.guestIndex = (byte)indexPlayer; // 나의 인덱스
        dataLoading.percent = (byte)percent;
        P2PLoadingPacket endPacket = new P2PLoadingPacket(dataLoading);

        if (curRoominfo.isHost)
        {
            //자신이 호스트라면 모든 게스트에게 보낸다.
            netManager.SendToAllGuest(endPacket);
        }
        else
        {
            // 자신이 게스트라면 호스트에게 보낸다
            netManager.SendToHost(endPacket);
        }
    }

    void DeleteLoadingBar()
    {
        
    }

    void SetPlayerInfoInLoadingbar(int index, string player)
    {

    }

    void SetLoadingProgress(int index, int percent)
    {

    }
}

////