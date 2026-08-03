using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SustainabilityXRToolkit.Data;
using SustainabilityXRToolkit.Import;

namespace SustainabilityXRToolkit.Tests
{
    public class CsvSustainabilityImporterTests
    {
        private string _tempFile;

        [SetUp]
        public void SetUp()
        {
            _tempFile = Path.Combine(Path.GetTempPath(), "sxr_test_data.csv");
            File.WriteAllLines(_tempFile, new[]
            {
                "Label,Category,Value,Unit,Date",
                "Crop Emissions,Agriculture,120.5,MtCO2e,2024-01",
                "Livestock Emissions,Agriculture,95.2,MtCO2e,2024-01",
                "Malformed Row,Agriculture,not_a_number,MtCO2e,2024-01"
            });
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_tempFile)) File.Delete(_tempFile);
        }

        [Test]
        public void LoadFromCsv_ParsesValidRows_AndSkipsMalformedRows()
        {
            List<SustainabilityDataPoint> points = CsvSustainabilityImporter.LoadFromCsv(_tempFile);

            Assert.AreEqual(2, points.Count, "Should parse exactly 2 valid rows and skip the malformed one.");
            Assert.AreEqual("Crop Emissions", points[0].Label);
            Assert.AreEqual(120.5f, points[0].Value, 0.001f);
            Assert.AreEqual("MtCO2e", points[0].Unit);
        }

        [Test]
        public void LoadFromCsv_ReturnsEmptyList_WhenFileMissing()
        {
            List<SustainabilityDataPoint> points = CsvSustainabilityImporter.LoadFromCsv("nonexistent_file.csv");
            Assert.AreEqual(0, points.Count);
        }

        [Test]
        public void LoadFromCsv_ReturnsEmptyList_WhenOnlyHeaderPresent()
        {
            string headerOnlyFile = Path.Combine(Path.GetTempPath(), "sxr_header_only.csv");
            File.WriteAllLines(headerOnlyFile, new[] { "Label,Category,Value,Unit,Date" });

            List<SustainabilityDataPoint> points = CsvSustainabilityImporter.LoadFromCsv(headerOnlyFile);

            Assert.AreEqual(0, points.Count);
            File.Delete(headerOnlyFile);
        }
    }
}
