using UnityEngine;
using UnityEngine.SceneManagement; // Required to load scenes.

// Main menu controller: starts the tutorial (same scene) or the assessment scene,
// fully resets the game back to the main menu, and exits the application.
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

    // Static flag so the tutorial auto-starts after a scene reload.
    public static bool autoStartTutorial = false;

    // If the scene was reloaded via the tutorial reset button, bypass the main menu.
    void Start()
    {
        // Check whether this scene was reloaded from the Reset Tutorial button.
        if (autoStartTutorial)
        {
            autoStartTutorial = false; // Clear the flag to prevent a loop.
            MulaiModeTutorial(); // Jump straight into the tutorial, bypassing the main menu!
        }
        else
        {
            // Normal case (fresh game start).
            if (canvasMainMenu != null) canvasMainMenu.SetActive(true);
            if (objekTutorialManager != null) objekTutorialManager.SetActive(false);
        }
    }

    // Returns the game fully to the beginning (main menu screen).
    public void ResetGame()
    {
        Debug.Log("Resetting: Mengembalikan semua ke awal (Main Menu)...");
        autoStartTutorial = false; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Hides the menu, repositions the player, and activates the tutorial objects.
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

    // Loads the assessment scene.
    public void MulaiModeAssessment()
    {
        if (!string.IsNullOrEmpty(namaSceneAssessment))
        {
            SceneManager.LoadScene(namaSceneAssessment); 
        }
    }

    // Quits the application (also stops play mode in the editor).
    public void KeluarGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
