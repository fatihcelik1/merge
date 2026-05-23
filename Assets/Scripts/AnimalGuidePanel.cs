using UnityEngine;

public class AnimalGuidePanel : MonoBehaviour
{
    public GameObject panel;
    public void Close() { if (panel != null) panel.SetActive(false); }
}
