using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ItemClick : MonoBehaviour, IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private static GameObject firstSelected = null;
    private Vector2 dragStartPos;

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

    // ---- Kaydirma (swipe) -> komsuyla merge ----
    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStartPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (GridManager.Instance == null) return;
        Vector2 delta = eventData.position - dragStartPos;
        float threshold = Screen.height * 0.025f; // ~%2.5 ekran yuksekligi
        if (delta.magnitude < threshold) return;

        int[] pos = GridManager.Instance.FindItem(gameObject);
        if (pos == null) return;
        int r = pos[0], c = pos[1];
        int nr = r, nc = c;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            nc += (delta.x > 0f) ? 1 : -1;
        else
            nr += (delta.y > 0f) ? -1 : 1; // ekranda yukari = satir-1

        if (nr < 0 || nr >= GridManager.Instance.rows ||
            nc < 0 || nc >= GridManager.Instance.cols) return;

        // mevcut tap secimini temizle ki karismasin
        if (firstSelected != null)
        {
            var img = firstSelected.GetComponent<Image>();
            if (img != null) img.color = Color.white;
            firstSelected = null;
        }

        // komsuyu al, level karsilastir
        GameObject other = GridManager.Instance.GetItemAt(nr, nc);
        if (other == null) return;

        ItemData myData = GetComponent<ItemData>();
        ItemData otherData = other.GetComponent<ItemData>();
        if (myData == null || otherData == null) return;

        if (myData.level == otherData.level)
        {
            GridManager.Instance.MergeItems(r, c, nr, nc);
        }
        else
        {
            // tiklamayla ayni "yanlis merge" efekti
            StartCoroutine(WrongMergeEffect(gameObject, other));
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