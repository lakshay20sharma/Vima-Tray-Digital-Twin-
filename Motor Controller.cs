using UnityEngine;
using System.Collections;

public class MotorController : MonoBehaviour
{
    [Header("Motor Default Settings")]
    public float defaultRPM = 1780f;
    public float defaultTemperature = 20f;
    public float defaultVibration = 0f;

    [Header("Current Motor State")]
    public float currentRPM = 0f;
    public float currentTemperature = 25f;
    public float currentVibration = 0f;
    public bool isMotorRunning = false;

    [Header("Visual Feedback (Glow Objects)")]
    public GlowLightController startLight;
    public GlowLightController stopLight;
    public GlowLightController emergencyLight;

    [Header("Motor Components")]
    public Transform motorShaft;

    [Header("Graceful Stop Settings")]
    public float rpmDecaySpeed = 500f;
    public float tempLerpSpeed = 2f;

    [HideInInspector]
    public bool isStopping = false;

    private Coroutine emergencyRoutine;

    void Update()
    {
        if ((isMotorRunning || isStopping) && motorShaft != null)
        {
            float rotationSpeed = currentRPM * 6f;
            motorShaft.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }

        if (isStopping)
        {
            currentRPM = Mathf.MoveTowards(currentRPM, 0f, rpmDecaySpeed * Time.deltaTime);
            currentTemperature = Mathf.MoveTowards(currentTemperature, defaultTemperature, tempLerpSpeed * Time.deltaTime);

            if (currentRPM < 1f)
                currentRPM = 0f;

            if (currentRPM == 0f &&
                Mathf.Abs(currentTemperature - defaultTemperature) < 0.1f)
            {
                isStopping = false;
                isMotorRunning = false;
                Debug.Log("Motor fully stopped gracefully.");
            }
        }
    }

    public void StartMotor()
    {
        Debug.Log("Motor starting via button!");

        currentRPM = defaultRPM;
        currentTemperature = defaultTemperature;
        currentVibration = defaultVibration;

        isMotorRunning = true;
        isStopping = false;

        SetAllLightsOff();
        if (startLight != null) startLight.SetGlow(true);
    }

    public void StopMotor()
    {
        if (isStopping) return;

        if (!isMotorRunning && currentRPM <= 0.1f) return;

        Debug.Log("Motor stopping gracefully...");
        isStopping = true;

        SetAllLightsOff();
        if (stopLight != null) stopLight.SetGlow(true);
    }

    public void EmergencyStop()
    {
        Debug.LogWarning("EMERGENCY STOP ACTIVATED!");

        isStopping = false;

        currentRPM = 0f;
        currentTemperature = defaultTemperature;
        currentVibration = defaultVibration;
        isMotorRunning = false;

        SetAllLightsOff();

        if (emergencyRoutine != null) StopCoroutine(emergencyRoutine);
        emergencyRoutine = StartCoroutine(EmergencyGlowPulse());
    }

    private void SetAllLightsOff()
    {
        if (startLight != null) startLight.SetGlow(false);
        if (stopLight != null) stopLight.SetGlow(false);
        if (emergencyLight != null) emergencyLight.SetGlow(false);
    }

    private IEnumerator EmergencyGlowPulse()
    {
        if (emergencyLight != null)
        {
            emergencyLight.SetCustomGlow(Color.red, 3f);
            yield return new WaitForSeconds(3f);
            emergencyLight.SetGlow(false);
        }
    }
}
