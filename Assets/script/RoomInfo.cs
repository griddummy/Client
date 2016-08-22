﻿using UnityEngine;
using System.Collections.Generic;

// 호스트가 방을 만들면 생기는 방정보
public class RoomInfo
{
    public enum State { Wait, Play }        // 빈방, 대기, 게임중
    public enum PlayerType{ Host, Guest }   // 플레이어의 타입 [ 게스트, 호스트 ]
    public const int MaxPlayer = 4;         // 최대 플레이어
    public const int HostIndex = 0;

    public int roomNumber;                  // 방번호
    public string title;                    // 방제목
    public int map;                         // 맵번호
    public State state;                     // 방 상태            
    public PlayerType myType;           // 나의 타입
    public int myIndex;                     // 나의 인덱스

    private PlayerInfo[] players;   // 방장 포함 (0번째). 순서대로. 나가면 빈칸이 있을 수 있음. 빈칸포함 앞번호부터 채워짐.
    private int m_playerCount;      // 플레이어 수  
    
    public RoomInfo(int roomNumber, string title, int map, PlayerInfo hostInfo, PlayerType myType)
    {
        this.roomNumber = roomNumber;
        this.title = title;
        m_playerCount = 0;

        players = new PlayerInfo[MaxPlayer];

        for (int i = 0; i < MaxPlayer; i++)
            players[i] = null;

        AddGuest(hostInfo);
        hostInfo.index = HostIndex;
        state = State.Wait;

        this.myType = myType;

        if(myType == PlayerType.Host)
        {
            myIndex = HostIndex;
        }
    }

    public bool isHost
    {
        get { return myType == PlayerType.Host; }
    }

    public int PlayerCount
    {
        get { return m_playerCount; }
    }

    public int AddGuest(PlayerInfo guestInfo)
    {
        if (m_playerCount >= MaxPlayer)
            return -1;
        for(int i = 0; i < players.Length; i++)
        {
            if(players[i] == null)
            {
                players[i] = guestInfo;
                guestInfo.index = i;
                m_playerCount++;
                return i;
            }
        }
        return -1;
        
    }

    public bool AddGuest(PlayerInfo guestInfo, int index)
    {
        if (players[index] != null)
        {
            Debug.Log("AddGuest::게스트정보가 해당 자리에 이미 있습니다" + players[index].playerName);
            return false;
        }
        players[index] = guestInfo;
        guestInfo.index = index;
        m_playerCount++;
        return true;
    }

    public void RemoveGuest(int index)
    {
        if (index == 0 || index >= MaxPlayer)
            return;
        if (players[index] != null)
        {
            players[index] = null;
            m_playerCount--;
        }      
    }

    public PlayerInfo GetGuestInfo(int index)
    {        
        return players[index];
    }

    public PlayerInfo GetHostInfo()
    {
        return players[0];
    }

    public void GetAllGuestInfo(out byte[] index, out string[] userName)
    {
        index = new byte[m_playerCount];
        userName = new string[m_playerCount];
        int count = 0;
        for(int i = 0; count < m_playerCount; i++)
        {
            if(players[i] != null)
            {
                index[count] = (byte)i;
                userName[count] = players[i].playerName;
                count++;
            }
        }
    }
}