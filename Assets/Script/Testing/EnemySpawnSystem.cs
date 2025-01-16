using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;


public partial struct EnemySpawnSystem:ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        EnemySpawnerComponent spawner = SystemAPI.GetSingleton<EnemySpawnerComponent>();
        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        NativeArray<Entity> enemies = new NativeArray<Entity>(spawner.amount, Allocator.Temp);
        commandBuffer.Instantiate(spawner.preFab, enemies);
        for (int i = 0; i < spawner.amount; i++)
        {
            commandBuffer.SetComponent(enemies[i], new EnemyComponent
            {
                moveSpeed = Random.Range(spawner.moveSpeedRange.x, spawner.moveSpeedRange.y),
                rotSpeed = Random.Range(spawner.rotSpeedRange.x, spawner.rotSpeedRange.y),
            });
            commandBuffer.SetComponent(enemies[i], new LocalTransform
            {
                Scale = 1f,
                Position = new float3(Random.Range(-spawner.spawnRadius, spawner.spawnRadius),
                    Random.Range(-spawner.spawnRadius, spawner.spawnRadius), 0),
                Rotation = Quaternion.identity,
            });
            
        }
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
    }

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemySpawnerComponent>();
    }
}
