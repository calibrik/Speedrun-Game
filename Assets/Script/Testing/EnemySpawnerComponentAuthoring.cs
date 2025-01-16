using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class EnemySpawnerComponentAuthoring : MonoBehaviour
{
    public bool isEnemiesJob;
    public GameObject preFab;
    public int amount;
    public float spawnRadius;
    public float2 moveSpeedRange;
    public float2 rotSpeedRange;
    public int batchAmount;
    private class Baker:Baker<EnemySpawnerComponentAuthoring>
    {
        public override void Bake(EnemySpawnerComponentAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity,new EnemySpawnerComponent
            {
                amount = authoring.amount,
                spawnRadius = authoring.spawnRadius,
                moveSpeedRange = authoring.moveSpeedRange,
                rotSpeedRange = authoring.rotSpeedRange,
                preFab = GetEntity(authoring.preFab,TransformUsageFlags.Dynamic),
                isEnemiesJob = authoring.isEnemiesJob,
                batchAmount = authoring.batchAmount,
            });
        }
    }
}

public struct EnemySpawnerComponent:IComponentData
{
    public bool isEnemiesJob;
    public Entity preFab;
    public int amount;
    public float spawnRadius;
    public float2 moveSpeedRange;
    public float2 rotSpeedRange;
    public int batchAmount;
}