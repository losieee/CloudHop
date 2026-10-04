using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CloudHop.Editor
{
    // Only this workbook is watched. No Excel installation or runtime spreadsheet library is needed.
    public sealed class UITextExcelImporter : AssetPostprocessor, IPreprocessBuildWithReport
    {
        public const string WorkbookPath = "Assets/_Project/Data/UITexts.xlsx";
        public const string OutputPath = "Assets/_Project/Resources/UITexts.json";
        public int callbackOrder => -100;
        private static bool queued;

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] oldPaths)
        {
            if (!imported.Contains(WorkbookPath) && !moved.Contains(WorkbookPath)) return;
            if (queued) return;
            queued = true;
            EditorApplication.delayCall += () =>
            {
                queued = false;
                try { Import(); }
                catch (Exception error) { Debug.LogError("UI 엑셀 반영 실패. 기존 데이터는 유지됩니다. " + error.Message); }
            };
        }

        [MenuItem("Cloud Hop/UI 텍스트/엑셀 열기")]
        public static void OpenWorkbook() => EditorUtility.OpenWithDefaultApp(Path.GetFullPath(WorkbookPath));

        [MenuItem("Cloud Hop/UI 텍스트/엑셀 다시 반영")]
        public static void Import()
        {
            var entries = ReadWorkbook(WorkbookPath);
            UIText.Validate(entries); // Validate every row before touching the previous good output.
            string hash;
            using (var stream = File.OpenRead(WorkbookPath))
            using (var sha = SHA256.Create()) hash = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
            string json = JsonUtility.ToJson(new UITextDocument { sourceHash = hash, entries = entries }, true);
            if (!File.Exists(OutputPath) || File.ReadAllText(OutputPath) != json)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
                File.WriteAllText(OutputPath, json, new System.Text.UTF8Encoding(false));
                AssetDatabase.ImportAsset(OutputPath, ImportAssetOptions.ForceSynchronousImport);
            }
            UIText.Reload();
            Debug.Log("UI 엑셀 반영 완료: " + entries.Length + "개 문구. 실행 중이었다면 플레이를 다시 시작하세요.");
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            try { Import(); }
            catch (Exception error) { throw new BuildFailedException("UITexts.xlsx 수정 내용을 확인하세요: " + error.Message); }
        }

        public static UITextEntry[] ReadWorkbook(string path)
        {
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
            {
                XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
                XNamespace relNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
                var workbook = ReadXml(zip, "xl/workbook.xml");
                var sheet = workbook.Descendants(ns + "sheet").SingleOrDefault(s => (string)s.Attribute("name") == "UI텍스트");
                if (sheet == null) throw new FormatException("UI텍스트 시트를 찾을 수 없습니다.");
                string relId = (string)sheet.Attribute(relNs + "id");
                var relation = ReadXml(zip, "xl/_rels/workbook.xml.rels").Root.Elements()
                    .Single(r => (string)r.Attribute("Id") == relId);
                string target = (string)relation.Attribute("Target");
                string sheetPath = new Uri(new Uri("http://workbook/xl/workbook.xml"), target).AbsolutePath.TrimStart('/');
                var shared = zip.GetEntry("xl/sharedStrings.xml") == null ? new string[0] :
                    ReadXml(zip, "xl/sharedStrings.xml").Descendants(ns + "si")
                        .Select(s => string.Concat(s.Descendants(ns + "t").Select(t => t.Value))).ToArray();
                var sheetRows = ReadXml(zip, sheetPath).Descendants(ns + "sheetData").Elements(ns + "row").ToArray();
                if (sheetRows.Length == 0) throw new FormatException("UI텍스트 시트가 비어 있습니다.");
                var headers = Cells(sheetRows[0], ns, shared);
                string keyColumn = headers.Single(p => p.Value == "Key").Key;
                string textColumn = headers.Single(p => p.Value == "한국어").Key;
                var result = new List<UITextEntry>();
                foreach (var row in sheetRows.Skip(1))
                {
                    var values = Cells(row, ns, shared);
                    values.TryGetValue(keyColumn, out string key);
                    values.TryGetValue(textColumn, out string text);
                    if (string.IsNullOrWhiteSpace(key) && string.IsNullOrWhiteSpace(text)) continue;
                    foreach (var cell in row.Elements(ns + "c"))
                        if ((Column(cell) == keyColumn || Column(cell) == textColumn) && cell.Element(ns + "f") != null)
                            throw new FormatException("문구에는 수식 대신 텍스트를 입력하세요. 행 " + (string)row.Attribute("r"));
                    result.Add(new UITextEntry { key = key?.Trim(), text = text?.Replace("\r\n", "\n") });
                }
                return result.ToArray();
            }
        }

        private static string Column(XElement cell) => new string(((string)cell.Attribute("r")).TakeWhile(char.IsLetter).ToArray());
        private static Dictionary<string, string> Cells(XElement row, XNamespace ns, string[] shared)
        {
            var result = new Dictionary<string, string>();
            foreach (var cell in row.Elements(ns + "c"))
            {
                string value = (string)cell.Element(ns + "v") ?? "";
                string type = (string)cell.Attribute("t");
                if (type == "s") value = shared[int.Parse(value)];
                else if (type == "inlineStr") value = string.Concat(cell.Descendants(ns + "t").Select(t => t.Value));
                result[Column(cell)] = value;
            }
            return result;
        }
        private static XDocument ReadXml(ZipArchive zip, string path)
        {
            var entry = zip.GetEntry(path) ?? throw new FormatException("Excel 항목이 없습니다: " + path);
            using (var stream = entry.Open())
            using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }))
                return XDocument.Load(reader);
        }
    }
}
