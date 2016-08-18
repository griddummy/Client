using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Net.Sockets;

public class LobbyForm : UIForm {

    public DialogCreateRoom dialogCreateRoom;
    public DialogMessage dialogMessage;
    public GameObject prefabRoom;    
    public Text txtRoomInfo;    
    public Button btnPopupCreateRoom;    
    public Button btnEnterRoom;    
    public Button btnLogout;
    public GameObject viewScroll;
    
    private Dictionary<int,Room> listRoomInfo = new Dictionary<int,Room>();
    private Room curSelectedRoom;
    private GameManager gameManager;
    private NetManager netManager;
    private CreateRoomData lastCreateRoomData;
    private Coroutine corRequestRoomListLoop;
    protected override void Awake()
    {
        base.Awake();
        gameManager = GameManager.instance;
        netManager = gameManager.netManager;
        dialogCreateRoom.OnClosed += OnClickCreateRoom;
    }

    protected override void OnResume()
    {        
        ClearAll();            
        // 네트워크 리시버 메서드 등록  
        netManager.RegisterReceiveNotificationServer((int)ServerPacketId.GetRoomListResult, OnReceiveResultRoomList);
        netManager.RegisterReceiveNotificationServer((int)ServerPacketId.CreateRoomResult, OnReceiveCreateRoomResult);
        netManager.RegisterReceiveNotificationServer((int)ServerPacketId.EnterRoomResult, OnReceiveEnterRoomResultFromServer);
        
        netManager.RegisterReceiveNotificationP2P((int)P2PPacketType.EnterRoomResult, OnReceiveP2PEnterRoomResult);
        // 방 리스트 요청 5초마다 
        corRequestRoomListLoop = StartCoroutine(RequestRoomListLoop());
    }
    

    protected override void OnPause()
    {
        // 방 리스트 요청 루프 취소
        StopCoroutine(corRequestRoomListLoop);

        // 네트워크 리시버 메서드 해제
        netManager.UnRegisterReceiveNotificationServer((int)ServerPacketId.GetRoomListResult);
        netManager.UnRegisterReceiveNotificationServer((int)ServerPacketId.CreateRoomResult);
        netManager.UnRegisterReceiveNotificationServer((int)ServerPacketId.EnterRoomResult);

        netManager.UnRegisterReceiveNotificationP2P((int)P2PPacketType.EnterRoomResult);
    }

    public void UpdateList() // 방 정보 리스트로 UI 다시 그리기
    {
        ClearRoomObject(); // 방 오브젝트 삭제
        foreach (KeyValuePair<int, Room> kv in listRoomInfo) 
        {
            CreateRoomNode(kv.Value);           
        }
    }
    public void CreateRoomNode(Room info) // 방 오브젝트 한개 생성
    {
        GameObject obj = Instantiate(prefabRoom);
        UIRoomNode node = obj.GetComponent<UIRoomNode>();
        node.SetupNode(info.roomName, info.roomNum, OnClickRoomNode);
        obj.transform.SetParent(viewScroll.transform);
        obj.GetComponent<RectTransform>().localScale = Vector3.one;
    }

    private void ClearSelectRoomInfo() // 선택한 방 정보 표시 삭제
    {
        curSelectedRoom = null;
        txtRoomInfo.text = "";
    }

    private void ClearRoomObject() // 방 오브젝트 삭제
    {
        foreach (Transform child in viewScroll.transform) // 스크롤뷰 자식 오브젝트 삭제
        {
            Destroy(child.gameObject);
        }
    }
    private void ClearAll() // 모든 방 관련 정보 삭제
    {
        ClearSelectRoomInfo();  // 선택한 방 정보 표시 삭제
        ClearRoomObject();      // 방 오브젝트 삭제
        listRoomInfo.Clear();   // 방 정보 삭제
    }
    private void SetSelectedRoom()
    {
        if (curSelectedRoom == null)
            return;
        txtRoomInfo.text = curSelectedRoom.roomName;
    }
    private void OnClickRoomNode(int number) // 리스트에서 방 클릭 시, 방선택 + 방정보 표시
    {
        Room info;
        if(listRoomInfo.TryGetValue(number, out info))
        {
            curSelectedRoom = info;
            SetSelectedRoom();
        }
        else
        {            
            ClearSelectRoomInfo();
        }
    }
    public void OnClickCreateRoomPopup() // 방생성 팝업
    {
        dialogCreateRoom.Open();
    }
    public void OnClickLogout() // 로그아웃
    {
        // TO DO
    }
    public void OnClickEnterRoom() // 방입장
    {
        if (curSelectedRoom != null)
        {
            EnterRoomData data = new EnterRoomData();
            data.roomNumber = (byte)curSelectedRoom.roomNum;
            EnterRoomPacket packet = new EnterRoomPacket(data);
            netManager.SendToServer(packet);
        }
    }    
    public void OnClickCreateRoom(bool bPositive, object data) // 방생성 - 생성 요청
    {
        if (bPositive)
        {
            dialogMessage.Alert("방을 생성 중");
            CreateRoomData createRoomData = data as CreateRoomData;            
            createRoomData.map = (byte)Room.MapType.Basic; 
            lastCreateRoomData = createRoomData;
            Debug.Log("방생성 : "+createRoomData.title +" "+createRoomData.map);
            CreateRoomPacket packet = new CreateRoomPacket(createRoomData);
            netManager.SendToServer(packet);
        }
    }        
    IEnumerator RequestRoomListLoop()
    {
        while (true)
        {
            RequestRoomList();
            yield return new WaitForSeconds(5f);
        }
    }
    private void RequestRoomList() // 방리스트 요청 전송
    {
        RequestRoomlistPacket packet = new RequestRoomlistPacket();
        netManager.SendToServer(packet);
    }

