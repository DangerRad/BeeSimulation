using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics;
using RaycastHit = Unity.Physics.RaycastHit;
using Plane = UnityEngine.Plane;

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

        var beehiveSpawnInfo = SystemAPI.GetSingletonRW<BeehvieSpawnInfo>();
        beehiveSpawnInfo.ValueRW.HasClicked = false;

        if (Input.GetMouseButtonDown(0))
        {
            SelectHiveToShowInfo(hitInfo);
        }

        if (Input.GetMouseButtonDown(1))
        {
            CreateNewHive(beehiveSpawnInfo);
        }
    }

    static void CreateNewHive(RefRW<BeehvieSpawnInfo> beehiveSpawnInfo)
    {
        bool hasHit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hasHit = new Plane(Vector3.up, 0f).Raycast(ray, out var dist);
        if (hasHit)
        {
            beehiveSpawnInfo.ValueRW.HasClicked = true;
            beehiveSpawnInfo.ValueRW.Position = ray.GetPoint(dist);
        }
    }

    void SelectHiveToShowInfo(RefRW<Hit> hitInfo)
    {
        byte hiveObjectLayer = 6;
        bool haveHit = Hit(hiveObjectLayer, out var hit);
        if (haveHit)
        {
            hitInfo.ValueRW.HitEntity = hit.Entity;
            hitInfo.ValueRW.HitChanged = true;
        }
    }

    bool Hit(byte layer, out RaycastHit hit)
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
                CollidesWith = (uint)1 << layer,
                GroupIndex = 0
            }
        };
        hit = new RaycastHit();
        bool haveHit = collisionWorld.CastRay(input, out hit);
        return haveHit;
    }
}
