using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeUI : MonoBehaviour
{
    private static FadeUI Instance;

    private Renderer targetRenderer;
    private Material material;

    public static void StartFade(Color from, Color to, float duration, System.Action callback)
    {
        if (Instance)
        {
            Instance.StartFadeInternal(from, to, duration, callback);
        }
        else
        {
            Debug.LogError("Missing FadeUI!");
            callback?.Invoke();
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer)
                material = targetRenderer.material;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void StartFadeInternal(Color from, Color to, float duration, System.Action callback)
    {
        StopAllCoroutines();
        StartCoroutine(StartFade_Coroutine(from, to, duration, callback));
    }

    IEnumerator StartFade_Coroutine(Color from, Color to, float duration, System.Action callback)
    {
        if (material == null)
        {
            yield break;
        }
        if (duration == 0)
        {
            material.color = to;
            callback?.Invoke();
            yield break;
        }

        float timer = duration;
        while (timer > 0)
        {
            material.color = Color.Lerp(to, from, timer / duration);
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                material.color = to;
            }
            yield return null;
        }
        callback?.Invoke();
    }
}
