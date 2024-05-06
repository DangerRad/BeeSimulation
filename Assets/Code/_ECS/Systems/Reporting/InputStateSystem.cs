using Unity.Burst;
using Unity.Entities;
using UnityEngine;


public partial struct InputStateSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InputState>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var inputState = SystemAPI.GetSingletonRW<InputState>();
        inputState.ValueRW.Delete = Input.GetKeyDown(KeyCode.Delete);
    }
}
