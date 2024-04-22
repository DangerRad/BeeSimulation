using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class BeehiveInfoDisplay : MonoBehaviour
{
    TMP_Text[] _infos;
    [SerializeField] Vector3 _offset;

    // Start is called before the first frame update
    void Awake()
    {
        _infos = GetComponentsInChildren<TMP_Text>();
    }

    public void UpdateInfo(int size, float foodLeft, float[] foodByType, int squadCount, float mitesInfestation)
    {
        _infos[0].text = "size: " + size.ToString() + " in " + squadCount + " squads";
        // _infos[1].text = "food: " + foodLeft.ToString("0.0");
        //todo choose top x of food type to display in percentage of total food stored
        _infos[1].text = FlowerSpecies.Daisy.ToString() + ": " + foodByType[0].ToString("0.0");
        _infos[2].text = FlowerSpecies.Lavender.ToString() + ": " + foodByType[1].ToString("0.0");
        _infos[3].text = FlowerSpecies.Goldenrod.ToString() + ": " + foodByType[2].ToString("0.0");
        _infos[4].text = "Total food: " + foodLeft.ToString("0.0");
        _infos[5].text = "Mites: " + mitesInfestation.ToString("0.000");
    }

    public void UpdatePosition(float3 newPosition)
    {
        transform.position = (Vector3)newPosition + _offset;
    }
}
