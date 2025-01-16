using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public bool isEnemiesJob;
    public GameObject preFab;
    public int amount;
    public float spawnRadius;
    public float2 moveSpeedRange;
    public float2 rotSpeedRange;
    public int batchAmount;

    private List<EnemyComponentAuthoring> enemies;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemies = new List<EnemyComponentAuthoring>();
        for (int i = 0; i < amount; i++)
        {
            EnemyComponentAuthoring enemy= Instantiate(preFab, new float3(Random.Range(-spawnRadius, spawnRadius),
                Random.Range(-spawnRadius, spawnRadius), 0), Quaternion.identity).GetComponent<EnemyComponentAuthoring>();
            enemy.rotSpeed = Random.Range(rotSpeedRange.x, rotSpeedRange.y);
            enemy.moveSpeed = Random.Range(moveSpeedRange.x, moveSpeedRange.y);
            enemies.Add(enemy);
        }
    }

    // void Update()
    // {
    //     foreach (EnemyComponentAuthoring enemy in enemies)
    //     {
    //         enemy.Move();
    //     }
    // }
}
