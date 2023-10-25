using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PythonManager : MonoBehaviour
{
    Process py = new Process();
    string pyPath = @"C:\Users\" + Environment.UserName + @"\UnityGraduate\";
    string yourPythonPath = "C:/Users/user/yolov5-master/venvyolo/Scripts/python.exe";

#if UNITY_EDITOR
    //테스트용, 에디터에서만 실행가능
    [MenuItem("test/Calibration.py_exe")]
    static void testPython()
    {
        Process py = new Process();
        string pyPath = @"C:\Users\" + Environment.UserName + @"\UnityGraduate\";
        string yourPythonPath = "C:/Users/user/yolov5-master/venvyolo/Scripts/python.exe";
        try
        {
            py.StartInfo.FileName = yourPythonPath;
            py.StartInfo.Arguments = pyPath + "Graduate.py";
            py.StartInfo.CreateNoWindow = true;
            py.StartInfo.UseShellExecute = false;
            py.Start();
            UnityEngine.Debug.Log("Calibration.py Start!");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("python Fuck Off: " + e.Message);
        }
    }
#endif
    void startPython()
    {
        try
        {
            py.StartInfo.FileName = yourPythonPath;
            py.StartInfo.Arguments = pyPath + "Calibration.py";
            py.StartInfo.Arguments = pyPath + "Calibration.py";
            py.StartInfo.CreateNoWindow = true;
            py.StartInfo.UseShellExecute = false;
            py.Start();
            UnityEngine.Debug.Log("Calibration.py Start!");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("python Fuck Off: " + e.Message);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        startPython();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnApplicationQuit()
    {
        if (!py.HasExited)
        {
            py.Kill();
            py.WaitForExit();
        }
    }
}

