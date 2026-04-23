using UnityEngine;
using UnityEngine.SceneManagement; // Dibutuhkan untuk memuat ulang scene

public class MainMenuManager : MonoBehaviour
{
    [Header("Pengaturan UI Utama")]
    public GameObject canvasMainMenu;

    [Header("Mode Tutorial (Satu Scene)")]
    public GameObject objekTutorialManager;
    
    [Header("Teleport Player (Khusus Tutorial)")]
    public Transform playerRig;
    public Transform titikMulaiSistem;

    [Header("Mode Assessment (Pindah Scene)")]
    public string namaSceneAssessment = "Scene_Assessment_Mesin";

    public static bool autoStartTutorial = false;

    void Start()
    {
        // Cek apakah scene ini di-reload dari tombol Reset Tutorial
        if (autoStartTutorial)
        {
            autoStartTutorial = false; // Matikan flag agar tidak looping
            MulaiModeTutorial(); // Langsung jalankan tutorial, bypass Main Menu!
        }
        else
        {
            // Kondisi normal (Game baru pertama dibuka)
            if (canvasMainMenu != null) canvasMainMenu.SetActive(true);
            if (objekTutorialManager != null) objekTutorialManager.SetActive(false);
        }
    }

    // Fungsi ini mengembalikan game BENAR-BENAR ke awal (ke layar Main Menu)
    public void ResetGame()
    {
        Debug.Log("Resetting: Mengembalikan semua ke awal (Main Menu)...");
        autoStartTutorial = false; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MulaiModeTutorial()
    {
        if (canvasMainMenu != null) canvasMainMenu.SetActive(false);
        
        if (playerRig != null && titikMulaiSistem != null)
        {
            playerRig.position = titikMulaiSistem.position;
            playerRig.rotation = Quaternion.Euler(0, titikMulaiSistem.eulerAngles.y, 0);
        }

        if (objekTutorialManager != null) objekTutorialManager.SetActive(true);
    }

    public void MulaiModeAssessment()
    {
        if (!string.IsNullOrEmpty(namaSceneAssessment))
        {
            SceneManager.LoadScene(namaSceneAssessment); 
        }
    }

    public void KeluarGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}