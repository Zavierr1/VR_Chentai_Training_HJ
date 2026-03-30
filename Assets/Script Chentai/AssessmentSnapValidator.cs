using UnityEngine;
using System.Collections.Generic;
using BNG;

[RequireComponent(typeof(SnapZone))]
[RequireComponent(typeof(GrabbablesInTrigger))]
public class AssessmentSnapValidator : MonoBehaviour
{
    [Header("Assessment Target")]
    [Tooltip("Ketik Tag dari barang yang BENAR (Sama seperti di SnapGroupManager)")]
    public string requiredTag = "Untagged";

    [Header("Scoring System")]
    [Tooltip("Jumlah poin yang didapat jika part ini dipasang")]
    public int pointValue = 10; 
    [Tooltip("Drag object yang memiliki script AssessmentScoreManager ke sini")]
    public AssessmentScoreManager scoreManager;

    [Header("Visual Feedback")]
    [Tooltip("Warna saat part salah (Merah)")]
    public Color wrongColor = new Color(1f, 0f, 0f, 0.8f);

    private SnapZone snapZone;
    private GrabbablesInTrigger gZone;
    private SnapZoneRingHelper ringHelper;

    private Color originalRestingColor;
    private Color originalValidColor;

    private const string BLOCK_CODE = "BLOCK_WRONG_ITEM_ASSESSMENT";

    void Start()
    {
        snapZone = GetComponent<SnapZone>();
        gZone = GetComponent<GrabbablesInTrigger>();
        ringHelper = GetComponentInChildren<SnapZoneRingHelper>();

        if (ringHelper != null)
        {
            originalRestingColor = ringHelper.RestingColor;
            originalValidColor = ringHelper.ValidSnapColor;
        }

        // --- SISTEM POIN: Daftarkan event otomatis ke BNG ---
        snapZone.OnSnapEvent.AddListener(HandleItemSnapped);
        snapZone.OnDetachEvent.AddListener(HandleItemDetached);

        BlockSnapping();
    }

    // Dipanggil otomatis oleh BNG saat barang berhasil menempel
    private void HandleItemSnapped(Grabbable grabbedItem)
    {
        if (scoreManager != null)
        {
            scoreManager.AddPoints(pointValue);
        }
    }

    // Dipanggil otomatis oleh BNG saat barang dicabut dari SnapZone
    private void HandleItemDetached(Grabbable grabbedItem)
    {
        if (scoreManager != null)
        {
            scoreManager.RemovePoints(pointValue);
        }
    }

    void Update()
    {
        if (snapZone.HeldItem != null)
        {
            SetRingColor(true); 
            return;
        }

        Grabbable closest = GetClosestHoveredGrabbable();

        if (closest != null)
        {
            if (closest.CompareTag(requiredTag))
            {
                SetRingColor(true); 
                AllowSnapping();
            }
            else
            {
                SetRingColor(false); 
                BlockSnapping();
            }
        }
        else
        {
            SetRingColor(true);
            BlockSnapping(); 
        }
    }

    private void SetRingColor(bool isValid)
    {
        if (ringHelper != null)
        {
            if (isValid)
            {
                ringHelper.RestingColor = originalRestingColor;
                ringHelper.ValidSnapColor = originalValidColor;
            }
            else
            {
                ringHelper.RestingColor = wrongColor;
                ringHelper.ValidSnapColor = wrongColor;
            }
        }
    }

    private void BlockSnapping()
    {
        if (snapZone.OnlyAllowNames == null) snapZone.OnlyAllowNames = new List<string>();

        if (!snapZone.OnlyAllowNames.Contains(BLOCK_CODE))
        {
            snapZone.OnlyAllowNames.Clear();
            snapZone.OnlyAllowNames.Add(BLOCK_CODE);
        }
    }

    private void AllowSnapping()
    {
        if (snapZone.OnlyAllowNames != null && snapZone.OnlyAllowNames.Contains(BLOCK_CODE))
        {
            snapZone.OnlyAllowNames.Clear();
        }
    }

    private Grabbable GetClosestHoveredGrabbable()
    {
        float closestDist = float.MaxValue;
        Grabbable closestGrab = null;

        foreach (var kvp in gZone.NearbyGrabbables)
        {
            Grabbable g = kvp.Value;
            if (g == null || !g.BeingHeld) continue; 

            float dist = Vector3.Distance(transform.position, g.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestGrab = g;
            }
        }

        return closestGrab;
    }
}