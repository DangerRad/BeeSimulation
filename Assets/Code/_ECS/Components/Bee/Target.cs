﻿using Unity.Entities;
using Unity.Mathematics;

public struct Target : IComponentData
{
    public float3 Position;
    public Entity Entity;
}
