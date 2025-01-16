
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Random = UnityEngine.Random;

public partial struct EnemiesMoveSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemySpawnerComponent>();
        state.RequireForUpdate<EnemyComponent>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EnemySpawnerComponent spawner = SystemAPI.GetSingleton<EnemySpawnerComponent>();
        if (spawner.isEnemiesJob)
        {
            NativeArray<EnemyAspect> enemies=new NativeArray<EnemyAspect>(spawner.amount, Allocator.TempJob);
            NativeArray<float3> randomDirs = new NativeArray<float3>(spawner.amount, Allocator.TempJob);
            int i = 0;
            foreach (EnemyAspect enemyAspect in SystemAPI.Query<EnemyAspect>())
            {
                enemies[i] = enemyAspect;
                randomDirs[i++] = Random.insideUnitSphere;
            }
            EnemyMoveJob job = new EnemyMoveJob
            {
                enemies = enemies,
                randomDirs = randomDirs,
                deltaTime = SystemAPI.Time.DeltaTime,
            };
            job.Schedule(spawner.amount,spawner.batchAmount).Complete();
            enemies.Dispose();
            randomDirs.Dispose();
        }
        else
        {
            foreach (EnemyAspect enemy in SystemAPI.Query<EnemyAspect>())
            {
                enemy.localTransform.ValueRW =
                    enemy.localTransform.ValueRO.RotateZ(enemy.enemyComponent.ValueRO.rotSpeed *
                                                         SystemAPI.Time.DeltaTime);
                enemy.localTransform.ValueRW =
                    enemy.localTransform.ValueRO.Translate(enemy.enemyComponent.ValueRO.moveSpeed *
                                                           SystemAPI.Time.DeltaTime * Random.insideUnitSphere);
                // MoveJob.HeavyTask();
            }
        }
    }

    [BurstCompile]
    private partial struct EnemyMoveJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<EnemyAspect> enemies;
        [ReadOnly] public NativeArray<float3> randomDirs;
        [ReadOnly] public float deltaTime;

        public void Execute(int i)
        {
            enemies[i].localTransform.ValueRW =
                enemies[i].localTransform.ValueRO.RotateZ(enemies[i].enemyComponent.ValueRO.rotSpeed *
                                                          deltaTime);
            enemies[i].localTransform.ValueRW =
                enemies[i].localTransform.ValueRO.Translate(enemies[i].enemyComponent.ValueRO.moveSpeed * deltaTime * randomDirs[i]);
            // MoveJob.HeavyTask();
        }
    }
}

public readonly partial struct EnemyAspect : IAspect
{
    public readonly RefRW<LocalTransform> localTransform;
    public readonly RefRO<EnemyComponent> enemyComponent;
}