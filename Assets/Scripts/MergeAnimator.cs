using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MergeAnimator : MonoBehaviour
{
    static MergeAnimator _instance;

    // Sahne yeniden yuklendiginde eski referans bayatlarsa
    // sahnedeki canli MergeAnimator'i bulup kendini onarir.
    public static MergeAnimator Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<MergeAnimator>();
            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;
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
        SpawnBurst(to);

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

    // ---- merge patlama efekti (yildiz partikulleri) ----
    static Sprite _burst;
    static Sprite BurstSprite
    {
        get
        {
            if (_burst == null) _burst = Resources.Load<Sprite>("star_particle");
            return _burst;
        }
    }

    void SpawnBurst(RectTransform center)
    {
        Sprite sp = BurstSprite;
        if (sp == null || center == null || center.parent == null) return;

        Transform parent = center.parent;
        Vector2 origin = center.anchoredPosition;
        int count = 9;

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("MergeStar", typeof(RectTransform));
            go.layer = 5; // UI
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = origin;
            rt.sizeDelta = new Vector2(48f, 48f);

            var img = go.AddComponent<Image>();
            img.sprite = sp;
            img.raycastTarget = false;

            float ang = (360f / count) * i + Random.Range(-18f, 18f);
            Vector2 dir = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad));
            StartCoroutine(BurstStar(rt, img, dir));
        }
    }

    IEnumerator BurstStar(RectTransform rt, Image img, Vector2 dir)
    {
        Vector2 start = rt.anchoredPosition;
        float dist = Random.Range(80f, 150f);
        float dur = Random.Range(0.45f, 0.7f);
        float spin = Random.Range(-300f, 300f);
        float t = 0f;
        Color c = img.color;

        while (t < dur)
        {
            t += Time.deltaTime;
            float k = t / dur;
            float ease = 1f - Mathf.Pow(1f - k, 3f);
            rt.anchoredPosition = start + dir * (dist * ease);
            float s = Mathf.Lerp(1.1f, 0.15f, k);
            rt.localScale = new Vector3(s, s, 1f);
            rt.localRotation = Quaternion.Euler(0f, 0f, spin * k);
            c.a = 1f - k * k;
            img.color = c;
            yield return null;
        }
        Destroy(rt.gameObject);
    }
}