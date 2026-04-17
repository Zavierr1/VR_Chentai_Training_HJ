using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using UnityEngine.SceneManagement; 

[System.Serializable]
public class SlideInfo
{
    [TextArea] public string teksLegenda;
    public GameObject uiTitik; 
    public List<KelapKelipTutorial> mesinHighlight; 
    
    [Tooltip("Tarik objek SnapZone yang punya script TutorialDynamicHint ke sini")]
    public List<TutorialDynamicHint> ghostHighlight; 
}

public class MonitorCameraController : MonoBehaviour
{
    [Header("Komponen Utama")]
    public Transform cctvCamera;
    public TextMeshProUGUI textInstruksi;

    [Header("Referensi Fisik (SnapGroupManager)")]
    public SnapGroupManager managerPartA;
    public SnapGroupManager managerPartB;
    public SnapGroupManager managerPartC;
    public bool debugBypassPerakitan = false;

    [Header("Voice Over")]
    [Tooltip("Masukkan AudioSource untuk VO Perakitan")]
    public AudioSource voMulaiTutorial;
    public AudioSource voMulaiPemasangan;

    [Header("Referensi Tombol UI Utama")]
    public Button tombolStartTutorial; 
    public Button tombolPanduanPojokKanan; // Tombol '?'
    public Button tombolPartA;
    public Button tombolPartB;
    public Button tombolPartC;
    public Button tombolBack;         
    public Button tombolNext;
    public Button tombolReset; // Tombol untuk lanjut ke scene berikutnya setelah tutorial selesai

    // >>> TAMBAHAN BARU: Tombol Ekstra Panduan <<<
    [Header("Tombol Ekstra Panduan")]
    [Tooltip("Tombol 'Klik untuk mulai pemasangan' (Muncul di ujung slide)")]
    public Button tombolMulaiPemasangan;
    [Tooltip("Tombol Back/Close saat membuka ulang panduan (Review)")]
    public Button tombolClosePanduan;

    [Header("Scene Transition")]
    [Tooltip("Tarik Canvas/Panel ucapan selamat yang muncul di akhir ke sini")]
    public GameObject panelSelesaiTutorial; 
    [Tooltip("Tarik Teks (TextMeshPro) untuk ucapan selamat ke sini")]
    public TextMeshProUGUI teksSelesaiTutorial;
    public Button tombolFinish; 
    public string namaSceneBerikutnya = "NamaSceneKamuDisini";

    [Header("Data Slideshow Buku Panduan (Kanan)")]
    public GameObject panelLegenda;
    public TextMeshProUGUI textLegenda;
    public Button tombolNextSlide; 
    public Button tombolPrevSlide; 

    public List<SlideInfo> slideAreaAtas;
    public List<SlideInfo> slideInfoPartC;
    public List<SlideInfo> slideInfoPartB;
    public List<SlideInfo> slideAreaBawah; 
    public List<SlideInfo> slideInfoPartA;

    public bool tampilkanSemuaTitik = false;

    private List<SlideInfo> slideAktif = new List<SlideInfo>();
    private int indeksSlideSekarang = 0;
    private int areaTutorialAktif = 0; 

    [Header("Target Posisi (Waypoints)")]
    public Transform targetDefault;
    public Transform targetInfoAtas;
    public Transform targetPartC;
    public Transform targetPartB;
    public Transform targetInfoBawah;
    public Transform targetPartA;

    public float transisiDurasi = 1.0f;
    private Coroutine moveCoroutine;
    private int tahapPerakitan = 1; 

    private bool tutorialSelesai = false;
    [HideInInspector] public bool isSedangKalibrasiVR = false;

    void Start()
    {
        if (targetDefault != null && cctvCamera != null)
        {
            cctvCamera.position = targetDefault.position;
            cctvCamera.rotation = targetDefault.rotation;
        }

        if (panelSelesaiTutorial != null)
        {
            panelSelesaiTutorial.SetActive(false);
        }
        
        MatikanSemuaSlideshow();

        // >>> TAMBAHAN BARU: Bersihkan layar TV secara default saat game mulai <<<
        KunciSemuaTombol();
        UpdateTeksUI("");
    }

    public void KunciSistemUtama(bool isKunci)
    {
        isSedangKalibrasiVR = isKunci;
        if (isKunci)
        {
            KunciSemuaTombol();
        }
        else KePosisiDefault(); 
    }

