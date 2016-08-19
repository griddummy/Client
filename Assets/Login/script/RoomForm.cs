using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Net.Sockets;

public class RoomForm : UIForm {

    public DialogMessage dialogMessage;
    public List<Text> listPlayer;
    public Button btnStart;
    public Button btnExit;
    public Button btnChat;
    public InputField inputChat;
    public Text txtChatWindow;

    private GameManager gm;
    private RoomInfo curRoomInfo;
    private NetManager netManger;

    protected override void OnResume()
    {
        gm = GameManager.instance;
        netManger = GameManager.instance.netManager;
        curRoomInfo = GameManager.instance.currentRoomInfo;
        SetPlayerSlot(0, curRoomInfo.GetHostInfo().userName);
        netManger.RegisterReceiveNotificationP2P((int)P2PPacketType.GuestLeave, OnReceiveP2PGuestLeave);
        netManger.RegisterReceiveNotificationP2P((int)P2PPacketType.EnterRoom, OnReceiveGuestEnterMyRoom);
        netManger.RegisterReceiveNotificationP2P((int)P2PPacketType.NewGuestEnter, OnReceiveP2PNewGuestEnter);
        netManger.RegisterReceiveNotificationP2P((int)P2PPacketType.HostLeave, OnReceiveP2PHostLeave);
        netManger.RegisterReceiveNotificationP2P((int)P2PPacketType.Chat, OnReceiveP2PChat);
        if (curRoomInfo.isHost) // 호스트
        {
            Debug.Log("호스트모드");
            
            
        }
        else
        {
            Debug.Log("게스트모드");
            
            // 플레이어 슬롯 등록
            byte[] index;
            string[] userName;
            curRoomInfo.GetAllGuestInfo(out index, out userName);
            for(int i = 0; i < index.Length; i++)
            {
                Debug.Log("유저이름 : "+userName[i]);
                SetPlayerSlot(index[i], userName[i]);
            }
        }
        dialogMessage.Close(true,1f);
    }

    protected override void OnPause()
    {
        netManger.UnRegisterReceiveNotificationP2P((int)P2PPacketType.EnterRoom);
        netManger.UnRegisterReceiveNotificationP2P((int)P2PPacketType.NewGuestEnter);
        netManger.UnRegisterReceiveNotificationP2P((int)P2PPacketType.GuestLeave);
        netManger.UnRegisterReceiveNotificationP2P((int)P2PPacketType.HostLeave);
        netManger.UnRegisterReceiveNotificationP2P((int)P2PPacketType.Chat);
        if (curRoomInfo.isHost) // 호스트
        {
            
        }
        else
        {
            
        }
       
    }

    public void SetPlayerSlot(int index, string playerName)
    {        
        listPlayer[index].text = playerName;
        listPlayer[index].gameObject.SetActive(true);
    }

    public void ClearPlayerSlot(int index)
    {        
        listPlayer[index].text = "";
        listPlayer[index].gameObject.SetActive(false);
    }

    public void OnClickExitRoom()
    {
        //서버에게 방 퇴장 전송
        LeaveRoomData sendDataToServer = new LeaveRoomData();
        sendDataToServer.roomNum = (byte)curRoomInfo.roomNumber;
        LeaveRoomPacket sendPacketToServer = new LeaveRoomPacket(sendDataToServer);
        netManger.SendToServer(sendPacketToServer);

        if(curRoomInfo.playerMode == RoomInfo.PlayerMode.Guest)
        {
            // 게스트가 나가려고 하면 호스트에게 알린다
            P2PGuestLeaveData sendDataToHost = new P2PGuestLeaveData();
            sendDataToHost.guestIndex = (byte)curRoomInfo.myIndex;
            P2PGuestLeavePacket sendPacketToHost = new P2PGuestLeavePacket(sendDataToHost);
            netManger.SendToHost(sendPacketToHost);
            //netManger.DisconnectGuestSocket();
        }
        else
        {
            // 호스트가 나가려고 하면 게스트들에게 알린다
            P2PHostLeavePacket sendDataToAllGuest = new P2PHostLeavePacket();
            netManger.SendToAllGuest(sendDataToAllGuest);
            //netManger.DisconnectHostSocket();
        }
        ChangeForm(typeof(LobbyForm).Name);
    }

