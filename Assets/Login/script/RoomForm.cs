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
        if (curRoomInfo.isHost) // 호스트
        {
            netManger.RegisterReceiveNotificationP2P((int)P2PPacketType.EnterRoom, OnReceiveGuestEnterMyRoom);
        }
        else
        {
            netManger.RegisterReceiveNotificationP2P((int)P2PPacketType.NewGuestEnter, OnReceiveP2PNewGuestEnter);
            // 플레이어 슬롯 등록
            byte[] index;
            string[] userName;
            curRoomInfo.GetAllGuestInfo(out index, out userName);
            for(int i = 0; i < index.Length; i++)
            {
                SetPlayerSlot(index[i], userName[i]);
            }
        }
        dialogMessage.Close(true,1f);
    }

    protected override void OnPause()
    {
        if (curRoomInfo.isHost) // 호스트
        {
            netManger.UnRegisterReceiveNotificationP2P((int)P2PPacketType.EnterRoom);
        }
        else
        {
            netManger.UnRegisterReceiveNotificationP2P((int)P2PPacketType.NewGuestEnter);
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
        
    }

    public void OnClickStartGame()
    {
        
    }
    private void OnReceiveGuestEnterMyRoom(Socket client, byte[] data)// 내가 호스트일 떄 게스트 입장시도
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
    private void OnReceiveP2PNewGuestEnter(Socket client, byte[] data)
    {        
        P2PNewGuestEnterPacket packet = new P2PNewGuestEnterPacket(data);
        SetPlayerSlot(packet.GetData().guestIndex, packet.GetData().userName);
        Debug.Log("RoomForm::새로운 게스트 입장 - " + packet.GetData().userName);
    }
}
