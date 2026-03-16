using UnityEngine;
using BNG;

/// <summary>
/// Add this component alongside a Grabbable to require two hands before the object can be picked up.
/// Works with BNG Framework.
/// </summary>
public class RequireTwoHands : MonoBehaviour
{
    [Header("Two Hand Requirement")]
    [Tooltip("If true, object cannot be held with only one hand — will be dropped unless second hand grabs within grace period.")]
    public bool RequireBothHands = true;

    [Tooltip("How long (seconds) the player has to grab with second hand before object is dropped. Set to 0 for instant drop.")]
    public float GracePeriod = 0.3f;

    [Tooltip("Show a hint message when grabbed with only one hand.")]
    public bool ShowHint = true;

    [Tooltip("Optional UI Text to show hint. Leave empty if not needed.")]
    public TMPro.TextMeshPro HintText;

    [Tooltip("Message shown when grabbed with one hand only.")]
    public string HintMessage = "Use both hands to carry this object!";

    private Grabbable _grabbable;
    private float _singleHandTimer = 0f;
    private bool _waitingForSecondHand = false;

    void Awake()
    {
        _grabbable = GetComponent<Grabbable>();

        if (_grabbable == null)
        {
            Debug.LogWarning("[RequireTwoHands] No Grabbable found on " + gameObject.name);
            return;
        }

        // Force DualGrab behavior so second hand can grab while first is holding
        _grabbable.SecondaryGrabBehavior = OtherGrabBehavior.DualGrab;
    }

    void Update()
    {
        if (!RequireBothHands) return;
        if (_grabbable == null) return;

        // Object is being held
        if (_grabbable.BeingHeld)
        {
            bool twoHanded = _grabbable.BeingHeldWithTwoHands;

            if (!twoHanded)
            {
                // Start grace period timer
                if (!_waitingForSecondHand)
                {
                    _waitingForSecondHand = true;
                    _singleHandTimer = 0f;
                    ShowHintMessage(true);
                }

                _singleHandTimer += Time.deltaTime;

                // Grace period exceeded — drop the object
                if (_singleHandTimer >= GracePeriod)
                {
                    _grabbable.DropItem(false, true);
                    _waitingForSecondHand = false;
                    _singleHandTimer = 0f;
                    ShowHintMessage(false);
                }
            }
            else
            {
                // Two hands detected — reset timer
                _waitingForSecondHand = false;
                _singleHandTimer = 0f;
                ShowHintMessage(false);
            }
        }
        else
        {
            // Not being held — reset everything
            _waitingForSecondHand = false;
            _singleHandTimer = 0f;
            ShowHintMessage(false);
        }
    }

    void ShowHintMessage(bool show)
    {
        if (!ShowHint) return;
        if (HintText == null) return;

        HintText.gameObject.SetActive(show);
        if (show) HintText.text = HintMessage;
    }
}