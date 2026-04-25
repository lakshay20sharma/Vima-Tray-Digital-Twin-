using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Temperature System - Manages the temperature parameter for the motor
/// Controls visual feedback (colors) and overheat protection
/// This is SEMI-INDEPENDENT - can read motor state and override it (emergency stop)
/// </summary>
public class TemperatureSystem : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the central MotorController")]
    public MotorController motorController;

    [Header("UI Elements")]
    [Tooltip("Slider for controlling temperature")]
    public Slider temperatureSlider;
    
    [Tooltip("Text display for current temperature value (optional)")]
    public TextMeshProUGUI temperatureText;

    [Header("Material Targets")]
    [Tooltip("Renderer for motor shaft (inner rotating part - intense colors)")]
    public Renderer shaftRenderer;
    
    [Tooltip("Renderer for motor shell/body (outer casing - softer colors)")]
    public Renderer shellRenderer;

    [Header("Temperature Settings")]
    [Tooltip("Minimum temperature (°C)")]
    public float minTemperature = -30f;
    
    [Tooltip("Maximum temperature (°C)")]
    public float maxTemperature = 100f;
    
    [Tooltip("Default temperature when motor starts (°C)")]
    public float defaultTemperature = 25f;
    
    [Tooltip("Ambient temperature - motor cools down to this when stopped (°C)")]
    public float ambientTemperature = 25f;

    [Header("Temperature Zones")]
    [Tooltip("Below this temperature = COLD ZONE (blue colors)")]
    public float coldThreshold = -10f;
    
    [Tooltip("Above this temperature = HOT ZONE (red colors)")]
    public float hotThreshold = 50f;
    
    [Tooltip("CRITICAL - Emergency motor shutdown threshold (°C)")]
    public float overheatThreshold = 80f;

    [Header("Shaft Colors (Primary - Intense)")]
    [ColorUsage(true, true)]
    [Tooltip("Shaft color at extreme cold (-30°C)")]
    public Color shaftColdColor = new Color(0.1f, 0.3f, 1f);
    
    [ColorUsage(true, true)]
    [Tooltip("Shaft color at extreme heat (80°C)")]
    public Color shaftHotColor = new Color(1f, 0.2f, 0.2f);
    
    [ColorUsage(true, true)]
    [Tooltip("Shaft color in normal range (-10 to 50°C)")]
    public Color shaftNeutralColor = Color.white;
    
    [Range(0f, 1f)]
    [Tooltip("How strong the shaft color effect is (0 = no effect, 1 = full)")]
    public float shaftColorIntensity = 0.9f;

    [Header("Shell Colors (Secondary - Subtle)")]
    [ColorUsage(true, true)]
    [Tooltip("Shell color at extreme cold (-30°C)")]
    public Color shellColdColor = new Color(0.4f, 0.6f, 1f);
    
    [ColorUsage(true, true)]
    [Tooltip("Shell color at extreme heat (80°C)")]
    public Color shellHotColor = new Color(1f, 0.5f, 0.5f);
    
    [ColorUsage(true, true)]
    [Tooltip("Shell color in normal range (-10 to 50°C)")]
    public Color shellNeutralColor = Color.white;
    
    [Range(0f, 1f)]
    [Tooltip("How strong the shell color effect is (0 = no effect, 1 = full)")]
    public float shellColorIntensity = 0.4f;    [Header("RPM Based Auto Heating (Optional)")]
    [Tooltip("Enable automatic heat generation based on RPM (realistic thermal equilibrium)")]
    public bool enableRPMBasedHeating = false;
    
    [Tooltip("Maximum RPM for heat calculation (used to normalize RPM to 0-1 range)")]
    public float maxRPM = 3000f;
    
    [Tooltip("Maximum temperature increase above ambient due to motor operation (°C)")]
    public float maxHeatGeneration = 50f;
    
    [Tooltip("Safety margin below overheat threshold (prevents auto-overheat)")]
    public float safetyMargin = 5f;

    [Header("Overheat Protection")]
    [Tooltip("Enable emergency motor shutdown at overheat threshold")]
    public bool enableOverheatProtection = true;
    
    [Tooltip("Temperature must cool below this to restart motor after overheat")]
    public float overheatCooldownRequired = 55f;

    [Header("Visual Warnings")]
    [Tooltip("Enable visual warning indicators")]
    public bool enableVisualWarnings = true;
    
    [Tooltip("Start showing warnings at this temperature")]
    public float warningTemperature = 50f;
    
    [Tooltip("Critical warnings at this temperature")]
    public float criticalTemperature = 80f;

    // Private variables
    private Material shaftMaterial;
    private Material shellMaterial;
    private float currentTemperature;
    private float targetTemperature;
    private bool isOverheated = false;
    private bool wasMotorRunning = false;

    private void Start()
    {
        // Validate references
        if (motorController == null)
        {
            Debug.LogError("[TemperatureSystem] MotorController reference is missing! Please assign it in the Inspector.");
            return;
        }

        if (temperatureSlider == null)
        {
            Debug.LogError("[TemperatureSystem] Temperature Slider reference is missing! Please assign it in the Inspector.");
            return;
        }

        if (shaftRenderer == null)
        {
            Debug.LogWarning("[TemperatureSystem] Shaft Renderer is missing. Shaft colors won't be applied.");
        }
        else
        {
            // Create unique material instances for shaft
            shaftMaterial = new Material(shaftRenderer.sharedMaterial);
            shaftRenderer.material = shaftMaterial;
        }

        if (shellRenderer == null)
        {
            Debug.LogWarning("[TemperatureSystem] Shell Renderer is missing. Shell colors won't be applied.");
        }
        else
        {
            // Create unique material instances for shell
            shellMaterial = new Material(shellRenderer.sharedMaterial);
            shellRenderer.material = shellMaterial;
        }

        // Configure slider range
        temperatureSlider.minValue = minTemperature;
        temperatureSlider.maxValue = maxTemperature;
        temperatureSlider.value = defaultTemperature;

        // Initialize temperatures
        currentTemperature = defaultTemperature;
        targetTemperature = defaultTemperature;

        // Add listener for slider changes
        temperatureSlider.onValueChanged.AddListener(OnTemperatureSliderChanged);

        // Initialize display and colors
        UpdateTemperatureDisplay(currentTemperature);
        UpdateColors(currentTemperature);

        Debug.Log("[TemperatureSystem] Initialized successfully!");
    }

    private void Update()
    {
        if (motorController == null || temperatureSlider == null) 
            return;        // Detect when motor starts
        if (motorController.isMotorRunning && !wasMotorRunning)
        {
            // Motor just started - initialize to ambient temperature
            currentTemperature = ambientTemperature;
            temperatureSlider.value = ambientTemperature;
            targetTemperature = ambientTemperature;
            Debug.Log($"[TemperatureSystem] Motor started, temperature set to {currentTemperature:F1}°C");
        }

        // Handle motor state
        if (motorController.isMotorRunning)
        {
            // Motor is running - handle heating
            HandleMotorRunning();
        }
        else
        {
            // Motor is stopped - cool down to ambient
            HandleMotorStopped();
        }

        // Check for overheat condition
        CheckOverheat();

        // Update visuals
        UpdateColors(currentTemperature);
        UpdateTemperatureDisplay(currentTemperature);

        // Update motor controller's temperature state
        motorController.currentTemperature = currentTemperature;

        // Remember motor state for next frame
        wasMotorRunning = motorController.isMotorRunning;
    }    /// <summary>
    /// Handle temperature when motor is running
    /// </summary>
    private void HandleMotorRunning()
    {
        // Determine target temperature
        if (enableRPMBasedHeating)
        {
            // THERMAL EQUILIBRIUM APPROACH
            // Calculate equilibrium temperature based on current RPM
            float rpmNormalized = Mathf.Clamp01(motorController.currentRPM / maxRPM); // 0 to 1
            float heatGeneration = rpmNormalized * maxHeatGeneration; // 0 to maxHeatGeneration
            
            // Calculate equilibrium temperature
            float equilibriumTemp = ambientTemperature + heatGeneration;
            
            // Apply safety margin - never auto-heat beyond overheat threshold
            float maxSafeTemp = overheatThreshold - safetyMargin;
            equilibriumTemp = Mathf.Min(equilibriumTemp, maxSafeTemp);
            
            // Set target to equilibrium
            targetTemperature = equilibriumTemp;
            
            // Update slider to show current equilibrium
            temperatureSlider.value = targetTemperature;
            
            // Instant temperature = equilibrium (realistic stabilization)
            currentTemperature = targetTemperature;
        }
        else
        {
            // Manual control - slider sets target
            targetTemperature = temperatureSlider.value;
            currentTemperature = targetTemperature;
        }
    }    /// <summary>
    /// Handle temperature when motor is stopped - cool down to ambient
    /// </summary>
    private void HandleMotorStopped()
    {
        targetTemperature = ambientTemperature;
        currentTemperature = ambientTemperature;

        // Update slider to show current temperature
        temperatureSlider.value = currentTemperature;

        // Reset overheat flag when cooled down enough
        if (isOverheated && currentTemperature < overheatCooldownRequired)
        {
            isOverheated = false;
            Debug.Log($"[TemperatureSystem] Motor cooled down to {currentTemperature:F1}°C. Can restart now.");
        }
    }

    /// <summary>
    /// Check for overheat condition and trigger emergency stop
    /// </summary>
    private void CheckOverheat()
    {
        if (!enableOverheatProtection) return;

        if (currentTemperature >= overheatThreshold && !isOverheated)
        {
            // Overheat detected!
            isOverheated = true;
            Debug.LogWarning($"[TemperatureSystem] ⚠️ OVERHEAT! Temperature: {currentTemperature:F1}°C - EMERGENCY STOP!");
            
            // Trigger emergency stop
            if (motorController != null)
            {
                motorController.EmergencyStop();
            }
        }

        // Prevent motor restart if still overheated
        if (isOverheated && currentTemperature >= overheatCooldownRequired)
        {
            if (motorController != null && motorController.isMotorRunning)
            {
                motorController.EmergencyStop();
                Debug.LogWarning($"[TemperatureSystem] Motor still too hot ({currentTemperature:F1}°C). Cannot run until below {overheatCooldownRequired}°C");
            }
        }
    }

    /// <summary>
    /// Update material colors based on current temperature
    /// </summary>
    private void UpdateColors(float temperature)
    {
        // Update shaft colors (intense)
        if (shaftMaterial != null)
        {
            Color shaftColor = CalculateTemperatureColor(
                temperature, 
                shaftColdColor, 
                shaftHotColor, 
                shaftNeutralColor, 
                shaftColorIntensity
            );
            shaftMaterial.color = shaftColor;
        }

        // Update shell colors (subtle)
        if (shellMaterial != null)
        {
            Color shellColor = CalculateTemperatureColor(
                temperature, 
                shellColdColor, 
                shellHotColor, 
                shellNeutralColor, 
                shellColorIntensity
            );
            shellMaterial.color = shellColor;
        }
    }

    /// <summary>
    /// Calculate color based on temperature zones
    /// </summary>
    private Color CalculateTemperatureColor(float temp, Color coldColor, Color hotColor, Color neutralColor, float intensity)
    {
        Color resultColor = neutralColor;

        if (temp < coldThreshold)
        {
            // COLD ZONE: Blend from neutral to cold color
            float coldBlend = Mathf.InverseLerp(coldThreshold, minTemperature, temp);
            resultColor = Color.Lerp(neutralColor, coldColor, coldBlend * intensity);
        }
        else if (temp > hotThreshold)
        {
            // HOT ZONE: Blend from neutral to hot color
            float heatBlend = Mathf.InverseLerp(hotThreshold, maxTemperature, temp);
            resultColor = Color.Lerp(neutralColor, hotColor, heatBlend * intensity);
        }
        else
        {
            // NORMAL ZONE: Neutral color
            resultColor = neutralColor;
        }

        return resultColor;
    }

    /// <summary>
    /// Called when the slider value changes
    /// </summary>
    private void OnTemperatureSliderChanged(float value)
    {
        if (motorController != null)
        {
            // Only allow manual changes if not using RPM-based heating
            if (!enableRPMBasedHeating)
            {
                Debug.Log($"[TemperatureSystem] Temperature slider changed to: {value:F1}°C");
            }
        }
    }    /// <summary>
    /// Updates the temperature text display with color coding
    /// </summary>
    private void UpdateTemperatureDisplay(float temp)
    {
        if (temperatureText != null)
        {
            // Format text
            string displayText = $"Temperature: {temp:F1}°C";

            // Add warning indicators if enabled
            if (enableVisualWarnings)
            {
                if (temp >= criticalTemperature)
                {
                    displayText = "[CRITICAL] " + displayText;
                    temperatureText.color = Color.red;
                }
                else if (temp >= warningTemperature)
                {
                    displayText = "[WARNING] " + displayText;
                    temperatureText.color = new Color(1f, 0.5f, 0f); // Orange
                }
                else if (temp <= coldThreshold)
                {
                    displayText = "[COLD] " + displayText;
                    temperatureText.color = new Color(0.5f, 0.7f, 1f); // Light blue
                }
                else
                {
                    temperatureText.color = Color.white;
                }
            }

            temperatureText.text = displayText;
        }
    }

    /// <summary>
    /// Reset temperature to default value
    /// </summary>
    public void ResetTemperature()
    {
        if (temperatureSlider != null)
        {
            temperatureSlider.value = defaultTemperature;
            currentTemperature = defaultTemperature;
            targetTemperature = defaultTemperature;
        }
    }    /// <summary>
    /// Set temperature to a specific value programmatically
    /// </summary>
    public void SetTemperature(float targetTemp)
    {
        if (temperatureSlider != null)
        {
            temperatureSlider.value = Mathf.Clamp(targetTemp, minTemperature, maxTemperature);
            currentTemperature = temperatureSlider.value;
        }
    }

    /// <summary>
    /// Get current temperature
    /// </summary>
    public float GetCurrentTemperature()
    {
        return currentTemperature;
    }

    /// <summary>
    /// Check if motor is currently overheated
    /// </summary>
    public bool IsOverheated()
    {
        return isOverheated;
    }

    private void OnDestroy()
    {
        // Clean up listener
        if (temperatureSlider != null)
        {
            temperatureSlider.onValueChanged.RemoveListener(OnTemperatureSliderChanged);
        }

        // Clean up materials
        if (shaftMaterial != null)
        {
            Destroy(shaftMaterial);
        }

        if (shellMaterial != null)
        {
            Destroy(shellMaterial);
        }
    }
}