    private void RefreshTampilanTombolDefault()
    {
        KunciSemuaTombol(); 
        if (isSedangKalibrasiVR) return;

        // >>> TAMBAHAN: Nyalakan tombol reset kapanpun tampilan TV kembali ke posisi default <<<
        if (tombolReset != null) tombolReset.gameObject.SetActive(true);

        if (!tutorialSelesai)
        {
            if (tombolStartTutorial != null) { tombolStartTutorial.gameObject.SetActive(true); tombolStartTutorial.interactable = true; }
        }
        else
        {
            if (tahapPerakitan <= 3)
            {
                if (tombolPartA != null) { tombolPartA.gameObject.SetActive(true); tombolPartA.interactable = (tahapPerakitan == 1); }
                if (tombolPartB != null) { tombolPartB.gameObject.SetActive(true); tombolPartB.interactable = (tahapPerakitan == 2); }
                if (tombolPartC != null) { tombolPartC.gameObject.SetActive(true); tombolPartC.interactable = (tahapPerakitan == 3); }
            }
            
            if (tombolPanduanPojokKanan != null) { tombolPanduanPojokKanan.gameObject.SetActive(true); tombolPanduanPojokKanan.interactable = true; }
        }
    }

    private void KunciSemuaTombol()
    {
        if (tombolStartTutorial != null) tombolStartTutorial.gameObject.SetActive(false);
        if (tombolPartA != null) tombolPartA.gameObject.SetActive(false);
        if (tombolPartB != null) tombolPartB.gameObject.SetActive(false);
        if (tombolPartC != null) tombolPartC.gameObject.SetActive(false);
        if (tombolNext != null) tombolNext.gameObject.SetActive(false); 
        if (tombolBack != null) tombolBack.gameObject.SetActive(false); 
        if (tombolPanduanPojokKanan != null) tombolPanduanPojokKanan.gameObject.SetActive(false);
        if (tombolFinish != null) tombolFinish.gameObject.SetActive(false);
        if (tombolReset != null) tombolReset.gameObject.SetActive(false);
    }

    public void MulaiTutorial()
    {
        KunciSemuaTombol();
        if (panelLegenda != null) panelLegenda.SetActive(true);
        if (tombolNextSlide != null) tombolNextSlide.gameObject.SetActive(true);
        if (tombolPrevSlide != null) tombolPrevSlide.gameObject.SetActive(true);

            // >>> TAMBAHAN: Memutar VO saat tombol start ditekan <<<
        if (voMulaiTutorial != null) 
        {
            voMulaiTutorial.Play();
        }
        
        areaTutorialAktif = 0; 
        indeksSlideSekarang = 0;
        slideAktif = slideAreaAtas;

        MulaiPindahKamera(targetInfoAtas);
        UpdateTeksUI("PENGENALAN BAGIAN MESIN:\nMari kenali bagian mesin sebelum merakit. Gunakan tombol [<] [>] untuk detail.");
        TampilkanSlideSekarang();
    }

    // >>> FUNGSI BARU: Dipanggil oleh Tombol "?" (Review Panduan) <<<
    public void BukaPanduanUlang()
    {
        KunciSemuaTombol();
        if (panelLegenda != null) panelLegenda.SetActive(true);
        if (tombolNextSlide != null) tombolNextSlide.gameObject.SetActive(true);
        if (tombolPrevSlide != null) tombolPrevSlide.gameObject.SetActive(true);
        
        areaTutorialAktif = 0; 
        indeksSlideSekarang = 0;
        slideAktif = slideAreaAtas;

        MulaiPindahKamera(targetInfoAtas);
        UpdateTeksUI("ULASAN PANDUAN MESIN:\nTekan tombol panah [<] [>] untuk membaca ulang informasi mesin.");
        TampilkanSlideSekarang();
    }

    // >>> FUNGSI BARU: Dipanggil oleh tombol "Klik untuk mulai pemasangan" <<<
    public void MulaiPemasanganSistem()
    {
        tutorialSelesai = true; // Kunci agar tombol start tidak muncul lagi
        MatikanSemuaSlideshow();

        // >>> TAMBAHAN VO: Mainkan suara saat masuk mode perakitan <<<
        if (voMulaiPemasangan != null)
        {
            voMulaiPemasangan.Play();
        }

        KePosisiDefault();
    }

    // >>> FUNGSI BARU: Dipanggil oleh tombol "Close/Back" saat review <<<
    public void TutupPanduanReview()
    {
        MatikanSemuaSlideshow();
        KePosisiDefault();
    }

