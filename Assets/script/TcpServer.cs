using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;

public class TcpServer
{
    class AsyncData
    {
        public Socket clientSock;
        public const int msgMaxLength = 1024;
        public byte[] msg = new byte[msgMaxLength];
        public int msgLength;
    }

    public delegate void OnAcceptedEvent(Socket sock);
    public delegate void OnReceivedEvent(Socket sock, byte[] msg, int size);
    public delegate void OnDisconnectedClientEvent(Socket sock);    
    public event OnReceivedEvent OnReceived;
    public event OnAcceptedEvent OnAccepted;
    public event OnDisconnectedClientEvent OnDisconnectedClient;

    private List<Socket> clientSockes = new List<Socket>();

    private Socket listenSock = null;
    private AsyncCallback asyncReceiveCallback;
    private string m_strIP;
    private int m_port;
    public TcpServer()
    {
        // create listening socket
        listenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        
    }
    public void Start()
    {
        if (listenSock.Connected)
            return;
        // bind listening socket
        m_strIP = GetLocalIPAddress();
        Debug.Log("내 로컬 IP " + m_strIP);
        listenSock.Bind(new IPEndPoint(IPAddress.Parse(m_strIP), m_port));

        //listen listening socket
        listenSock.Listen(10);

        AsyncCallback asyncAcceptCallback = new AsyncCallback(HandleAsyncAccept);
        AsyncData asyncData = new AsyncData();
        object ob = asyncData;
        ob = listenSock;
        listenSock.BeginAccept(asyncAcceptCallback, ob);
    }
    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {                
                return ip.ToString();
            }
        }
        throw new Exception("Local IP Address Not Found!");
    }
    public void Setup(int port)
    {
        if (listenSock.Connected)
            return;
        
        m_port = port;
    }

    public void ServerClose()
    {
        if (listenSock == null)
            return;
        try
        {            
            foreach (Socket sock in clientSockes)
            {
                sock.Disconnect(false);
                sock.Close();
            }
            clientSockes.Clear();
            listenSock.Disconnect(false);
            listenSock.Close();
        }
        catch
        {

        }
    }
    public void DisconnectClient(Socket client)
    {
        try
        {
            client.Disconnect(false);
            client.Close();
            clientSockes.Remove(client);
            if (OnDisconnectedClient != null)
                OnDisconnectedClient(client);
        }
        catch
        {
            Debug.Log("TCPSERVER::DisconnectClient() 실패");
        }
        Debug.Log("TCPSERVER::DisconnectClient() 남은 클라 - " + clientSockes.Count);
    }
    private void HandleAsyncAccept(IAsyncResult asyncResult)
    {
        Socket listenSock = (Socket)asyncResult.AsyncState;
        Socket clientSock = listenSock.EndAccept(asyncResult);
        clientSockes.Add(clientSock);
        if(OnAccepted != null)
        {
            OnAccepted(clientSock);
        }            
        AsyncCallback asyncReceiveCallback = new AsyncCallback(HandleAsyncReceive);
        AsyncData asyncData = new AsyncData();
        asyncData.clientSock = clientSock;
        object ob = asyncData;

        try { clientSock.BeginReceive(asyncData.msg, 0, AsyncData.msgMaxLength, SocketFlags.None, asyncReceiveCallback, ob); }
        catch {
            DisconnectClient(clientSock); }

        AsyncCallback asyncAcceptCallback = new AsyncCallback(HandleAsyncAccept);
        ob = listenSock;

        listenSock.BeginAccept(asyncAcceptCallback, ob);

    }
    private void HandleAsyncReceive(IAsyncResult asyncResult)
    {
        AsyncData asyncData = (AsyncData)asyncResult.AsyncState;
        Socket clientSock = asyncData.clientSock;

        try
        {
            asyncData.msgLength = clientSock.EndReceive(asyncResult);
        }
        catch
        {
            Debug.Log("TcpServer::HandleAsyncReceive() : EndReceive - 예외");            
            DisconnectClient(clientSock);
            return;
        }
        if (OnReceived != null)
        {
            OnReceived(clientSock, asyncData.msg, asyncData.msgLength);
        }

        try
        {
            clientSock.BeginReceive(asyncData.msg, 0, AsyncData.msgMaxLength, SocketFlags.None, asyncReceiveCallback, asyncData);
        }
        catch
        {
            Debug.Log("TcpServer::HandleAsyncReceive() : BeginReceive - 예외");
            DisconnectClient(clientSock);
        }
    }
    public void SendAll(byte[] data, int size)
    {
        foreach (Socket client in clientSockes)
        {
            try
            {
                client.Send(data, size, SocketFlags.None);
            }
            catch
            {
                Debug.Log("TcpServer::SendAll() : Send - 예외");
            }
        }
    }
    public void SendAll(Socket excludeSock, byte[] data, int size)
    {
        foreach (Socket client in clientSockes)
        {
            if(client != excludeSock)
            {
                try
                {
                    client.Send(data, size, SocketFlags.None);
                }
                catch
                {
                    Debug.Log("TcpServer::SendAll() : Send - 예외");
                }
            }            
        }
    }

    public int Send(Socket _client, byte[] data, int size)
    {
        foreach (Socket client in clientSockes)
        {
            if (client == _client)
            {
                try
                {
                    client.Send(data, size, SocketFlags.None);
                }
                catch
                {
                    Debug.Log("TcpServer::Send() : Send - 예외");
                }
                break;
            }
        }
        return -1;
    }
}
