using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject settingsPanel;

    public void PlayGame()
    {
        // Bir sonraki yenilmemis level'a yonlen
        int unlocked = PlayerPrefs.GetInt("highestUnlocked", 1);
        PlayerPrefs.SetInt("currentLevel", unlocked);
        PlayerPrefs.Save();
        SceneManager.LoadScene("SampleScene");
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}