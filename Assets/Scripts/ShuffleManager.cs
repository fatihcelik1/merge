using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShuffleManager : MonoBehaviour
{
    public static ShuffleManager Instance;

    public int shuffleCount = 0;
    public TextMeshProUGUI shuffleCountText;
    public GameObject adPopup;

    [Header("Shuffle butonu gorunumu")]
    public Image shuffleButtonImage;                 // bos birakilirsa otomatik bulunur
    public Color fadedColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    void Awake()
    {
        Instance = this;
        shuffleCount = PlayerPrefs.GetInt("shuffleCount", 0);
        if (shuffleButtonImage == null)
        {
            GameObject sb = GameObject.Find("ShuffleButton");
            if (sb != null) shuffleButtonImage = sb.GetComponent<Image>();
        }
        UpdateUI();
    }

    void Start()
    {
        if (adPopup != null) adPopup.SetActive(false);
    }

    public void OnShuffleButtonClicked()
    {
        if (shuffleCount > 0)
        {
            shuffleCount--;
            PlayerPrefs.SetInt("shuffleCount", shuffleCount);
            UpdateUI();
            GridManager.Instance.Shuffle();
        }
        else
        {
            if (adPopup != null) adPopup.SetActive(true);
        }
    }

    public void OnWatchAdClicked()
    {
        shuffleCount++;
        PlayerPrefs.SetInt("shuffleCount", shuffleCount);
        UpdateUI();
        if (adPopup != null) adPopup.SetActive(false);
    }

    public void ClosePopup()
    {
        if (adPopup != null) adPopup.SetActive(false);
    }

    void UpdateUI()
    {
        if (shuffleCountText != null)
            shuffleCountText.text = "x" + shuffleCount;

        // hak yoksa buton soluk, varsa normal
        if (shuffleButtonImage != null)
            shuffleButtonImage.color = shuffleCount > 0 ? Color.white : fadedColor;
    }
}