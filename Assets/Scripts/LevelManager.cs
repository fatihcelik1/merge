using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public int currentLevel = 1;
    public int movesLeft;
    public int levelStartMoney;
    public int targetMoney;

    public TextMeshProUGUI levelText;
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI targetText;

    public GameObject winPanel;
    public GameObject losePanel;

    public TextMeshProUGUI winRewardText;
    public TextMeshProUGUI loseProgressText;

    void Awake()
    {
        Instance = this;
        currentLevel = PlayerPrefs.GetInt("currentLevel", 1);
    }

    void Start()
    {
        StartLevel(currentLevel);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    public void StartLevel(int level)
    {
        currentLevel = level;
        movesLeft = GetMovesForLevel(level);
        targetMoney = GetTargetForLevel(level);
        levelStartMoney = Mathf.FloorToInt(GameManager.Instance.money);
        UpdateUI();
    }

    int GetMovesForLevel(int level)
    {
        int moves = 25 - ((level - 1) / 5);
        return Mathf.Max(moves, 10);
    }

    int GetTargetForLevel(int level)
    {
        // Progressively increasing target: Level 1 = 150, Level 50 = 100000
        return Mathf.RoundToInt(100 * Mathf.Pow(1.15f, level - 1)) + 50;
    }

    // Zorluk egrisi: kac farkli hayvan turu (level) cikabilsin?
    // Dusuk seviye -> az cesit -> daha cok birlestirme = daha kasitli oyun.
    public int GetSpawnMaxLevel(int level)
    {
        if (level <= 8)  return 3;   // tutorial / kolay
        if (level <= 15) return 4;
        if (level <= 22) return 5;
        if (level <= 30) return 6;
        if (level <= 40) return 7;
        return 8;                    // L 41-50 zor
    }

    public void OnMergeHappened(int mergeLevel, RectTransform at)
    {
        movesLeft--;
        int reward = mergeLevel * mergeLevel * 10;
        GameManager.Instance.AddMoneyDeferred(reward);
        ShowGain(reward, at);
        UpdateUI();

        int currentLevelMoney = Mathf.FloorToInt(GameManager.Instance.money) - levelStartMoney;
        if (currentLevelMoney >= targetMoney)
        {
            WinLevel();
        }
        else if (movesLeft <= 0)
        {
            LoseLevel();
        }
    }

    void UpdateUI()
    {
        if (levelText != null) levelText.text = currentLevel.ToString();
        if (movesText != null) movesText.text = movesLeft.ToString();
        if (targetText != null)
        {
            int current = Mathf.FloorToInt(GameManager.Instance.money) - levelStartMoney;
            targetText.text = current + "/" + targetMoney;
        }
    }

    void WinLevel()
    {
        // currentLevel'i otomatik artirma - sadece bir ust level'i kilidini ac.
        // Tekrar oynama / sonraki level'e gecme buton bazli olur.
        int unlocked = PlayerPrefs.GetInt("highestUnlocked", 1);
        int newUnlocked = Mathf.Max(unlocked, currentLevel + 1);
        PlayerPrefs.SetInt("highestUnlocked", newUnlocked);
        PlayerPrefs.Save();
        if (SettingsManager.Instance != null) SettingsManager.Instance.Vibrate();
        int collected = Mathf.FloorToInt(GameManager.Instance.money) - levelStartMoney;
        if (winRewardText != null) winRewardText.text = "+" + collected;
        if (winPanel != null) winPanel.SetActive(true);
    }

    void LoseLevel()
    {
        int collected = Mathf.FloorToInt(GameManager.Instance.money) - levelStartMoney;
        if (loseProgressText != null) loseProgressText.text = collected + " / " + targetMoney;
        if (losePanel != null) losePanel.SetActive(true);
    }

    public void NextLevel()
    {
        PlayerPrefs.SetInt("currentLevel", currentLevel + 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    // ---- "+X" para kazanc yazisi (birlesmede ucar) ----
    void ShowGain(int amount, RectTransform at)
    {
        if (at == null || at.parent == null) return;

        var go = new GameObject("GainText", typeof(RectTransform));
        go.layer = 5; // UI
        go.transform.SetParent(at.parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = at.anchoredPosition;
        rt.sizeDelta = new Vector2(220f, 90f);

        var txt = go.AddComponent<TextMeshProUGUI>();
        txt.text = "+" + amount;
        txt.fontSize = 56f;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = new Color(1f, 0.84f, 0.22f, 1f);
        txt.raycastTarget = false;

        StartCoroutine(FloatUp(rt, txt));
        FlyCoins(at, amount);
    }

    IEnumerator FloatUp(RectTransform rt, TextMeshProUGUI txt)
    {
        Vector2 start = rt.anchoredPosition;
        float dur = 0.9f;
        float t = 0f;
        Color c = txt.color;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = t / dur;
            rt.anchoredPosition = start + new Vector2(0f, 130f * k);
            float s = Mathf.Lerp(0.5f, 1.15f, Mathf.Min(k * 3.5f, 1f));
            rt.localScale = new Vector3(s, s, 1f);
            c.a = k < 0.55f ? 1f : 1f - (k - 0.55f) / 0.45f;
            txt.color = c;
            yield return null;
        }
        Destroy(rt.gameObject);
    }

    // ---- sayaca ucan coin'ler ----
    static Sprite _coinFly;
    static Sprite CoinSprite
    {
        get
        {
            if (_coinFly == null) _coinFly = Resources.Load<Sprite>("coin_fly");
            return _coinFly;
        }
    }

    void FlyCoins(RectTransform at, int reward)
    {
        Sprite sp = CoinSprite;
        if (sp == null || at == null) return;

        Canvas canvas = at.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        GameObject targetGo = GameObject.Find("CoinIcon");
        if (targetGo == null) targetGo = GameObject.Find("CoinPanel");
        if (targetGo == null) return;

        Vector3 startW = at.position;
        Vector3 endW = targetGo.transform.position;
        const int count = 6;
        float perCoin = reward / (float)count;

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("FlyCoin", typeof(RectTransform));
            go.layer = 5; // UI
            go.transform.SetParent(canvas.transform, false);
            go.transform.SetAsLastSibling();
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(64f, 64f);
            rt.position = startW;

            var img = go.AddComponent<Image>();
            img.sprite = sp;
            img.raycastTarget = false;

            StartCoroutine(FlyCoin(rt, startW, endW, i, perCoin));
        }
    }

    IEnumerator FlyCoin(RectTransform rt, Vector3 startW, Vector3 endW, int idx, float perCoin)
    {
        yield return new WaitForSeconds(idx * 0.05f);

        // 1. kisa dagilma
        Vector3 scatter = startW + (Vector3)(Random.insideUnitCircle * 95f);
        float t = 0f, d1 = 0.2f;
        while (t < d1)
        {
            t += Time.deltaTime;
            rt.position = Vector3.Lerp(startW, scatter, t / d1);
            yield return null;
        }

        // 2. sayaca ucus
        t = 0f;
        float d2 = 0.5f;
        while (t < d2)
        {
            t += Time.deltaTime;
            float k = t / d2;
            rt.position = Vector3.Lerp(scatter, endW, k * k);
            float s = Mathf.Lerp(1f, 0.45f, k);
            rt.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        // coin sayaca vardi -> sayac artsin
        if (GameManager.Instance != null)
            GameManager.Instance.AddDisplayMoney(perCoin);

        Destroy(rt.gameObject);
    }
}