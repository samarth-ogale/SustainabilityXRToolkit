using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SustainabilityXRToolkit.Data;
using SustainabilityXRToolkit.Import;

namespace SustainabilityXRToolkit.Visualization
{
    /// <summary>
    /// Spawns a simple animated 3D bar-chart visualization for a sustainability dataset.
    ///
    /// Usage:
    ///   1. Attach this component to an empty GameObject in a scene.
    ///   2. Either assign a SustainabilityDataset asset, OR leave it empty and
    ///      set CsvFilePath to a CSV under Assets/ (e.g. via StreamingAssets).
    ///   3. Press Play — bars grow up out of the ground, one per data row.
    ///
    /// This is deliberately simple (primitive cubes by default) so it's easy to
    /// read and extend — swap in a custom prefab, add AR placement, etc.
    /// </summary>
    public class SustainabilityVisualizer : MonoBehaviour
    {
        [Header("Data Source")]
        [Tooltip("Optional pre-built dataset asset. If left empty, CsvFilePath is used instead.")]
        public SustainabilityDataset Dataset;

        [Tooltip("Path to a CSV file, relative to Application.dataPath. Used only if Dataset is not assigned.")]
        public string CsvFilePath = "StreamingAssets/sample_sustainability_data.csv";

        [Header("Visualization Settings")]
        [Tooltip("Optional custom prefab per data point. If left empty, a primitive cube is used.")]
        public GameObject DataPointPrefab;
        public float Spacing = 1.5f;
        public float HeightScale = 0.5f;
        public float GrowDuration = 0.6f;

        private readonly List<GameObject> _spawnedVisuals = new List<GameObject>();

        private void Start()
        {
            List<SustainabilityDataPoint> points = Dataset != null
                ? Dataset.DataPoints
                : CsvSustainabilityImporter.LoadFromCsv(Path.Combine(Application.dataPath, CsvFilePath));

            if (points == null || points.Count == 0)
            {
                Debug.LogWarning("[SustainabilityVisualizer] No data points to visualize. " +
                                 "Assign a Dataset or check CsvFilePath.");
                return;
            }

            SpawnVisualization(points);
        }

        private void SpawnVisualization(List<SustainabilityDataPoint> points)
        {
            float maxValue = 0f;
            foreach (var p in points)
            {
                if (p.Value > maxValue) maxValue = p.Value;
            }
            if (maxValue <= 0f) maxValue = 1f;

            for (int i = 0; i < points.Count; i++)
            {
                SustainabilityDataPoint point = points[i];
                Vector3 basePosition = transform.position + new Vector3(i * Spacing, 0f, 0f);

                GameObject visual = DataPointPrefab != null
                    ? Instantiate(DataPointPrefab, basePosition, Quaternion.identity, transform)
                    : GameObject.CreatePrimitive(PrimitiveType.Cube);

                visual.transform.SetParent(transform);
                visual.transform.position = basePosition;
                visual.name = $"DataPoint_{point.Label}";

                DataPointVisual dataVisual = visual.GetComponent<DataPointVisual>();
                if (dataVisual == null) dataVisual = visual.AddComponent<DataPointVisual>();
                dataVisual.Initialize(point);

                float targetHeight = Mathf.Max(0.05f, (point.Value / maxValue) * HeightScale * 10f);
                StartCoroutine(GrowBar(visual.transform, targetHeight, GrowDuration));

                _spawnedVisuals.Add(visual);
            }
        }

        private IEnumerator GrowBar(Transform t, float targetHeight, float duration)
        {
            Vector3 startScale = new Vector3(t.localScale.x, 0.01f, t.localScale.z);
            Vector3 endScale = new Vector3(t.localScale.x, targetHeight, t.localScale.z);
            Vector3 startPos = t.position;

            t.localScale = startScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float pct = Mathf.Clamp01(elapsed / duration);
                t.localScale = Vector3.Lerp(startScale, endScale, pct);
                t.position = new Vector3(startPos.x, endScale.y * 0.5f * pct, startPos.z);
                yield return null;
            }

            t.localScale = endScale;
            t.position = new Vector3(startPos.x, endScale.y * 0.5f, startPos.z);
        }

        /// <summary>Removes all currently spawned visuals (e.g., before loading a new dataset).</summary>
        public void ClearVisualization()
        {
            foreach (GameObject v in _spawnedVisuals)
            {
                if (v != null) Destroy(v);
            }
            _spawnedVisuals.Clear();
        }
    }
}
