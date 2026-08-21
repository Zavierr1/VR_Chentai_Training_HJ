using UnityEngine;
using UnityEngine.UI;
using BNG;
using TMPro;
using System.Collections;
using System.Collections.Generic;

// Guides the player through a step-by-step controller tutorial: trigger, analog
// sticks, and grip. Each stage is unlocked by performing the required action.
public class InputTutorialManager : MonoBehaviour
{
    [Header("Referensi UI Utama")]
    public GameObject welcomePanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public Image gambarController; 
    public MonitorCameraController monitorMesinUtama;
    public GameObject tombolSelesaiUI; 

    [Header("Assets Gambar Kontroler")]
    public Sprite gambarFullController;
    public Sprite gambarTrigger;       
    public Sprite gambarAnalog;         
    public Sprite gambarGrip;

    [Header("Audio Feedback & VO")]
    public AudioSource suaraStepSukses;
    public AudioSource suaraTutorialSelesai;
    public AudioSource suaraVOSelamatDatang;

    [Header("Objek Praktek Langsung")]
    [Tooltip("Tombol UI untuk ditembak/diklik pake Trigger")]
    public GameObject tombolLatihanTrigger;
    
    [Tooltip("Barang latihan di meja untuk tes Grip")]
    public Grabbable barangLatihanGrip;

    [Header("Hint Otomatis Jika Pemain Terjebak (Idle)")]
    [Tooltip("Detik tanpa aksi sebelum hint pertama muncul")]
    public float waktuHintPertama = 10f;
    [Tooltip("Detik antara hint berikutnya")]
    public float waktuAntarHint = 8f;
    [Tooltip("Suara reminder hint (kosongkan = otomatis pakai suaraStepSukses)")]
    public AudioSource suaraHint;
    [Tooltip("Blink highlight untuk barang latihan Grip (kosongkan = cari otomatis dari barangLatihanGrip)")]
    public KelapKelipTutorial blinkBarangLatihan;

    [Header("Kamera Panduan (Cutscene ke Panel)")]
    [Tooltip("Titik pandang panel START. Bisa pakai empty GameObject biasa (hanya Transform-nya yang dipakai)")]
    public Transform titikPandangPanel;
    [Tooltip("Titik pandang MESIN ASLI di sebelah kiri. Stop pertama tur kamera (opsional)")]
    public Transform titikPandangMesin;
    [Tooltip("Kamera utama pemain (kosongkan = otomatis Camera.main)")]
    public Camera kameraUtamaPemain;
    [Tooltip("Durasi (detik) kamera meluncur antar titik pandang")]
    public float durasiPindahKamera = 1.5f;
    [Tooltip("Lama (detik) tampilan mesin dipertahankan sebelum lanjut ke panel")]
    public float durasiTahanMesin = 2f;
    [Tooltip("Lama (detik) tampilan panel dipertahankan sebelum kembali ke pemain")]
    public float durasiTahanPanduan = 3f;

    [Header("Petunjuk Grab (Tahap 3)")]
    [Tooltip("Delay sebelum panah muncul (detik)")]
    public float delayPanahGrab = 1.5f;
    [Tooltip("Jarak panah di atas objek (meter)")]
    public float tinggiPanah = 0.4f;
    [Tooltip("Skala panah")]
    public float skalaPanah = 0.15f;
    [Tooltip("Warna panah")]
    public Color warnaPanah = new Color(0f, 1f, 0.2f, 1f);

    [Header("Rig Pemain (AUTO)")]
    [Tooltip("AUTO: GameObject 'CameraRig' dari prefab XR Rig Advanced. Biarkan kosong")]
    public Transform rigPemain;
    [Tooltip("AUTO: GameObject 'CenterEyeAnchor'. Biarkan kosong")]
    public Transform titikMataPemain;

    [Header("Debug / Testing")]
    public UnityEngine.UI.Button tombolNextDebug;

    private int tahapTutorial = 0; 
    
