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
    private bool _isInitialSpawn = false;

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
        _isInitialSpawn = true;

        int totalSlots = rows * cols;
        int maxLvl = (LevelManager.Instance != null)
            ? LevelManager.Instance.GetSpawnMaxLevel(LevelManager.Instance.currentLevel)
            : 8;

        // Slot listesi - random sirayla spawn icin
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

        // 1) LEVELLERI ÖNCEDEN PLANLA (görsel yok, sadece 2D array)
        int[,] plannedLevels = new int[rows, cols];
        // 0 = bos/atanmamis isareti olarak kullanma, var sayilan tum hucreler -1
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                plannedLevels[r, c] = -1;

        foreach (var slot in allSlots)
        {
            int r = slot[0], c = slot[1];
            int level = RollWeightedLevel(maxLvl);
            for (int t = 0; t < 5 && PlannedHasSameNeighbor(plannedLevels, r, c, level); t++)
                level = RollWeightedLevel(maxLvl);
            plannedLevels[r, c] = level;
        }

        // 2) DEADLOCK KONTROLU - planlama duzeyinde duzelt (henuz hicbir sey spawn edilmedi)
        if (!PlannedHasAnyMergeablePair(plannedLevels))
        {
            // En basit fix: iki komsuyu esit yap. Merkeze yakin bir konum sec
            int mr = rows / 2;
            int mc = cols / 2;
            if (mc + 1 < cols)
                plannedLevels[mr, mc + 1] = plannedLevels[mr, mc];
            else if (mc > 0)
                plannedLevels[mr, mc - 1] = plannedLevels[mr, mc];
            else if (mr + 1 < rows)
                plannedLevels[mr + 1, mc] = plannedLevels[mr, mc];
        }

        // 3) SPAWN ET (görsel başlasın, oyuncu artık tutarli grid görür)
        foreach (var slot in allSlots)
        {
            int r = slot[0], c = slot[1];
            SpawnItemAt(r, c, plannedLevels[r, c]);
            yield return new WaitForSeconds(0.02f);
        }

        _isInitialSpawn = false;
    }

    // Planlama 2D dizisi icin: hucrenin komsulari ayni level mi?
    bool PlannedHasSameNeighbor(int[,] planned, int r, int c, int level)
    {
        if (r > 0 && planned[r - 1, c] == level) return true;
        if (r < rows - 1 && planned[r + 1, c] == level) return true;
        if (c > 0 && planned[r, c - 1] == level) return true;
        if (c < cols - 1 && planned[r, c + 1] == level) return true;
        return false;
    }

    // Planlama 2D dizisinde en az bir komsu cift (merge yapilabilir) var mi?
    bool PlannedHasAnyMergeablePair(int[,] planned)
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int lv = planned[r, c];
                if (lv < 1) continue;
                if (c + 1 < cols && planned[r, c + 1] == lv) return true;
                if (r + 1 < rows && planned[r + 1, c] == lv) return true;
            }
        }
        return false;
    }

    void SpawnItemAt(int r, int c, int forcedLevel = -1)
    {
        GameObject newItem = Instantiate(itemPrefab, gridParent);
        RectTransform itemRt = newItem.GetComponent<RectTransform>();
        Vector2 targetPos = GetSlotPosition(r, c);
        itemRt.anchoredPosition = new Vector2(targetPos.x, targetPos.y + 800f);
        itemRt.sizeDelta = new Vector2(cellSize, cellSize) * itemScale;

        int randomLevel;
        if (forcedLevel > 0)
        {
            randomLevel = forcedLevel;
        }
        else
        {
            int maxLvl = (LevelManager.Instance != null)
                ? LevelManager.Instance.GetSpawnMaxLevel(LevelManager.Instance.currentLevel)
                : 8;
            randomLevel = RollWeightedLevel(maxLvl);
            for (int t = 0; t < 5 && HasSameNeighbor(r, c, randomLevel); t++)
                randomLevel = RollWeightedLevel(maxLvl);
        }

        newItem.GetComponent<ItemData>().level = randomLevel;
        newItem.GetComponent<ItemVisual>().UpdateVisual(randomLevel);
        grid[r, c] = newItem;

        StartCoroutine(DropAnimation(itemRt, targetPos));
        TryShowNewFriend(randomLevel, newItem.transform, 0.7f);
    }

    // Belli bir level ilk kez goruluyorsa "NEW FRIEND" kutlamasi tetikler.
    // Initial spawn'da (oyun ilk acilis) seen olarak isaretle ama gosterme.
    void TryShowNewFriend(int level, Transform target, float delay)
    {
        string key = "seenLevel_" + level;
        if (PlayerPrefs.GetInt(key, 0) != 0) return;
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        if (_isInitialSpawn) return;
        StartCoroutine(DelayedNewFriend(level, target, delay));
    }

    IEnumerator DelayedNewFriend(int level, Transform target, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (target != null && NewFriendCelebration.Instance != null)
            NewFriendCelebration.Instance.Show(level, target);
    }

    // Dusuk leveller cok daha sik gelsin (weight = (maxLvl-level+1)^2).
    // Orn. maxLvl=4 ise Lv1=%53, Lv2=%30, Lv3=%13, Lv4=%3.
    int RollWeightedLevel(int maxLvl)
    {
        int total = 0;
        for (int i = 1; i <= maxLvl; i++)
        {
            int w = (maxLvl - i + 1);
            total += w * w;
        }
        int roll = Random.Range(0, total);
        int acc = 0;
        for (int i = 1; i <= maxLvl; i++)
        {
            int w = (maxLvl - i + 1);
            acc += w * w;
            if (roll < acc) return i;
        }
        return 1;
    }

    // Grid'de ayni level olan komsu cifti bulur (tutorial icin).
    public bool TryGetNeighborPair(out Transform a, out Transform b)
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var go = grid[r, c];
                if (go == null) continue;
                int lv = go.GetComponent<ItemData>().level;
                // sag komsu
                if (c + 1 < cols && grid[r, c + 1] != null &&
                    grid[r, c + 1].GetComponent<ItemData>().level == lv)
                { a = go.transform; b = grid[r, c + 1].transform; return true; }
                // alt komsu
                if (r + 1 < rows && grid[r + 1, c] != null &&
                    grid[r + 1, c].GetComponent<ItemData>().level == lv)
                { a = go.transform; b = grid[r + 1, c].transform; return true; }
            }
        }
        a = b = null;
        return false;
    }

    // 4 komsudan (yukari/asagi/sol/sag) herhangi biri ayni level mi?
    bool HasSameNeighbor(int r, int c, int level)
    {
        return IsLevelAt(r - 1, c, level)
            || IsLevelAt(r + 1, c, level)
            || IsLevelAt(r, c - 1, level)
            || IsLevelAt(r, c + 1, level);
    }

    bool IsLevelAt(int r, int c, int level)
    {
        if (r < 0 || r >= rows || c < 0 || c >= cols) return false;
        var go = grid[r, c];
        if (go == null) return false;
        var data = go.GetComponent<ItemData>();
        return data != null && data.level == level;
    }

    IEnumerator DropAnimation(RectTransform rt, Vector2 targetPos)
    {
        float duration = 0.13f;
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
            TryShowNewFriend(newLevel, toObj.transform, 0.2f);
            if (TutorialManager.Instance != null) TutorialManager.Instance.NotifyMergeHappened();
            if (LevelManager.Instance != null)
            LevelManager.Instance.OnMergeHappened(level1, toRt);
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

    public GameObject GetItemAt(int r, int c)
    {
        if (r < 0 || r >= rows || c < 0 || c >= cols) return null;
        return grid[r, c];
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