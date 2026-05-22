# Merge Idle - Notlar

## ÇÖZÜLEMEYEN / BEKLEYEN

### Izgara - Çerçeve oturması (cosmetic)
- **Sorun:** Hayvan ızgarası, jungle çerçevenin iç boşluğunu temiz doldurmuyor —
  alt ve üstte boşluk kalıyor.
- **Neden çözülemedi:** Çerçevenin iç şeffaf deliğinin gerçek piksel ölçüsü,
  oyun render'ı görülmeden kör tahminle doğru bulunamadı. Birden çok deneme
  tutmadı.
- **Gerçek çözüm yolu:** Unity Editör'de Game görünümüne bakarak ELLE ayar:
  1. Hierarchy > Canvas > GameFrame seç → Inspector'da Rect Transform
     Width/Height ile çerçeve boyutu.
  2. Canvas > Game Area > Grid seç → Rect Transform Width/Height (koyu panel).
  3. Grid üzerindeki GridManager bileşeni → Cell Size değeri (ızgara hücre
     boyutu). Cell Size değişikliği sadece Play'e basınca etkili olur.
  4. Play → bak → düzelt → tekrar Play, oturana kadar.
- **Mevcut durum:** Oyun oynanabilir; bu yalnızca görsel bir cilalama maddesi.

## YAPILDI (commit bekliyor)
- Yeni 10 hayvan (boyut sırası), şeffaf arka plan
- Üst bar Moves/Target rozetleri (ikonlu)
- Shuffle butonu: hak yokken soluk
- Settings müzik/ses düzeltmesi (DontDestroyOnLoad kaldırıldı)
- Ana menüye dön butonu (prew ikonu)
- Win/Lose panelleri
- Izgara arka planı: yeşil yerine koyu panel; çerçeve şeffaf sürüm, ön planda