    // Flags for analog detection (Stage 2).
    private bool kiriOk = false;
    private bool kananOk = false;

    // Data untuk sistem hint idle (pemain terjebak).
    private float waktuMulaiTahap = 0f;
    private int jumlahHintDitampilkan = 0;
    private Coroutine coroutinePulseUI;
    private string teksDasarTahap = "";
    private Image gambarTombolLatihanUI;

    // Panah petunjuk grab (tahap 3)
    private GameObject panahGrabObj;
    private Coroutine coroutinePanahGrab;

    // State kunci rig saat cutscene kamera.
    private bool rigSedangDikunci = false;
    private readonly List<KomponenKunci> daftarKomponenTerkunci = new List<KomponenKunci>();

    // Menyimpan komponen rig yang dinonaktifkan sementara beserta status sebelumnya.
    private class KomponenKunci
    {
        public MonoBehaviour komponen;
        public bool aktifSebelumnya;
    }

    // Initializes the tutorial: hides the welcome panel, locks the machine, and
    // starts the automatic opening sequence.
    void Start()
    {
        if (welcomePanel != null) welcomePanel.SetActive(false);
        if (tombolNextDebug != null) tombolNextDebug.gameObject.SetActive(true);
        if (tombolSelesaiUI != null) tombolSelesaiUI.SetActive(false); 

        // Auto-cari komponen blink pada barang latihan Grip supaya nggak perlu wiring manual.
        if (blinkBarangLatihan == null && barangLatihanGrip != null)
        {
            blinkBarangLatihan = barangLatihanGrip.GetComponentInChildren<KelapKelipTutorial>();
        }

        SembunyikanSemuaAlatPraktek();

        if (monitorMesinUtama != null) monitorMesinUtama.KunciSistemUtama(true);
        tahapTutorial = 0; 
        StartCoroutine(SequencePembukaanOtomatis());
    }

    // Hides all hands-on practice objects until their stage is reached.
    private void SembunyikanSemuaAlatPraktek()
    {
        if (tombolLatihanTrigger != null) tombolLatihanTrigger.SetActive(false);
        if (barangLatihanGrip != null) barangLatihanGrip.gameObject.SetActive(false);
    }

    // Plays the welcome voice-over and shows the welcome panel before starting stage 1.
    private IEnumerator SequencePembukaanOtomatis()
    {
        yield return new WaitForSeconds(2f);

        if (welcomePanel != null) welcomePanel.SetActive(true);
        titleText.text = "SELAMAT DATANG DI VR";
        descText.text = "Selamat datang di Modul Pelatihan VR Mesin Stripping. Mari kenali kontrolermu sebelum mulai merakit.";
        
        if (gambarController != null && gambarFullController != null) 
        {
            gambarController.gameObject.SetActive(true);
            gambarController.sprite = gambarFullController;
        }

        yield return new WaitForSeconds(2f);

        float durasiVO = 4f; 
        if (suaraVOSelamatDatang != null && suaraVOSelamatDatang.clip != null)
        {
            suaraVOSelamatDatang.Play();
            durasiVO = suaraVOSelamatDatang.clip.length; 
        }

        yield return new WaitForSeconds(durasiVO + 5f);
        MulaiTahap(1);
    }

