using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CloudHop
{
    [Serializable] public sealed class UITextEntry { public string key; public string text; }
    [Serializable] public sealed class UITextDocument { public string sourceHash; public UITextEntry[] entries; }

    public static class UIText
    {
        private static Dictionary<string, string> texts;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reload() => texts = null;

        public static string Get(string key, params object[] arguments)
        {
            if (texts == null)
            {
                var asset = Resources.Load<TextAsset>("UITexts");
                if (asset == null) throw new InvalidOperationException("UITexts.json is missing. Import UITexts.xlsx before playing.");
                var document = JsonUtility.FromJson<UITextDocument>(asset.text);
                texts = Validate(document.entries);
            }
            if (!texts.TryGetValue(key, out var text)) throw new KeyNotFoundException("Missing UI text: " + key);
            return arguments.Length == 0 ? text : string.Format(CultureInfo.InvariantCulture, text, arguments);
        }

        public static Dictionary<string, string> Validate(IEnumerable<UITextEntry> entries)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry.key) || string.IsNullOrWhiteSpace(entry.text))
                    throw new FormatException("UI text key/value cannot be blank: " + entry.key);
                if (result.ContainsKey(entry.key)) throw new FormatException("Duplicate UI key: " + entry.key);
                if (!UITextSchema.Arguments.TryGetValue(entry.key, out int count))
                    throw new FormatException("Unknown UI key: " + entry.key);
                try
                {
                    string.Format(CultureInfo.InvariantCulture, entry.text, Enumerable.Repeat<object>(0, count).ToArray());
                    string unescaped = entry.text.Replace("{{", "").Replace("}}", "");
                    var found = Regex.Matches(unescaped, @"\{(\d+)(?:,[^}:]+)?(?::[^}]+)?\}")
                        .Cast<Match>().Select(m => int.Parse(m.Groups[1].Value)).Distinct().OrderBy(i => i).ToArray();
                    if (!found.SequenceEqual(Enumerable.Range(0, count)))
                        throw new FormatException("Keep all placeholders {0} ... required by this row.");
                }
                catch (FormatException error) { throw new FormatException(entry.key + ": " + error.Message, error); }
                result.Add(entry.key, entry.text);
            }
            foreach (string key in UITextSchema.Arguments.Keys)
                if (!result.ContainsKey(key)) throw new FormatException("Required UI key is missing: " + key);
            return result;
        }
    }
}
