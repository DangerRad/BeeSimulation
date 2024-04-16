using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;
    }

    // Update is called once per frame
    // void Update()
    // {
    //     Application.targetFrameRate = -1;
    //     QualitySettings.vSyncCount = 0;
    // }
}