    private void OnReceiveResultRoomList(Socket sock, byte[] data) // 방목록 결과 리시버
    {
        Room[] rooms = null;
        RoomSerializer serializer = new RoomSerializer();
        serializer.SetDeserializedData(data);
        if(serializer.Deserialize(ref rooms))
        {
            Debug.Log("받은 방 개수 : "+rooms.Length);
            listRoomInfo.Clear();
            foreach (Room room in rooms)
            {
                listRoomInfo.Add(room.roomNum,room);                
            }
            // 이전에 선택한 방이 현재 리스트에 그대로 존재하는지
            Room sameRoom;
            if(curSelectedRoom != null)
            {
                if (listRoomInfo.TryGetValue(curSelectedRoom.roomNum, out sameRoom))
                {
                    // 인덱스는 같고 제목이 다르면
                    if (curSelectedRoom.roomName != sameRoom.roomName)
                    {
                        // 다른방 이므로 선택한 방 정보 클리어
                        ClearSelectRoomInfo();
                    }
                }
            }            
            UpdateList();
        }
    }
    private void OnReceiveEnterRoomResultFromServer(Socket sock, byte[] data) // 방 입장 결과 리시버
    {        
        // 성공이면
        EnterRoomResultPacket packet = new EnterRoomResultPacket(data);
        EnterRoomResultData resultData = packet.GetData();
        if(resultData.result < 1)
        {
            dialogMessage.Alert("연결실패");
            dialogMessage.Close(false, 2f);
            return;
        }
        // 방입장 성공(From Server)

        // 호스트에게 연결을 시도한다.
        Debug.Log("호스트 IP" + resultData.hostIP);
        if (GameManager.instance.netManager.ConnectToHost(resultData.hostIP))
        {
            dialogMessage.Alert("연결성공");
        }
        else // 호스트에 연결 실패
        {
            dialogMessage.Alert("연결실패");
            dialogMessage.Close(false, 2f);
            
            // 서버에게 방퇴장 알림
            LeaveRoomData leaveData = new LeaveRoomData();
            leaveData.roomNum = (byte)curSelectedRoom.roomNum;
            LeaveRoomPacket leavePacket = new LeaveRoomPacket(leaveData);
            GameManager.instance.netManager.SendToServer(leavePacket);
            return;
        }
        // 입장요청 To Host
        P2PEnterRoomData enterRoomData = new P2PEnterRoomData();
        enterRoomData.userName = GameManager.instance.login.id;
        P2PEnterRoomPacket enterRoomPacket = new P2PEnterRoomPacket(enterRoomData);
        GameManager.instance.netManager.SendToHost(enterRoomPacket);

       
    }
    private void OnReceiveCreateRoomResult(Socket sock, byte[] data) // 방 생성 결과 리시버
    {
        CreateRoomResultPacket packet = new CreateRoomResultPacket(data);
        if(packet.GetData().roomNumber == CreateRoomResultData.Fail)
        {   
            dialogMessage.Alert("방 만들기 실패");
            dialogMessage.Close(false, 3f);
        }
        else // 방 생성 성공
        {
                
            // 현재 플레이어정보를 방의 호스트로 설정한다.
            LoginData loginData = GameManager.instance.login;
            RoomInfo roomInfo = new RoomInfo(packet.GetData().roomNumber, lastCreateRoomData.title, lastCreateRoomData.map, new PlayerInfo(loginData.id), RoomInfo.PlayerMode.Host);
            GameManager.instance.currentRoomInfo = roomInfo;
            dialogMessage.Alert("방 생성에 성공하였습니다");
            ChangeForm(typeof(RoomForm).Name); // 폼 변경
        }
    }
    private void OnReceiveP2PEnterRoomResult(Socket sock, byte[] data) // Host To Guest,방입장요청 결과
    {
        // From 호스트 방입장 요청  결과
        // 현재 방 정보를 매니저에 저장한다.
        P2PEnterRoomResultPacket resultPacket = new P2PEnterRoomResultPacket(data);
        P2PEnterRoomResultData resultData = resultPacket.GetData();
        if (resultData.result == (byte)P2PEnterRoomResultData.RESULT.Fail)
        {
            dialogMessage.Alert("방입장실패");
            dialogMessage.Close(false, 1f);
            GameManager.instance.netManager.DisconnectGuestSocket(); // 호스트와 연결 종료
            return;
        }

        // 성공
        RoomInfo roomInfo = new RoomInfo(curSelectedRoom.roomNum, curSelectedRoom.roomName, curSelectedRoom.mapType, new PlayerInfo(curSelectedRoom.hostId), RoomInfo.PlayerMode.Guest);
        Debug.Log("입장허가받음::이전 사람수:" + resultData.otherGuestCount+" myIndex:"+resultData.myIndex);
        // 방정보에 다른 사람 정보 넣기
        for(int i = 0; i < resultData.otherGuestCount; i++)
        {
            roomInfo.AddGuest(new PlayerInfo(resultData.otherGuestID[i]), resultData.otherGuestIndex[i]);
        }
        // 방정보에 내정보 넣기
        roomInfo.myIndex = resultData.myIndex;
        roomInfo.AddGuest(new PlayerInfo(GameManager.instance.login.id), resultData.myIndex);
        
        GameManager.instance.currentRoomInfo = roomInfo;
        ChangeForm(typeof(RoomForm).Name); // 폼 변경
    }
}