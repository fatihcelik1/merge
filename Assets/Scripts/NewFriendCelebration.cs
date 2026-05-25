using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Bir hayvan turu ilk kez ortaya cikinca tetiklenen kutlama efekti.
// Iris karartma: ekranin disindan hayvanin etrafina dogru kararma toplanir,
// hayvan oldugu gibi kalir, kapanista tersine merkezden disa dagilir.
// Hayvanin ustunde minimal beyaz "NEW FRIEND" yazisi.
// Etrafinda sparkle (parildaylan kucuk noktalar) partikulleri.
public class NewFriendCelebration : MonoBehaviour
{
    public static NewFriendCelebration Instance;

    static readonly string[] AnimalNames = {
        "MOUSE", "RABBIT", "CAT", "MONKEY", "DOG",
        "PANDA", "LION", "GIRAFFE", "HIPPO", "ELEPHANT"
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("NewFriendCelebration");
        go.AddComponent<NewFriendCelebration>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Show(int level, Transform target)
    {
        if (target == null) return;
        StartCoroutine(ShowCoroutine(level, target));
    }

    IEnumerator ShowCoroutine(int level, Transform target)
    {
        var canvas = target.GetComponentInParent<Canvas>();
        if (canvas == null) yield break;

        var canvasRt = (RectTransform)canvas.transform;
        Vector2 canvasSize = canvasRt.rect.size;

        // Hayvanin canvas local pozisyonu ve boyutu
        Vector3 targetWorld = target.position;
        Vector2 targetLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRt,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, targetWorld),
            canvas.worldCamera,
            out targetLocal);

        var targetRt = target as RectTransform;
        float holeW = (targetRt != null ? targetRt.rect.width * target.lossyScale.x / canvasRt.lossyScale.x : 170f) + 40f;
        float holeH = (targetRt != null ? targetRt.rect.height * target.lossyScale.y / canvasRt.lossyScale.y : 170f) + 40f;

        // 4 karartma paneli - hayvanin etrafini bos birakir, geri kalani karanlik
        Color darkC = new Color(0, 0, 0, 0.96f);

        float halfW = canvasSize.x * 0.5f;
        float halfH = canvasSize.y * 0.5f;

        // TOP
        var top = MakePanel("Top", canvas.transform, darkC);
        var topRt = (RectTransform)top.transform;
        topRt.anchorMin = topRt.anchorMax = new Vector2(0.5f, 0.5f);
        topRt.pivot = new Vector2(0.5f, 0f);
        topRt.sizeDelta = new Vector2(canvasSize.x, halfH - (targetLocal.y + holeH * 0.5f));
        Vector2 topFinalPos = new Vector2(0, targetLocal.y + holeH * 0.5f);
        Vector2 topStartPos = topFinalPos + new Vector2(0, canvasSize.y); // off-screen yukari

        // BOT
        var bot = MakePanel("Bot", canvas.transform, darkC);
        var botRt = (RectTransform)bot.transform;
        botRt.anchorMin = botRt.anchorMax = new Vector2(0.5f, 0.5f);
        botRt.pivot = new Vector2(0.5f, 1f);
        botRt.sizeDelta = new Vector2(canvasSize.x, halfH + targetLocal.y - holeH * 0.5f);
        Vector2 botFinalPos = new Vector2(0, targetLocal.y - holeH * 0.5f);
        Vector2 botStartPos = botFinalPos - new Vector2(0, canvasSize.y);

        // LEFT
        var left = MakePanel("Left", canvas.transform, darkC);
        var leftRt = (RectTransform)left.transform;
        leftRt.anchorMin = leftRt.anchorMax = new Vector2(0.5f, 0.5f);
        leftRt.pivot = new Vector2(1f, 0.5f);
        leftRt.sizeDelta = new Vector2(halfW + targetLocal.x - holeW * 0.5f, holeH);
        Vector2 leftFinalPos = new Vector2(targetLocal.x - holeW * 0.5f, targetLocal.y);
        Vector2 leftStartPos = leftFinalPos - new Vector2(canvasSize.x, 0);

        // RIGHT
        var right = MakePanel("Right", canvas.transform, darkC);
        var rightRt = (RectTransform)right.transform;
        rightRt.anchorMin = rightRt.anchorMax = new Vector2(0.5f, 0.5f);
        rightRt.pivot = new Vector2(0f, 0.5f);
        rightRt.sizeDelta = new Vector2(halfW - (targetLocal.x + holeW * 0.5f), holeH);
        Vector2 rightFinalPos = new Vector2(targetLocal.x + holeW * 0.5f, targetLocal.y);
        Vector2 rightStartPos = rightFinalPos + new Vector2(canvasSize.x, 0);

        // Baslangic: tum paneller off-screen
        topRt.anchoredPosition = topStartPos;
        botRt.anchoredPosition = botStartPos;
        leftRt.anchoredPosition = leftStartPos;
        rightRt.anchoredPosition = rightStartPos;

        // Hayvani en uste cikar (kararmanin ustunde gorunsun)
        Transform origParent = target.parent;
        int origSibling = target.GetSiblingIndex();
        target.SetParent(canvas.transform, true);
        target.SetAsLastSibling();

