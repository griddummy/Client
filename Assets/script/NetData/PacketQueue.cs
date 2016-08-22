using System.Collections.Generic;
using System.IO;
using System;
public class PacketQueue  {

    struct PacketInfo
    {
        public int offset; // 읽어야 되는 위치
        public int size; // 마지막위치+1 ?
    };

    // 데이터를 보존 할 버퍼
    private MemoryStream m_streamBuffer;

    // 패킷 정보 관리 리스트
    private List<PacketInfo> m_offsetList;

    private int m_offest; // 메모리 배치 오프셋

    private object lockObj = new object(); // 락 오브젝트

    public PacketQueue() // 생성자
    {
        m_streamBuffer = new MemoryStream();
        m_offsetList = new List<PacketInfo>();
    }
    public int Count
    {
        get
        {
            return m_offsetList.Count;
        }
    }
    public int Enqueue(byte[] data, int size)
    {
        PacketInfo info = new PacketInfo();

        // 패킷 정보 작성
        info.offset = m_offest;
        info.size = size;

        lock (lockObj)
        {
            m_offsetList.Add(info); // 패킷 저장 정보 보존

            // 패킷데이터 쓰기
            m_streamBuffer.Position = m_offest;
            m_streamBuffer.Write(data, 0, size);
            m_streamBuffer.Flush();
            m_offest += size;
        }
        return size;
    }

    public int Dequeue(ref byte[] buffer, int size)
    {
        if(m_offsetList.Count <= 0)
        {
            return -1;
        }

        int recvSize = 0;
        lock (lockObj)
        {
            PacketInfo info = m_offsetList[0];

            // 버퍼에서 데이터 가져오기
            int dataSize = Math.Min(size, info.size);
            m_streamBuffer.Position = info.offset;
            recvSize = m_streamBuffer.Read(buffer, 0, dataSize);

            // 큐 데이터를 꺼냈으면 맨 앞 데이터는 삭제
            if(recvSize > 0)
            {
                m_offsetList.RemoveAt(0);
            }

            // 모든 큐 데이터를 꺼냇으면 스트림 정리해서 메모리 절약한다.
            if(m_offsetList.Count == 0)
            {
                Clear();
                m_offest = 0;
            }
        }
        return recvSize;
    }

    public void Clear()
    {
        byte[] buffer = m_streamBuffer.GetBuffer();
        Array.Clear(buffer, 0, buffer.Length);

        m_streamBuffer.Position = 0;
        m_streamBuffer.SetLength(0);
    }
}