    // Sets up the UI and practice objects for the given tutorial stage.
    private void MulaiTahap(int tahap)
    {
        tahapTutorial = tahap;
        SembunyikanSemuaAlatPraktek(); // Clear previous practice objects.

        // Bersihkan blink/hint dari tahap sebelumnya dan reset timer idle.
        MatikanBlinkHintAktif();
        waktuMulaiTahap = Time.time;
        jumlahHintDitampilkan = 0;

        switch (tahap)
        {
            case 1:
                TampilkanJudulLangkah(1, "CARA TAP (INTERAKSI)");
                descText.text = "Arahkan laser dari tanganmu dan tekan tombol <color=#00FFFF>Trigger</color> pada tombol 'Next' yang muncul di layar.";
                if (gambarController != null && gambarTrigger != null) gambarController.sprite = gambarTrigger;
                
                if (tombolLatihanTrigger != null) tombolLatihanTrigger.SetActive(true);
                break;

            case 2:
                // Reset detection flags for the analog stage.
                kiriOk = false; kananOk = false; // Reset detection status.
                TampilkanJudulLangkah(2, "CARA BERGERAK & MENGARAHKAN PANDANGAN");
                descText.text = "Gunakan jempolmu untuk menggeser stik <color=#00FFFF>Analog Kiri / Kanan</color>.\nIni berfungsi seperti Joystick pada umumnya.";
                if (gambarController != null && gambarAnalog != null) gambarController.sprite = gambarAnalog;
                break;

            case 3:
                TampilkanJudulLangkah(3, "CARA MENGAMBIL BARANG");
                descText.text = "Gunakan jari tengahmu untuk menahan tombol <color=#00FFFF>Grip</color> dan ambil barang yang ada di depanmu.";
                if (gambarController != null && gambarGrip != null) gambarController.sprite = gambarGrip;
                
                if (barangLatihanGrip != null) 
                {
                    barangLatihanGrip.gameObject.SetActive(true);
                    MulaiPanahGrab();
                }
                break;

            case 4:
                TampilkanJudulLangkah(4, "TUTORIAL SELESAI!");
                descText.text = "<color=green>Kerja Bagus!</color>\nTekan tombol 'Selesai' di bawah ini untuk memulai dan perhatikan layar di kanan.";
                
                if (gambarController != null) gambarController.gameObject.SetActive(false); 
                if (suaraTutorialSelesai != null) suaraTutorialSelesai.Play();
                if (tombolNextDebug != null) tombolNextDebug.gameObject.SetActive(false);
                if (tombolSelesaiUI != null) tombolSelesaiUI.SetActive(true);
                break;
        }

        // Simpan instruksi dasar tahap ini untuk dipakai hint idle (jangan menumpuk teks).
        teksDasarTahap = descText != null ? descText.text : "";
    }

    // Shows the step counter (e.g. "LANGKAH 1/4") above the stage title.
    private void TampilkanJudulLangkah(int langkah, string judul)
    {
        if (titleText != null)
        {
            titleText.text = "<color=#00FFFF>LANGKAH " + langkah + "/4</color>\n" + judul;
        }
    }

    // Called by the UI button when the trigger practice is completed.
    public void SuksesLatihanTrigger()
    {
        if (tahapTutorial == 1) LanjutKeTahapBerikutnya(2);
    }

    // Detects analog stick movement (stage 2) and grip hold (stage 3).
    void Update()
    {
        // Hint idle: pemain kebanyakan diam pada tahap 1-3, tampilkan pengingat.
        if (tahapTutorial >= 1 && tahapTutorial <= 3)
        {
            float batasHint = waktuHintPertama + (jumlahHintDitampilkan * waktuAntarHint);
            if (Time.time - waktuMulaiTahap >= batasHint)
            {
                jumlahHintDitampilkan++;
                TampilkanHintIdle();
            }
        }

        // Detect analog input: check if both sticks were moved.
        if (tahapTutorial == 2) 
        {
            if (InputBridge.Instance.LeftThumbstickAxis.magnitude > 0.5f) kiriOk = true;
            if (InputBridge.Instance.RightThumbstickAxis.magnitude > 0.5f) kananOk = true;
            
            if (kiriOk && kananOk) LanjutKeTahapBerikutnya(3);
        }
        // Detect grip: check if the practice item is being held.
        else if (tahapTutorial == 3) 
        {
            if (barangLatihanGrip != null && barangLatihanGrip.BeingHeld)
            {
                HapusPanahGrab();
                LanjutKeTahapBerikutnya(4);
            }
        }
    }

