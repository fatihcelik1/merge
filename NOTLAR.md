# Merge Idle - Notlar

## KALAN İŞLER
- [ ] Para animasyonu — "+10" uçarak coin paneline gitsin
- [ ] Logo tasarımı — ana menü logosu (şu an yer tutucu)
- [ ] Level seçim ekranı — pack'te `level_select` var
- [ ] AdMob gerçek reklam entegrasyonu (yayın öncesi)

### İptal edilenler (istenmedi)
- Pause butonu — oyunda gerek yok
- Hayvan üstünde seviye rakamı — istenmedi

## ÇÖZÜLEMEYEN / BEKLEYEN

### Izgara - Çerçeve oturması (cosmetic)
- **Sorun:** Hayvan ızgarası, jungle çerçevenin iç boşluğunu temiz doldurmuyor.
- **Çözüm yolu:** Unity Editör'de Game görünümüne bakarak ELLE ayar:
  GameFrame Rect Transform W/H, Grid Rect Transform W/H, GridManager Cell Size.
  Cell Size değişikliği Play'e basınca etkili olur.
- **Durum:** Oyun oynanabilir; yalnızca görsel cilalama.

## YAPILDI
- Win/Lose panelleri
- Yeni 10 hayvan (boyut sırası, şeffaf arka plan)
- Üst bar Moves/Target rozetleri (ikonlu)
- Shuffle butonu: hak yokken soluk
- Settings müzik/ses düzeltmesi
- Ana menüye dön butonu
- Izgara koyu panel + şeffaf çerçeve ön planda
- Merge partikül efekti (yıldız patlaması)
- İngilizce çeviri
- Son commit: 872b92e
