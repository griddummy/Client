using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DialogMessage : Dialog {
    
    public Text text;
    
    public void SetMessage(string msg)
    {        
        text.text = msg;
    }
    public void Alert(string msg)
    {
        SetMessage(msg);
        Open();
    }
}