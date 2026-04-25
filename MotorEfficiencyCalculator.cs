using UnityEngine;
using TMPro;

/// <summary>
/// Physically accurate induction motor efficiency calculator
/// Implements IEEE 112 / IEC 60034-2-1 loss segregation
/// </summary>
public class MotorEfficiencyCalculator : MonoBehaviour
{
    [Header("Operating Parameters")]
    [Range(-30f, 150f)] 
    public float temperature = 25f;            // °C
    
    [Range(0f, 3600f)]
    public float rpm = 1780f;                  // actual mechanical speed
    
    [Range(0f, 10f)]
    public float vibration = 0f;               // mm amplitude (RMS)

    [Header("Motor Nameplate Data")]
    public float ratedPower = 3000f;           // mechanical output (W)
    public float supplyFreq = 50f;             // Hz
    public int poleCount = 4;                  // poles
    public float ratedCurrent = 4.5f;          // A per phase

    [Header("Motor Parameters at 20°C")]
    public float statorResistance20 = 2.1f;    // ohms
    public float rotorResistance20 = 1.8f;     // ohms

    [Header("Fixed Loss Components")]
    public float coreLoss = 120f;              // W (constant)

    [Header("Mechanical Loss Coefficients")]
    public float frictionCoeff = 0.015f;       // W/rpm
    public float windageCoeff = 1e-7f;         // W/(rad/s)^3

    [Header("Vibration Damping")]
    public float dampingCoeff = 0.25f;         // N·s/m

    [Header("Physical Constants")]
    public float tempCoeffCopper = 0.00393f;   // 1/°C
    public float refTemp = 20f;                // °C

    [Header("UI Display")]
    public TextMeshProUGUI efficiencyText;
    public TextMeshProUGUI detailsText;

    private float efficiency = 0f;
    private MotorLossBreakdown lossBreakdown;

    void Update()
    {
        lossBreakdown = ComputeEfficiencyWithBreakdown(temperature, rpm, vibration);
        efficiency = lossBreakdown.efficiency;
        UpdateUI();
    }

    /// <summary>
    /// Computes efficiency using IEEE 112 loss components
    /// </summary>
    private MotorLossBreakdown ComputeEfficiencyWithBreakdown(float T, float n, float vib_mm)
    {
        var L = new MotorLossBreakdown();

        // === 1. TEMPERATURE-DEPENDENT RESISTANCE ===
        float Rs_T = statorResistance20 * (1f + tempCoeffCopper * (T - refTemp));
        float Rr_T = rotorResistance20 * (1f + tempCoeffCopper * (T - refTemp));

        // === 2. STATOR COPPER LOSS (3-phase) ===
        L.statorCopperLoss = 3f * ratedCurrent * ratedCurrent * Rs_T;

        // === 3. SYNCHRONOUS SPEED & SLIP ===
        float n_sync = (120f * supplyFreq) / poleCount;
        float slip = Mathf.Clamp01((n_sync - n) / (n_sync + 1e-6f));

        // === 4. MECHANICAL SPEED (rad/s) ===
        float omega = (2f * Mathf.PI * n) / 60f;

        // === 5. MECHANICAL LOSSES ===
        // Friction loss (linear with rpm)
        L.frictionLoss = frictionCoeff * n;

        // Windage loss (cubic with speed)
        L.windageLoss = windageCoeff * Mathf.Pow(omega, 3);

        // Vibration loss (damped vibration physics)
        float vib_m = vib_mm / 1000f;
        L.vibrationLoss = 0.5f * dampingCoeff * omega * omega * vib_m * vib_m;

        // === 6. CORE LOSS ===
        L.coreLoss = coreLoss;

        // === 7. MECHANICAL DEVELOPED POWER ===
        // IEEE correct definition:
        // P_mech_dev = P_out + mechanical losses (not copper losses)
        float P_mech_dev =
            ratedPower +
            L.frictionLoss +
            L.windageLoss +
            L.vibrationLoss;

        // === 8. ROTOR COPPER LOSS (IEEE CORRECT FORMULA) ===
        // P_rotor_cu = (s / (1 - s)) * P_mech_dev
        L.rotorCopperLoss = (slip / (1f - slip + 1e-6f)) * P_mech_dev;

        // === 9. TOTAL LOSS ===
        L.totalLoss =
            L.statorCopperLoss +
            L.rotorCopperLoss +
            L.coreLoss +
            L.frictionLoss +
            L.windageLoss +
            L.vibrationLoss;

        // === 10. EFFICIENCY ===
        float P_in = ratedPower + L.totalLoss;
        L.efficiency = Mathf.Clamp01(ratedPower / P_in);

        return L;
    }

    void UpdateUI()
    {
        if (efficiencyText != null)
            efficiencyText.text = $"Efficiency: {(efficiency * 100f):F2}%";

        if (detailsText != null && lossBreakdown != null)
        {
            detailsText.text =
$@"Loss Breakdown:
Stator Cu:   {lossBreakdown.statorCopperLoss:F1} W
Rotor Cu:    {lossBreakdown.rotorCopperLoss:F1} W
Core:        {lossBreakdown.coreLoss:F1} W
Friction:    {lossBreakdown.frictionLoss:F1} W
Windage:     {lossBreakdown.windageLoss:F1} W
Vibration:   {lossBreakdown.vibrationLoss:F1} W
-------------------------
Total Loss:  {lossBreakdown.totalLoss:F1} W";
        }
    }

    public float GetEfficiency() => efficiency;
    public MotorLossBreakdown GetLossBreakdown() => lossBreakdown;
}

/// <summary>
/// Container for detailed loss components
/// </summary>
[System.Serializable]
public class MotorLossBreakdown
{
    public float statorCopperLoss;
    public float rotorCopperLoss;
    public float coreLoss;
    public float frictionLoss;
    public float windageLoss;
    public float vibrationLoss;
    public float totalLoss;
    public float efficiency;
}
