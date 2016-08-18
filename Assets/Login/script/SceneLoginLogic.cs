using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SceneLoginLogic : MonoBehaviour {

    public DialogMessage msgBox;    
    public DialogCreateRoom dialogCreateRoom;
    public RoomForm dialogRoom;

    private NetManager m_network;

    void Start()
    {      
        m_network = FindObjectOfType<NetManager>();
    }    

    public void OpenCreateRoomPopup()
    {
        // 로비 - 생성다이얼로그 오픈
        dialogCreateRoom.Open();
    }

     
    
    private void EnterGuest() // 내방에 손님이 들어오면
    {
        //dialogRoom.SetPlayer()
    }

    public void StartGame()
    {

    }

    public void ExitRoom()
    {

    }    
}
