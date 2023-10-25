using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CMBlendListSetting : MonoBehaviour
{
    CinemachineBlendListCamera blendList;

    GameObject vCamObj1;
    GameObject vCamObj2;
    CinemachineVirtualCameraBase vCam1;
    CinemachineVirtualCameraBase vCam2;

    void Start()
    {
        blendList = this.GetComponent<CinemachineBlendListCamera>();

        blendList.m_Loop = false;

        vCamObj1 = GameObject.Find("POVCAM");
        vCamObj2 = GameObject.Find("MainVcam");
        vCam1 = vCamObj1.GetComponent<CinemachineVirtualCameraBase>();
        vCam2 = vCamObj2.GetComponent<CinemachineVirtualCameraBase>();
        blendList.m_Instructions[0].m_Hold = 9999.0f;
    }

    public void buttonLeft()
    {
        vCamObj1.transform.SetParent(this.transform);
        vCamObj2.transform.SetParent(this.transform);

        blendList.m_Instructions[0].m_VirtualCamera = vCam1;
        blendList.m_Instructions[1].m_VirtualCamera = vCam2;

        blendList.m_Instructions[1].m_Blend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        blendList.m_Instructions[1].m_Blend.m_Time = 2.0f;

        blendList.m_Instructions[0].m_Hold = 1.0f;
    }

    public void buttonRight()
    {
        vCamObj2.transform.SetParent(this.transform);
        vCamObj1.transform.SetParent(this.transform);

        blendList.m_Instructions[0].m_VirtualCamera = vCam2;
        blendList.m_Instructions[1].m_VirtualCamera = vCam1;

        blendList.m_Instructions[1].m_Blend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        blendList.m_Instructions[1].m_Blend.m_Time = 2.0f;

        blendList.m_Instructions[0].m_Hold = 1.0f;
    }
}