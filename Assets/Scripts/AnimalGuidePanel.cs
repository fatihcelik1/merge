using UnityEngine;

public class AnimalGuidePanel : MonoBehaviour
{
    public GameObject panel;
    public void Close() { if (panel != null) panel.SetActive(false); }

    void OnEnable()
    {
        // Panel acildiginda Canvas hiyerarsisinde en uste cikar
        // Arkadaki butonlarin (LEVELS vb.) tiklamasini engelle
        transform.SetAsLastSibling();
    }
}