    // Debug helper: skips the current stage.
    public void LewatiTahapIniDebug()
    {
        if (tahapTutorial == 0) { StopAllCoroutines(); MulaiTahap(1); }
        else if (tahapTutorial == 1) LanjutKeTahapBerikutnya(2);
        else if (tahapTutorial == 2) LanjutKeTahapBerikutnya(3);
        else if (tahapTutorial == 3) LanjutKeTahapBerikutnya(4);
    }

    // Shows an escalating reminder when the player is idle too long:
    // hint text + sound + haptic buzz + blink on the target object.
    private void TampilkanHintIdle()
    {
        // 1. Suara pengingat (fallback ke suara sukses jika suaraHint kosong).
        AudioSource sumberSuara = suaraHint != null ? suaraHint : suaraStepSukses;
        if (sumberSuara != null) sumberSuara.Play();

        // 2. Getaran halus di kedua kontroler biar pemain sadar ada pesan.
        if (InputBridge.Instance != null)
        {
            InputBridge.Instance.VibrateController(0.6f, 0.4f, 0.25f, ControllerHand.Right);
            InputBridge.Instance.VibrateController(0.6f, 0.4f, 0.25f, ControllerHand.Left);
        }

        // 3. Teks pengingat sesuai tahap (dipasang di atas instruksi dasar).
        string pesanHint = "";
        if (tahapTutorial == 1) pesanHint = "INGAT! Arahkan laser ke tombol <color=yellow>'NEXT'</color> lalu tekan <color=yellow>Trigger</color> (jari telunjuk).";
        else if (tahapTutorial == 2) pesanHint = "INGAT! Geser <color=yellow>Analog Kiri</color> DAN <color=yellow>Analog Kanan</color> dengan jempolmu.";
        else if (tahapTutorial == 3) pesanHint = "INGAT! Tahan tombol <color=yellow>Grip</color> (jari tengah) lalu pegang barang di depanmu.";
        if (descText != null && pesanHint != "")
        {
            descText.text = "<color=yellow><b>" + pesanHint + "</b></color>\n\n" + teksDasarTahap;
        }

        // 4. Blink highlight pada barang latihan Grip biar keliatan tujuannya.
        if (tahapTutorial == 3 && blinkBarangLatihan != null)
        {
            blinkBarangLatihan.enabled = true;
            blinkBarangLatihan.MulaiKedip();
        }

        // 5. Pulse tombol latihan Trigger (tahap 1) biar keliatan.
        if (tahapTutorial == 1)
        {
            if (coroutinePulseUI != null) StopCoroutine(coroutinePulseUI);
            coroutinePulseUI = StartCoroutine(PulseTombolLatihan());
        }
    }

    // Stops every active blink/pulse from the idle hint system.
    private void MatikanBlinkHintAktif()
    {
        if (blinkBarangLatihan != null) blinkBarangLatihan.BerhentiKedip();

        if (coroutinePulseUI != null)
        {
            StopCoroutine(coroutinePulseUI);
            coroutinePulseUI = null;
        }

        // Kembalikan alpha tombol latihan ke semula.
        if (gambarTombolLatihanUI != null)
        {
            Color c = gambarTombolLatihanUI.color;
            c.a = 1f;
            gambarTombolLatihanUI.color = c;
        }
    }

