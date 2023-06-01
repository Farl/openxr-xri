using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OVRTest : MonoBehaviour
{
    public int targetFramerate;
    public int vsynCount;

    // Start is called before the first frame update
    void Awake()
    {
        Application.targetFrameRate = targetFramerate;
        QualitySettings.vSyncCount = vsynCount;
    }
}
