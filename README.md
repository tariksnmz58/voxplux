<p align="center">
  <img src="https://capsule-render.vercel.app/api?type=soft&color=0:3B82F6,100:06B6D4&height=250&section=header&text=Voxplux&fontSize=80&fontAlignY=40&desc=Ultimate%20Audio%20Routing%20System&descAlignY=65&descAlign=50&fontColor=FFFFFF&animation=twinkling" width="100%" />
</p>

<h3 align="center">
  <b>Sıradan Ses Deneyimini Kırın. Aynı Anda Her Yerde Olun.</b>
</h3>

<p align="center">
  <a href="https://git.io/typing-svg">
    <img src="https://readme-typing-svg.demolab.com?font=Fira+Code&weight=500&size=18&pause=1000&color=06B6D4&center=true&vCenter=true&width=600&lines=Sistem+Sesini+4+Farkl%C4%B1+Cihaza+B%C3%B6l;Kusursuz+Gecikme+(~20ms);Dinamik+DSP+ve+3-Bant+EQ;Her+Cihaza+%C3%96zel+Ses+Kontrol%C3%BC;Windows+Mica+Dark+Theme" alt="Typing SVG" />
  </a>
</p>

<p align="center">
  <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET_9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET"></a>
  <a href="https://learn.microsoft.com/wpf/"><img src="https://img.shields.io/badge/WPF_UI-0078D7?style=for-the-badge&logo=windows&logoColor=white" alt="WPF"></a>
  <a href="https://github.com/naudio/NAudio"><img src="https://img.shields.io/badge/NAudio_2.3-FF6F00?style=for-the-badge&logo=audacity&logoColor=white" alt="NAudio"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-10B981?style=for-the-badge&logo=open-source-initiative&logoColor=white" alt="License"></a>
</p>

---

## 🌌 Neden Voxplux?

Windows, yapısı gereği sesi yalnızca **tek bir çıkış cihazına** vermeye programlanmıştır. Kulaklığınız takılıysa hoparlör susar; hoparlör açıksa kulaklık iptal olur. 

**Voxplux (Vox: Ses + Plux: Çoğaltma)** bu duvarı yıkar.  
Oyun oynarken kulaklığınızdan kusursuz ses alırken, arkanızdaki ses sisteminden müziği odaya verebilirsiniz. Üstelik her bir cihazın **ses seviyesini, basını ve ekolaizer ayarlarını tamamen birbirinden bağımsız** kontrol ederek.

---

## 🚀 Sınırları Aşan Özellikler

| 🎛️ **Bölünmüş Kontrol** | ⚡ **Kusursuz Akış** | 🎚️ **Pro DSP Motoru** |
| :--- | :--- | :--- |
| **Aynı anda 4 çıkış:** Bluetooth, Jak, USB veya HDMI ayrımı yapmaksızın sesi klonlar. | **~20ms Gecikme:** WASAPI Loopback teknolojisi ile sıfır gecikme hissi. | **3-Bant EQ & Bass:** Her cihaz için Low/Mid/High ve Bass Boost kontrolü. |
| **Bağımsız Volüm:** Kulaklıkta %100, hoparlörde %20 ses seviyesi. | **Senkronizasyon:** Farklı cihazlar arası gecikmeyi milisaniyelik ayarla eşitleyin. | **Smooth Ramping:** Sesi aniden kesmez, "Click/Pop" patlamalarını engeller. |

---

## 🧠 Gelişmiş Mimari

Voxplux, arka planda karmaşık bir dijital sinyal işleme (DSP) ağı kurarak çalışır. Sistem sesi yakalandıktan sonra her bir aktif cihaz için özel bir tünel (Pipeline) oluşturulur.

```mermaid
graph TD
    classDef sys fill:#0B0E14,stroke:#3B82F6,stroke-width:2px,color:#fff
    classDef dsp fill:#162040,stroke:#06B6D4,stroke-width:2px,color:#fff
    classDef out fill:#10B981,stroke:#047857,stroke-width:2px,color:#fff

    A(🔊 Windows Sistem Sesi) -->|WASAPI Loopback Capture| B{Voxplux Multiplexer}
    class A,B sys

    B --> C[DSP Pipeline 1]
    B --> D[DSP Pipeline 2]
    B --> E[DSP Pipeline 3]
    class C,D,E dsp

    C -->|Volume + EQ| F((🎧 Bluetooth Kulaklık))
    D -->|Bass Boost| G((🔊 Masaüstü Hoparlör))
    E -->|Delay Sync| H((📺 HDMI TV))
    class F,G,H out
```

---

## 💻 Kurulum ve Kullanım

<details>
<summary><b>🔥 Hızlı Başlangıç (Tek Tıkla Çalıştır)</b></summary>
<br>

En kolay yöntem! Herhangi bir yazılım veya `.NET` kurmanıza gerek yoktur.
1. **[Releases](../../releases)** sayfasından en güncel `Voxplux.App.exe` dosyasını indirin.
2. Dosyaya çift tıklayın ve kullanmaya başlayın!

</details>

<details>
<summary><b>🛠️ Geliştiriciler İçin (Kaynak Koddan)</b></summary>
<br>

Kendi bilgisayarınızda derlemek veya projeye katkıda bulunmak isterseniz:

```powershell
# Repoyu bilgisayarınıza klonlayın
git clone https://github.com/tariksnmz58/voxplux.git
cd voxplux

# Projeyi çalıştırın
dotnet run --project src/Voxplux.App

# Tek bir EXE dosyası olarak derlemek (Publish) için:
dotnet publish src/Voxplux.App -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```
</details>

---

## 🎨 Arayüz (Mica Dark Theme)

Uygulama, Windows 11'in **Mica** materyalini kullanarak arka plandaki duvar kağıdınızın renklerini hafifçe emer. **Electric Blue** ve **Cyan** detaylarla bezenmiş arayüz, göz yormayan, premium bir "Quiet Luxury" tasarım dili sunar.

- **Otomatik Başlatma:** Sol menüden ilk cihazı aktifleştirdiğiniz an ses yakalama motoru otomatik devreye girer.
- **Canlı Geri Bildirim:** Alt kısımdaki durum çubuğu, kaç cihazın aktif olduğunu ve sistemin o anki durumunu bildirir.

---

<div align="center">

**Geliştirici:** [@tariksnmz58](https://github.com/tariksnmz58) &nbsp; | &nbsp; **Lisans:** [MIT](LICENSE)

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:06B6D4,100:3B82F6&height=120&section=footer" width="100%" />

</div>
