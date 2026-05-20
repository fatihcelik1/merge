using UnityEngine;
using UnityEngine.UI;

public class GridFullPopup : MonoBehaviour
{
    public static GridFullPopup Instance;
    public GameObject popupPanel;

    void Awake()
    {
        Instance = this;
        popupPanel.SetActive(false);
    }

    public void ShowPopup()
    {
        popupPanel.SetActive(true);
    }

    public void HidePopup()
    {
        popupPanel.SetActive(false);
    }

    public void OnWatchAdClicked()
    {
        HidePopup();
        GridManager.Instance.Shuffle();
    }
}