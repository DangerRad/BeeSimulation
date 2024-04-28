using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;
    }
}
