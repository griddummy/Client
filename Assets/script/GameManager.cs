using UnityEngine;

public class GameManager : MonoBehaviour{
        
    public NetManager m_netManager;


    private LoginData m_login = null;
    private RoomInfo m_currentRoomInfo;
    
    public LoginData login {
        set { m_login = value; }
        get { return m_login; }
    }
    
    public NetManager netManager { get { return m_netManager; } }
    public static GameManager m_instance = null;
    public static GameManager instance
    {
        get
        {
            if(m_instance == null)
            {
                m_instance = FindObjectOfType<GameManager>();
                if(m_instance == null)
                {
                    GameObject gob = new GameObject("Logic");                    
                    GameObject inst = Instantiate(gob) as GameObject;
                    m_instance = inst.AddComponent<GameManager>();
                }
            }
            return m_instance;
        }
    }
    public RoomInfo currentRoomInfo
    {
        set { m_currentRoomInfo = value; }
        get { return m_currentRoomInfo; }
    }
    void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this;
            DontDestroyOnLoad(gameObject);
        }            
        else if(m_instance != this)
        {
            Destroy(this);
            return;
        }

        
    }
    void Start()
    {
        m_netManager.ConnectToGameServer();
    }
}
