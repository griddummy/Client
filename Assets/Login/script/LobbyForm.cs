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
        netManager.RegisterReceiveNotificationServer((int)ServerPacketId.EnterRoomResult, OnReceiveEnterRoomResult);
        RequestRoomList();
    }
    

    protected override void OnPause()
    {
        // 네트워크 리시버 메서드 해제
        netManager.UnRegisterReceiveNotificationServer((int)ServerPacketId.GetRoomListResult);
        netManager.UnRegisterReceiveNotificationServer((int)ServerPacketId.CreateRoomResult);
        netManager.UnRegisterReceiveNotificationServer((int)ServerPacketId.EnterRoomResult);
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
    private void OnReceiveEnterRoomResult(Socket sock, byte[] data) // 방 입장 결과 리시버
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

        // 호스트에게 연결을 시도한다.
        Debug.Log("호스트 IP" + resultData.hostIP);
        if (GameManager.instance.netManager.ConnectToHost(resultData.hostIP))
        {
            dialogMessage.Alert("연결성공");
        }
        else
        {
            dialogMessage.Alert("연결실패");
            dialogMessage.Close(false, 2f);
            //TO DO 방퇴장 서버에게 알림
            return;
        }
        // 현재 플레이어 정보를 게스트로 설정한다. 
        Room curRoom = new Room(curSelectedRoom.hostId, curSelectedRoom.roomName, curSelectedRoom.mapType, Room.RoomState.waiting);        
        curRoom.playerMode = Room.Player.Guest;
        GameManager.instance.curRoom = curRoom;
        
        ChangeForm(typeof(RoomForm).Name); // 폼 변경
    }
    private void OnReceiveCreateRoomResult(Socket sock, byte[] data) // 방 생성 결과 리시버
    {
        CreateRoomResultPacket packet = new CreateRoomResultPacket(data);
        if(packet.GetData().result == CreateRoomResultData.Fail)
        {   
            dialogMessage.Alert("방 만들기 실패");
            dialogMessage.Close(false, 3f);
        }
        else // 방 생성 성공
        {
                
            // 현재 플레이어정보를 방의 호스트로 설정한다.
            LoginData loginData = GameManager.instance.login;
            Room curRoom = new Room(loginData.id, lastCreateRoomData.title, lastCreateRoomData.map, Room.RoomState.waiting);
            curRoom.playerMode = Room.Player.Host;
            GameManager.instance.curRoom = curRoom;            
            dialogMessage.Alert("방 생성에 성공하였습니다");
            ChangeForm(typeof(RoomForm).Name); // 폼 변경
        }
    }
}