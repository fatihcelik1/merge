using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MergeAnimator : MonoBehaviour
{
    public static MergeAnimator Instance;

    void Awake()
    {
        Instance = this;
    }

    public void PlayMerge(RectTransform from, RectTransform to, System.Action onDone)
    {
        StartCoroutine(MergeSequence(from, to, onDone));
    }

    IEnumerator MergeSequence(RectTransform from, RectTransform to, System.Action onDone)
    {
        // 1. FLY
        Vector2 startPos = from.anchoredPosition;
        Vector2 targetPos = to.anchoredPosition;
        float flyDuration = 0.28f;
        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flyDuration;
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            float arc = Mathf.Sin(Mathf.PI * t) * 80f;
            from.anchoredPosition = Vector2.Lerp(startPos, targetPos, eased) + new Vector2(0, arc);
            float scale = Mathf.Lerp(1f, 0.3f, eased);
            from.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        from.gameObject.SetActive(false);

        // 2. SHAKE
        float shakeDuration = 0.2f;
        elapsed = 0f;
        Vector2 toStart = to.anchoredPosition;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shakeDuration;
            float shake = Mathf.Sin(t * Mathf.PI * 5f) * 6f * (1f - t);
            to.anchoredPosition = toStart + new Vector2(shake, 0);
            yield return null;
        }

        to.anchoredPosition = toStart;

        // 3. POP
        onDone?.Invoke();

        float popDuration = 0.4f;
        elapsed = 0f;

        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;
            float s = EaseOutBack(t);
            to.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        to.localScale = Vector3.one;
    }

    float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}