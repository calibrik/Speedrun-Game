using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct AudioInfo
{
    public AudioClip clip;
    [Range(0,256)]
    public int priority;
    [Range(0f,1f)]
    public float volume;
    [Range(-3f, 3f)]
    public float pitch;
    [Range(0f, 360)]
    public float spread;
    [Range(0, 1)]
    public float spatialBlend;
    [Range(0, 5)]
    public float dopplerLevel;
    public float minDistance;
    public float maxDistance;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager S
    {
        get; private set;
    }
    private void Start()
    {
        if (!S)
            S = this;
        else
            Destroy(gameObject);
    }
    IEnumerator DeleteSoundAfter(GameObject go, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(go);
    }
    public void PlayOnLocation(AudioInfo info, Vector2 pos)
    {
        if (!info.clip)
            return;
        GameObject go = new GameObject($"{info.clip.name} play");
        go.transform.position = pos;
        AudioSource source=go.AddComponent<AudioSource>();
        source.priority=info.priority;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.playOnAwake = false;
        source.volume = info.volume;
        source.pitch = info.pitch;
        source.spread = info.spread;
        source.spatialBlend = info.spatialBlend;
        source.minDistance = info.minDistance;
        source.maxDistance = info.maxDistance;
        source.dopplerLevel = info.dopplerLevel;
        source.PlayOneShot(info.clip);
        StartCoroutine(DeleteSoundAfter(go,info.clip.length));
    }
}