    public void NextSlide()
    {
        if (indeksSlideSekarang < slideAktif.Count - 1)
        {
            indeksSlideSekarang++;
            TampilkanSlideSekarang();
        }
        else
        {
            // >>> SEMUA UPDATE TEKS DI SINI DIHAPUS BIAR GAK TIMPA-TIMPAAN <<<
            if (areaTutorialAktif == 0) 
            {
                areaTutorialAktif = 1; slideAktif = slideInfoPartC; indeksSlideSekarang = 0;
                MulaiPindahKamera(targetPartC); 
                TampilkanSlideSekarang();
            }
            else if (areaTutorialAktif == 1) 
            {
                areaTutorialAktif = 2; slideAktif = slideInfoPartB; indeksSlideSekarang = 0;
                MulaiPindahKamera(targetPartB); 
                TampilkanSlideSekarang();
            }
            else if (areaTutorialAktif == 2) 
            {
                areaTutorialAktif = 3; slideAktif = slideAreaBawah; indeksSlideSekarang = 0;
                MulaiPindahKamera(targetInfoBawah); 
                TampilkanSlideSekarang();
            }
            else if (areaTutorialAktif == 3) 
            {
                areaTutorialAktif = 4; slideAktif = slideInfoPartA; indeksSlideSekarang = 0;
                MulaiPindahKamera(targetPartA); 
                TampilkanSlideSekarang();
            }
        }
    }

    public void PrevSlide()
    {
        if (indeksSlideSekarang > 0)
        {
            indeksSlideSekarang--;
            TampilkanSlideSekarang();
        }
        else
        {
            // >>> SEMUA UPDATE TEKS DI SINI JUGA DIHAPUS <<<
            if (areaTutorialAktif == 4) 
            {
                areaTutorialAktif = 3; slideAktif = slideAreaBawah; indeksSlideSekarang = slideAktif.Count - 1; 
                MulaiPindahKamera(targetInfoBawah); 
                TampilkanSlideSekarang();
            }
            else if (areaTutorialAktif == 3) 
            {
                areaTutorialAktif = 2; slideAktif = slideInfoPartB; indeksSlideSekarang = slideAktif.Count - 1; 
                MulaiPindahKamera(targetPartB); 
                TampilkanSlideSekarang();
            }
            else if (areaTutorialAktif == 2) 
            {
                areaTutorialAktif = 1; slideAktif = slideInfoPartC; indeksSlideSekarang = slideAktif.Count - 1; 
                MulaiPindahKamera(targetPartC); 
                TampilkanSlideSekarang();
            }
            else if (areaTutorialAktif == 1) 
            {
                areaTutorialAktif = 0; slideAktif = slideAreaAtas; indeksSlideSekarang = slideAktif.Count - 1; 
                MulaiPindahKamera(targetInfoAtas); 
                TampilkanSlideSekarang();
            }
        }
    }

    private void TampilkanSlideSekarang()
    {
        MatikanSemuaLampuSlide();

        if (slideAktif.Count == 0) return;

        SlideInfo slideSekarang = slideAktif[indeksSlideSekarang];
        if (slideSekarang.uiTitik != null) slideSekarang.uiTitik.SetActive(true);
        
        if (slideSekarang.mesinHighlight != null)
        {
            foreach (var highlight in slideSekarang.mesinHighlight)
                if (highlight != null) highlight.MulaiKedip();
        }

        if (slideSekarang.ghostHighlight != null)
        {
            foreach (var ghost in slideSekarang.ghostHighlight)
                if (ghost != null) ghost.PaksaMunculInfo();
        }

        if (textLegenda != null) textLegenda.text = slideSekarang.teksLegenda;
        
        bool isLastSlideTotal = (areaTutorialAktif == 4 && indeksSlideSekarang == slideAktif.Count - 1);
        bool bisaMundur = !(areaTutorialAktif == 0 && indeksSlideSekarang == 0);
        bool bisaMaju = !isLastSlideTotal; // Tidak bisa next kalau udah di mentok kanan
        
        if (tombolPrevSlide != null) tombolPrevSlide.interactable = bisaMundur;
        if (tombolNextSlide != null) tombolNextSlide.interactable = bisaMaju;

        // >>> LOGIKA MUNCULNYA TOMBOL START vs CLOSE <<<
        if (tombolMulaiPemasangan != null) tombolMulaiPemasangan.gameObject.SetActive(isLastSlideTotal && !tutorialSelesai);
        
        // Tombol Close Panduan selalu muncul jika tutorialSelesai = true (artinya player cuma sedang baca ulang)
        if (tombolClosePanduan != null) tombolClosePanduan.gameObject.SetActive(tutorialSelesai);
    }

