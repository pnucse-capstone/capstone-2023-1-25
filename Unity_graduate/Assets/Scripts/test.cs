using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Scripting.Python;
using UnityEditor;
#endif

using System.IO;
using UnityEngine.UI;

using System.Diagnostics;
using System;

public class test : MonoBehaviour
{
    bool flag = false;
    public Camera streamcameraN;
    public Camera streamcameraS;
    public Camera streamcameraW;
    public Camera streamcameraE;
    private Texture2D change_img;
    public RawImage thisImg;
    public static StreamingManager streamingmanager;
    public string path;
    string unitypath;
    string pythonpath;
    private int resWidth;
    private int resHeight;
    //#if UNITY_EDITOR
    //    [MenuItem("Python/Ensure Naming")]
    //#endif
    //    static void RunEnsureNaming()
    //    {
    //        string scriptPath = Path.Combine(Application.dataPath, "test.py");

    //        UnityEngine.Debug.Log("It's wokring on out");

    //#if UNITY_EDITOR
    //        PythonRunner.RunFile(scriptPath);
    //        UnityEngine.Debug.Log("It's working");
    //#endif
    //    }

#if UNITY_EDITOR
    [MenuItem("Python/test_Diagnostics")]
#endif
    static void pythonTest()
    {
        //if (Input.GetKeyDown(KeyCode.R))
        //{
            try
            {
                Process psi = new Process();
                psi.StartInfo.FileName = "C:/Users/user/yolov5-master/venvyolo/Scripts/python.exe";
                psi.StartInfo.Arguments = "C:/Users/user/Documents/GitHub/Unity_graduate/Assets/test.py";
                psi.StartInfo.CreateNoWindow = true;
                psi.StartInfo.UseShellExecute = false;
                psi.Start();
                UnityEngine.Debug.Log("python execute");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("python fuck off: " + e.Message);
            }
        //}
    }

    static void PrintHelloWorldFromPython()
    {
#if UNITY_EDITOR
        PythonRunner.RunString(@"
import UnityEngine;
import cv2;
cv2.imshow(
UnityEngine.Debug.Log('why not?')
");
#endif
    }

    private void SystemIOFileLoad()
    {
        pythonTest();

        if (!flag)
        {
            var path = "F:/testing/test.png";
            byte[] byteTexture = File.ReadAllBytes(path);
            if (byteTexture.Length > 0)
            {
                flag = true;
                UnityEngine.Debug.Log("Loding success");
                Texture2D texture = new Texture2D(0, 0);
                texture.LoadImage(byteTexture);
                UnityEngine.Debug.Log(byteTexture.Length);
                UnityEngine.Debug.Log(thisImg.texture);
                thisImg.texture = texture;
            }
            

        }
    }

    public void Streamall(int count)
    {

        Stream(streamcameraN, "N", count);
        Stream(streamcameraE, "E", count);
        Stream(streamcameraS, "S", count);
        Stream(streamcameraW, "W", count);


    }

    public string Stream(Camera streamcamera, string cameraname, int count)
    {

        string name;
        string rawname;

        name = unitypath + "Image" + count.ToString() + cameraname + ".png";
        rawname = unitypath + "Image" + count.ToString();


        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        streamcamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        Rect rec = new Rect(0, 0, screenShot.width, screenShot.height);
        streamcamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        screenShot.Apply();

        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(name, bytes);

        Destroy(rt);
        Destroy(screenShot);


        return rawname;
    }


    // Start is called before the first frame update
    void Start()
    {
        //    PrintHelloWorldFromPython();
        //UnityEngine.Debug.Log("It's working on real outside");

        streamingmanager = GetComponent<StreamingManager>();


        resWidth = 640;
        resHeight = 480;

        path = @"C:\Users\" + Environment.UserName + @"\UnityGraduate\";

        unitypath = path + @"UnityStreamtest\";
        pythonpath = path + @"PythonStreamtest\";

    }

    public void Calibrationimg()
    {
        int testcounter = 0;

        Streamall(testcounter);

        try
        {
            Process psi = new Process();
            psi.StartInfo.FileName = "C:/Users/user/yolov5-master/venvyolo/Scripts/python.exe";
            psi.StartInfo.Arguments = "C:/Users/user/Documents/GitHub/Unity_graduate/Assets/Scripts/python/testcalibration.py";
            psi.StartInfo.CreateNoWindow = true;
            psi.StartInfo.UseShellExecute = false;
            psi.Start();
            UnityEngine.Debug.Log("python execute");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("python fuck off: " + e.Message);
        }
    }


    // Update is called once per frame
    void Update()
    {
        //pythonTest();

        //change_img = Resources.Load("test") as Texture2D;

        //UnityEngine.Debug.Log(change_img);

        //thisImg.texture = change_img;

        //if (thisImg.texture==null)
        //{
        //    SystemIOFileLoad();
        //}
    }
}