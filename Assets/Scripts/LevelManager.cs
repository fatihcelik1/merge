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
        // Kademeli artan hedef: Level 1 = 150, Level 50 = 100000
        return Mathf.RoundToInt(100 * Mathf.Pow(1.15f, level - 1)) + 50;
    }

    public void OnMergeHappened(int mergeLevel)
    {
        movesLeft--;
        GameManager.Instance.AddMoney(mergeLevel * mergeLevel * 10);
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
        if (movesText != null) movesText.text = "Hamle: " + movesLeft;
        if (targetText != null)
        {
            int current = Mathf.FloorToInt(GameManager.Instance.money) - levelStartMoney;
            targetText.text = current + " / " + targetMoney;
        }
    }

    void WinLevel()
    {
        PlayerPrefs.SetInt("currentLevel", currentLevel + 1);
        PlayerPrefs.Save();
        if (winPanel != null) winPanel.SetActive(true);
    }

    void LoseLevel()
    {
        if (losePanel != null) losePanel.SetActive(true);
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}