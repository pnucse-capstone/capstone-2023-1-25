using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Diagnostics;
using UnityEngine.XR;
using System.Text.RegularExpressions;
using TMPro;
using System.Linq;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    string pyPath = @"C:\Users\" + Environment.UserName + @"\UnityGraduate\";
    string yourPythonPath = "F:/Anaconda3_envs/graduate/Scripts/python.exe";
    string ImagePath;
    private NetworkManager net;
    private PhysicsMovment phy;
    private int framecounter = 0;
    private int resWidth = 640;
    private int resHeight = 480;
    public Camera streamcameraN;
    public Camera streamcameraS;
    public Camera streamcameraW;
    public Camera streamcameraE;
    private byte[] camera1png;
    private byte[] camera2png;
    private byte[] camera3png;
    private byte[] camera4png;
    private byte[] resultimg;
    private string path;
    private string unitypath;
    private string pythonpath;
    private Texture2D texture;
    private Texture2D change_img;
    public RawImage streaming;
    bool socketflag = false;
    public List<bool> boolList = new List<bool>();
    public List<TextMeshProUGUI> textMeshProUGUIList;
    public List<RectTransform> rectTransformList;
    public TextMeshProUGUI situationtext;
    public WheelCollider[] wheels = new WheelCollider[4];
    public GameObject particle;
    Rigidbody rb;
    private float startingdistance = 9999;
    private float standardangle = 90;
    private float standarddistance = 0;
    private Situation currentState;
    private Situation previoustate;
    private int paintingcounter = 0;
    private bool start = false;
    private float linecoordinate = 0;
    public enum Situation
    {
        BeforeEntering,
        Passing,
        Painting,
        ReEntry,
        JobFinish
    }
    public bool GetSocketFlag()
    {
        return socketflag;
    }
    public void SetSocketFlag(bool value)
    {
        socketflag = value;
    }
    public void Streaming(int count)
    {
        camera1png = Stream(streamcameraN, "N");
        camera2png = Stream(streamcameraE, "E");
        camera3png = Stream(streamcameraS, "S");
        camera4png = Stream(streamcameraW, "W");
        resultimg = net.Send4png(camera1png, camera2png, camera3png, camera4png);
    }
    // 촬영
    public byte[] Stream(Camera streamcamera , string cameraname)
    {
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        streamcamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        Rect rec = new Rect(0, 0, screenShot.width, screenShot.height);
        streamcamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        screenShot.Apply();

        byte[] img = screenShot.EncodeToPNG();

        ImagePath = pyPath + @"UnityStream2\Image" + framecounter.ToString() + cameraname + ".png";

        File.WriteAllBytes(ImagePath, img);

        UnityEngine.Object.Destroy(rt);
        UnityEngine.Object.Destroy(screenShot);

        return img;
    }
    // 이미지 이름 만들기
    public string ReturnImgPathAndFramecounter()
    {
        if (net.returnGetMsg())
        {
            return pyPath + @"UnityStream2\Image" + framecounter++.ToString();

        }
        return pyPath + @"UnityStream2\Image" + framecounter.ToString();
    }
    // 종료 시 모든 파일 다 지우기
    void Removepydir()
    {
        if (pythonpath == null) return;

        foreach (string _file in Directory.GetFiles(pythonpath))
        {
            File.SetAttributes(_file, FileAttributes.Normal);
            File.Delete(_file);
        }
    }
    // 종료 시 모든 파일 다 지우기2
    void Removeunitydir()
    {
        if (unitypath == null) return;

        foreach (string _file in Directory.GetFiles(unitypath))
        {
            File.SetAttributes(_file, FileAttributes.Normal);
            File.Delete(_file);
        }
    }
    // 정보 읽어오기
    public void SystemIOFileLoad(string path)
    {
        string txtPath = path + ".txt";
        Dictionary<string, List<string> > textureInfo = new Dictionary<string, List<string>>();
        byte[] byteTexture = File.ReadAllBytes(path+".png");
        if (byteTexture.Length > 0)
        {
            change_img = texture;
            texture = new Texture2D(0, 0);
            texture.LoadImage(byteTexture);
            streaming.texture = texture;
            if (change_img != null) Texture2D.Destroy(change_img);

            try
            {
                // 텍스트 파일에서 데이터 읽기
                string[] lines = File.ReadAllLines(txtPath);
                start = true;
                foreach (string line in lines)
                {
                    string[] parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        if (key == "point"){
                            textureInfo[key] = new List<string>(value.Split(new string[] { "]]" }, StringSplitOptions.None));
                        }
                        else
                        {
                            textureInfo[key] = new List<string>(value.Split(','));
                        }
                    }
                }

                InfoHandling(textureInfo);
                // 데이터 확인
                foreach (var entry in textureInfo)
                {
                    //UnityEngine.Debug.Log($"{entry.Key}: {string.Join(", ", entry.Value)}");
                }
            }
            catch (Exception e)
            {
                //UnityEngine.Debug.Log($"Error reading the file: {e.Message}");
            }
        }
    }
    // 정보를 이용해서 기준선 정하기
    public void InfoHandling(Dictionary<string, List<string>> textureInfo)
    {
        int mindistance = 9999;
        int lineamount = textureInfo["degree"].Count;
        //UnityEngine.Debug.Log(lineamount);

        //UnityEngine.Debug.Log(textureInfo["point"]);

        for (int i = 0; i < lineamount; i++)
        {
            string cls = textureInfo["cls"][i];
            string strdegree = textureInfo["degree"][i];
            string strdistance = textureInfo["distance"][i];
            string strpoint = textureInfo["point"][i];

            float degree = ConvertTofloat(strdegree);
            float distance = ConvertTofloat(strdistance);
            degree = (float)Math.Round(degree, 2);
            List<float> points = ConvertTofloats(strpoint);
            List<float> linelocation = linelocationavg(points);

            textMeshProUGUIList[i].text = "CLASS : " + ExtractAlphabets(cls) + "\nAngle : " + degree + "\nDISTANCE : " + distance/100.0;
            rectTransformList[i].position = new Vector3(linelocation[0]*2/3+1520, 400-linelocation[1]*2/3+680, 0);
            if(currentState == Situation.ReEntry)
            {
                rectTransformList[i].position = new Vector3(9999,9999, 0);
            }
            //UnityEngine.Debug.Log(degree);
            //UnityEngine.Debug.Log(distance);
            //UnityEngine.Debug.Log(textureInfo["cls"][i]);
            //UnityEngine.Debug.Log(points[0]+ points[1]+ points[2]+ points[3]);
            bool isHave = cls.Contains("welding");
            if (isHave)
            {  
                if (distance < mindistance)
                {
                    standardangle = degree;
                    standarddistance = distance;
                    linecoordinate = linelocation[0]; // x좌표
                }
            }
            else
            {
                startingdistance = distance;
                    UnityEngine.Debug.Log("notTurn~");
                    // 상태 변화
                    if (distance < 85 && currentState != Situation.Passing)
                    {
                        previoustate = currentState;
                        currentState = Situation.Passing;
                        particle.SetActive(false);
                    }
                    else if (distance > 85 & currentState == Situation.Passing)
                    {
                        if (previoustate == Situation.BeforeEntering)
                        {
                            currentState = Situation.Painting;
                            paintingcounter += 1;
                            particle.SetActive(true);
                        }
                        else if (previoustate == Situation.Painting && paintingcounter != 2)
                        {
                            currentState = Situation.ReEntry;
                            particle.SetActive(false);
                        }
                        else if (previoustate == Situation.ReEntry)
                        {
                            currentState = Situation.Painting;
                            paintingcounter += 1;
                            particle.SetActive(true);
                        }
                        else if (previoustate == Situation.Painting && paintingcounter == 2)
                        {
                            currentState = Situation.JobFinish;
                            particle.SetActive(false);
                        }
                        UnityEngine.Debug.Log(paintingcounter);
                    }
                    UnityEngine.Debug.Log("paintingcounter : " + paintingcounter);
            }
        }
        // 최소거리 초기화
        mindistance = 9999;
        // 텍스트 상자 조종
        if (lineamount < 3)
        {
            for (int i = 2; i > lineamount-1; i--)
            {
                rectTransformList[i].position = new Vector3(-1000, -1000, 0);
            }
        }
    }
    // 지저분한 문자열을 float로
    private float ConvertTofloat(string str)
    {
        Regex r = new Regex(@"-?[0-9]*\.*[0-9]+");
        Match m = r.Match(str);
        return float.Parse(m.Value);
    }
    // 지저분한 문자열을 float 배열로
    private List<float> ConvertTofloats(string str)
    {
        List<float> returnlist = new List<float>();
        Regex r = new Regex(@"-?[0-9]*\.?[0-9]+");
        MatchCollection m = r.Matches(str);

        foreach (Match match in m) 
        {
            returnlist.Add(float.Parse(match.Value));
        }
        return returnlist;
    }

    private string ExtractAlphabets(string str)
    {
        string pattern = "[^a-zA-Z\\s]";
        string result = Regex.Replace(str, pattern, "");

        return result;
    }

    private List<float> linelocationavg(List<float> list)
    {
        List<float> result = new List<float>();
        result.Add((list[0] + list[2])/2);
        result.Add((list[1] + list[3])/2);
        return result;
    }

    public float returnAngle() => standardangle;

    public float returnDistance() => standarddistance;

    public float returnCordinate() => linecoordinate; 
    public Situation ReturnSituation() => currentState;
    public int returnFrameCounter() => framecounter;

    public float returnStartingDistance() => startingdistance;

    //public bool streamStart() => start;

    void Awake()
    {
        instance = this;
        net = NetworkManager.instance;
        phy = PhysicsMovment.instance;
    }
    // Start is called before the first frame update
    void Start()
    {
        path = @"C:\Users\" + Environment.UserName + @"\UnityGraduate\";

        unitypath = path + @"UnityStream2\";
        pythonpath = path + @"PythonStream\";
        currentState = Situation.BeforeEntering;
    }

    private void FixedUpdate()
    {
    }
    // Update is called once per frame
    void Update()
    {

        if (socketflag == false && currentState!=Situation.JobFinish)
        {
            
            Streaming(framecounter);
            socketflag = true;
        }
            situationtext.text = currentState.ToString();
    }

    private void OnApplicationQuit()
    { 
        Removeunitydir();
        Removepydir();
    }
}
