using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using SustainabilityXRToolkit.Data;
using SustainabilityXRToolkit.Import;

namespace SustainabilityXRToolkit.Visualization
{
    /// <summary>
    /// Spawns a simple animated 3D bar-chart visualization for a sustainability dataset,
    /// grouped and color-coded by category (e.g., Energy / Water / Waste), with
    /// floating category labels and per-bar value labels.
    ///
    /// Usage:
    ///   1. Attach this component to an empty GameObject in a scene.
    ///   2. Either assign a SustainabilityDataset asset, OR leave it empty and
    ///      set CsvFilePath to a CSV under Assets/StreamingAssets.
    ///   3. Press Play — bars grow up out of the ground, clustered by category,
    ///      each showing its own value once it finishes growing.
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
        public float ClusterGap = 2.5f;
        public float HeightScale = 0.5f;
        public float GrowDuration = 0.6f;
        public bool ShowCategoryLabels = true;
        public bool ShowValueLabels = true;

        private readonly List<GameObject> _spawnedVisuals = new List<GameObject>();

        // Simple, deterministic color palette by category. Falls back to grey for
        // any category not explicitly listed here, so new categories never break.
        private static readonly Dictionary<string, Color> CategoryColors = new Dictionary<string, Color>
        {
            { "Waste", new Color(0.58f, 0.38f, 0.72f) },       // purple
            { "Water", new Color(0.25f, 0.50f, 0.85f) },       // blue
            { "Energy", new Color(0.95f, 0.62f, 0.15f) },      // orange
        };

        private static Color GetCategoryColor(string category)
        {
            return CategoryColors.TryGetValue(category, out Color color) ? color : Color.gray;
        }

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

            // Group by category, preserving first-seen order so the layout is stable
            // and matches the order categories appear in the source CSV.
            var groups = points
                .GroupBy(p => p.Category)
                .OrderBy(g => points.FindIndex(p => p.Category == g.Key));

            float cursorX = 0f;

            foreach (var group in groups)
            {
                float clusterStartX = cursorX;

                foreach (SustainabilityDataPoint point in group)
                {
                    Vector3 basePosition = transform.position + new Vector3(cursorX, 0f, 0f);

                    GameObject visual = DataPointPrefab != null
                        ? Instantiate(DataPointPrefab, basePosition, Quaternion.identity, transform)
                        : GameObject.CreatePrimitive(PrimitiveType.Cube);

                    visual.transform.SetParent(transform);
                    visual.transform.position = basePosition;
                    visual.name = $"DataPoint_{point.Label}";

                    // Give each bar its own material instance so color changes don't
                    // affect other bars sharing the same default primitive material.
                    Renderer renderer = visual.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = GetCategoryColor(point.Category);
                    }

                    DataPointVisual dataVisual = visual.GetComponent<DataPointVisual>();
                    if (dataVisual == null) dataVisual = visual.AddComponent<DataPointVisual>();
                    dataVisual.Initialize(point);

                    float targetHeight = Mathf.Max(0.05f, (point.Value / maxValue) * HeightScale * 10f);
                    StartCoroutine(GrowBar(visual.transform, targetHeight, GrowDuration, point));

                    _spawnedVisuals.Add(visual);
                    cursorX += Spacing;
                }

                if (ShowCategoryLabels)
                {
                    float clusterCenterX = (clusterStartX + (cursorX - Spacing)) * 0.5f;
                    CreateCategoryLabel(group.Key, clusterCenterX);
                }

                cursorX += ClusterGap;
            }
        }

        /// <summary>
        /// Creates a simple floating 3D text label above a category cluster using
        /// Unity's built-in TextMesh (no external package required, unlike TextMeshPro).
        /// </summary>
        private void CreateCategoryLabel(string category, float centerX)
        {
            GameObject labelObject = new GameObject($"Label_{category}");
            labelObject.transform.SetParent(transform);
            labelObject.transform.position = transform.position + new Vector3(centerX, 3.5f, 0f);
            labelObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // face default camera direction

            TextMesh textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.text = category;
            textMesh.fontSize = 48;
            textMesh.characterSize = 0.15f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = GetCategoryColor(category);

            _spawnedVisuals.Add(labelObject);
        }

        /// <summary>
        /// Creates a small floating value label (e.g. "620.4 MWh") just above a
        /// finished bar, using the same built-in TextMesh approach as category labels.
        /// </summary>
        private void CreateValueLabel(SustainabilityDataPoint point, Vector3 barTopPosition)
        {
            GameObject labelObject = new GameObject($"ValueLabel_{point.Label}");
            labelObject.transform.SetParent(transform);
            labelObject.transform.position = barTopPosition + new Vector3(0f, 0.3f, 0f);
            labelObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            TextMesh textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.text = $"{point.Value:0.#} {point.Unit}";
            textMesh.fontSize = 32;
            textMesh.characterSize = 0.09f;
            textMesh.anchor = TextAnchor.LowerCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.black;

            _spawnedVisuals.Add(labelObject);
        }

        private IEnumerator GrowBar(Transform t, float targetHeight, float duration, SustainabilityDataPoint point)
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
            Vector3 finalPos = new Vector3(startPos.x, endScale.y * 0.5f, startPos.z);
            t.position = finalPos;

            if (ShowValueLabels)
            {
                Vector3 barTop = new Vector3(startPos.x, endScale.y, startPos.z);
                CreateValueLabel(point, barTop);
            }
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