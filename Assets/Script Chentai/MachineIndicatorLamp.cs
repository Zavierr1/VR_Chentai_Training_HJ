using UnityEngine;

public class MachineIndicatorLamp : MonoBehaviour
{
    [Header("Machine Reference")]
    public MachineController machineController;

    [Header("Lamp Renderer")]
    public Renderer targetRenderer;
    public bool useMultipleMaterialIndexes = false;
    public int materialIndex = 0;
    public int[] materialIndexes;

    [Header("State Materials")]
    public Material redMaterial;
    public Material yellowMaterial; // Disimpan agar Inspector tidak error
    public Material greenMaterial;

    public bool setRedOnStart = true;

    private void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        if (machineController != null)
        {
            machineController.OnMachineStateChanged += HandleMachineStateChanged;
            HandleMachineStateChanged(machineController.isMachineOn);
        }
        else if (setRedOnStart)
        {
            SetLampMaterial(redMaterial);
        }
    }

    private void OnDisable()
    {
        if (machineController != null) machineController.OnMachineStateChanged -= HandleMachineStateChanged;
    }

    private void HandleMachineStateChanged(bool isOn)
    {
        SetLampMaterial(isOn ? greenMaterial : redMaterial);
    }

    private void SetLampMaterial(Material matToSet)
    {
        // Pengecekan aman di awal
        if (targetRenderer == null || matToSet == null) return;

        // Ambil array material dari renderer
        Material[] mats = targetRenderer.materials;
        if (mats.Length == 0) return;

        // Logika pengisian material yang sudah dirampingkan
        if (useMultipleMaterialIndexes && materialIndexes != null)
        {
            foreach (int idx in materialIndexes)
            {
                if (idx >= 0 && idx < mats.Length) mats[idx] = matToSet;
            }
        }
        else if (!useMultipleMaterialIndexes && materialIndex >= 0 && materialIndex < mats.Length)
        {
            mats[materialIndex] = matToSet;
        }

        // Terapkan kembali material ke renderer
        targetRenderer.materials = mats;
    }
}