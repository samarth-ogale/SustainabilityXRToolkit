using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using SustainabilityXRToolkit.Data;

namespace SustainabilityXRToolkit.Import
{
    /// <summary>
    /// Loads sustainability datasets from simple CSV files.
    /// Expected columns (with header row): Label,Category,Value,Unit,Date
    ///
    /// This is intentionally a minimal parser (not full RFC-4180) — it's built
    /// for small, clean sustainability datasets rather than arbitrary CSV input.
    /// </summary>
    public static class CsvSustainabilityImporter
    {
        public static List<SustainabilityDataPoint> LoadFromCsv(string filePath)
        {
            var points = new List<SustainabilityDataPoint>();

            if (!File.Exists(filePath))
            {
                Debug.LogError($"[CsvSustainabilityImporter] File not found: {filePath}");
                return points;
            }

            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length < 2)
            {
                Debug.LogWarning("[CsvSustainabilityImporter] CSV has no data rows.");
                return points;
            }

            // Row 0 is assumed to be the header (Label,Category,Value,Unit,Date) and is skipped.
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] fields = ParseCsvLine(line);
                if (fields.Length < 5)
                {
                    Debug.LogWarning($"[CsvSustainabilityImporter] Skipping malformed row {i + 1}: {line}");
                    continue;
                }

                if (!float.TryParse(fields[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                {
                    Debug.LogWarning($"[CsvSustainabilityImporter] Could not parse value on row {i + 1}: '{fields[2]}'");
                    continue;
                }

                points.Add(new SustainabilityDataPoint(
                    label: fields[0],
                    category: fields[1],
                    value: value,
                    unit: fields[3],
                    date: fields[4]
                ));
            }

            return points;
        }

        /// <summary>
        /// Splits a single CSV line into fields, respecting simple double-quoted
        /// fields that may contain commas (e.g., "Solar, Residential").
        /// </summary>
        private static string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            bool inQuotes = false;
            var current = new StringBuilder();

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            fields.Add(current.ToString().Trim());
            return fields.ToArray();
        }
    }
}
