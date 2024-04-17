using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    // public static Action<int> DayNightTickPassed;

    [SerializeField] LightDayPhaseInfo[] _dayPhases;
    [SerializeField] Light _light;

    void Awake()
    {
        if (_light == null)
        {
            Debug.LogError("there is no light attached to light controller");
            Destroy(this);
        }
    }

    void Start()
    {
        _light.intensity -= 1;
    }

    void OnEnable()
    {
        SimulationControlSystem.DayNightTickPassed += ChangeLightValue;
    }


    void OnDisable()
    {
        SimulationControlSystem.DayNightTickPassed -= ChangeLightValue;
    }

    //todo add dotween and ease it.
    void ChangeLightValue(int tick)
    {
        int i = _dayPhases.Length - 1;
        while (_dayPhases[i].TickStart > tick)
        {
            i--;
        }

        LightDayPhaseInfo currentDayPhase = _dayPhases[i];
        LightDayPhaseInfo nextDayPhase = _dayPhases[(i + 1) % _dayPhases.Length];
        float t = Mathf.InverseLerp(currentDayPhase.TickStart, nextDayPhase.TickStart, tick);
        float temperature = Mathf.Lerp(currentDayPhase.Temperature, nextDayPhase.Temperature, t);
        float intensity = Mathf.Lerp(currentDayPhase.Intensity, nextDayPhase.Intensity, t);
        _light.colorTemperature = temperature;
        _light.intensity = intensity;


    }
}

[Serializable]
public struct LightDayPhaseInfo
{
    public int TickStart;
    public float Temperature;
    public float Intensity;
}