        // Minimal beyaz "NEW FRIEND" yazisi - hayvanin hemen ustunde
        var title = NewUI("NewFriendLabel", canvas.transform);
        var tRt = (RectTransform)title.transform;
        tRt.anchorMin = tRt.anchorMax = new Vector2(0.5f, 0.5f);
        tRt.pivot = new Vector2(0.5f, 0.5f);
        tRt.anchoredPosition = new Vector2(targetLocal.x, targetLocal.y + holeH * 0.5f + 50f);
        tRt.sizeDelta = new Vector2(420, 60);
        var tTxt = title.AddComponent<TextMeshProUGUI>();
        tTxt.text = "NEW FRIEND";
        tTxt.fontSize = 38;
        tTxt.fontStyle = FontStyles.Normal;
        tTxt.alignment = TextAlignmentOptions.Center;
        tTxt.color = new Color(1f, 1f, 1f, 0f);
        tTxt.characterSpacing = 8f;
        tTxt.raycastTarget = false;

        // Hayvan ismi - title ile ayni style, altta
        var name = NewUI("AnimalLabel", canvas.transform);
        var nRt = (RectTransform)name.transform;
        nRt.anchorMin = nRt.anchorMax = new Vector2(0.5f, 0.5f);
        nRt.pivot = new Vector2(0.5f, 0.5f);
        nRt.anchoredPosition = new Vector2(targetLocal.x, targetLocal.y - holeH * 0.5f - 50f);
        nRt.sizeDelta = new Vector2(420, 60);
        var nTxt = name.AddComponent<TextMeshProUGUI>();
        nTxt.text = AnimalNames[Mathf.Clamp(level - 1, 0, AnimalNames.Length - 1)];
        nTxt.fontSize = 38;
        nTxt.fontStyle = FontStyles.Normal;
        nTxt.alignment = TextAlignmentOptions.Center;
        nTxt.color = new Color(1f, 1f, 1f, 0f);
        nTxt.characterSpacing = 8f;
        nTxt.raycastTarget = false;

        // Iris in (0.5 sn) - paneller iceri kayar
        float t = 0f;
        float dur = 0.5f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            float ease = 1f - Mathf.Pow(1f - k, 3f); // ease out cubic
            topRt.anchoredPosition = Vector2.Lerp(topStartPos, topFinalPos, ease);
            botRt.anchoredPosition = Vector2.Lerp(botStartPos, botFinalPos, ease);
            leftRt.anchoredPosition = Vector2.Lerp(leftStartPos, leftFinalPos, ease);
            rightRt.anchoredPosition = Vector2.Lerp(rightStartPos, rightFinalPos, ease);
            tTxt.color = new Color(1f, 1f, 1f, k);
            nTxt.color = new Color(1f, 1f, 1f, k);
            yield return null;
        }

        // Sparkle partikullerini baslat
        var sparkleCo = StartCoroutine(SpawnSparkles(canvas.transform, target, 2.6f));

        // Bekle (3 sn ana gosterim)
        yield return new WaitForSeconds(3f);

        // Iris out (0.5 sn) - paneller disari dagilir
        t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            float ease = k * k; // ease in
            topRt.anchoredPosition = Vector2.Lerp(topFinalPos, topStartPos, ease);
            botRt.anchoredPosition = Vector2.Lerp(botFinalPos, botStartPos, ease);
            leftRt.anchoredPosition = Vector2.Lerp(leftFinalPos, leftStartPos, ease);
            rightRt.anchoredPosition = Vector2.Lerp(rightFinalPos, rightStartPos, ease);
            tTxt.color = new Color(1f, 1f, 1f, 1f - k);
            nTxt.color = new Color(1f, 1f, 1f, 1f - k);
            yield return null;
        }

        // Hayvani eski yerine geri koy
        if (target != null && origParent != null)
        {
            target.SetParent(origParent, true);
            target.SetSiblingIndex(origSibling);
        }
        Destroy(top);
        Destroy(bot);
        Destroy(left);
        Destroy(right);
        Destroy(title);
        Destroy(name);
    }

    // Hayvanin etrafinda surekli belirip kaybolan kucuk parlak baklava sekiller.
    IEnumerator SpawnSparkles(Transform canvasT, Transform target, float duration)
    {
        float t = 0f;
        while (t < duration && target != null)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(75f, 170f);
            Vector3 pos = target.position + new Vector3(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist, 0);
            SpawnSparkle(canvasT, pos);
            float delay = Random.Range(0.05f, 0.12f);
            yield return new WaitForSeconds(delay);
            t += delay;
        }
    }

    void SpawnSparkle(Transform canvasT, Vector3 worldPos)
    {
        var go = NewUI("Sparkle", canvasT);
        var rt = (RectTransform)go.transform;
        rt.position = worldPos;
        rt.sizeDelta = new Vector2(20, 20);
        rt.localRotation = Quaternion.Euler(0, 0, 45f);
        var img = go.AddComponent<Image>();
        img.color = Random.value < 0.5f
            ? new Color(1f, 1f, 1f, 0f)
            : new Color(1f, 0.92f, 0.5f, 0f);
        img.raycastTarget = false;
        StartCoroutine(SparkleAnim(rt, img));
    }

    IEnumerator SparkleAnim(RectTransform rt, Image img)
    {
        Color c = img.color;
        float dur = Random.Range(0.55f, 0.9f);
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = t / dur;
            float a = Mathf.Sin(k * Mathf.PI);
            img.color = new Color(c.r, c.g, c.b, a);
            float s = a * 1.1f + 0.1f;
            rt.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        if (rt != null) Destroy(rt.gameObject);
    }

    static GameObject MakePanel(string n, Transform p, Color c)
    {
        var g = NewUI(n, p);
        var img = g.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = true;
        return g;
    }

    static GameObject NewUI(string n, Transform p)
    {
        var g = new GameObject(n, typeof(RectTransform));
        g.layer = 5;
        g.transform.SetParent(p, false);
        return g;
    }
}
