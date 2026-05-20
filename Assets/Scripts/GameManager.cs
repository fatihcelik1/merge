using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public float money = 0f;
    public TextMeshProUGUI moneyText;

    void Awake()
    {
        Instance = this;
        LoadGame();
        CheckDailyReward();
    }

    void Start()
    {
        InvokeRepeating("SaveGame", 10f, 10f);
        UpdateMoneyUI();
    }

    public void AddMoney(float amount)
    {
        money += amount;
        UpdateMoneyUI();
    }

    void UpdateMoneyUI()
    {
        moneyText.text = Mathf.FloorToInt(money).ToString();
    }

    public void TrySpawn()
    {
        GridManager.Instance.SpawnItem();
    }

    public void SaveGame()
    {
        PlayerPrefs.SetFloat("money", money);
        PlayerPrefs.SetString("lastSaveTime", DateTime.Now.ToString());
        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        money = PlayerPrefs.GetFloat("money", 0f);
    }

    void CheckDailyReward()
    {
        string lastDailyStr = PlayerPrefs.GetString("lastDailyReward", "");
        int dailyStreak = PlayerPrefs.GetInt("dailyStreak", 0);

        if (string.IsNullOrEmpty(lastDailyStr))
        {
            GiveDailyReward(dailyStreak);
            return;
        }

        DateTime lastDaily = DateTime.Parse(lastDailyStr);
        TimeSpan diff = DateTime.Now - lastDaily;

        if (diff.TotalHours >= 24)
        {
            if (diff.TotalHours <= 48) dailyStreak++;
            else dailyStreak = 1;
            PlayerPrefs.SetInt("dailyStreak", dailyStreak);
            GiveDailyReward(dailyStreak);
        }
    }

    void GiveDailyReward(int streak)
    {
        float reward = 100f * streak;
        AddMoney(reward);
        PlayerPrefs.SetString("lastDailyReward", DateTime.Now.ToString());
        PlayerPrefs.Save();
    }

    void OnApplicationQuit() { SaveGame(); }
    void OnApplicationPause(bool pause) { if (pause) SaveGame(); }
}