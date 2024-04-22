using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

public class BeehiveInfoController : MonoBehaviour
{
    [SerializeField] BeehiveInfoDisplay _display;
    public static Action<int, float, float[], int, float> BeehiveInfoChanged;
    public static Action<float3> DisplayPositionChanged;

    void OnEnable()
    {
        BeehiveInfoChanged += UpdateDisplay;
        DisplayPositionChanged += UpdatePosition;
    }

    void OnDisable()
    {
        BeehiveInfoChanged -= UpdateDisplay;
        DisplayPositionChanged -= UpdatePosition;
    }

    void UpdatePosition(float3 position)
    {
        _display.UpdatePosition(position);
    }

    void UpdateDisplay(int population, float foodLeft, float[] foodByType, int squadCount, float mitesInfestation)
    {
        _display.UpdateInfo(population, foodLeft, foodByType, squadCount, mitesInfestation);
    }
}