    private void MatikanSemuaSlideshow()
    {
        if (tombolNextSlide != null) tombolNextSlide.gameObject.SetActive(false);
        if (tombolPrevSlide != null) tombolPrevSlide.gameObject.SetActive(false);
        if (tombolMulaiPemasangan != null) tombolMulaiPemasangan.gameObject.SetActive(false);
        if (tombolClosePanduan != null) tombolClosePanduan.gameObject.SetActive(false);
        if (textLegenda != null) textLegenda.text = "";
        if (panelLegenda != null) panelLegenda.SetActive(false);

        MatikanSemuaLampuSlide();
    }

    private void MatikanSemuaLampuSlide()
    {
        MatikanSpesifikLampu(slideAreaAtas);
        MatikanSpesifikLampu(slideAreaBawah);
        MatikanSpesifikLampu(slideInfoPartA);
        MatikanSpesifikLampu(slideInfoPartB);
        MatikanSpesifikLampu(slideInfoPartC);
    }

    private void MatikanSpesifikLampu(List<SlideInfo> daftarSlide)
    {
        foreach (var slide in daftarSlide)
        {
            if (slide.uiTitik != null) slide.uiTitik.SetActive(false);

            if (slide.mesinHighlight != null)
            {
                foreach (var highlight in slide.mesinHighlight)
                    if (highlight != null) highlight.BerhentiKedip();
            }

            if (slide.ghostHighlight != null)
            {
                foreach (var ghost in slide.ghostHighlight)
                    if (ghost != null) ghost.HentikanInfo();
            }
        }
    }

    public void KePosisiDefault() 
    { 
        MulaiPindahKamera(targetDefault); 
        RefreshTampilanTombolDefault();
        MatikanSemuaSlideshow();

        if (isSedangKalibrasiVR) return;

        if (!tutorialSelesai) UpdateTeksUI("Tekan tombol [START] (Trigger) untuk mulai pengenalan mesin.");
        else 
        {
            if (tahapPerakitan == 1) UpdateTeksUI("MODE PERAKITAN.\nTekan tombol [Part A] untuk mulai memasang Cover yang ada di atas meja sebelah kanan.");
            else if (tahapPerakitan == 2) UpdateTeksUI("PROGRES: 33%.\nTekan tombol [Part B] untuk memasang bagian mesin yang ada di atas meja tengah.");
            else if (tahapPerakitan == 3) UpdateTeksUI("PROGRES: 66% (step terakhir).\nTekan tombol [Part C] untuk memasang bagian mesin yang ada di atas meja sebelah kiri.");
            else UpdateTeksUI("PROGRES: 100% (SELESAI!).");
        }
    }
    
    public void KePartA() 
    { 
        MulaiPindahKamera(targetPartA); 
        KunciSemuaTombol(); 
        if (tombolBack != null) 
        { 
            tombolBack.gameObject.SetActive(true); 
            tombolBack.interactable = true; 
        } 
        MatikanSemuaSlideshow(); 
        if (managerPartA != null) 
        { 
            managerPartA.AktifkanGrup(); 
            managerPartA.UpdateHighlightBerurutan(); 
        } 
    }

    public void KePartB() 
    { 
        MulaiPindahKamera(targetPartB); 
        KunciSemuaTombol(); 
        if (tombolBack != null) 
        { 
            tombolBack.gameObject.SetActive(true); 
            tombolBack.interactable = true; 
        } 
        MatikanSemuaSlideshow(); 
        if (managerPartB != null) 
        { 
            managerPartB.AktifkanGrup(); 
            managerPartB.UpdateHighlightBerurutan(); 
        } 
    }

    public void KePartC() 
    { 
        MulaiPindahKamera(targetPartC); 
        KunciSemuaTombol(); 
        if (tombolBack != null) 
        { 
            tombolBack.gameObject.SetActive(true); 
            tombolBack.interactable = true; 
        } 
        MatikanSemuaSlideshow(); 
        if (managerPartC != null) 
        { 
            managerPartC.AktifkanGrup(); 
            managerPartC.UpdateHighlightBerurutan(); 
        } 
    }

