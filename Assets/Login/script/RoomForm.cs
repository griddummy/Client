using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RoomForm : UIForm {

    public List<Text> listPlayer;
    public Button btnStart;
    public Button btnExit;
    public Button btnChat;
    public InputField inputChat;
    public Text txtChatWindow;

    private GameManager gm;
    private Room curRoom;
    void Start()
    {
        gm = GameManager.instance;
    }
    protected override void OnResume()
    {
        curRoom = GameManager.instance.curRoom;
        if (curRoom == null)
        {
            Debug.Log("RoomForm::OnResume - 방 정보 누락");
        }
        SetPlayer(0, curRoom.hostId); // 호스트 ID를 셋팅
        //GameManager.instance.netManager.RegisterReceiveNotificationClient()
        if (curRoom.playerMode == Room.Player.Host) // 호스트
        {
             
        }
        else // 게스트
        {
            //GameManager.instance.netManager.ConnectToHost(curRoom.i)
            // 1. 호스트와 연결            
            // 1.1 다른 게스트의 정보 얻기
            // 2. 다른 게스트와 연결
        } 
    }

    protected override void OnPause()
    {
        
    }

    public void SetPlayer(int index, string playerName)
    {        
        listPlayer[index].text = playerName;
        listPlayer[index].gameObject.SetActive(true);
    }

    public void RemovePlayer(int index)
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
    private void OnReceiveGuestEnterMyRoom()// 내가 호스트일 떄 게스트 입장시도
    {
        // 접속 성공
        // 다른 게스트 정보(id + ip) 전송                
    }
    // private void OnReceiveGuest

}
