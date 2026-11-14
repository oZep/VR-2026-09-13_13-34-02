using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

// Put this on the same GameObject as the tip collider ("FloorDetection"):
//   - SphereCollider, Is Trigger ON
//   - Rigidbody, Is Kinematic ON, Use Gravity OFF
// Remove or disable CaneTipContact so the two scripts don't both run.
//
// The object you want to trigger vibration needs a Collider (trigger or solid)
// and EITHER the tag set below OR to be dragged into "Specific Targets".
public class CaneHapticContact : MonoBehaviour
{
    [Header("Which objects trigger vibration")]
    [Tooltip("Any object with this tag triggers vibration. Create the tag in Tags & Layers first. Leave blank to use only Specific Targets.")]
    public string triggerTag = "VibrateOnContact";

    [Tooltip("Optional: drag specific scene objects here (matches the object or its parents).")]
    public GameObject[] specificTargets;

    [Header("Controller")]
    [Tooltip("The HapticImpulsePlayer on the controller holding the cane (XRI 3.x). Drag it in.")]
    public HapticImpulsePlayer hapticPlayer;

    [Tooltip("Fallback if no HapticImpulsePlayer is assigned.")]
    public XRNode fallbackNode = XRNode.RightHand;

    [Header("Vibration feel")]
    [Range(0f, 1f)] public float amplitude = 0.7f;
    public float duration = 0.15f;
    public float minTimeBetweenPulses = 0.1f;

    float _timeSinceLastPulse = 999f;

    void Awake()
    {
        Debug.Log($"[CaneHaptic] Ready. Tag='{triggerTag}', specific targets={specificTargets?.Length ?? 0}, " +
            $"HapticImpulsePlayer assigned={hapticPlayer != null}, " +
            $"Rigidbody={GetComponent<Rigidbody>() != null}, trigger collider={GetComponent<Collider>()?.isTrigger}");
    }

    void Update() => _timeSinceLastPulse += Time.deltaTime;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[CaneHaptic] Touched '{other.name}' (tag '{other.tag}')");

        if (!IsTarget(other))
        {
            Debug.Log("[CaneHaptic] Not a target, ignoring.");
            return;
        }
        if (_timeSinceLastPulse < minTimeBetweenPulses) return;

        _timeSinceLastPulse = 0f;
        Vibrate();
    }

    bool IsTarget(Collider other)
    {
        if (!string.IsNullOrEmpty(triggerTag) && other.CompareTag(triggerTag))
            return true;

        if (specificTargets != null)
        {
            foreach (var target in specificTargets)
            {
                if (target == null) continue;
                if (other.gameObject == target || other.transform.IsChildOf(target.transform))
                    return true;
            }
        }
        return false;
    }

    void Vibrate()
    {
        if (hapticPlayer != null)
        {
            hapticPlayer.SendHapticImpulse(amplitude, duration);
            Debug.Log($"[CaneHaptic] Vibrate via HapticImpulsePlayer: amp={amplitude}, dur={duration}");
            return;
        }

        var device = InputDevices.GetDeviceAtXRNode(fallbackNode);
        if (device.isValid && device.TryGetHapticCapabilities(out var caps) && caps.supportsImpulse)
        {
            device.SendHapticImpulse(0u, amplitude, duration);
            Debug.Log($"[CaneHaptic] Vibrate via XRNode {fallbackNode}: amp={amplitude}, dur={duration}");
        }
        else
        {
            Debug.Log("[CaneHaptic] Contact detected, but no haptic-capable controller found " +
                "(expected in the Device Simulator — the trigger logic is working).");
        }
    }
}