using UnityEngine;
using System.Collections;
public class PulsePanel : MonoBehaviour
{
    
    [SerializeField] private float minAlpha = 0.2f;
    [SerializeField] private float maxAlpha = 0.8f;
    [SerializeField] private float fadeDuration  = 1f;
    
    [SerializeField] private CanvasGroup canvasGroup;
    private Coroutine pulseCoroutine;

    
    private void Awake()
    {
        canvasGroup.alpha = minAlpha;
    }
    
    void OnEnable()
    {
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
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, elapsedTime / fadeDuration);
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
