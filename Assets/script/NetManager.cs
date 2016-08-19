using UnityEngine;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.Net.Sockets;

public class NetManager : MonoBehaviour {

    TcpClient m_client;                 // 서버 연결 소켓
    TcpServer m_host;                   // 호스트 소켓
    TcpClient m_guest;                  // 게스트 소켓

    PacketQueue m_recvQueueFromServer;      // 서버로 부터 패킷 받은 큐
    PacketQueue m_sendQueueFromServer;      // 서버로 패킷 보내는 큐

    PacketQueue m_recvQueueFromGuest;        // 패킷 받는 큐
    PacketQueue m_sendQueueFromGuest;        // 패킷 보내는 큐

    PacketQueue m_recvQueueFromHost;       // 패킷 받는 큐
    PacketQueue m_sendQueueFromHost;       // 패킷 보내는 큐

    Queue<Socket> indexGuestQueue;       // 다른 게스트들이 보낸 패킷을 소켓으로 구분하기 위한 리스트

    byte[] m_sendBuffer;
    byte[] m_recvBuffer;
    const int BufferSize = 1024;

    public delegate void RecvNotifier(Socket sock, byte[] data); // 누가, 어떤 데이타를 보냇는지    
    private Dictionary<int, RecvNotifier> m_notiServer = new Dictionary<int, RecvNotifier>();
    private Dictionary<int, RecvNotifier> m_notiP2P = new Dictionary<int, RecvNotifier>();

    public delegate void OnDisconnectGuest(Socket sock); // 클라이언트 끊어짐

    public string GameServerIP;
    public int GameServerPort = 9800;
    
    public int HostPort = 9700;
    object m_objLockGuestPacketQueue = new object();
    void Awake()
    {
        m_sendBuffer = new byte[BufferSize];
        m_recvBuffer = new byte[BufferSize];

        // GameServer Connection Context
        m_client = new TcpClient();        
        m_recvQueueFromServer = new PacketQueue();
        m_sendQueueFromServer = new PacketQueue();
        m_client.OnReceived += OnReceivedPacketFromServer;
        m_client.Setup(GameServerIP, GameServerPort);

        // Host Connection Context
        m_host = new TcpServer();
        m_recvQueueFromGuest = new PacketQueue();
        m_sendQueueFromGuest = new PacketQueue();
        indexGuestQueue = new Queue<Socket>();
        m_host.OnReceived += OnReceivedPacketFromGuest;
        m_host.Setup(HostPort);

        // Guest Connection Context
        m_guest = new TcpClient();
        m_recvQueueFromHost = new PacketQueue();
        m_sendQueueFromHost = new PacketQueue();
        m_guest.OnReceived += OnReceivedPacketFromHost;
    }
    void Start()
    {        
        StartHostServer();
    }
    void Update()
    {

        Receive(m_recvQueueFromServer, m_client.socket, m_notiServer);
        Receive(m_recvQueueFromHost, m_guest.socket, m_notiP2P);
        ReceiveFromGuest();
    }   
    void OnApplicationQuit()
    {
        Debug.Log("Quit::Server Socket Close");
        ProgramExitPacket packet = new ProgramExitPacket();
        SendToServer(packet);
        m_client.DisConnect();
        Debug.Log("Quit::Host Socket Close");
        m_host.ServerClose();
        Debug.Log("Quit::End");
    }
    public void StartHostServer()
    {
        m_host.Start();
    }
    public bool ConnectToGameServer() // 서버로 연결
    {
        return m_client.Connect();
    }
    public bool ConnectToHost(string ip) // 호스트에게 연결
    {
        m_guest.Setup(ip, HostPort);
        return m_guest.Connect();
    }
    public void DisconnectMyGuestSocket() // (내가 게스트일때) 호스트와 연결을 끊는다.
    {
        m_guest.DisConnect();
    }
    public void DisconnectGuestSocket(Socket other) //(내가 호스트일때) 특정 게스트와 연결을 끊는다.
    {
        m_host.DisconnectClient(other);
    }
    public void CloseHostSocket() // (내가 호스트 일 떄) 모든 게스트와 연결을 끊는다.
    {
        m_host.DisconnectAll();
    }
    private void Receive(PacketQueue queue, Socket sock, Dictionary<int, RecvNotifier> noti) // 서버나 호스트에게 받은 큐
    {        
        int Count = queue.Count;
        
        for( int i = 0; i < Count; i++)
        {
            int recvSize = 0;
            recvSize = queue.Dequeue(ref m_recvBuffer, m_recvBuffer.Length);                
            
            if (recvSize > 0)
            {
                byte[] msg = new byte[recvSize];

                Array.Copy(m_recvBuffer, msg, recvSize);
                ReceivePacket(noti, sock, msg); // 서버나 호스트로 부터 받은건 0번
            }
        }
    }
    private void ReceiveFromGuest()
    {
        int Count = m_recvQueueFromGuest.Count;

        for (int i = 0; i < Count; i++)
        {
            int recvSize = 0;
            recvSize = m_recvQueueFromGuest.Dequeue(ref m_recvBuffer, m_recvBuffer.Length);
            Socket sock;
            lock (m_objLockGuestPacketQueue)
            {
                 sock = indexGuestQueue.Dequeue();
            }            
            if (recvSize > 0)
            {
                byte[] msg = new byte[recvSize];
                Array.Copy(m_recvBuffer, msg, recvSize);
                ReceivePacket(m_notiP2P, sock, msg);
            }
        }
    }
    private void OnReceivedPacketFromServer(byte[] msg, int size) // 게임서버에서 받는 
    {
        m_recvQueueFromServer.Enqueue(msg, size);
    }
    private void OnReceivedPacketFromGuest(Socket socket, byte[] msg, int size) // 게스트들에게 받는
    {
        Debug.Log("OnReceivedPacketFromGuest::" + socket.RemoteEndPoint.ToString() + " " + msg.Length);
        m_recvQueueFromGuest.Enqueue(msg, size);
        lock (m_objLockGuestPacketQueue)
        {
            indexGuestQueue.Enqueue(socket);
        }            
    }

