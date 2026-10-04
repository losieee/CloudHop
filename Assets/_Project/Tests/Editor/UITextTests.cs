using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using CloudHop.Editor;
using NUnit.Framework;

namespace CloudHop.Tests
{
    public sealed class UITextTests
    {
        [Test]
        public void WorkbookMatchesEveryRequiredRuntimeKey()
        {
            var entries = UITextExcelImporter.ReadWorkbook(UITextExcelImporter.WorkbookPath);
            var values = UIText.Validate(entries);
            Assert.AreEqual(UITextSchema.Arguments.Count, values.Count);
            UIText.Reload();
            Assert.AreEqual(values["pause.title"], UIText.Get("pause.title"));
            Assert.AreEqual("스테이지 2 / 3", UIText.Get("hud.stage", 2, 3));
            Assert.IsTrue(values["name.rules"].Contains("\n"));
        }

        [TestCase("duplicate")]
        [TestCase("missing")]
        [TestCase("blank")]
        [TestCase("placeholder")]
        public void InvalidEditsAreRejected(string kind)
        {
            var entries = UITextExcelImporter.ReadWorkbook(UITextExcelImporter.WorkbookPath).ToList();
            if (kind == "duplicate") entries.Add(entries[0]);
            if (kind == "missing") entries.RemoveAt(0);
            if (kind == "blank") entries[0].text = "";
            if (kind == "placeholder") entries.Single(e => e.key == "hud.stage").text = "스테이지 {9}";
            Assert.Throws<FormatException>(() => UIText.Validate(entries));
        }

        [Test]
        public void SavedExcelEditSurvivesRowReorderingAndMultilineText()
        {
            string fixture = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xlsx");
            try
            {
                File.Copy(UITextExcelImporter.WorkbookPath, fixture);
                using (var zip = ZipFile.Open(fixture, ZipArchiveMode.Update))
                {
                    var entry = zip.GetEntry("xl/worksheets/sheet1.xml");
                    XDocument doc;
                    using (var input = entry.Open()) doc = XDocument.Load(input);
                    XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
                    var data = doc.Descendants(ns + "sheetData").Single();
                    var row = data.Elements(ns + "row").Skip(1).First();
                    var cell = row.Elements(ns + "c").Single(c => ((string)c.Attribute("r")).StartsWith("C"));
                    cell.RemoveNodes(); cell.SetAttributeValue("t", "inlineStr");
                    cell.Add(new XElement(ns + "is", new XElement(ns + "t", "새 문구, \"따옴표\"\n두 번째 줄")));
                    row.Remove(); data.Add(row);
                    entry.Delete();
                    using (var output = zip.CreateEntry("xl/worksheets/sheet1.xml").Open()) doc.Save(output);
                }
                var values = UIText.Validate(UITextExcelImporter.ReadWorkbook(fixture));
                Assert.AreEqual("새 문구, \"따옴표\"\n두 번째 줄", values["common.start"]);
            }
            finally { if (File.Exists(fixture)) File.Delete(fixture); }
        }
    }
}
