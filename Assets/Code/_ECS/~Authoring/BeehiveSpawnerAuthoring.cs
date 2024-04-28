using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class BeehiveSpawnerAuthoring : MonoBehaviour
{
    public GameObject BeehivePrefab;
    public float SpawnArea;
    public int Amount;

    class Baker : Baker<BeehiveSpawnerAuthoring>
    {
        public override void Bake(BeehiveSpawnerAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new BeehiveSpawner
            {
                Beehive = GetEntity(authoring.BeehivePrefab, TransformUsageFlags.Dynamic),
                SpawnArea = authoring.SpawnArea,
                Amount = authoring.Amount,
            });
        }
    }
}
