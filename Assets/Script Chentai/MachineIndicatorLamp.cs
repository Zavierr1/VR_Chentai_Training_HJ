using UnityEngine;

// Shows the machine's on/off state by swapping the material of an indicator lamp
// (red = off, green = on) based on MachineController's state change event.
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
    public Material yellowMaterial; // Kept so the Inspector does not error.
    public Material greenMaterial;

    public bool setRedOnStart = true;

    // Ensures a renderer reference is available.
    private void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
    }

    // Subscribes to the machine state event and applies the current state.
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

    // Unsubscribes from the machine state event.
    private void OnDisable()
    {
        if (machineController != null) machineController.OnMachineStateChanged -= HandleMachineStateChanged;
    }

    // Picks the green or red material based on the machine's on/off state.
    private void HandleMachineStateChanged(bool isOn)
    {
        SetLampMaterial(isOn ? greenMaterial : redMaterial);
    }

    // Swaps the material at the configured index(es) on the lamp renderer.
    private void SetLampMaterial(Material matToSet)
    {
        // Early safety check.
        if (targetRenderer == null || matToSet == null) return;

        // Get the material array from the renderer.
        Material[] mats = targetRenderer.materials;
        if (mats.Length == 0) return;

        // Apply the material to the configured index or indexes.
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

        // Write the material array back to the renderer.
        targetRenderer.materials = mats;
    }
}
