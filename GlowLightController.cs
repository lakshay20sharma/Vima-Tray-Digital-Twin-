using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class GlowLightController : MonoBehaviour
{
    private Material mat;
    private Renderer rend;

    [Header("Colors")]
    public Color offColor = Color.red;        // default for STOP button
    public Color onColor = Color.green;       // default for ON state

    [Header("Brightness")]
    public float offIntensity = 0.2f;
    public float onIntensity = 3f;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        // Clone material so each glow object can change emission independently
        mat = rend.material;
        SetGlow(false); // default OFF
    }

    /// <summary>
    /// Standard on/off glow using predefined colors/intensity.
    /// </summary>
    public void SetGlow(bool state)
    {
        Color targetColor = state ? onColor : offColor;
        float intensity = state ? onIntensity : offIntensity;
        ApplyEmission(targetColor, intensity);
    }

    /// <summary>
    /// Allows a one-off custom glow (e.g. emergency flash) without
    /// permanently altering normal on/off settings.
    /// </summary>
    public void SetCustomGlow(Color color, float intensity)
    {
        ApplyEmission(color, intensity);
    }

    private void ApplyEmission(Color baseColor, float intensity)
    {
        Color final = baseColor * intensity;
        mat.SetColor("_EmissionColor", final);
        mat.EnableKeyword("_EMISSION");

        // Needed for real-time emission updates in Unity lighting
        DynamicGI.SetEmissive(rend, final);
    }
}
