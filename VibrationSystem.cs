using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Vibration System - Manages the vibration parameter for the motor
/// Reads RPM from MotorController and applies vibration effect
/// This is DEPENDENT on RPM (has threshold logic at 2000 RPM)
/// </summary>
public class VibrationSystem : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the central MotorController")]
    public MotorController motorController;

    [Header("UI Elements")]
    [Tooltip("Slider for controlling vibration amplitude")]
    public Slider vibrationSlider;
    
    [Tooltip("Text display for current vibration value (optional)")]
    public TextMeshProUGUI vibrationText;

    [Header("Vibration Target")]
    [Tooltip("Transform to apply vibration effect to (usually Motor Assembly)")]
    public Transform targetTransform;

    [Header("Vibration Settings")]
    [Tooltip("Minimum vibration amplitude (mm)")]
    public float minVibration = 0f;
    
    [Tooltip("Maximum vibration amplitude (mm)")]
    public float maxVibration = 20f;
    
    [Tooltip("Default vibration when motor starts")]
    public float defaultVibration = 0f;

    [Header("Threshold Settings")]
    [Tooltip("RPM threshold above which minimum vibration is enforced")]
    public float rpmThreshold = 2000f;
    
    [Tooltip("Minimum vibration enforced above threshold (mm)")]
    public float minimumVibrationAboveThreshold = 5f;    [Header("Vibration Physics")]
    [Tooltip("Frequency of vibration oscillation")]
    public float frequency = 10f;
    
    [Tooltip("Scale multiplier for vibration amplitude (increase if motor is large)")]
    public float vibrationScaleMultiplier = 1f;

    private Vector3 initialPosition;
    private float previousRPM = 0f;
    private float effectiveVibrationMM = 0f;
    private bool wasMotorRunning = false;

    private void Start()
    {
        // Validate references
        if (motorController == null)
        {
            Debug.LogError("[VibrationSystem] MotorController reference is missing! Please assign it in the Inspector.");
            return;
        }

        if (vibrationSlider == null)
        {
            Debug.LogError("[VibrationSystem] Vibration Slider reference is missing! Please assign it in the Inspector.");
            return;
        }

        if (targetTransform == null)
        {
            Debug.LogError("[VibrationSystem] Target Transform reference is missing! Please assign it in the Inspector.");
            return;
        }

        // Store initial position
        initialPosition = targetTransform.localPosition;

        // Configure slider range
        vibrationSlider.minValue = minVibration;
        vibrationSlider.maxValue = maxVibration;
        vibrationSlider.value = defaultVibration;

        // Add listener for slider changes
        vibrationSlider.onValueChanged.AddListener(OnVibrationSliderChanged);

        // Initialize display
        UpdateVibrationDisplay(0f);

        Debug.Log("[VibrationSystem] Initialized successfully!");
    }

    private void Update()
    {
        if (motorController == null || vibrationSlider == null || targetTransform == null) 
            return;

        // Detect when motor starts
        if (motorController.isMotorRunning && !wasMotorRunning)
        {
            // Motor just started - reset slider to default
            vibrationSlider.value = defaultVibration;
            Debug.Log($"[VibrationSystem] Motor started, vibration set to {defaultVibration} mm");
        }

        if (!motorController.isMotorRunning)
        {
            // Motor stopped - no vibration
            effectiveVibrationMM = 0f;
            vibrationSlider.value = 0f;
            targetTransform.localPosition = initialPosition;
            UpdateVibrationDisplay(0f);
            wasMotorRunning = false;
            return;
        }

        // Motor is running - calculate vibration
        float currentRPM = motorController.currentRPM;
        float sliderValue = vibrationSlider.value;

        // Apply threshold logic
        if (currentRPM < rpmThreshold)
        {
            // Below threshold: Check if we just crossed down
            if (previousRPM >= rpmThreshold)
            {
                // Just crossed below threshold - snap slider to 0
                vibrationSlider.value = 0f;
                sliderValue = 0f;
                Debug.Log($"[VibrationSystem] RPM below {rpmThreshold}, vibration reset to 0");
            }

            effectiveVibrationMM = sliderValue;
        }
        else
        {
            // Above threshold: enforce minimum vibration
            if (sliderValue < minimumVibrationAboveThreshold)
            {
                effectiveVibrationMM = minimumVibrationAboveThreshold;
                vibrationSlider.value = minimumVibrationAboveThreshold;
                Debug.Log($"[VibrationSystem] RPM above {rpmThreshold}, enforcing minimum {minimumVibrationAboveThreshold}mm");
            }
            else
            {
                effectiveVibrationMM = sliderValue;
            }
        }

        // Apply vibration effect
        ApplyVibration(effectiveVibrationMM);

        // Update display
        UpdateVibrationDisplay(effectiveVibrationMM);

        // Store current RPM for next frame comparison
        previousRPM = currentRPM;
        wasMotorRunning = motorController.isMotorRunning;
    }    /// <summary>
    /// Applies vibration effect to the target transform
    /// </summary>
    private void ApplyVibration(float amplitudeMM)
    {
        if (amplitudeMM <= 0f)
        {
            targetTransform.localPosition = initialPosition;
            return;
        }

        // Convert mm to meters and apply scale multiplier
        float amplitudeMeters = (amplitudeMM / 1000f) * vibrationScaleMultiplier;

        // Calculate time-based offset
        float t = Time.time * frequency * 2f * Mathf.PI;

        // Create 3D vibration pattern (multi-axis)
        Vector3 offset = new Vector3(
            Mathf.Sin(t * 1.0f),           // X-axis
            Mathf.Sin(t * 1.3f + 0.5f),    // Y-axis (different phase)
            Mathf.Sin(t * 0.8f + 1.0f)     // Z-axis (different phase)
        ) * amplitudeMeters;

        // Apply offset
        targetTransform.localPosition = initialPosition + offset;
    }

    /// <summary>
    /// Called when the slider value changes
    /// </summary>
    private void OnVibrationSliderChanged(float value)
    {
        if (motorController != null && motorController.isMotorRunning)
        {
            Debug.Log($"[VibrationSystem] Vibration slider changed to: {value:F1} mm");
        }
    }

    /// <summary>
    /// Updates the vibration text display
    /// </summary>
    private void UpdateVibrationDisplay(float vibrationMM)
    {
        if (vibrationText != null)
        {
            vibrationText.text = $"Vibration: {vibrationMM:F1} mm";
        }

        // Update motor controller's vibration state
        if (motorController != null)
        {
            motorController.currentVibration = vibrationMM;
        }
    }

    /// <summary>
    /// Reset vibration to default value
    /// </summary>
    public void ResetVibration()
    {
        if (vibrationSlider != null)
        {
            vibrationSlider.value = defaultVibration;
        }
    }

    /// <summary>
    /// Set vibration to a specific value programmatically
    /// </summary>
    public void SetVibration(float targetVibration)
    {
        if (vibrationSlider != null)
        {
            vibrationSlider.value = Mathf.Clamp(targetVibration, minVibration, maxVibration);
        }
    }

    private void OnDestroy()
    {
        // Clean up listener
        if (vibrationSlider != null)
        {
            vibrationSlider.onValueChanged.RemoveListener(OnVibrationSliderChanged);
        }

        // Reset position
        if (targetTransform != null)
        {
            targetTransform.localPosition = initialPosition;
        }
    }
}
