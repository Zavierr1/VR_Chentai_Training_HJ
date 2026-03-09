using UnityEngine;

public class PartLabel : MonoBehaviour
{
    [Header("UI Referensi")]
    [Tooltip("Masukkan GameObject UI Canvas atau Text Label yang mau dimunculkan/dihilangkan")]
    public GameObject labelUI;

    void Start()
    {
        // Opsional: Matikan label di awal game agar tidak memenuhi layar
        if (labelUI != null)
        {
            labelUI.SetActive(false);
        }
    }

    // Fungsi ini yang dicari dan dipanggil oleh PartLabelButton.cs
    public void ToggleLabel()
    {
        if (labelUI != null)
        {
            // Membalikkan status (kalau sedang mati jadi nyala, kalau nyala jadi mati)
            bool isCurrentlyActive = labelUI.activeSelf;
            labelUI.SetActive(!isCurrentlyActive);
        }
        else
        {
            Debug.LogWarning("⚠️ Objek Label UI belum dimasukkan ke Inspector di script PartLabel!");
        }
    }
}