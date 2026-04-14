using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Pengaturan UI")]
    [Tooltip("Tarik Canvas Main Menu-mu ke sini")]
    public GameObject canvasMainMenu;
    
    [Tooltip("Tarik GameObject yang memegang script InputTutorialManager ke sini")]
    public GameObject objekTutorialManager;

    [Header("Opsional: Teleport Player")]
    [Tooltip("Tarik objek PlayerController/XR Rig BNG ke sini jika ingin player dipindah")]
    public Transform playerRig;
    [Tooltip("Titik berdiri player di depan mesin")]
    public Transform titikMulaiTutorial;

    void Start()
    {
        // Kondisi saat awal game: Menu nyala, Tutorial mati
        if (canvasMainMenu != null) canvasMainMenu.SetActive(true);
        if (objekTutorialManager != null) objekTutorialManager.SetActive(false);
    }

    // FUNGSI INI DIHUBUNGKAN KE TOMBOL "MULAI TUTORIAL" DI MAIN MENU
    public void MulaiModeTutorial()
    {
        // 1. Matikan layar Main Menu
        if (canvasMainMenu != null) canvasMainMenu.SetActive(false);

        // 2. (Opsional) Teleport player ke depan mesin jika menu beradanya agak jauh
        if (playerRig != null && titikMulaiTutorial != null)
        {
            playerRig.position = titikMulaiTutorial.position;
            // Gunakan rotasi Y saja agar player tidak miring/nunduk
            playerRig.rotation = Quaternion.Euler(0, titikMulaiTutorial.eulerAngles.y, 0);
        }

        // 3. Bangunkan Tutorial Manager!
        // Karena di InputTutorialManager ada fungsi Start() yang memanggil SequencePembukaanOtomatis,
        // begitu objek ini dinyalakan, tutorial akan otomatis berjalan.
        if (objekTutorialManager != null) objekTutorialManager.SetActive(true);
    }
}