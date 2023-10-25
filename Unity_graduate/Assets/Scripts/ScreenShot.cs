using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ScreenShot : MonoBehaviour
{
    public Camera around_camera;       //보여지는 카메라.

    private int resWidth;
    private int resHeight;
    string path;

    RaycastHit hit;

    // Use this for initialization
    void Start()
    {
        //Debug.Log(camera.name + " : \n" + camera.projectionMatrix);
        resWidth = Screen.width;
        resHeight = Screen.height;
        resWidth = 640;
        resHeight = 480;
        path = Application.dataPath + "/ScreenShot/";
        //Debug.Log(path);

    }

    public void ClickScreenShot()
    {
        //if (Physics.Raycast(camera.transform.position,Vector3.up,out hit,MaxDistance))
        //{
        //    Debug.Log(hit.distance);
        //}
        DirectoryInfo dir = new DirectoryInfo(path);
        if (!dir.Exists)
        {
            Directory.CreateDirectory(path);
        }
        string name;
        if (around_camera.name == "Camera_N")
        {
            name = path + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-N") + ".png";
        } else if (around_camera.name == "Camera_W")
        {
            name = path + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-W") + ".png";
        } else if (around_camera.name == "Camera_E")
        {
            name = path + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-E") + ".png";
        } else
        {
            name = path + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-S") + ".png";
        }
        
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        around_camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        Rect rec = new Rect(0, 0, screenShot.width, screenShot.height);
        around_camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        screenShot.Apply();

        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(name, bytes);
    }
}