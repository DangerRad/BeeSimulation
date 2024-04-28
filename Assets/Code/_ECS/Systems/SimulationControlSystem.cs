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
        state.RequireForUpdate<Simulation>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var Simulation = SystemAPI.GetSingletonRW<Simulation>();

        int currentTick = Simulation.ValueRO.CurrentTick;
        int ticksInYear = Simulation.ValueRO.TicksInYear;

        if (Simulation.ValueRO.TimeToNextStep > 0)
        {
            Simulation.ValueRW.TimeToNextStep -= Time.deltaTime;
            return;
        }

        Simulation.ValueRW.CurrentTick++;
        Simulation.ValueRW.TimeToNextStep = Simulation.ValueRO.TickLength;

        int ticksInDay = Simulation.ValueRO.TicksInDay;
        int ticksInNight = Simulation.ValueRO.TicksInNight;
        int ticksInDayNight = ticksInDay + ticksInNight;
        int ticksInSeason = ticksInYear / 4;
        int currentYearTick = currentTick % ticksInYear;
        Season currentSeason = (Season)((currentYearTick / ticksInSeason + 1) % 4);
        int dayOfTheYear = (currentTick % ticksInYear) / ticksInDayNight;
        int currentTickInDay = currentTick % ticksInDayNight;
        int currentDayNightTick = currentTick % ticksInDayNight;
        DayPhase currentDayPhase = CalculateDayPhase(currentTickInDay);

        GlobalInfoController.TickPassed?.Invoke(currentTick, ticksInYear, dayOfTheYear, currentSeason,
            currentDayPhase, Simulation.ValueRO.BeeSquadCount); // move action to here and subscribe from controller.
        DayNightTickPassed?.Invoke(currentDayNightTick);
        Simulation.ValueRW.CurrentSeason = currentSeason;
        Simulation.ValueRW.SpawnWinterBees = SpawnWinterBees(ticksInSeason, currentYearTick);
        Simulation.ValueRW.CurrentDayPhase = currentDayPhase;
        Simulation.ValueRW.CurrentYearTick = currentYearTick;
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
