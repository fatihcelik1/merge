using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Level secim panelini doldurur ve tiklamalari yonetir.
public class LevelSelectManager : MonoBehaviour
{
    public RectTransform content;       // ScrollRect Content
    public GameObject panel;            // LevelSelectPanel (bu objeyi kapatmak icin)
    public Sprite buttonSprite;         // hucre arka plani (sliced)
    public int totalLevels = 50;

    public Color unlockedColor = new Color(1f,    0.78f, 0.20f, 1f);
    public Color lockedColor   = new Color(0.30f, 0.36f, 0.46f, 0.85f);
    public Color unlockedText  = new Color(0.30f, 0.18f, 0f,    1f);
    public Color lockedText    = new Color(0.65f, 0.72f, 0.85f, 1f);

    void OnEnable()
    {
        if (content == null) return;
        BuildButtons();
    }

    void BuildButtons()
    {
        // eski cocuklari temizle
        for (int i = content.childCount - 1; i >= 0; i--)
            DestroyImmediate(content.GetChild(i).gameObject);

        int highest = PlayerPrefs.GetInt("highestUnlocked", 1);

        for (int i = 1; i <= totalLevels; i++)
        {
            int levelNum = i;
            bool unlocked = (levelNum <= highest);

            var go = new GameObject("Lvl" + levelNum, typeof(RectTransform));
            go.layer = 5;
            go.transform.SetParent(content, false);

            var img = go.AddComponent<Image>();
            img.sprite = buttonSprite;
            if (buttonSprite != null) img.type = Image.Type.Sliced;
            img.color = unlocked ? unlockedColor : lockedColor;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.interactable = unlocked;

            var txtGo = new GameObject("Number", typeof(RectTransform));
            txtGo.layer = 5;
            txtGo.transform.SetParent(go.transform, false);
            var trt = (RectTransform)txtGo.transform;
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
            var txt = txtGo.AddComponent<TextMeshProUGUI>();
            txt.text = levelNum.ToString();
            txt.fontSize = 48f;
            txt.fontStyle = FontStyles.Bold;
            txt.color = unlocked ? unlockedText : lockedText;
            txt.alignment = TextAlignmentOptions.Center;
            txt.raycastTarget = false;

            if (unlocked)
                btn.onClick.AddListener(() => LoadLevel(levelNum));
        }
    }

    void LoadLevel(int lvl)
    {
        PlayerPrefs.SetInt("currentLevel", lvl);
        PlayerPrefs.Save();
        SceneManager.LoadScene("SampleScene");
    }

    // Back butonu icin
    public void Close()
    {
        if (panel != null) panel.SetActive(false);
    }
}
