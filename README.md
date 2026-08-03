# Sustainability XR Toolkit

A lightweight Unity/C# SDK for importing real-world sustainability datasets (GHG emissions,
water usage, energy generation, etc.) and rendering them as interactive 3D/AR data
visualizations.

**Status: early / in progress.** This is a personal project extending prior data-science
work (see [Climate Smart-AG](#background)) into XR. Core CSV import + a basic 3D bar-chart
visualizer are working; AR placement and richer visuals are next.

## Why this exists

I've spent most of my recent work on the data side of sustainability problems — GHG
emissions modeling, environmental data pipelines. This project is about closing the loop:
taking that kind of real-world data and making it something you can actually walk around
and look at in 3D/AR, rather than just a chart in a notebook.

## Architecture

```
SustainabilityXRToolkit/
├── package.json                    # Unity Package Manager manifest
├── Runtime/
│   └── Scripts/
│       ├── Data/
│       │   ├── SustainabilityDataPoint.cs   # One row of data (label, category, value, unit, date)
│       │   └── SustainabilityDataset.cs     # ScriptableObject container for a full dataset
│       ├── Import/
│       │   └── CsvSustainabilityImporter.cs # Parses CSV files into SustainabilityDataPoint lists
│       └── Visualization/
│           ├── SustainabilityVisualizer.cs  # MonoBehaviour: spawns + animates a 3D bar chart
│           └── DataPointVisual.cs           # Attached to each spawned visual; holds its data
├── Tests/Editor/
│   └── CsvSustainabilityImporterTests.cs    # NUnit tests for the CSV parser
└── Samples~/BasicDemo/
    ├── SampleData/sample_sustainability_data.csv  # Synthetic demo data
    └── SETUP.md                              # Step-by-step scene setup
```

**Design choices, and why:**
- **Data / Import / Visualization are separate namespaces** so the import logic has zero
  Unity-scene dependencies — it's plain C#, easy to unit test (see `Tests/`), and reusable
  outside of any specific scene setup.
- **`SustainabilityDataset` is a `ScriptableObject`** so non-programmers (e.g., a researcher
  on the team) could eventually assign pre-imported data in the Inspector without touching
  code or CSV file paths.
- **Packaged as a UPM package** (`package.json` at the root) rather than a loose set of
  scripts in `Assets/`, so it can be installed into any project via Package Manager —
  matching how a real internal SDK would be distributed.
- **The CSV parser is intentionally minimal**, not a full RFC-4180 implementation — it's
  scoped to the kind of small, clean sustainability datasets this toolkit targets, with
  room to swap in a more robust parser later if needed.

## Quick start

See [`Samples~/BasicDemo/SETUP.md`](Samples~/BasicDemo/SETUP.md) for a 5-minute walkthrough
to get the demo running in a fresh Unity project.

## Roadmap

- [ ] AR Foundation integration — place visualizations on real detected surfaces
- [ ] Category-grouped layouts (cluster by Agriculture / Water / Energy, not just a flat line)
- [ ] Swap primitive cubes for a proper labeled-bar prefab (TextMeshPro value labels)
- [ ] Time-series support — animate a dataset across multiple dates, not just one snapshot
- [ ] Swap in real data from the Climate Smart-AG GHG emissions project

## Background

This project extends work from **Climate Smart-AG: GHG Emissions Prediction from
Agricultural Activities** (published at ICBAES International Conference, Jan. 2024),
where I built SARIMA/XGBoost/Adaptive-KNN models on multi-source greenhouse-gas datasets.
This toolkit is about giving that kind of data a spatial, walkable form.

## License

MIT — see [LICENSE](LICENSE).
