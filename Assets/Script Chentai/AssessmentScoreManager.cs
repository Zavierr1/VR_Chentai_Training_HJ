using UnityEngine;
// using TMPro; // Nanti buka comment ini jika ingin menampilkan skor di UI TextMeshPro

public class AssessmentScoreManager : MonoBehaviour
{
    [Header("Score Status")]
    public int currentScore = 0;
    public int maxScore = 100; // 30 + 40 + 30 = 100

    // public TMP_Text scoreTextUI; // Nanti buka comment ini untuk UI

    public void AddPoints(int points)
    {
        currentScore += points;
        Debug.Log($"<color=green>+{points} Poin!</color> Total Skor: {currentScore} / {maxScore}");
        UpdateUI();
    }

    public void RemovePoints(int points)
    {
        currentScore -= points;
        
        // Mencegah skor menjadi minus
        if (currentScore < 0) currentScore = 0; 
        
        Debug.Log($"<color=red>-{points} Poin (Part dilepas).</color> Total Skor: {currentScore} / {maxScore}");
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Jika nanti kamu punya UI Text, logika updatenya taruh sini
        // if (scoreTextUI != null) scoreTextUI.text = $"Score: {currentScore}";
    }
}