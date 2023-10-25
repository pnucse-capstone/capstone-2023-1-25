using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleManager : MonoBehaviour
{
    private Toggle toggle;
    private Text toggletext;
    private StreamingManager streamingManager;
    private SocketManager socketManager;
    private string ImgPath;
    static private ToggleManager instance;
    public int framecounter;
    // Start is called before the first frame update

    public static ToggleManager Instance = null;



    public bool State()
    {
        return toggle.isOn;
    }

    public string ImgPathreturn()
    {
        if (toggle.isOn) return ImgPath;
        else return null;
    }

    public int framecounter_return()
    {
        return framecounter;
    }
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        instance = this;
        toggle = GetComponent<Toggle>();
        toggletext = toggle.GetComponentInChildren<Text>();
        streamingManager = GetComponent<StreamingManager>();
        socketManager = GetComponent<SocketManager>();
        framecounter = 0;

        toggle.isOn = false;
        //streamingManager.removedir(streamingManager.unityPath());
        
    }

    // Update is called once per frame
    void Update()
    {
        if (toggle.isOn)
        {
            toggletext.text = "Running";
            if (framecounter == 0 || socketManager.RecevingPath())
            {
                
                ImgPath = streamingManager.Streamall(framecounter);
                framecounter++;
            }
        }
        else
        {
            toggletext.text = "Stop";
            streamingManager.removedir(streamingManager.unityPath());
            ImgPath = null;
        }
    }
}