    public void SelesaikanSemuaPerakitan()
    {
        tahapPerakitan = 4; // Tandai bahwa semua tahap sudah beres
        
        // 1. Bersihkan semua tombol dan UI gambar
        KunciSemuaTombol(); 
        MatikanSemuaSlideshow(); 
        
        // 2. Kembalikan kamera ke posisi awal menghadap mesin utuh
        MulaiPindahKamera(targetDefault); 
        
        // 3. Tampilkan teks intsruksi terakhir di panel Control
        UpdateTeksUI("PERAKITAN SELESAI!\nKerja bagus, seluruh komponen mesin telah berhasil dipasang dengan sempurna.");
        
        // >>> SAFEGUARD: Paksa semua Snap Group untuk nyala kembali! <<<
        if (managerPartA != null) managerPartA.AktifkanGrup();
        if (managerPartB != null) managerPartB.AktifkanGrup();
        if (managerPartC != null) managerPartC.AktifkanGrup();
        // Mainkan suara sukses (opsional jika kamu punya file suaranya)
        // if (suaraStepSukses != null) suaraStepSukses.Play(); 
    }   
    
    // >>> FUNGSI BARU: Hook fungsi ini ke Tombol "CLOSE" yang ada di panel merah (Control Panel)
    public void TutupControlPanelDanSelesai()
    {
        // >>> PERHATIKAN: Sekarang kita tambahkan "|| debugBypassPerakitan" <<<
        if (tahapPerakitan > 3 || debugBypassPerakitan)
        {
            // Skenario 1: Player meng-close panel setelah semua tahapan perakitan selesai (atau Debug aktif)
            KunciSemuaTombol();
            MatikanSemuaSlideshow();
            
            // Munculkan Canvas "SELAMAT"
            if (panelSelesaiTutorial != null)
            {
                panelSelesaiTutorial.SetActive(true);
            }
            
            // Isi teks ucapan selamat
            if (teksSelesaiTutorial != null)
            {
                teksSelesaiTutorial.text = "<color=yellow>SELAMAT!</color>\nAnda telah menyelesaikan mode Tutorial.\nSilakan tekan tombol Finish untuk melanjutkan ke mode Assessment.";
            }

            // Nyalakan tombol Finish (Bisa diletakkan di dalam Canvas tersebut)
            if (tombolFinish != null)
            {
                tombolFinish.gameObject.SetActive(true);
                tombolFinish.interactable = true;
            }
            
            UpdateTeksUI(""); // Kosongkan layar tengah
            MulaiPindahKamera(targetDefault); // Kembali lurus menatap mesin
        }
        else
        {
            // Skenario 2: Player meng-close panel padahal perakitan belum selesai
            // Maka bersikaplah normal kembali ke tampilan pemilihan menu Part A/B/C
            KePosisiDefault();
        }
    }
    
    public void PartSelesai() 
    { 
        KunciSemuaTombol(); 
        if (tombolNext != null) 
        { 
            tombolNext.gameObject.SetActive(true); 
            tombolNext.interactable = true; 
        } 
        UpdateTeksUI("KERJA BAGUS!.\nTekan tombol [NEXT] untuk lanjut."); 
    }

    public void LanjutKeTahapBerikutnya() 
    { 
        tahapPerakitan++; 
        KePosisiDefault(); 
    }

    public void UpdateTeksUI(string pesan) 
    { 
        if (textInstruksi != null) 
            textInstruksi.text = pesan; 
    }

    public void PindahKeSceneBerikutnya() 
    { 
        if (!string.IsNullOrEmpty(namaSceneBerikutnya)) 
            SceneManager.LoadScene(namaSceneBerikutnya); 
    }

    private void MulaiPindahKamera(Transform targetTujuan)
    {
        if (cctvCamera == null || targetTujuan == null) return;
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(ProsesPindahKamera(targetTujuan));
    }

    private IEnumerator ProsesPindahKamera(Transform target)
    {
        Vector3 posisiAwal = cctvCamera.position;
        Quaternion rotasiAwal = cctvCamera.rotation;
        float timer = 0f;
        while (timer < transisiDurasi)
        {
            timer += Time.deltaTime;
            float persen = Mathf.SmoothStep(0f, 1f, timer / transisiDurasi);
            cctvCamera.position = Vector3.Lerp(posisiAwal, target.position, persen);
            cctvCamera.rotation = Quaternion.Slerp(rotasiAwal, target.rotation, persen);
            yield return null;
        }
        cctvCamera.position = target.position;
        cctvCamera.rotation = target.rotation;
    }
}