    // Creates a simple downward-pointing arrow above the grab practice object.
    private GameObject BuatPanahGrab(Vector3 posisi)
    {
        // Arrow = pyramid tip + cylinder shaft combined in a parent.
        GameObject panah = new GameObject("PanahGrab");
        panah.transform.position = posisi + Vector3.up * tinggiPanah;
        panah.transform.localScale = Vector3.one * skalaPanah;

        // Pyramid tip (4 triangles, points down) - procedural mesh
        GameObject tip = new GameObject("Tip");
        tip.transform.SetParent(panah.transform);
        tip.transform.localPosition = new Vector3(0, -0.5f, 0);
        tip.transform.localRotation = Quaternion.Euler(180, 0, 0); // point down
        tip.transform.localScale = new Vector3(1f, 1.2f, 1f);
        
        MeshFilter tipMF = tip.AddComponent<MeshFilter>();
        MeshRenderer tipMR = tip.AddComponent<MeshRenderer>();
        tipMF.mesh = BuatMeshPyramid();

        // Cylinder shaft
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "Shaft";
        shaft.transform.SetParent(panah.transform);
        shaft.transform.localPosition = new Vector3(0, -1.2f, 0);
        shaft.transform.localScale = new Vector3(0.3f, 1f, 0.3f);
        Destroy(shaft.GetComponent<Collider>());

        // Material with emission for visibility
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = warnaPanah;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", warnaPanah * 2f);
        mat.renderQueue = 3000;
        tipMR.material = mat;
        shaft.GetComponent<Renderer>().material = mat;

        // Gentle bounce animation
        panah.AddComponent<PanahBounce>().amplitudo = 0.05f;

        return panah;
    }

    // Procedural pyramid mesh (4 triangles, points up in local space)
    private Mesh BuatMeshPyramid()
    {
        Mesh mesh = new Mesh();
        mesh.name = "PyramidTip";
        
        // Vertices: apex (0, 0.5, 0) + 4 base corners at y = -0.5
        Vector3[] vertices = new Vector3[5];
        vertices[0] = new Vector3(0, 0.5f, 0); // apex
        vertices[1] = new Vector3(-0.5f, -0.5f, -0.5f); // base 0
        vertices[2] = new Vector3(0.5f, -0.5f, -0.5f);  // base 1
        vertices[3] = new Vector3(0.5f, -0.5f, 0.5f);   // base 2
        vertices[4] = new Vector3(-0.5f, -0.5f, 0.5f);  // base 3
        
        // Triangles (4 faces, clockwise from outside)
        int[] triangles = new int[12];
        // Face 0: apex, base1, base0
        triangles[0] = 0; triangles[1] = 2; triangles[2] = 1;
        // Face 1: apex, base2, base1
        triangles[3] = 0; triangles[4] = 3; triangles[5] = 2;
        // Face 2: apex, base3, base2
        triangles[6] = 0; triangles[7] = 4; triangles[8] = 3;
        // Face 3: apex, base0, base3
        triangles[9] = 0; triangles[10] = 1; triangles[11] = 4;
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }

    // Starts the delayed arrow coroutine for stage 3.
    private void MulaiPanahGrab()
    {
        if (coroutinePanahGrab != null) StopCoroutine(coroutinePanahGrab);
        coroutinePanahGrab = StartCoroutine(RutinPanahGrab());
    }

    // Waits delayPanahGrab seconds, then spawns the arrow above the practice object.
    private IEnumerator RutinPanahGrab()
    {
        yield return new WaitForSeconds(delayPanahGrab);

        if (barangLatihanGrip != null && barangLatihanGrip.gameObject.activeInHierarchy)
        {
            Vector3 spawnPos = barangLatihanGrip.transform.position;
            panahGrabObj = BuatPanahGrab(spawnPos);
        }
    }

    // Removes the grab arrow if it exists.
    private void HapusPanahGrab()
    {
        if (coroutinePanahGrab != null)
        {
            StopCoroutine(coroutinePanahGrab);
            coroutinePanahGrab = null;
        }
        if (panahGrabObj != null)
        {
            Destroy(panahGrabObj);
            panahGrabObj = null;
        }
    }

