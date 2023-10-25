using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using System.Diagnostics;
#endif

public class StreamingManager : MonoBehaviour
{
    public Camera streamcameraN;
    public Camera streamcameraS;
    public Camera streamcameraW;
    public Camera streamcameraE;
    public int FOV;
    private int resWidth;
    private int resHeight;
    public string path;
    string unitypath;
    string pythonpath;
    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 24;

        resWidth = 640;
        resHeight = 480;

        path = @"C:\Users\" + Environment.UserName + @"\UnityGraduate\";

        unitypath = path + @"UnityStream\";
        pythonpath = path + @"PythonStream\";
    }
#if UNITY_EDITOR
    [MenuItem("test/username")]
    static void test()
    {
        string path = @"C:\Users\" + Environment.UserName + @"\test\" + @"intothetest\";

        UnityEngine.Debug.Log(path);

        Directory.CreateDirectory(path);
    }
#endif

    void makedir()
    {

        DirectoryInfo dirUnity = new DirectoryInfo(unitypath);
        DirectoryInfo dirPython = new DirectoryInfo(pythonpath);

        if (!dirPython.Exists)
        {
            Directory.CreateDirectory(pythonpath);
        }
        if (!dirUnity.Exists)
        {
            Directory.CreateDirectory(unitypath);
        }
    }

    public void removedir(string delpath)
    {

        if (delpath == null) return;

        DirectoryInfo dirdelpath = new DirectoryInfo(delpath);

        if (dirdelpath.Exists && delpath != null)
        {
            File.SetAttributes(delpath, FileAttributes.Normal);

            foreach (string _folder in Directory.GetDirectories(delpath))
            {
                removedir(_folder);
            }

            foreach (string _file in Directory.GetFiles(delpath))
            {
                File.SetAttributes(_file, FileAttributes.Normal);
                File.Delete(_file);
            }
            Directory.Delete(delpath);
        }
    }

    void removepydir()
    {
        if (pythonpath == null) return;

        foreach (string _file in Directory.GetFiles(pythonpath))
        {
            File.SetAttributes(_file, FileAttributes.Normal);
            File.Delete(_file);
        }
    }

    public string unityPath()
    {
        return unitypath;
    }

    public string Streamall(int count)
    {
        string rawname;

        rawname = Stream(streamcameraN, "N", count);
        Stream(streamcameraE, "E", count);
        Stream(streamcameraS, "S", count);
        Stream(streamcameraW, "W", count);

        return rawname;

    }

    public string Stream(Camera streamcamera,string cameraname,int count)
    {
        makedir();

        string name;
        string rawname;

        name = unitypath + "Image" + count.ToString() + cameraname +".png";
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

    // Update is called once per frame
    void Update()
    {
        streamcameraE.fieldOfView = FOV;
        streamcameraN.fieldOfView = FOV;
        streamcameraW.fieldOfView = FOV;
        streamcameraS.fieldOfView = FOV;

    }

    private void OnApplicationQuit()
    {
        removepydir();
    }


}
