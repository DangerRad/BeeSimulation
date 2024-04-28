using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics;
using RaycastHit = Unity.Physics.RaycastHit;

public partial class SelectionSystem : SystemBase
{
    Camera _mainCamera;
    Entity _selectedEntity;

    protected override void OnCreate()
    {

        RequireForUpdate<Hit>();
        RequireForUpdate<Simulation>();
        RequireForUpdate<PhysicsWorldSingleton>();
    }

    protected override void OnStartRunning()
    {
        _mainCamera = Camera.main;
    }

    protected override void OnUpdate()
    {

        var hitInfo = SystemAPI.GetSingletonRW<Hit>();
        hitInfo.ValueRW.HitChanged = false;
        if (Input.GetMouseButtonDown(0))
        {
            PhysicsWorldSingleton collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastInput input = new RaycastInput()
            {
                Start = ray.origin,
                End = ray.GetPoint(100f),
                Filter = new CollisionFilter()
                {
                    BelongsTo = ~0u,
                    CollidesWith = (uint)1 << 6,
                    GroupIndex = 0
                }
            };

            RaycastHit hit = new RaycastHit();
            bool haveHit = collisionWorld.CastRay(input, out hit);
            if (haveHit)
            {
                hitInfo.ValueRW.HitEntity = hit.Entity;
                hitInfo.ValueRW.HitChanged = true;
            }
        }
    }
}
