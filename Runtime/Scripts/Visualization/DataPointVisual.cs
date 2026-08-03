using UnityEngine;
using SustainabilityXRToolkit.Data;

namespace SustainabilityXRToolkit.Visualization
{
    /// <summary>
    /// Attached automatically to each spawned data-point visual by SustainabilityVisualizer.
    /// Holds the underlying data so it can be inspected or reacted to (e.g., tap in AR/VR
    /// to show a label with the real value).
    /// </summary>
    public class DataPointVisual : MonoBehaviour
    {
        public SustainabilityDataPoint Data { get; private set; }

        public void Initialize(SustainabilityDataPoint data)
        {
            Data = data;
        }

        // Basic desktop-testing interaction. In an AR build this would be replaced
        // with a raycast-based tap handler (e.g., via AR Foundation's input system).
        private void OnMouseDown()
        {
            Debug.Log($"[SustainabilityXRToolkit] {Data}");
        }
    }
}
