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

    void Start()
    {
        if (canvasMainMenu != null) canvasMainMenu.SetActive(true);
        if (objekTutorialManager != null) objekTutorialManager.SetActive(false);
    }

    // >>> FUNGSI BARU: RESET GAME (UNTUK MENGATASI BUG) <<<
    public void ResetGame()
    {
        Debug.Log("Resetting: Mengembalikan semua ke awal...");
        
        // Perintah ini akan memuat ulang scene yang sedang dimainkan saat ini.
        // Sangat ampuh untuk membersihkan bug atau state script yang error.
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