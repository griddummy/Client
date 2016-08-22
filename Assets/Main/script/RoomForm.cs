﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine.SceneManagement;

public class RoomForm : UIForm {

    public DialogMessage dialogMessage;
    public List<Text> listPlayer;
    public Button btnStart;
    public Button btnExit;
    public Button btnChat;
    public InputField inputChat;
    public Text txtChatWindow;

    private MainManager gm;
    private RoomInfo curRoomInfo;
    private NetManager netManger;
    private Coroutine corCountDown;
    protected override void OnResume()
    {
        gm = MainManager.instance;
        netManger = MainManager.instance.netManager;
        curRoomInfo = MainManager.instance.currentRoomInfo;
        SetPlayerSlot(0, curRoomInfo.GetHostInfo().playerName);
        netManger.RegisterReceiveNotificationP2P((int)P2PPacketType.GuestLeave, OnReceiveP2PGuestLeave);
        netManger.RegisterReceiveNotificationP2P((int)P2PPacketType.EnterRoom, OnReceiveGuestEnterMyRoom);
        netManger.RegisterReceiveNotificationP2P((int)P2PPacketType.NewGuestEnter, OnReceiveP2PNewGuestEnter);
        netManger.RegisterReceiveNotificationP2P((int)P2PPacketType.HostLeave, OnReceiveP2PHostLeave);
        netManger.RegisterReceiveNotificationP2P((int)P2PPacketType.Chat, OnReceiveP2PChat);
        netManger.RegisterReceiveNotificationP2P((int)P2PPacketType.GameStartCount, OnReceiveP2PStartCountDown);
        netManger.RegisterReceiveNotificationP2P((int)P2PPacketType.StartGameScene, OnReceiveP2PStartGameScene);
        btnExit.gameObject.SetActive(true);
        if (curRoomInfo.isHost) // 호스트
        {
            Debug.Log("호스트모드");
            btnStart.gameObject.SetActive(true);

        }
        else
        {
            Debug.Log("게스트모드");
            btnStart.gameObject.SetActive(false);
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
        netManger.UnRegisterReceiveNotificationP2P((int)P2PPacketType.GameStartCount);
        netManger.UnRegisterReceiveNotificationP2P((int)P2PPacketType.StartGameScene);

        if (curRoomInfo.isHost) // 호스트
        {
            
        }
        else
        {
            
        }
        if (corCountDown != null)
            StopCoroutine(corCountDown);
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
    IEnumerator StartCountDown()
    {
       
        int count = 5;
        while (count > 0)
        {
            AddChat(count + "초 후 게임이 시작됩니다");
            yield return new WaitForSeconds(1f);
            count--;
        }
        // 호스트면 게스트들에게 게임씬을 실행하도록 전한다.
        if (curRoomInfo.isHost)
        {
            P2PStartGameScenePacket packet = new P2PStartGameScenePacket();
            netManger.SendToAllGuest(packet);

            // 게임 씬 전환
            StartGameScene();
        }
    }
   
    public void OnClickExitRoom()
    {        
        Debug.Log("방퇴장- 방번호 " + curRoomInfo.roomNumber);

        //서버에게 방 퇴장 전송
        LeaveRoomData sendDataToServer = new LeaveRoomData();
        sendDataToServer.roomNum = (byte)curRoomInfo.roomNumber;
        LeaveRoomPacket sendPacketToServer = new LeaveRoomPacket(sendDataToServer);        
        netManger.SendToServer(sendPacketToServer);

        if(curRoomInfo.myType == RoomInfo.PlayerType.Guest)
        {
            // 게스트가 나가려고 하면 호스트에게 알린다
            P2PGuestLeaveData sendDataToHost = new P2PGuestLeaveData();
            sendDataToHost.guestIndex = (byte)curRoomInfo.myIndex;
            P2PGuestLeavePacket sendPacketToHost = new P2PGuestLeavePacket(sendDataToHost);
            netManger.SendToHost(sendPacketToHost);
            netManger.DisconnectMyGuestSocket();
        }
        else
        {
            // 호스트가 나가려고 하면 게스트들에게 알린다
            P2PHostLeavePacket sendDataToAllGuest = new P2PHostLeavePacket();
            netManger.SendToAllGuest(sendDataToAllGuest);
            netManger.CloseHostSocket();
        }

        MainManager.instance.currentRoomInfo = null;
        ChangeForm(typeof(LobbyForm).Name);
    }

    public void OnClickStartGame()
    {
        btnExit.gameObject.SetActive(false);
        P2PGameStartCountPacket packetCount = new P2PGameStartCountPacket();
        netManger.SendToAllGuest(packetCount);
        corCountDown = StartCoroutine(StartCountDown());
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
        txtChatWindow.text += curRoomInfo.GetGuestInfo(index).playerName+ ":"+chat + "\n";        
    }
    private void AddChat(string chat)
    {
        txtChatWindow.text += chat + "\n";
    }


    // [호스트가 처리하는 패킷 리시브 메서드] 게스트 입장시도
    private void OnReceiveGuestEnterMyRoom(Socket client, byte[] data)
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

    // [게스트가 처리하는 패킷 리시브 메서드] 새로운 게스트 입장
    private void OnReceiveP2PNewGuestEnter(Socket client, byte[] data) 
    {
        
        P2PNewGuestEnterPacket packet = new P2PNewGuestEnterPacket(data);
        P2PNewGuestEnterData newGuest = packet.GetData();

        // 현재 방 정보에 새로온 게스트 정보를 넣는다.
        curRoomInfo.AddGuest(new PlayerInfo(newGuest.userName), newGuest.guestIndex);

        // 플레이어 슬롯에 새로운 게스트 정보를 넣는다.
        SetPlayerSlot(newGuest.guestIndex, newGuest.userName);
        Debug.Log("RoomForm::새로운 게스트 입장 - " + packet.GetData().userName);
    }

    // [ 게스트, 호스트 둘다 받는 패킷 리시브 메서드] - 게스트 퇴장
    public void OnReceiveP2PGuestLeave(Socket client, byte[] data) 
    {
        Debug.Log("GuestLeave");
        P2PGuestLeavePacket prePacket = new P2PGuestLeavePacket(data);        
        int index = prePacket.GetData().guestIndex;

        if (curRoomInfo.isHost) // 자신이 호스트면 
        {
            // 해당 게스트와 연결을 끊는다.
            netManger.DisconnectGuestSocket(client);

            // 다른 게스트에게 해당 게스트의 퇴장을 알린다.
            P2PGuestLeaveData newData = new P2PGuestLeaveData();
            newData.guestIndex = (byte)index;
            P2PGuestLeavePacket packet = new P2PGuestLeavePacket(newData);
            netManger.SendToAllGuest(client, packet);
        }
        // 아래부터는  호스트 게스트 둘다 해야 되는 일들이다.

        // 해당 게스트를 방 정보에서 제거한다.
        curRoomInfo.RemoveGuest(index);

        // 해당 플레이어 슬롯을 초기화 한다.
        SetPlayerSlot(index, "");
    }

    // [ 게스트가 처리하는 패킷 리시브 메서드 ] 호스트 퇴장
    public void OnReceiveP2PHostLeave(Socket client, byte[] data)
    {
        Debug.Log("HostLeave");        
        // 호스트와 연결한 소켓을 닫고        
        netManger.DisconnectMyGuestSocket();

        // 서버에게 나의 방 퇴장 알림        
        LeaveRoomData sendDataToServer = new LeaveRoomData();
        sendDataToServer.roomNum = (byte)curRoomInfo.roomNumber;
        LeaveRoomPacket sendPacketToServer = new LeaveRoomPacket(sendDataToServer);
        netManger.SendToServer(sendPacketToServer);

        // 방 정보 제거
        MainManager.instance.currentRoomInfo = null;

        // 로비로 폼 변경
        ChangeForm(typeof(LobbyForm).Name);
    }

    // [호스트, 게스트 둘다 처리하는 패킷 리시브 메서드] - 채팅 메세지
    public void OnReceiveP2PChat(Socket client, byte[] data) 
    {        
        P2PChatPacket packet = new P2PChatPacket(data);

        // 화면에 출력
        AddChat(packet.GetData().chat, packet.GetData().guestIndex);
        
        if (curRoomInfo.isHost) // 호스트면
        {
            // 브로드캐스트 ( 모든 게스트들에게 알려준다 )
            P2PChatPacket sendPacket = new P2PChatPacket(packet.GetData());
            netManger.SendToAllGuest(sendPacket);
            return;
        }
    }

    // [ 게스트가 처리하는 패킷 리시브 메서드 ] - 카운트다운 시작
    private void OnReceiveP2PStartCountDown(Socket host, byte[] data)
    {
        btnExit.gameObject.SetActive(false);
        corCountDown = StartCoroutine(StartCountDown());
    }

    // [ 게스트가 처리하는 패킷 리시브 메서드 ] - 게임씬으로 전환
    private void OnReceiveP2PStartGameScene(Socket host, byte[] data)
    {
        // 맵에 따른 씬 부르기
        StartGameScene();
    }

    private void StartGameScene()
    {
        //TODO
        // 현재 맵정보를 얻어서 해당 Scene을 실행한다.
        SceneManager.LoadScene("SceneTestGame"); // TEST

    }
}
