using UnityEngine;
using UnityEngine.UI;

public class ItemVisual : MonoBehaviour
{
    public Sprite[] animalSprites;

    public void UpdateVisual(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, animalSprites.Length - 1);
        GetComponent<Image>().sprite = animalSprites[index];
        GetComponent<Image>().color = Color.white;
    }
}