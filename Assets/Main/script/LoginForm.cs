using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Net.Sockets;

public class LoginForm : UIForm
{

    public InputField inputID;
    public InputField inputPassword;
    public Button btnLogin;
    public Button btnCreateId;
    public Text txtWarning;
    public DialogCreateId dialogCreateId;
    public DialogMessage msgBox;
    private NetManager netManager;
    private GameManager gameManager;
    private LoginData lastLoginData;
    void Start()
    {
        gameManager = GameManager.instance;
        netManager = gameManager.netManager;
        inputID.onValueChanged.AddListener(delegate { ClearWarning(); });
        inputPassword.onValueChanged.AddListener(delegate { ClearWarning(); });
        dialogCreateId.OnClosed += OnClickCreateId;
        // 로그인 된 상태인지 검사 -> 게임이 끝나서 메인씬으로 이동했다면 로그인폼부터 실행하기 때문
        // 물론 로그인폼이 가장 먼저 Start() 될지는 보장 못함..
        
        if(GameManager.instance.login != null) // 로그인 정보가 이미 있으면
        {
            // 로그인 정보가 있으면 로비 씬으로
            ChangeForm(typeof(LobbyForm).Name);
        }
        else // 로그인 정보가 없으면 로그인 씬으로
        {
            ChangeForm(GetType().Name);
        }
    }
    protected override void OnResume()
    {
        netManager.RegisterReceiveNotificationServer((int)ServerPacketId.CreateIdResult, OnReceiveResultCreateID);
        netManager.RegisterReceiveNotificationServer((int)ServerPacketId.LoginResult, OnReceiveResultLogin);
    }
    protected override void OnPause()
    {
        netManager.UnRegisterReceiveNotificationServer((int)ServerPacketId.CreateIdResult);
        netManager.UnRegisterReceiveNotificationServer((int)ServerPacketId.LoginResult);
    }
    private void ClearWarning()
    {
        SetWarningText("");
    }
    public void SetWarningText(string str)
    {       
        txtWarning.text = str;
    }
    public void Init()
    {
        inputID.text = "";
        inputPassword.text = "";
        txtWarning.text = "";
    }

    public void OnClickLogin()
    {
        if (inputID.text == "" || inputPassword.text == "")
        {            
            SetWarningText(SystemMessage.EmptyInfo);            
            return;
        }
        // 서버로 로그인 시도
        msgBox.Alert("서버에 접속 중입니다");
        LoginData data = new LoginData();
        data.id = inputID.text;
        data.password = inputPassword.text;
        lastLoginData = data;
        LoginPacket packet = new LoginPacket(data);
        netManager.SendToServer(packet);
    }
   
    public void OnClickCreateIDPopup()
    {
        Init();
        dialogCreateId.Open();
    }

    private void OnClickCreateId(bool bPositive, object data)
    {        
        if(bPositive)
        {
            CreateIdData createIdData = data as CreateIdData;
            if(data != null)
            {
                Debug.Log("TryCreateID() - 가입시도");
                msgBox.Alert("서버에 데이터를 확인 중입니다");
                CreateIdPacket createIdPacket = new CreateIdPacket(createIdData);
                netManager.SendToServer(createIdPacket);
            }
        }
    }

    private void OnReceiveResultLogin(Socket sock, byte[] data)
    {
        LoginResultPacket packet = new LoginResultPacket(data);

        string message = packet.GetData().result;
        bool bLoginSuccess = false;
        if (message == LoginResultData.Success)
        {
            bLoginSuccess = true;
        }

        if (bLoginSuccess)
        {
            GameManager.instance.login = lastLoginData;
            if (!ChangeForm(typeof(LobbyForm).Name))
            {
                Debug.Log("Fail 폼 체인징");
            }
            msgBox.Alert("접속 성공");
            msgBox.Close(true, 3f);
        }
        else
        {            
            SetWarningText("로그인실패");
            msgBox.Close();
        }
    }

    private void OnReceiveResultCreateID(Socket sock, byte[] data)
    {
        CreateIdResultPacket packet = new CreateIdResultPacket(data);

        string message = packet.GetData().result;
        bool bOK = false;

        if (message == CreateIdResultData.Success)
        {
            bOK = true;
        }

        if (bOK)
        {
            Debug.Log("ResultCreateID() - 가입 성공");
            dialogCreateId.Close(true);            
            msgBox.Alert("가입 성공");
            msgBox.Close(true, 2f);
        }
        else
        {
            Debug.Log("ResultCreateID() - 가입 실패");
            dialogCreateId.setWarningText("가입 실패");
            msgBox.Close(true);
        }
    }
}
