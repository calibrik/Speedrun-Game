using System.Collections.Generic;
using System.Data;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Entities;
using Unity.Burst;

public class TestJob : MonoBehaviour
{
    public List<Transform> transforms;
    public GameObject prefab;
    public bool isMoveJob;
    public int count = 10000;

    private void Start()
    {
        transforms = new List<Transform>();
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(prefab);
            go.transform.position = new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
            transforms.Add(go.transform);
        }
        NativeArray<int> ints = new NativeArray<int>(new int[10000], Allocator.TempJob);
        SpawnJob job = new SpawnJob
        {
            ints = ints
        };
        job.Schedule(10000,10).Complete();
        for (int i = 0;i<10000;i++)
            Debug.Log(ints[i]);
        ints.Dispose();
    }
    void Update()
    {
        if (isMoveJob)
        {
            TransformAccessArray transformsAccess = new TransformAccessArray(transforms.Count);
            NativeList<float> randomSpeeds = new NativeList<float>(transforms.Count, Allocator.TempJob);
            for (int i = 0; i < transforms.Count; i++)
            {
                randomSpeeds.Add(Random.Range(-1f, 1f));
                transformsAccess.Add(transforms[i]);
            }
            MoveJob job = new MoveJob
            {
                randomSpeeds = randomSpeeds,
                deltaTime = Time.deltaTime,
            };
            JobHandle jh= job.Schedule(transformsAccess);
            jh.Complete();
            transformsAccess.Dispose();
            randomSpeeds.Dispose();
        }
        else
        {
            foreach (Transform t in transforms)
            {
                t.Translate(0, Random.Range(-1f, 1f) * Time.deltaTime, 0);
                MoveJob.HeavyTask();
            }
        }
    }
    [BurstCompile]
    public struct SpawnJob:IJobParallelFor
    {
        public NativeArray<int> ints;
        public void Execute(int i)
        {
            ints[i] = i;
        }
    }
}

[BurstCompile]
public struct MoveJob:IJobParallelForTransform
{
    [ReadOnly]
    public NativeList<float> randomSpeeds;
    [ReadOnly]
    public float deltaTime;
    public void Execute(int index,TransformAccess transform)
    {
        transform.position += new Vector3(0, randomSpeeds[index] * deltaTime, 0);
        HeavyTask();
    }
    static public void HeavyTask()
    {
        float value = 1;
        for (int i = 0; i < 2000; i++)
        {
            value = Mathf.Exp(Mathf.Pow(value, 10));
        }
    }
}
