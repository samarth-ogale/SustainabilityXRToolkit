using System.Collections.Generic;
using UnityEngine;

namespace SustainabilityXRToolkit.Data
{
    /// <summary>
    /// A reusable, inspector-friendly container for a set of sustainability data points.
    /// Can be created as an asset (Assets > Create > Sustainability XR Toolkit > Dataset)
    /// so designers/researchers can assign pre-imported data without touching code.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSustainabilityDataset", menuName = "Sustainability XR Toolkit/Dataset")]
    public class SustainabilityDataset : ScriptableObject
    {
        public string DatasetName;

        [TextArea(1, 3)]
        public string Description;

        public List<SustainabilityDataPoint> DataPoints = new List<SustainabilityDataPoint>();

        public float GetMaxValue()
        {
            float max = 0f;
            foreach (var point in DataPoints)
            {
                if (point.Value > max) max = point.Value;
            }
            return max;
        }

        public IEnumerable<SustainabilityDataPoint> GetByCategory(string category)
        {
            foreach (var point in DataPoints)
            {
                if (point.Category == category) yield return point;
            }
        }

        public void LoadFrom(List<SustainabilityDataPoint> points)
        {
            DataPoints = points;
        }
    }
}
