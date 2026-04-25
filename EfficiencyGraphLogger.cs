using UnityEngine;
using XCharts.Runtime;

public class EfficiencyGraphLogger : MonoBehaviour
{
    [Header("References")]
    public MotorEfficiencyCalculator motor;   // Drag MotorEfficiencyCalculator object
    public LineChart chart;                   // Drag LineChart here

    [Header("Graph Settings")]
    public string seriesName = "Efficiency";
    public int maxPoints = 100;
    public float sampleInterval = 0.1f;

    private float timer = 0f;
    private int index = 0;

    void Start()
    {
        if (chart == null)
        {
            Debug.LogError("EfficiencyGraphLogger: LineChart reference missing.");
            enabled = false;
            return;
        }

        // Reset any previous data
        chart.RemoveData();

        // Add a Line series
        chart.AddSerie<Line>(seriesName);

        // Setup Axis
        var xAxis = chart.EnsureChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Category;

        var yAxis = chart.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;

        chart.RefreshChart();
    }

    void Update()
    {
        if (motor == null || chart == null) return;

        timer += Time.deltaTime;
        if (timer < sampleInterval) return;
        timer = 0f;

        float eff = motor.GetEfficiency() * 100f; // convert 0–1 to %

        // Add X-axis label
        var xAxis = chart.GetChartComponent<XAxis>();
        xAxis.data.Add("t" + index);

        // Add Y data
        chart.AddData(0, eff);
        index++;

        // Get series using the correct API
        var serie = chart.series[0];   // <<< THIS IS CORRECT FOR YOUR VERSION

        // Rolling buffer
        while (serie.data.Count > maxPoints)
        {
            // remove y
            serie.data.RemoveAt(0);

            // remove x
            if (xAxis.data.Count > 0)
                xAxis.data.RemoveAt(0);
        }

        chart.RefreshChart();
    }
}
