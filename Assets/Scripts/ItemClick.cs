using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ItemClick : MonoBehaviour, IPointerClickHandler
{
    private static GameObject firstSelected = null;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (firstSelected == null)
        {
            firstSelected = gameObject;
            GetComponent<Image>().color = Color.yellow;
        }
        else if (firstSelected == gameObject)
        {
            GetComponent<Image>().color = Color.white;
            firstSelected = null;
        }
        else
        {
            ItemData data1 = firstSelected.GetComponent<ItemData>();
            ItemData data2 = gameObject.GetComponent<ItemData>();

            if (data1.level == data2.level)
            {
                int[] pos1 = GridManager.Instance.FindItem(firstSelected);
                int[] pos2 = GridManager.Instance.FindItem(gameObject);

                if (pos1 != null && pos2 != null)
                {
                    firstSelected.GetComponent<Image>().color = Color.white;
                    GridManager.Instance.MergeItems(pos1[0], pos1[1], pos2[0], pos2[1]);
                }
            }
            else
            {
                StartCoroutine(WrongMergeEffect(firstSelected, gameObject));
                firstSelected = null;
            }
        }
    }

    IEnumerator WrongMergeEffect(GameObject obj1, GameObject obj2)
    {
        Image img1 = obj1.GetComponent<Image>();
        Image img2 = obj2.GetComponent<Image>();
        RectTransform rt1 = obj1.GetComponent<RectTransform>();
        RectTransform rt2 = obj2.GetComponent<RectTransform>();

        Color red = new Color(0.75f, 0.22f, 0.17f, 1f);
        img1.color = red;
        img2.color = red;

        float duration = 0.35f;
        float elapsed = 0f;
        Vector2 pos1 = rt1.anchoredPosition;
        Vector2 pos2 = rt2.anchoredPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float shake = Mathf.Sin(t * Mathf.PI * 6f) * 7f * (1f - t);
            rt1.anchoredPosition = pos1 + new Vector2(shake, 0);
            rt2.anchoredPosition = pos2 + new Vector2(-shake, 0);
            yield return null;
        }

        rt1.anchoredPosition = pos1;
        rt2.anchoredPosition = pos2;
        img1.color = Color.white;
        img2.color = Color.white;
    }
}