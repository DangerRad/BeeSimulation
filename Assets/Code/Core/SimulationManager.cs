using System;
using Code.Core;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [SerializeField] BeeSpeciesSO[] _beeSpecies;
    // [Serializable]
    public BeeSpeciesSO[] BeeSpecies => _beeSpecies;
}
