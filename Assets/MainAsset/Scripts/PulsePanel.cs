using UnityEngine;
using System.Collections;
public class PulsePanel : MonoBehaviour
{
    
    [SerializeField] private float minAlpha = 0.2f;
    [SerializeField] private float maxAlpha = 0.8f;
    [Min(0.01f)] [SerializeField] private float fadeDuration  = 1f;
    
    [SerializeField] private CanvasGroup canvasGroup;
    private Coroutine pulseCoroutine;

    
    private void Awake()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = minAlpha;
    }
    
    void OnEnable()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = minAlpha;
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }
        pulseCoroutine = StartCoroutine(Pulse());
    }

    void OnDisable()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
    }

    IEnumerator Fade(float minAlpha, float maxAlpha)
    {
        float duration = Mathf.Max(0.01f, fadeDuration);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, elapsedTime / duration);
            yield return null;
        }
        
        canvasGroup.alpha = maxAlpha;
    }

    IEnumerator Pulse()
    {
        while (true)
        {
            yield return Fade(minAlpha, maxAlpha);
            yield return Fade(maxAlpha, minAlpha);
        }
    }
    
}