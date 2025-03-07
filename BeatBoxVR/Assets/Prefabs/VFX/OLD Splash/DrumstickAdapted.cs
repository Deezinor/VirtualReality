using UnityEngine;
using UnityEngine.XR;

public class DrumstickAdapted : MonoBehaviour
{
    private SoundManager soundManager;

    public GameObject smallSplashPrefab;
    public GameObject mediumSplashPrefab;
    public GameObject largeSplashPrefab;

    public float vfxLifetime = 1.4f;
    private const float MaxVelocity = 10f;

    public Transform tipTransform;
    private Vector3 previousTipPosition;
    private Vector3 tipMovementDirection;
    private float tipVelocity;

    public bool instantiateVFX = true; // Flag to control VFX instantiation
    public bool enableHapticFeedback = true; // Flag to control haptic feedback
    public float LastHitVelocity { get; private set; }

    void Start()
    {
        soundManager = FindObjectOfType<SoundManager>();
        if (soundManager == null)
        {
            Debug.LogError("SoundManager not found in the scene");
        }

        previousTipPosition = tipTransform.position;
    }

    void Update()
    {
        Vector3 currentTipPosition = tipTransform.position;
        tipMovementDirection = currentTipPosition - previousTipPosition;
        tipVelocity = tipMovementDirection.magnitude / Time.deltaTime;
        previousTipPosition = currentTipPosition;
    }

    // Method to get current velocity
    public float GetCurrentVelocity()
    {
        return Mathf.Clamp(tipVelocity, 0, MaxVelocity);
    }

    void OnTriggerEnter(Collider other)
    {
        if (tipMovementDirection.y < 0)
        {
            float clampedVelocity = GetCurrentVelocity();

            Debug.Log($"Drumstick hit detected. Velocity: {clampedVelocity}. Collider Tag: {other.tag}");

            Vector3 collisionPoint = tipTransform.position;
            soundManager.PlaySound(other.tag, collisionPoint, clampedVelocity / MaxVelocity);

            if (clampedVelocity > 1 && instantiateVFX)
            {
                GameObject vfxPrefab = SelectVFXPrefabBasedOnVelocity(clampedVelocity);
                if (vfxPrefab != null)
                {
                    InstantiateVFX(vfxPrefab, collisionPoint, other.transform.position - collisionPoint);
                }
            }

            if (enableHapticFeedback)
            {
                TriggerHapticFeedback(gameObject.tag, 0.1f, Mathf.InverseLerp(0, MaxVelocity, clampedVelocity));
            }
        }
    }

    // Method to toggle VFX instantiation
    public void ToggleVFXInstantiation(bool isEnabled)
    {
        instantiateVFX = isEnabled;
    }

    public void ToggleHapticFeedback(bool isEnabled)
    {
        enableHapticFeedback = isEnabled;
    }

    private GameObject SelectVFXPrefabBasedOnVelocity(float velocity)
    {
        if (velocity <= 4)
            return smallSplashPrefab;
        else if (velocity <= 7)
            return mediumSplashPrefab;
        else
            return largeSplashPrefab;
    }

    private void InstantiateVFX(GameObject vfxPrefab, Vector3 position, Vector3 direction)
    {
        GameObject vfxInstance = Instantiate(vfxPrefab, position, Quaternion.identity);

        Destroy(vfxInstance, vfxLifetime);
        Debug.Log($"Instantiated VFX: {vfxPrefab.name} at position: {position}");

    }

    // Method to calculate scale multiplier based on tip velocity
    // Can be used on haptic
    private float CalculateScaleMultiplier(float velocity)
    {
        if (velocity <= 4) // Slow hits
        {
            // Smaller scale for slower hits
            return 0.5f + (velocity / MaxVelocity) * 0.5f; // Scale from 0.5 to 1 for slow hits
        }
        else if (velocity <= 7) // Medium hits
        {
            // Medium scale for medium hits
            return 1f + ((velocity - 4) / (7 - 4)) * 0.5f; // Scale from 1 to 1.5 for medium hits
        }
        else // Fast hits
        {
            // Larger scale for fast hits
            return 1.5f + ((velocity - 7) / (MaxVelocity - 7)) * 1f; // Scale from 1.5 to 2.5 for fast hits
        }
    }

    private void TriggerHapticFeedback(string drumstickTag, float duration, float strength)
    {
        InputDevice device = GetDeviceByDrumstickTag(drumstickTag);

        if (device.isValid)
        {
            device.SendHapticImpulse(0, strength, duration);
        }
    }

    private InputDevice GetDeviceByDrumstickTag(string tag)
    {
        // This method should be implemented to return the correct InputDevice
        // based on whether the tag is LeftDrumstick or RightDrumstick.

        if (tag == "LeftDrumstick")
        {
            return InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        }
        else if (tag == "RightDrumstick")
        {
            return InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        }

        return new InputDevice(); // (Fallback) ideally never used
    }
}