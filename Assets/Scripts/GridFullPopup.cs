using UnityEngine;
using UnityEngine.UI;

public class GridFullPopup : MonoBehaviour
{
    public static GridFullPopup Instance;
    public GameObject popupPanel;

    void Awake()
    {
        Instance = this;
        // NOT: popupPanel'i burada kapatmiyoruz. popupPanel bu objenin
        // kendisi oldugu icin SetActive(false) Awake'i ilk acilista
        // popup'i aninda kapatiyordu. Baslangic gizleme ShuffleManager.Start()
        // tarafindan yapiliyor (adPopup.SetActive(false)).
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