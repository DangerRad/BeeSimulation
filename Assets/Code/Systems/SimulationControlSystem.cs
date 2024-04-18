using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

public partial struct SimulationControlSystem : ISystem
{
    public static Action<int> DayNightTickPassed;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingletonRW<Config>();

        int currentTick = config.ValueRO.CurrentTick;
        int ticksInYear = config.ValueRO.TicksInYear;

        if (config.ValueRO.TimeToNextStep > 0)
        {
            config.ValueRW.TimeToNextStep -= Time.deltaTime;
            return;
        }

        config.ValueRW.CurrentTick++;
        config.ValueRW.TimeToNextStep = config.ValueRO.TickLength;

        int ticksInDay = config.ValueRO.TicksInDay;
        int ticksInNight = config.ValueRO.TicksInNight;
        int ticksInDayNight = ticksInDay + ticksInNight;
        int ticksInSeason = ticksInYear / 4;
        int currentYearTick = currentTick % ticksInYear;
        Season currentSeason = (Season)((currentYearTick / ticksInSeason + 1) % 4);
        int dayOfTheYear = (currentTick % ticksInYear) / ticksInDayNight;
        int currentTickInDay = currentTick % ticksInDayNight;
        int currentDayNightTick = currentTick % ticksInDayNight;
        DayPhase currentDayPhase = CalculateDayPhase(currentTickInDay);

        GlobalInfoController.TickPassed?.Invoke(currentTick, ticksInYear, dayOfTheYear, currentSeason,
            currentDayPhase, config.ValueRO.BeeSquadCount); // move action to here and subscribe from controller.
        DayNightTickPassed?.Invoke(currentDayNightTick);
        config.ValueRW.currentSeason = currentSeason;
        config.ValueRW.SpawnWinterBees = SpawnWinterBees(ticksInSeason, currentYearTick);
        config.ValueRW.currentDayPhase = currentDayPhase;
    }

    static bool SpawnWinterBees(int ticksInSeason, int currentYearTick)
    {
        int tickToStartSpawnWinterBee = ticksInSeason * 3 - SimulationData.TICKS_BEFORE_WINTER_TO_SPAWN_WINTER_BEES;
        if (tickToStartSpawnWinterBee <= currentYearTick)
        {
            return true;
        }

        return false;
    }

    //TODO change to more scalable version
    static DayPhase CalculateDayPhase(int currentTickInDay)
    {
        DayPhase currentDayPhase;
        if (currentTickInDay < 15)
            currentDayPhase = DayPhase.Morning;
        else if (currentTickInDay < 45)
            currentDayPhase = DayPhase.Midday;
        else if (currentTickInDay < 60)
            currentDayPhase = DayPhase.Evening;
        else
            currentDayPhase = DayPhase.Night;
        return currentDayPhase;
    }
}