    private void OnReceivedPacketFromHost(byte[] msg, int size) // 호스트에게 받는
    {
        m_recvQueueFromHost.Enqueue(msg, size);
    }
    private byte[] CreateCompletedPacket<T>(IPacket<T> packet)
    {
        byte[] packetData = packet.GetPacketData(); // 패킷의 데이터를 바이트화

        // 헤더 생성
        PacketHeader header = new PacketHeader();
        HeaderSerializer serializer = new HeaderSerializer();

        header.length = (short)packetData.Length; // 패킷 데이터의 길이를 헤더에 입력
        header.id = (byte)packet.GetPacketId(); // 패킷 데이터에서 ID를 가져와 헤더에 입력
        Debug.Log("패킷 전송 - id : " + header.id.ToString() + " length :" + header.length);
        byte[] headerData = null;
        if (serializer.Serialize(header) == false)
        {
            return null;
        }

        headerData = serializer.GetSerializedData(); // 헤더 데이터를 패킷 바이트로 변환


        byte[] data = new byte[headerData.Length + header.length]; // 최종 패킷의 길이 = 헤더패킷길이+내용패킷길이

        // 헤더와 내용을 하나의 배열로 복사
        int headerSize = Marshal.SizeOf(header.id) + Marshal.SizeOf(header.length);
        Buffer.BlockCopy(headerData, 0, data, 0, headerSize);
        Buffer.BlockCopy(packetData, 0, data, headerSize, packetData.Length);
        return data;
    }
    public int SendToHost<T>(IPacket<T> packet) // 패킷에 헤더를 부여하고 송신하는 메서드
    {
        int sendSize = 0;
        byte[] data = CreateCompletedPacket(packet);
        if (data == null)
            return 0;
        Debug.Log("SendToHost()::소켓에 패킷 Send" + data.Length);
        sendSize = m_guest.Send(data, data.Length);
        return sendSize;
    }
    public int SendToGuest<T>(Socket guest, IPacket<T> packet) // 패킷에 헤더를 부여하고 송신하는 메서드
    {
        int sendSize = 0;
        byte[] data = CreateCompletedPacket(packet);
        if (data == null)
            return 0;
        sendSize = m_host.Send(guest, data, data.Length);
        
        return sendSize;
    }
    public int SendToAllGuest<T>(IPacket<T> packet) // 패킷에 헤더를 부여하고 송신하는 메서드
    {
        int sendSize = 0;
        byte[] data = CreateCompletedPacket(packet);
        if (data == null)
        {
            Debug.Log("SendToAllGuest() - 잘못된 패킷" + packet.GetPacketId().ToString());
            return 0;
        }            

        //전송
        m_host.SendAll( data, data.Length);        
        return sendSize;
    }
    public int SendToAllGuest<T>(Socket excludeClient, IPacket<T> packet) // 패킷에 헤더를 부여하고 송신하는 메서드
    {
        int sendSize = 0;
        byte[] data = CreateCompletedPacket(packet);
        if (data == null)
            return 0;

        //전송
        m_host.SendAll(excludeClient, data, data.Length);
        return sendSize;
    }
    public int SendToServer<T>(IPacket<T> packet) // 패킷에 헤더를 부여하고 송신하는 메서드
    {
        int sendSize = 0;
        byte[] data = CreateCompletedPacket(packet);
        if (data == null)
            return 0;

        //전송
        sendSize = m_client.Send(data, data.Length);
        return sendSize;
    }

