using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShuffleManager : MonoBehaviour
{
    public static ShuffleManager Instance;

    public int shuffleCount = 0;
    public TextMeshProUGUI shuffleCountText;
    public GameObject adPopup;

    void Awake()
    {
        Instance = this;
        shuffleCount = PlayerPrefs.GetInt("shuffleCount", 0);
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
    }
}