    public void OnClickStartGame()
    {
        
    }
    public void OnClickChat()
    {
        string chat = inputChat.text;
        if (chat.Length == 0)
            return;
        inputChat.text = "";
        P2PChatData data = new P2PChatData();
        data.chat = chat;
        data.guestIndex = (byte)curRoomInfo.myIndex;       
        P2PChatPacket packet = new P2PChatPacket(data);
        if (curRoomInfo.isHost)
        {
            AddChat(chat,0);
            netManger.SendToAllGuest(packet);
            return;
        }
        netManger.SendToHost(packet);
    }
    private void AddChat(string chat, int index)
    {
        txtChatWindow.text += curRoomInfo.GetGuestInfo(index).userName+ ":"+chat + "\n";        
    }
    private void OnReceiveGuestEnterMyRoom(Socket client, byte[] data)// GuestToHost 게스트 입장시도
    {
        // 성공시
        P2PEnterRoomPacket resultPacket = new P2PEnterRoomPacket(data);
        P2PEnterRoomData resultData = resultPacket.GetData(); // 접속한 아이디 얻기
        Debug.Log("RoomForm::게스트 입장시도 " + resultData.userName);
        P2PEnterRoomResultData sendData = new P2PEnterRoomResultData();
        bool bOk = true;
        // 인원확인
        if (curRoomInfo.PlayerCount >= RoomInfo.MaxPlayer) // 인원 초과
        {
            bOk = false;

        }
        // 룸 상태(게임시작인지 대기인지) 확인
        if(curRoomInfo.state == RoomInfo.State.Play) // 게임중이면 실패
        {
            bOk = false;
        }
        if (!bOk)
        {
            
            sendData.result = (byte)P2PEnterRoomResultData.RESULT.Fail;
            P2PEnterRoomResultPacket sendFailPacket = new P2PEnterRoomResultPacket(sendData);
            netManger.SendToGuest(client, sendFailPacket);
            Debug.Log("RoomForm::게스트 입장실패 " + resultData.userName);
            return;
        }
        Debug.Log("RoomForm::게스트 입장성공 " + resultData.userName);
        // 보낼 패킷 만들기
        sendData.result = (byte)P2PEnterRoomResultData.RESULT.Success; // 성공
        sendData.otherGuestCount = (byte)curRoomInfo.PlayerCount; // 이전 접속자 수
        curRoomInfo.GetAllGuestInfo(out sendData.otherGuestIndex, out sendData.otherGuestID); //이전 접속자 정보
        int newIndex = curRoomInfo.AddGuest(new PlayerInfo(resultData.userName)); // 게스트 추가
        SetPlayerSlot(newIndex, resultData.userName); //슬롯에 표시
        sendData.myIndex = (byte)newIndex; // 게스트 인덱스 부여

        // 성공패킷 전송
        P2PEnterRoomResultPacket sendPacket = new P2PEnterRoomResultPacket(sendData);
        netManger.SendToGuest(client, sendPacket);
        // 기존 인원들에게 새로운 게스트 알림
        P2PNewGuestEnterData newGuestdata = new P2PNewGuestEnterData();
        newGuestdata.guestIndex = (byte)newIndex; // 인덱스
        newGuestdata.userName = resultData.userName; // 유져아이디
        P2PNewGuestEnterPacket newGuestPacket = new P2PNewGuestEnterPacket(newGuestdata);
        netManger.SendToAllGuest(client, newGuestPacket);
    }
    private void OnReceiveP2PNewGuestEnter(Socket client, byte[] data) // HostToGuest 새로운 게스트 입장
    {        
        P2PNewGuestEnterPacket packet = new P2PNewGuestEnterPacket(data);
        SetPlayerSlot(packet.GetData().guestIndex, packet.GetData().userName);
        Debug.Log("RoomForm::새로운 게스트 입장 - " + packet.GetData().userName);
    }
    public void OnReceiveP2PGuestLeave(Socket client, byte[] data) // Guest To Host or Host To ALL Guest
    {
        Debug.Log("GuestLeave");
        P2PGuestLeavePacket prePacket = new P2PGuestLeavePacket(data);        
        int index = prePacket.GetData().guestIndex;
        if (curRoomInfo.isHost)
        {
            // 호스트면 게스트 퇴장을 알린다
            P2PGuestLeaveData newData = new P2PGuestLeaveData();
            newData.guestIndex = (byte)index;
            P2PGuestLeavePacket packet = new P2PGuestLeavePacket(newData);
            netManger.SendToAllGuest(client, packet);
        }
        // 게스트를 방 정보에서 제거한다.
        curRoomInfo.RemoveGuest(index);
        // 슬롯을 초기화 한다.
        SetPlayerSlot(index, "");
    }
    public void OnReceiveP2PHostLeave(Socket client, byte[] data) // Host TO Guest 호스트 퇴장
    {
        Debug.Log("HostLeave");
        // 호스트가 퇴장 했으면
        // 소켓닫고
        netManger.DisconnectGuestSocket();
        // 서버에게 방 퇴장 알림
        //서버에게 방 퇴장 전송
        LeaveRoomData sendDataToServer = new LeaveRoomData();
        sendDataToServer.roomNum = (byte)curRoomInfo.roomNumber;
        LeaveRoomPacket sendPacketToServer = new LeaveRoomPacket(sendDataToServer);
        netManger.SendToServer(sendPacketToServer);
        // 로비로
        ChangeForm(typeof(LobbyForm).Name);
    }
    public void OnReceiveP2PChat(Socket client, byte[] data)
    {
        // 출력       
        P2PChatPacket packet = new P2PChatPacket(data);
                
        AddChat(packet.GetData().chat, packet.GetData().guestIndex);
        
        if (curRoomInfo.isHost)
        {
            // 브로드캐스트
            P2PChatPacket sendPacket = new P2PChatPacket(packet.GetData());
            netManger.SendToAllGuest(sendPacket);
            return;
        }
        
        
    }
}