    // Pulsates the alpha of the practice trigger button while stage 1 is active.
    private IEnumerator PulseTombolLatihan()
    {
        if (gambarTombolLatihanUI == null && tombolLatihanTrigger != null)
        {
            gambarTombolLatihanUI = tombolLatihanTrigger.GetComponentInChildren<Image>();
        }
        if (gambarTombolLatihanUI == null) yield break;

        Color warnaAsli = gambarTombolLatihanUI.color;
        while (tahapTutorial == 1)
        {
            float alpha = Mathf.Lerp(0.3f, 1f, Mathf.PingPong(Time.time * 3.5f, 1f));
            Color c = warnaAsli;
            c.a = alpha;
            gambarTombolLatihanUI.color = c;
            yield return null;
        }
        gambarTombolLatihanUI.color = warnaAsli;
    }

    // Advances to the next stage after playing a success sound and short delay.
    private void LanjutKeTahapBerikutnya(int tahapSelanjutnya)
    {
        tahapTutorial = -1; 
        if (suaraStepSukses != null) suaraStepSukses.Play();
        StartCoroutine(JedaTransisi(tahapSelanjutnya, 0.5f));
    }

    // Waits, then starts the next stage.
    private IEnumerator JedaTransisi(int tahapSelanjutnya, float lamaJeda)
    {
        yield return new WaitForSeconds(lamaJeda);
        MulaiTahap(tahapSelanjutnya);
    }

    // Closes the welcome panel, stops any remaining voice-over, and shows a camera
    // cutscene pointing at the panel so the player knows where to go next.
    public void TutupWelcomePanel()
    {
        // Stop the welcome voice-over (if still playing).
        if (suaraVOSelamatDatang != null && suaraVOSelamatDatang.isPlaying)
        {
            suaraVOSelamatDatang.Stop();
        }

        // Also stop the tutorial-complete voice-over if the player clicks Finish early.
        if (suaraTutorialSelesai != null && suaraTutorialSelesai.isPlaying)
        {
            suaraTutorialSelesai.Stop();
        }

        if (welcomePanel != null) welcomePanel.SetActive(false);

        // Lock the machine while the cutscene plays, then guide the player to the panel.
        if (monitorMesinUtama != null) monitorMesinUtama.KunciSistemUtama(true);
        StartCoroutine(SequencePanduanKePanel());
    }

    // Camera cutscene: smoothly flies the player's EYE (CenterEyeAnchor) on a short
    // tour so they can SEE where things are: machine (left) → panel → back.
    // Input (mouse look / right analog / movement) is locked during the flight so
    // the player can't fight the cutscene — the hands stay put at the body.
    // Then unlocks the monitor so the player can walk over and press START.
    private IEnumerator SequencePanduanKePanel()
    {
        // Cache the player camera before moving anything.
        if (kameraUtamaPemain == null) kameraUtamaPemain = Camera.main;

        // No guide points assigned? Skip the cutscene and just unlock.
        if (kameraUtamaPemain == null || (titikPandangPanel == null && titikPandangMesin == null))
        {
            if (monitorMesinUtama != null) monitorMesinUtama.ArahkanKePanelStart();
            yield break;
        }

        Transform mata = CariMataPemain();
        if (mata == null) mata = kameraUtamaPemain.transform;

        // KUNCI DULU: matikan input look/move player supaya tangan & kamera
        // tidak ikut bergerak liar saat cutscene berjalan.
        KunciRigSementara(true);

        Vector3 posisiAwal = mata.position;
        Quaternion rotasiAwal = mata.rotation;

        // Stop 1: show the real machine on the left.
        if (titikPandangMesin != null)
        {
            yield return StartCoroutine(GerakkanKameraKe(mata, titikPandangMesin.position, titikPandangMesin.rotation, durasiPindahKamera));
            yield return new WaitForSeconds(durasiTahanMesin);
        }

        // Stop 2: show the panel (indicator screen).
        if (titikPandangPanel != null)
        {
            yield return StartCoroutine(GerakkanKameraKe(mata, titikPandangPanel.position, titikPandangPanel.rotation, durasiPindahKamera));
            yield return new WaitForSeconds(durasiTahanPanduan);
        }

        // Fly back to the player.
        yield return StartCoroutine(GerakkanKameraKe(mata, posisiAwal, rotasiAwal, durasiPindahKamera));

        // BUKA kunci: kembalikan kendali ke pemain.
        KunciRigSementara(false);

        // Unlock the monitor and point it at the panel with the START button.
        if (monitorMesinUtama != null) monitorMesinUtama.ArahkanKePanelStart();
    }

