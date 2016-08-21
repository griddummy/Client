using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIRoomNode : MonoBehaviour {

    public delegate void OnClickRoomNodeEvent(int roomNumber);
    private OnClickRoomNodeEvent OnClickRoomNode;
    public Button button;       
    public Text textRoomTitle;  // 방 제목

    int roomNumber;

    public void SetupNode(string title, int roomNumber, OnClickRoomNodeEvent onClick)
    {
        this.roomNumber = roomNumber;
        textRoomTitle.text = title;
        OnClickRoomNode = onClick;
    }

    public void OnClick(){
        if (OnClickRoomNode != null)
            OnClickRoomNode(roomNumber);
    }
}
