using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactPool : MonoBehaviour
{
    readonly Queue<ParticleSystem> pool = new Queue<ParticleSystem>();

    public void Initialize(int amount)
    {
        for (int i = 0; i < amount; i++)
            pool.Enqueue(Create());
    }

    ParticleSystem Create()
    {
        var go = new GameObject("Pooled_Impact");
        go.transform.SetParent(transform);

        var ps = go.AddComponent<ParticleSystem>();
        // Important in Unity 6: disable automatic playback before changing duration.
        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.45f;
        main.startLifetime = 0.45f;
        main.startSpeed = 2.2f;
        main.startSize = 0.12f;
        main.maxParticles = 20;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.15f;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        go.SetActive(false);
        return ps;
    }

    public void Play(Vector3 pos, Color color)
    {
        if (pool.Count == 0) return;

        var ps = pool.Dequeue();
        ps.transform.position = pos;
        ps.gameObject.SetActive(true);

        var main = ps.main;
        main.startColor = color;
        ps.Clear();
        ps.Play();
        StartCoroutine(Return(ps));
    }

    IEnumerator Return(ParticleSystem ps)
    {
        yield return new WaitForSeconds(0.55f);
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);
        pool.Enqueue(ps);
    }
}
