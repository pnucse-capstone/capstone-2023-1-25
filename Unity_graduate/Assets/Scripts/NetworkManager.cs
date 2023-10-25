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
    public string image_data; // �̹��� �����͸� ���ڿ��� ����
    public string control_signal; // ���� ��ȣ�� ���ڿ��� ����
}
    public void ConnectToServer()
    {
        try
        {
            client = new TcpClient(serverIp, serverPort);
            stream = client.GetStream();
            isRunning = true;

            // �����κ��� �����͸� �޴� ������ ����
            receiveThread = new Thread(ReceiveData2);
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("���� ����: " + e.Message);
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
                    //Debug.Log("�����κ��� ���� �޽���: " + receivedMessage);

                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("������ ���� ����: " + e.Message);
        }
    }


// Ŭ���̾�Ʈ���� ������ ������ ������
    public void SendData(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
        catch (Exception e)
        {
            Debug.LogError("������ ���� ����: " + e.Message);
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
        // ��ư�� Ŭ���ϸ� �� �޼��尡 ȣ��˴ϴ�.

        if (isRunning)
        {
            Debug.Log("�̹� ���� ���� ���Դϴ�.");
            return;
        }

        // ���� ���� ����
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
        //������ ��� ����
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
