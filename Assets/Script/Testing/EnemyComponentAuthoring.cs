using UnityEngine;
using Unity.Entities;

public class EnemyComponentAuthoring : MonoBehaviour
{
    public float rotSpeed;
    public float moveSpeed;

    // public void Update()
    // {
    //     transform.Rotate(0, 0, rotSpeed * Time.deltaTime);
    //     transform.Translate(moveSpeed * Time.deltaTime*Random.insideUnitCircle);
    //     // MoveJob.HeavyTask();
    // }
    public class Baker : Baker<EnemyComponentAuthoring>
    {
        public override void Bake(EnemyComponentAuthoring authoring)
        {
            Entity entity=GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemyComponent
            {
                rotSpeed = 0,
                moveSpeed = 0,
            });
        }
    }
}

public struct EnemyComponent:IComponentData
{
    public float rotSpeed;
    public float moveSpeed;
}