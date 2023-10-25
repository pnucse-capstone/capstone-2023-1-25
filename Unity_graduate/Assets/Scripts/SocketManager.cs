using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class SocketManager : MonoBehaviour
{
    Thread mThread;
    public string connectionIp = "127.0.0.1";
    public int connectionPort = 25001;
    IPAddress localAdd;
    TcpListener listener;
    TcpClient client;
    string streamImagePath;
    string temp;
    string sendpath;
    bool running;
    bool IsRecevingPath;
    ToggleManager toggleManager;

    void GetInfo()
    {
        localAdd = IPAddress.Parse(connectionIp);
        listener = new TcpListener(IPAddress.Any, connectionPort);
        listener.Start();
        client = listener.AcceptTcpClient();
        running = true;
        while(running)
        {
            sendpath = toggleManager.ImgPathreturn();
            //Debug.Log(sendpath);
            if (sendpath!=null)
            {
                IsRecevingPath = SendAndReceiveData();
                
            }
        }
        Debug.Log("disconnect");
        listener.Stop();
    }

    bool SendAndReceiveData()
    {
        NetworkStream nwStream = client.GetStream();

        
        byte[] myWriteBuffer = Encoding.ASCII.GetBytes(sendpath);
        nwStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

        byte[] buffer = new byte[client.ReceiveBufferSize];
        int bytesRead = 0;
        while (bytesRead == 0) // 데이터를 수신할 때까지 블로킹
        {
            bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
        }
        string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        if (dataReceived != null)
        {
            streamImagePath = dataReceived;
            Debug.Log("receving");
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool RecevingPath()
    {
        return IsRecevingPath;
    }
    // Start is called before the first frame update
    void Start()
    {
        toggleManager = GetComponent<ToggleManager>();
        ThreadStart ts = new ThreadStart(GetInfo);
        mThread = new Thread(ts);
        mThread.Start();
    }

    // Update is called once per frame
    void Update()
    {

        /*if (IsRecevingPath && temp != streamImagePath)
        {
            Debug.Log(streamImagePath);
            temp = streamImagePath;
        }*/
        Debug.Log(streamImagePath);
    }

    private void OnApplicationQuit()
    {
        running = false;
        listener?.Stop();
        client?.Close();
        mThread?.Join();
    }
}
