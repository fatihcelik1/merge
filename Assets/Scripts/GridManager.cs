using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public GameObject itemPrefab;
    public GameObject emptySlotPrefab;
    public Transform gridParent;

    public int rows = 6;
    public int cols = 4;
    public float cellSize = 190f;
    public float spacing = 12f;
    public float itemScale = 0.92f;

    private GameObject[,] grid;
    private GameObject[,] slots;
    private Vector2 gridStartPos;

    void Awake()
    {
        Instance = this;
        grid = new GameObject[rows, cols];
        slots = new GameObject[rows, cols];
    }

    void Start()
    {
        CalculateStartPos();
        CreateEmptySlots();
        StartCoroutine(SpawnInitialItems());
    }

    void CalculateStartPos()
    {
        float totalWidth = cols * cellSize + (cols - 1) * spacing;
        float totalHeight = rows * cellSize + (rows - 1) * spacing;

        gridStartPos = new Vector2(
            -totalWidth / 2 + cellSize / 2,
            totalHeight / 2 - cellSize / 2
        );
    }

    void CreateEmptySlots()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                GameObject slot = Instantiate(emptySlotPrefab, gridParent);
                RectTransform rt = slot.GetComponent<RectTransform>();
                rt.anchoredPosition = GetSlotPosition(r, c);
                rt.sizeDelta = new Vector2(cellSize, cellSize);
                slots[r, c] = slot;
            }
        }
    }

    IEnumerator SpawnInitialItems()
    {
        int totalSlots = rows * cols;
        int fillCount = totalSlots;

        List<int[]> allSlots = new List<int[]>();
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                allSlots.Add(new int[] { r, c });

        for (int i = 0; i < allSlots.Count; i++)
        {
            int rand = Random.Range(i, allSlots.Count);
            var temp = allSlots[i];
            allSlots[i] = allSlots[rand];
            allSlots[rand] = temp;
        }

        for (int i = 0; i < fillCount; i++)
        {
            int r = allSlots[i][0];
            int c = allSlots[i][1];
            SpawnItemAt(r, c);
            yield return new WaitForSeconds(0.08f);
        }
    }

    void SpawnItemAt(int r, int c)
    {
        GameObject newItem = Instantiate(itemPrefab, gridParent);
        RectTransform itemRt = newItem.GetComponent<RectTransform>();
        Vector2 targetPos = GetSlotPosition(r, c);
        itemRt.anchoredPosition = new Vector2(targetPos.x, targetPos.y + 800f);
        itemRt.sizeDelta = new Vector2(cellSize, cellSize) * itemScale;

        int randomLevel = Random.Range(1, 9);
        newItem.GetComponent<ItemData>().level = randomLevel;
        newItem.GetComponent<ItemVisual>().UpdateVisual(randomLevel);
        grid[r, c] = newItem;

        StartCoroutine(DropAnimation(itemRt, targetPos));
    }

    IEnumerator DropAnimation(RectTransform rt, Vector2 targetPos)
    {
        float duration = 0.35f;
        float elapsed = 0f;
        Vector2 startPos = rt.anchoredPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = 1f - Mathf.Pow(1f - t, 3f);
            rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        rt.anchoredPosition = targetPos;
    }

    Vector2 GetSlotPosition(int r, int c)
    {
        float x = gridStartPos.x + c * (cellSize + spacing);
        float y = gridStartPos.y - r * (cellSize + spacing);
        return new Vector2(x, y);
    }

    public void SpawnItem()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (grid[r, c] == null)
                {
                    SpawnItemAt(r, c);
                    return;
                }
            }
        }
    }

    public bool AreNeighbors(int r1, int c1, int r2, int c2)
    {
        int dr = Mathf.Abs(r1 - r2);
        int dc = Mathf.Abs(c1 - c2);
        return (dr == 1 && dc == 0) || (dr == 0 && dc == 1);
    }

    public void MergeItems(int r1, int c1, int r2, int c2)
    {
        if (grid[r1, c1] == null || grid[r2, c2] == null) return;
        if (!AreNeighbors(r1, c1, r2, c2)) return;

        int level1 = grid[r1, c1].GetComponent<ItemData>().level;
        int level2 = grid[r2, c2].GetComponent<ItemData>().level;

        if (level1 != level2) return;

        RectTransform fromRt = grid[r1, c1].GetComponent<RectTransform>();
        RectTransform toRt = grid[r2, c2].GetComponent<RectTransform>();

        int newLevel = Mathf.Min(level1 + 1, 10);
        GameObject toObj = grid[r2, c2];
        GameObject fromObj = grid[r1, c1];
        int fromC = c1;

        grid[r1, c1] = null;

        MergeAnimator.Instance.PlayMerge(fromRt, toRt, () =>
        {
            toObj.GetComponent<ItemData>().level = newLevel;
            toObj.GetComponent<ItemVisual>().UpdateVisual(newLevel);
            Destroy(fromObj);
            if (LevelManager.Instance != null)
            LevelManager.Instance.OnMergeHappened(level1);
            else
            GameManager.Instance.AddMoney(level1 * 10);
            ApplyGravity(fromC);
        });
    }

    public void ApplyGravity(int col)
    {
        for (int r = rows - 1; r >= 0; r--)
        {
            if (grid[r, col] == null)
            {
                for (int aboveR = r - 1; aboveR >= 0; aboveR--)
                {
                    if (grid[aboveR, col] != null)
                    {
                        grid[r, col] = grid[aboveR, col];
                        grid[aboveR, col] = null;
                        RectTransform rt = grid[r, col].GetComponent<RectTransform>();
                        StartCoroutine(DropAnimation(rt, GetSlotPosition(r, col)));
                        break;
                    }
                }
            }
        }

        for (int r = 0; r < rows; r++)
        {
            if (grid[r, col] == null)
            {
                SpawnItemAt(r, col);
            }
        }
    }

    public int[] FindItem(GameObject item)
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (grid[r, c] == item)
                    return new int[] { r, c };
            }
        }
        return null;
    }

    public void Shuffle()
    {
        List<GameObject> allItems = new List<GameObject>();
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (grid[r, c] != null) allItems.Add(grid[r, c]);

        for (int i = 0; i < allItems.Count; i++)
        {
            int rand = Random.Range(i, allItems.Count);
            var temp = allItems[i];
            allItems[i] = allItems[rand];
            allItems[rand] = temp;
        }

        int idx = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (idx < allItems.Count)
                {
                    grid[r, c] = allItems[idx];
                    RectTransform rt = grid[r, c].GetComponent<RectTransform>();
                    StartCoroutine(DropAnimation(rt, GetSlotPosition(r, c)));
                    idx++;
                }
                else
                {
                    grid[r, c] = null;
                }
            }
        }
    }

    public void ResetGrid()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (grid[r, c] != null)
                {
                    Destroy(grid[r, c]);
                    grid[r, c] = null;
                }
            }
        }
    }
}