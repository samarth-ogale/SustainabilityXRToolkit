# Basic Demo — Setup (5–10 minutes)

This package doesn't ship a pre-built `.unity` scene file (those are fiddly to hand-author
correctly outside the Unity Editor). Instead, here's exactly what to click — it's fast.

## 1. Create a new Unity project
- Unity Hub → New Project → **3D (URP or Built-In, either is fine)**
- Unity 2021.3 LTS or newer recommended

## 2. Install this package
- Copy the whole `SustainabilityXRToolkit` folder into your project's `Packages/` directory
  **OR**
- Window → Package Manager → `+` → **Add package from disk...** → select this package's `package.json`

## 3. Add the sample data
- Create a `StreamingAssets` folder under `Assets/` if it doesn't exist
- Copy `SampleData/sample_sustainability_data.csv` into `Assets/StreamingAssets/`

## 4. Build the demo scene
1. File → New Scene (or use the default `SampleScene`)
2. Hierarchy → right-click → Create Empty, rename it `Visualizer`
3. With `Visualizer` selected, Inspector → Add Component → **Sustainability Visualizer**
4. Leave `Dataset` empty, set `Csv File Path` to `StreamingAssets/sample_sustainability_data.csv`
   (this is the default value, so you likely don't need to change anything)
5. Press **Play**

You should see 7 cubes grow up out of the ground, one per row in the sample CSV, each
scaled to that row's value relative to the largest value in the dataset. Click any cube
in the Game view to log its underlying data point to the Console.

## 5. Next steps to extend this
- Swap `DataPointPrefab` for a custom prefab (e.g., a labeled bar with TextMeshPro)
- Replace the flat-line layout in `SustainabilityVisualizer.SpawnVisualization` with a
  grouped-by-category layout (e.g., clusters for Agriculture / Water / Energy)
- Add AR Foundation plane detection so the visualization places itself on a real
  detected surface instead of at a fixed world position
- Swap the sample CSV for real data (e.g., from the Climate Smart-AG project)