    // Finds the CameraRig (parent of CenterEyeAnchor / controller anchors).
    private Transform CariRigPemain()
    {
        if (rigPemain != null) return rigPemain;
        GameObject rig = GameObject.Find("CameraRig");
        if (rig != null) rigPemain = rig.transform;
        return rigPemain;
    }

    // Finds the eye anchor (CenterEyeAnchor), falling back to the main camera.
    private Transform CariMataPemain()
    {
        if (titikMataPemain != null) return titikMataPemain;
        GameObject mata = GameObject.Find("CenterEyeAnchor");
        if (mata != null) titikMataPemain = mata.transform;
        else if (kameraUtamaPemain != null) titikMataPemain = kameraUtamaPemain.transform;
        return titikMataPemain;
    }

    // Temporarily disables the rig's look/move/teleport components (and VREmulator
    // in the editor) so the player can't fight the cutscene. Restores them after.
    private void KunciRigSementara(bool kunci)
    {
        Transform rig = CariRigPemain();
        if (rig == null) return;

        if (kunci)
        {
            if (rigSedangDikunci) return;
            rigSedangDikunci = true;
            daftarKomponenTerkunci.Clear();

            Transform playerController = rig.parent;
            if (playerController != null)
            {
                SimpanDanMatikan(playerController.GetComponent<BNGPlayerController>());
                SimpanDanMatikan(playerController.GetComponent<SmoothLocomotion>());
                SimpanDanMatikan(playerController.GetComponent<PlayerRotation>());
                SimpanDanMatikan(playerController.GetComponent<PlayerTeleport>());

                Transform root = playerController.parent;
                if (root != null) SimpanDanMatikan(root.GetComponent<VREmulator>());
            }
        }
        else
        {
            if (!rigSedangDikunci) return;
            rigSedangDikunci = false;
            foreach (var item in daftarKomponenTerkunci)
            {
                if (item.komponen != null && item.aktifSebelumnya) item.komponen.enabled = true;
            }
            daftarKomponenTerkunci.Clear();
        }
    }

    // Remembers the component's state and disables it (only if it was active).
    private void SimpanDanMatikan(MonoBehaviour komponen)
    {
        if (komponen == null) return;
        daftarKomponenTerkunci.Add(new KomponenKunci { komponen = komponen, aktifSebelumnya = komponen.enabled });
        komponen.enabled = false;
    }

    // Smoothly lerps the eye transform to a destination position/rotation.
    private IEnumerator GerakkanKameraKe(Transform kamera, Vector3 posisiTujuan, Quaternion rotasiTujuan, float durasi)
    {
        Vector3 posisiAwal = kamera.position;
        Quaternion rotasiAwal = kamera.rotation;
        float timer = 0f;
        while (timer < durasi)
        {
            timer += Time.deltaTime;
            float persen = Mathf.SmoothStep(0f, 1f, timer / durasi);
            kamera.position = Vector3.Lerp(posisiAwal, posisiTujuan, persen);
            kamera.rotation = Quaternion.Slerp(rotasiAwal, rotasiTujuan, persen);
            yield return null;
        }
        kamera.position = posisiTujuan;
        kamera.rotation = rotasiTujuan;
    }
}

// Simple bounce animation for the grab arrow indicator.
public class PanahBounce : MonoBehaviour
{
    public float amplitudo = 0.05f;
    public float kecepatan = 2f;

    private Vector3 posisiAwal;

    void Start()
    {
        posisiAwal = transform.localPosition;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * kecepatan) * amplitudo;
        transform.localPosition = posisiAwal + Vector3.up * offset;
    }
}