    public void RegisterReceiveNotificationServer( int packetID , RecvNotifier notifier)
    {
        m_notiServer.Add(packetID, notifier);
    }

    public void UnRegisterReceiveNotificationServer(int packetID)
    {
        m_notiServer.Remove(packetID);
    }
    public void RegisterReceiveNotificationP2P(int packetID, RecvNotifier notifier)
    {
        m_notiP2P.Add(packetID, notifier);
    }
    public void UnRegisterReceiveNotificationP2P(int packetID)
    {
        m_notiP2P.Remove(packetID);
    }
    private bool getPacketData(byte[] data, out int id, out byte[] outData)
    {
        PacketHeader header = new PacketHeader();
        HeaderSerializer serializer = new HeaderSerializer();

        serializer.SetDeserializedData(data);
        serializer.Deserialize(ref header);

        
        int headerSize = Marshal.SizeOf(header.id) + Marshal.SizeOf(header.length);
        int packetDataSize = data.Length - headerSize;
        byte[] packetData = null;
        if (packetDataSize > 0) //헤더만 있는 패킷을 대비해서 예외처리, 데이터가 있는 패킷만 데이터를 만든다
        {
            packetData = new byte[packetDataSize];
            Buffer.BlockCopy(data, headerSize, packetData, 0, packetData.Length);
        }
        else
        {
            id = header.id;
            outData = null;
            return false;
        }
        Debug.Log("받은 패킷 - id : " + header.id + " dataLength : " + packetData.Length);
        id = header.id;
        outData = packetData;
        return true;
    }
    private void ReceivePacket(Dictionary<int,RecvNotifier> noti, Socket sock , byte[] data)
    {
        byte[] packetData;
        int packetId;
        getPacketData(data, out packetId, out packetData);
        Debug.Log("ReceivePacket::"+sock.RemoteEndPoint.ToString()+"패킷id:" + packetId + " 길이:" + data.Length);
        RecvNotifier recvNoti;
        if (noti.TryGetValue(packetId, out recvNoti))
        {
            recvNoti(sock, packetData);
        }
        else
        {
            Debug.Log("NetManager::ReceivePacket() - 존재하지 않는 타입 패킷 :"+ packetId);
        }        
    }
}
