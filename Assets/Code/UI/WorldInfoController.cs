using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GlobalInfoController : MonoBehaviour
{
    public static Action<int, int, int, Season, DayPhase, int> TickPassed;

    TMP_Text _amountOfTicks;

    void Awake()
    {
        _amountOfTicks = this.GetComponentInChildren<TMP_Text>();
    }

    void OnEnable()
    {
        TickPassed += UpdateTickInfo;
    }

    void OnDisable()
    {
        TickPassed -= UpdateTickInfo;
    }

    void UpdateTickInfo(int ticks, int ticksInYear, int dayNumber, Season currentSeason, DayPhase currentDayPhase,
        int beeSquads)
    {
        float year = 1f * ticks / ticksInYear;

        _amountOfTicks.text = "tick: " + ticks.ToString() + "\nyear: " + year.ToString("0.00") + "\nday: " + dayNumber +
                              "\n" + currentSeason + "\n" + currentDayPhase + "\nbeeSquads: " +beeSquads;
    }
}
