using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    private GameManager gm;
    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;
    private bool isRunning = false;
    private bool firstone = true;
    private byte[] receiveBuffer = new byte[1024];

    private Thread receiveThread;


    private string receivedMessage;
    private string control_signal;
    public string serverIp = "127.0.0.1";
    public int serverPort = 25002;

    int i = 0;

    [System.Serializable]
public class YourDataClass
{
    public string image_data; // 이미지 데이터를 문자열로 저장
    public string control_signal; // 제어 신호를 문자열로 저장
}
    public void ConnectToServer()
    {
        try
        {
            client = new TcpClient(serverIp, serverPort);
            stream = client.GetStream();
            isRunning = true;

            // 서버로부터 데이터를 받는 스레드 시작
            receiveThread = new Thread(ReceiveData2);
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("연결 오류: " + e.Message);
        }
    }

    private void ReceiveData2()
    {
        byte[] receiveBuffer = new byte[1024];
        try
        {
            while (isRunning)
            {
                int bytesRead = stream.Read(receiveBuffer, 0, receiveBuffer.Length);
                if (bytesRead > 0)
                {
                    receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, bytesRead);
                    //Debug.Log("서버로부터 받은 메시지: " + receivedMessage);

                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("데이터 수신 오류: " + e.Message);
        }
    }


// 클라이언트에서 서버로 데이터 보내기
    public void SendData(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
        catch (Exception e)
        {
            Debug.LogError("데이터 전송 오류: " + e.Message);
        }
    }

    private void OnDestroy()
    {
        isRunning = false;
        if (client != null)
        {
            client.Close();
        }
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        if (server != null)
        {
            server.Stop();
        }
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
    }

    public void StartSocketConnection()
    {
        // 버튼을 클릭하면 이 메서드가 호출됩니다.

        if (isRunning)
        {
            Debug.Log("이미 소켓 연결 중입니다.");
            return;
        }

        // 소켓 연결 시작
        ConnectToServer();
    }

    public bool returnGetMsg()
    {
        return receivedMessage != null;
    }

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        //ConnectToServer();
        gm = GameManager.instance;
    }

    public byte[] Send4png(byte[] CamN, byte[] CamE, byte[] CamS, byte[] CamW)
    {
        ///////////////////
        //도착한 결과 리턴
        return CamN;
    }

    // Update is called once per frame
    void Update()
    {   
        if (gm.GetSocketFlag() == true && isRunning)
        {
            //Debug.Log("test");
            SendData(gm.ReturnImgPathAndFramecounter());
            //Debug.Log(receivedMessage);
            gm.SystemIOFileLoad(receivedMessage);
            gm.SetSocketFlag(false);
        }
    }
}
