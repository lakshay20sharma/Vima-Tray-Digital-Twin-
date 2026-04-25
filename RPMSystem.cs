using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RPMSystem : MonoBehaviour
{
    [Header("References")]
    public MotorController motorController;

    [Header("UI Elements")]
    public Slider rpmSlider;
    public TextMeshProUGUI rpmText;

    [Header("RPM Settings")]
    public float minRPM = 0f;
    public float maxRPM = 3000f;
    public float defaultRPM = 1000f;

    private void Start()
    {
        if (motorController == null)
        {
            Debug.LogError("[RPMSystem] MotorController missing!");
            return;
        }

        rpmSlider.minValue = minRPM;
        rpmSlider.maxValue = maxRPM;
        rpmSlider.value = defaultRPM;

        rpmSlider.onValueChanged.AddListener(OnRPMSliderChanged);

        UpdateRPMDisplay(rpmSlider.value);
    }

    private bool wasMotorRunning = false;

    private void Update()
    {
        if (motorController == null || rpmSlider == null) return;

        // Detect motor start
        if (motorController.isMotorRunning && !wasMotorRunning)
        {
            rpmSlider.value = motorController.currentRPM;
            Debug.Log($"[RPMSystem] Motor started -> slider {motorController.currentRPM}");
        }

        // -------------------------------
        // FIX: Do NOT override RPM if stopping
        // -------------------------------
        if (motorController.isMotorRunning && !motorController.isStopping)
        {
            // Allow user control only when running normally
            motorController.currentRPM = rpmSlider.value;
        }
        else if (!motorController.isMotorRunning)
        {
            // Motor stopped completely
            motorController.currentRPM = 0f;
            rpmSlider.value = 0f;
        }
        else if (motorController.isStopping)
        {
            // FOLLOW the motor's graceful decay visually
            rpmSlider.value = motorController.currentRPM;
        }

        UpdateRPMDisplay(motorController.currentRPM);

        wasMotorRunning = motorController.isMotorRunning;
    }

    private void OnRPMSliderChanged(float value)
    {
        if (motorController != null && motorController.isMotorRunning && !motorController.isStopping)
        {
            motorController.currentRPM = value;
            UpdateRPMDisplay(value);
            Debug.Log($"[RPMSystem] RPM changed to: {value:F1}");
        }
    }

    private void UpdateRPMDisplay(float rpm)
    {
        if (rpmText != null)
            rpmText.text = $"RPM: {rpm:F0}";
    }

    public void ResetRPM()
    {
        if (rpmSlider != null)
            rpmSlider.value = defaultRPM;
    }

    public void SetRPM(float targetRPM)
    {
        if (rpmSlider != null)
            rpmSlider.value = Mathf.Clamp(targetRPM, minRPM, maxRPM);
    }

    private void OnDestroy()
    {
        if (rpmSlider != null)
            rpmSlider.onValueChanged.RemoveListener(OnRPMSliderChanged);
    }
}
