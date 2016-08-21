using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour {

    public GameObject prefabLoadingBar; // 로딩바 프리팹
    public Transform transGridLayout; // 로딩바의 부모, 1열 그리드뷰

    List<PlayerLoadingBar> listBar = new List<PlayerLoadingBar>(); // 로딩바 스크립트
    RoomInfo curRoominfo; // 현재 방 정보
    NetManager netManager; // 네트워크 매니저
    AsyncOperation ao; // 비동기 로딩 정보
    int LoadingEndPlayerCount = 0;
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
        string mapScene = "SceneTestGame";

        // 로딩
        ao = SceneManager.LoadSceneAsync(mapScene);
        StartCoroutine(StartLoadingGameScene(ao));
    }

    void OnDestroy()
    {
        netManager.UnRegisterReceiveNotificationP2P((int)P2PPacketType.Loading);
        netManager.UnRegisterReceiveNotificationP2P((int)P2PPacketType.StartGameScene);
    }

    // 로딩바를 만든다.
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


    // 로딩값 패킷을 받으면 실행되는 메서드 [ 게스트, 호스트 둘다 받음 ]
    void OnReceiveGuestLoading(Socket client, byte[] data)
    {
        P2PLoadingPacket packet = new P2PLoadingPacket(data);
        P2PLoadingData dataLoading = packet.GetData();
                
        // 로딩바 UI에 변경값을 넣는다.
        UpdateLoadingBar(dataLoading.guestIndex, dataLoading.percent);

        // 자신이 호스트라면       
        if (curRoominfo.isHost)
        {
            // 100퍼센트면 카운트 증가
            LoadingEndPlayerCount++;

            // 이 패킷을 보낸 게스트를 제외하고 다른 게스트들에게 브로드캐스트한다.  
            SendLoadingProgress(dataLoading.guestIndex, dataLoading.percent, client);
        }
    }


    // 게임씬 시작 패킷을 받으면 실행되는 메서드 [ 게스트만 받음 ]
    void OnReceiveStartGameScene(Socket client, byte[] data)
    {
        ao.allowSceneActivation = true; // 막아놨던 로딩을 허가한다.
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
                UpdateLoadingBar(curRoominfo.myIndex, percent); // 화면 표시
                SendLoadingProgress(curRoominfo.myIndex, percent); // 백분율로 전환
            }
        }// end Of While
        
        // 화면에 100 표시
        UpdateLoadingBar(curRoominfo.myIndex, 100);
        
        // 100%패킷전송
        SendLoadingProgress(curRoominfo.myIndex, 100);

        // 호스트는 다른사람들이 끝날때까지 검사한다.
        if (curRoominfo.isHost)
        {
            LoadingEndPlayerCount++; // 일단 자신이 끝났으므로 카운트 증가.

            // 로딩이 끝난 인원 < 현재 룸인원
            while(LoadingEndPlayerCount < curRoominfo.PlayerCount)
            {
                yield return new WaitForSeconds(1f);
            }
            // 모두 로딩이 끝나면 게임 씬 시작 패킷을 전송한다.
            P2PStartGameScenePacket startPacket = new P2PStartGameScenePacket();
            netManager.SendToAllGuest(startPacket);

            ao.allowSceneActivation = true; // 막아놧던 로딩을 허가한다.
        }        
    }

    // 로딩 값을 전송한다.
    void SendLoadingProgress(int indexPlayer, int percent, Socket excludeSock = null)
    {
        P2PLoadingData dataLoading = new P2PLoadingData(); // 로딩 값을 담을 데이터 객체
        dataLoading.guestIndex = (byte)indexPlayer; // 나의 인덱스
        dataLoading.percent = (byte)percent;
        P2PLoadingPacket loadingPacket = new P2PLoadingPacket(dataLoading);

        if (curRoominfo.isHost) // 자신이 호스트일때
        {
            if (excludeSock != null) // 어떤 게스트의 정보를 다른게스트들에게 뿌려줄 때, 해당 소켓을 제외하고 보낸다.
            {
                netManager.SendToAllGuest(excludeSock, loadingPacket);
            }
            else // 호스트 자신의 정보라면
            {
                // 모두에게 보낸다
                netManager.SendToAllGuest(loadingPacket);
            }            
        }
        else
        {
            // 자신이 게스트라면 호스트에게 보낸다
            netManager.SendToHost(loadingPacket);
        }
    }    

    // 로딩바에 플레이어 이름을 입력한다.
    void SetupPlayerInfoInLoadingbar(int index, string player)
    {
        listBar[index].SetPlayerName(player);
    }

    // 로딩바의 값을 수정한다.
    void UpdateLoadingBar(int index, int percent)
    {
        listBar[index].SetLoadingProgress(percent);
    }
}

////