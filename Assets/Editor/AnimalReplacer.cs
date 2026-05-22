using UnityEngine;
using UnityEditor;

// Item prefab'indaki ItemVisual.animalSprites dizisini
// Assets/Sprites/animals/ icindeki 10 yeni hayvanla degistirir.
// Unity menusu:  Tools > Hayvanlari Degistir
public static class AnimalReplacer
{
    // boyut mantigiyla level sirasi (1 -> 10)
    static readonly string[] Names = {
        "01_mouse", "02_rabbit", "03_cat", "04_monkey", "05_dog",
        "06_panda", "07_lion", "08_giraffe", "09_hippo", "10_elephant"
    };

    [MenuItem("Tools/Hayvanlari Degistir")]
    public static void Replace()
    {
        var sprites = new Sprite[Names.Length];
        for (int i = 0; i < Names.Length; i++)
        {
            string path = "Assets/Sprites/animals/" + Names[i] + ".png";
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprites[i] == null)
            {
                EditorUtility.DisplayDialog("Hata",
                    "Sprite bulunamadi: " + path + "\nUnity'nin PNG'leri import etmesini bekleyip tekrar dene.", "Tamam");
                return;
            }
        }

        var itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Item.prefab");
        if (itemPrefab == null)
        {
            EditorUtility.DisplayDialog("Hata", "Assets/Prefabs/Item.prefab bulunamadi.", "Tamam");
            return;
        }

        var visual = itemPrefab.GetComponent<ItemVisual>();
        if (visual == null)
        {
            EditorUtility.DisplayDialog("Hata", "Item prefab'inda ItemVisual yok.", "Tamam");
            return;
        }

        visual.animalSprites = sprites;
        EditorUtility.SetDirty(itemPrefab);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Tamam",
            "10 yeni hayvan Item prefab'ina atandi (level 1->10 boyut sirasi):\n" +
            "fare, tavsan, kedi, maymun, kopek, panda, aslan, zurafa, su aygiri, fil.", "Tamam");
        Debug.Log("[AnimalReplacer] animalSprites guncellendi.");
    }
}
