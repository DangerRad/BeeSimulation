using System.Xml;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[UpdateBefore(typeof(MoveSystem))]
public partial struct CollectSystem : ISystem
{
    [ReadOnly] ComponentLookup<Flower> flowerLookUp;
    [ReadOnly] ComponentLookup<Beehive> hiveLookUp;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Collecting>();
        flowerLookUp = state.GetComponentLookup<Flower>();
        hiveLookUp = state.GetComponentLookup<Beehive>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        flowerLookUp.Update(ref state);
        hiveLookUp.Update(ref state);
        state.Dependency = new CollectJob
        {
            HiveLookUp = hiveLookUp,
            FlowerLookUp = flowerLookUp,
            dt = SystemAPI.Time.DeltaTime,
            ECB = ECB,
        }.Schedule(state.Dependency);
    }
}

[BurstCompile]
[WithAll(typeof(Collecting))]
[WithDisabled(typeof(Moving))]
public partial struct CollectJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public float dt;
    [ReadOnly] public ComponentLookup<Flower> FlowerLookUp;
    [ReadOnly] public ComponentLookup<Beehive> HiveLookUp;

    public void Execute(ref BeeSquad beeSquad, ref Timer timer, ref Target target, in BeeColonyStats stats,
        HiveLocationInfo info, Entity entity)
    {
        timer.TimeLeft -= dt;

        if (timer.TimeLeft < 0)
        {
            ECB.AddComponent<Searching>(entity);
            ECB.RemoveComponent<Collecting>(entity);
        }
        else
        {
            Flower flower = FlowerLookUp[target.Entity];
            float nectarToCollect = beeSquad.Size * stats.FoodGatherSpeed * dt;
            float spaceLeftInBackpack = stats.MaxFoodHeld * beeSquad.Size - beeSquad.FoodHeld;
            float nectarLeftOnFlower = flower.NectarHeld;
            float maximumClamp = math.max(spaceLeftInBackpack, nectarLeftOnFlower);
            float finalNectarToCollect = math.clamp(nectarToCollect, 0, maximumClamp);
            flower.NectarHeld -= finalNectarToCollect;
            beeSquad.FoodHeld += finalNectarToCollect;
            Beehive beehive = HiveLookUp[info.BeehiveEntity];
            beehive[(int)flower.Species] += finalNectarToCollect;

            ECB.SetComponent(target.Entity, flower);
            ECB.SetComponent(entity, beeSquad);
            ECB.SetComponent(info.BeehiveEntity, beehive);

            if (flower.NectarHeld <= 0)
            {
                ECB.AddComponent<Searching>(entity);
                ECB.RemoveComponent<Collecting>(entity);
                ECB.SetComponentEnabled<Flower>(target.Entity, false);
            }
            else if (spaceLeftInBackpack <= 0)
            {
                ECB.RemoveComponent<Collecting>(entity);
                ECB.SetComponentEnabled<Moving>(entity, true);
                ECB.AddComponent<Delivering>(entity);
                ECB.SetComponent(entity, new Target { Position = info.HivePosition + new float3(0, 1, 0) });
            }
        }
    }
}
