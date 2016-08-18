using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DialogCreateRoom : Dialog {

    public InputField input;

    public override void Init()
    {
        input.text = "";
    }
    protected override void OnClickPositive()
    {
        string title = input.text;
        if (title.Length > 0)
        {
            CreateRoomData data = new CreateRoomData();
            data.title = title;
            data.map = 1;
            SetResultObject(data);
            base.OnClickPositive();
        }        
    }   
}
