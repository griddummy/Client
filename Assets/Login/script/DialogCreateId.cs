using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DialogCreateId : Dialog {

    public InputField inputID;
    public InputField inputPassword;
    public InputField inputPassword2;
    public Text txtWarning;
    
    void Start()
    {
        inputID.onValueChanged.AddListener(delegate { ClearWarning(); });
        inputPassword.onValueChanged.AddListener(delegate { ClearWarning(); });
        inputPassword2.onValueChanged.AddListener(delegate { ClearWarning(); });
    }
   
    private void ClearWarning()
    {
        setWarningText("");
    }
    public override void Init()
    {
        inputID.text = "";
        inputPassword.text = "";
        inputPassword2.text = "";
        txtWarning.text = "";
    }
    public void setWarningText(string str)
    {
        //SetContentChangedListener(false);
        txtWarning.text = str;
        //SetContentChangedListener(true);
    }
    
    protected override void OnClickPositive()
    {
        txtWarning.text = "";
        if(inputID.text == "" || inputPassword.text == "" || inputPassword2.text == "")
        {
            setWarningText(SystemMessage.EmptyInfo);
        }
        else if (inputPassword.text != inputPassword2.text)
        {
            inputPassword.text = "";
            inputPassword2.text = "";
            setWarningText(SystemMessage.NotEqualPw1Pw2);            
        }
        else
        {            
            CreateIdData data = new CreateIdData();
            data.id = inputID.text;
            data.password = inputPassword.text;
            SetResultObject(data);
            base.OnClickPositive();        }           
    }
}
