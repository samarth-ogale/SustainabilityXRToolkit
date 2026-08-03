namespace SustainabilityXRToolkit.Data
{
    /// <summary>
    /// A single row of sustainability data (e.g., one line from a GHG emissions
    /// or water-usage CSV) ready to be rendered in a 3D/AR scene.
    /// </summary>
    [System.Serializable]
    public struct SustainabilityDataPoint
    {
        public string Label;
        public string Category;
        public float Value;
        public string Unit;
        public string Date;

        public SustainabilityDataPoint(string label, string category, float value, string unit, string date)
        {
            Label = label;
            Category = category;
            Value = value;
            Unit = unit;
            Date = date;
        }

        public override string ToString()
        {
            return $"{Label} ({Category}): {Value} {Unit} [{Date}]";
        }
    }
}
