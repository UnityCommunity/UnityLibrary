// SceneSizeAnalyzer.cs — put in an Editor/ folder.
// Tools > UnityLibrary > Scene Size Analyzer
//
// Parses a .unity scene file (text/YAML serialization) directly from disk and reports
// where the bytes go: per component type, per script, per field, per GameObject and
// per hierarchy subtree. Exports a text or HTML report.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityLibrary.EditorTools
{
    // ------------------------------------------------------------------ Window

    public class SceneSizeAnalyzerWindow : EditorWindow
    {
        [MenuItem("Tools/UnityLibrary/Scene Size Analyzer")]
        public static void Open() => GetWindow<SceneSizeAnalyzerWindow>("Scene Size Analyzer");

        SceneAsset _scene;
        int _topN = 25;
        Vector2 _scroll;
        SceneSizeReport _report;
        string _text = "";

        void OnGUI()
        {
            _scene = (SceneAsset)EditorGUILayout.ObjectField("Scene", _scene, typeof(SceneAsset), false);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Use Active Scene"))
                {
                    var p = EditorSceneManager.GetActiveScene().path;
                    if (!string.IsNullOrEmpty(p)) _scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(p);
                }
                _topN = EditorGUILayout.IntSlider("Top N", _topN, 5, 100);
            }

            using (new EditorGUI.DisabledScope(_scene == null))
            {
                if (GUILayout.Button("Analyze", GUILayout.Height(28))) Analyze();
            }

            if (_report == null) return;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save HTML Report")) Save(true);
                if (GUILayout.Button("Save Text Report")) Save(false);
                if (GUILayout.Button("Copy Text")) EditorGUIUtility.systemCopyBuffer = _text;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.TextArea(_text, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        void Analyze()
        {
            string path = AssetDatabase.GetAssetPath(_scene);
            var active = EditorSceneManager.GetActiveScene();
            if (active.path == path && active.isDirty)
                Debug.LogWarning("Scene has unsaved changes — the report reflects the file on disk.");

            try
            {
                _report = SceneSizeReport.Analyze(path, _topN);
                _text = SceneSizeReportWriter.ToText(_report);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _report = null;
            }
        }

        void Save(bool html)
        {
            string ext = html ? "html" : "txt";
            string name = Path.GetFileNameWithoutExtension(_report.ScenePath) + "_size_report";
            string file = EditorUtility.SaveFilePanel("Save report", "", name, ext);
            if (string.IsNullOrEmpty(file)) return;

            File.WriteAllText(file, html ? SceneSizeReportWriter.ToHtml(_report) : _text);
            if (html) Application.OpenURL("file://" + file);
            else EditorUtility.RevealInFinder(file);
        }
    }

    // ------------------------------------------------------------------ Data

    public class SceneDoc
    {
        public int ClassId;
        public long FileId;
        public bool Stripped;
        public string TypeName = "?";
        public string DisplayType = "?";
        public string Name;              // m_Name
        public long GameObjectId;        // m_GameObject
        public long ParentTransformId;   // m_Father / m_TransformParent
        public long PrefabInstanceId;    // m_PrefabInstance (stripped docs)
        public string ScriptGuid;        // m_Script
        public string SourcePrefabGuid;  // m_SourcePrefab
        public long Bytes;
        public List<KeyValuePair<string, long>> Fields = new List<KeyValuePair<string, long>>();
    }

    public class GoNode
    {
        public long Id;
        public string Name;
        public string Path;
        public long OwnBytes;      // GameObject doc + its components
        public long SubtreeBytes;
        public GoNode Parent;
        public List<GoNode> Children = new List<GoNode>();
        public long ParentTransformId;
        public long ParentPrefabInstanceId;
        public bool IsPrefabInstance;
    }

    public struct Entry
    {
        public string Key;
        public int Count;
        public long Bytes;
    }

    public class SceneSizeReport
    {
        public string ScenePath;
        public long FileBytes;
        public bool IsText;
        public int TopN;
        public long HeaderBytes;
        public long UnownedBytes;
        public List<SceneDoc> Docs = new List<SceneDoc>();
        public List<Entry> ByType = new List<Entry>();
        public List<Entry> ByField = new List<Entry>();
        public List<SceneDoc> TopDocs = new List<SceneDoc>();
        public List<GoNode> TopObjects = new List<GoNode>();
        public List<GoNode> TopSubtrees = new List<GoNode>();
        public List<string> Hints = new List<string>();

        // ---------------------------------------------------------------- Analyze

        public static SceneSizeReport Analyze(string scenePath, int topN)
        {
            var r = new SceneSizeReport { ScenePath = scenePath, TopN = topN };
            r.FileBytes = new FileInfo(scenePath).Length;

            string text = File.ReadAllText(scenePath);
            r.IsText = text.StartsWith("%YAML", StringComparison.Ordinal);
            if (!r.IsText)
            {
                r.Hints.Add("Scene is not text-serialized. Set Edit > Project Settings > Editor > Asset Serialization to 'Force Text' and re-save.");
                return r;
            }

            Parse(r, text);
            Aggregate(r);
            BuildHints(r);
            return r;
        }

        // ---------------------------------------------------------------- Parse

        static void Parse(SceneSizeReport r, string text)
        {
            int nl = text.IndexOf("\r\n", StringComparison.Ordinal) >= 0 ? 2 : 1;
            var reader = new StringReader(text);
            SceneDoc doc = null;
            string field = null;
            long fieldBytes = 0;
            bool expectType = false;

            void Flush()
            {
                if (doc != null && field != null)
                    doc.Fields.Add(new KeyValuePair<string, long>(field, fieldBytes));
                field = null;
                fieldBytes = 0;
            }

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                long bytes = Encoding.UTF8.GetByteCount(line) + nl;

                if (line.StartsWith("--- !u!", StringComparison.Ordinal))
                {
                    Flush();
                    doc = new SceneDoc { Bytes = bytes };
                    r.Docs.Add(doc);
                    ParseHeader(line, doc);
                    expectType = true;
                    continue;
                }

                if (doc == null) { r.HeaderBytes += bytes; continue; }
                doc.Bytes += bytes;

                if (expectType)
                {
                    doc.TypeName = line.TrimEnd(':');
                    expectType = false;
                    continue;
                }

                // Top-level field of the document: exactly two spaces of indentation.
                if (line.Length > 2 && line[0] == ' ' && line[1] == ' ' && line[2] != ' ' && line[2] != '-')
                {
                    Flush();
                    int colon = line.IndexOf(':');
                    field = colon > 2 ? line.Substring(2, colon - 2) : line.Trim();
                    ExtractKnown(doc, field, line);
                }
                else if (line.StartsWith("    m_TransformParent:", StringComparison.Ordinal))
                {
                    doc.ParentTransformId = ParseFileId(line); // inside PrefabInstance.m_Modification
                }

                fieldBytes += bytes;
            }
            Flush();
        }

        static void ParseHeader(string line, SceneDoc d)
        {
            // "--- !u!4 &12345" or "--- !u!4 &12345 stripped"
            int i = 7;
            int start = i;
            while (i < line.Length && char.IsDigit(line[i])) i++;
            int.TryParse(line.Substring(start, i - start), out d.ClassId);

            int amp = line.IndexOf('&', i);
            if (amp >= 0)
            {
                int j = amp + 1;
                if (j < line.Length && line[j] == '-') j++;
                while (j < line.Length && char.IsDigit(line[j])) j++;
                long.TryParse(line.Substring(amp + 1, j - amp - 1), out d.FileId);
            }
            d.Stripped = line.EndsWith("stripped", StringComparison.Ordinal);
        }

        static void ExtractKnown(SceneDoc d, string key, string line)
        {
            switch (key)
            {
                case "m_Name":
                    if (d.Name == null)
                    {
                        int c = line.IndexOf(':');
                        d.Name = line.Substring(c + 1).Trim().Trim('\'', '"');
                    }
                    break;
                case "m_GameObject": d.GameObjectId = ParseFileId(line); break;
                case "m_Father": d.ParentTransformId = ParseFileId(line); break;
                case "m_PrefabInstance": d.PrefabInstanceId = ParseFileId(line); break;
                case "m_Script": d.ScriptGuid = ParseGuid(line); break;
                case "m_SourcePrefab": d.SourcePrefabGuid = ParseGuid(line); break;
            }
        }

        static long ParseFileId(string line)
        {
            int i = line.IndexOf("fileID:", StringComparison.Ordinal);
            if (i < 0) return 0;
            i += 7;
            while (i < line.Length && line[i] == ' ') i++;
            int s = i;
            if (i < line.Length && line[i] == '-') i++;
            while (i < line.Length && char.IsDigit(line[i])) i++;
            long.TryParse(line.Substring(s, i - s), out long id);
            return id;
        }

        static string ParseGuid(string line)
        {
            int i = line.IndexOf("guid:", StringComparison.Ordinal);
            if (i < 0) return null;
            i += 5;
            while (i < line.Length && line[i] == ' ') i++;
            int s = i;
            while (i < line.Length && Uri.IsHexDigit(line[i])) i++;
            return i - s == 32 ? line.Substring(s, 32) : null;
        }

        // ---------------------------------------------------------------- Aggregate

        static void Aggregate(SceneSizeReport r)
        {
            var guidNames = new Dictionary<string, string>();
            string GuidName(string guid)
            {
                if (guid == null) return null;
                if (!guidNames.TryGetValue(guid, out var n))
                {
                    string p = AssetDatabase.GUIDToAssetPath(guid);
                    n = string.IsNullOrEmpty(p) ? "missing " + guid.Substring(0, 8) : Path.GetFileNameWithoutExtension(p);
                    guidNames[guid] = n;
                }
                return n;
            }

            // Display type names
            foreach (var d in r.Docs)
            {
                d.DisplayType = d.TypeName;
                if (d.ClassId == 114 && d.ScriptGuid != null) d.DisplayType = "MonoBehaviour: " + GuidName(d.ScriptGuid);
                else if (d.ClassId == 1001 && d.SourcePrefabGuid != null) d.DisplayType = "PrefabInstance: " + GuidName(d.SourcePrefabGuid);
                if (d.Stripped) d.DisplayType += " (stripped)";
            }

            // Per type
            var byType = new Dictionary<string, Entry>();
            var byField = new Dictionary<string, Entry>();
            foreach (var d in r.Docs)
            {
                Add(byType, d.DisplayType, d.Bytes);
                foreach (var f in d.Fields) Add(byField, d.TypeName + "." + f.Key, f.Value);
            }
            r.ByType = byType.Values.OrderByDescending(e => e.Bytes).ToList();
            r.ByField = byField.Values.OrderByDescending(e => e.Bytes).Take(r.TopN).ToList();
            r.TopDocs = r.Docs.OrderByDescending(d => d.Bytes).Take(r.TopN).ToList();

            // Hierarchy
            var nodes = new Dictionary<long, GoNode>();
            var transformToGo = new Dictionary<long, long>();

            foreach (var d in r.Docs)
            {
                if (d.ClassId == 1)
                    nodes[d.FileId] = new GoNode
                    {
                        Id = d.FileId,
                        Name = d.Stripped ? "(prefab child)" : (string.IsNullOrEmpty(d.Name) ? "(unnamed)" : d.Name),
                        OwnBytes = d.Bytes,
                        ParentPrefabInstanceId = d.Stripped ? d.PrefabInstanceId : 0
                    };
                else if (d.ClassId == 1001)
                    nodes[d.FileId] = new GoNode
                    {
                        Id = d.FileId,
                        Name = "[" + d.DisplayType + "]",
                        OwnBytes = d.Bytes,
                        IsPrefabInstance = true,
                        ParentTransformId = d.ParentTransformId
                    };
            }

            foreach (var d in r.Docs)
            {
                if (d.ClassId != 4 && d.ClassId != 224) continue;
                transformToGo[d.FileId] = d.GameObjectId;
                if (!d.Stripped && nodes.TryGetValue(d.GameObjectId, out var n)) n.ParentTransformId = d.ParentTransformId;
            }

            // Components -> owner
            foreach (var d in r.Docs)
            {
                if (d.ClassId == 1 || d.ClassId == 1001) continue;
                GoNode owner = null;
                if (d.GameObjectId != 0) nodes.TryGetValue(d.GameObjectId, out owner);
                if (owner == null && d.Stripped) nodes.TryGetValue(d.PrefabInstanceId, out owner);
                if (owner != null) owner.OwnBytes += d.Bytes;
                else r.UnownedBytes += d.Bytes;
            }

            // Parent links
            foreach (var n in nodes.Values)
            {
                GoNode parent = null;
                if (n.ParentPrefabInstanceId != 0) nodes.TryGetValue(n.ParentPrefabInstanceId, out parent);
                else if (n.ParentTransformId != 0 && transformToGo.TryGetValue(n.ParentTransformId, out long goId))
                    nodes.TryGetValue(goId, out parent);
                if (parent != null && parent != n)
                {
                    n.Parent = parent;
                    parent.Children.Add(n);
                }
            }

            var roots = nodes.Values.Where(n => n.Parent == null).ToList();
            var visited = new HashSet<GoNode>();
            foreach (var root in roots) Accumulate(root, root.Name, visited);

            r.TopObjects = nodes.Values.OrderByDescending(n => n.OwnBytes).Take(r.TopN).ToList();
            r.TopSubtrees = nodes.Values.Where(n => n.Children.Count > 0).OrderByDescending(n => n.SubtreeBytes).Take(r.TopN).ToList();
        }

        static void Accumulate(GoNode n, string path, HashSet<GoNode> visited)
        {
            if (!visited.Add(n)) return;
            n.Path = path;
            n.SubtreeBytes = n.OwnBytes;
            foreach (var c in n.Children)
            {
                Accumulate(c, path + "/" + c.Name, visited);
                n.SubtreeBytes += c.SubtreeBytes;
            }
        }

        static void Add(Dictionary<string, Entry> dict, string key, long bytes)
        {
            dict.TryGetValue(key, out var e);
            e.Key = key; e.Count++; e.Bytes += bytes;
            dict[key] = e;
        }

        // ---------------------------------------------------------------- Hints

        static readonly Dictionary<int, string> EmbeddedAssetTypes = new Dictionary<int, string>
        {
            { 43, "Mesh" }, { 28, "Texture2D" }, { 21, "Material" }, { 74, "AnimationClip" },
            { 48, "Shader" }, { 89, "Cubemap" }, { 117, "Texture3D" }, { 91, "AnimatorController" },
        };

        static void BuildHints(SceneSizeReport r)
        {
            long total = Math.Max(1, r.FileBytes);
            double Share(long b) => 100.0 * b / total;

            long prefab = r.ByType.Where(e => e.Key.StartsWith("PrefabInstance")).Sum(e => e.Bytes);
            if (Share(prefab) > 15)
                r.Hints.Add($"PrefabInstance overrides take {Share(prefab):0}% of the file. Large m_Modifications lists usually mean many per-instance overrides. Apply overrides to the prefab, use prefab variants, or reduce serialized fields on prefab components.");

            var scripts = r.ByType.Where(e => e.Key.StartsWith("MonoBehaviour")).ToList();
            long mono = scripts.Sum(e => e.Bytes);
            if (Share(mono) > 15)
                r.Hints.Add($"MonoBehaviour data takes {Share(mono):0}%. Biggest scripts: " +
                            string.Join(", ", scripts.Take(3).Select(e => e.Key.Replace("MonoBehaviour: ", "") + " " + Fmt(e.Bytes))) +
                            ". Check for large serialized arrays/strings; mark runtime caches [NonSerialized] or move data to ScriptableObjects / assets.");

            var embedded = r.Docs.Where(d => EmbeddedAssetTypes.ContainsKey(d.ClassId)).ToList();
            if (embedded.Count > 0)
                r.Hints.Add($"{embedded.Count} asset object(s) are embedded in the scene ({Fmt(embedded.Sum(d => d.Bytes))}): " +
                            string.Join(", ", embedded.GroupBy(d => EmbeddedAssetTypes[d.ClassId]).Select(g => g.Key + " x" + g.Count())) +
                            ". Save them as project assets (e.g. generated/ProBuilder meshes, runtime-created materials).");

            if (r.TopDocs.Count > 0 && Share(r.TopDocs[0].Bytes) > 10)
                r.Hints.Add($"A single object dominates: {r.TopDocs[0].DisplayType} '{r.TopDocs[0].Name}' is {Share(r.TopDocs[0].Bytes):0}% of the file. See its largest fields in the report.");

            int goCount = r.Docs.Count(d => d.ClassId == 1);
            if (goCount > 20000)
                r.Hints.Add($"{goCount:N0} GameObjects in the scene. Consider prefabs, static batching via assets, additive scenes, or generating content at runtime.");

            long perObject = goCount > 0 ? (r.FileBytes - r.UnownedBytes) / goCount : 0;
            if (goCount > 0 && r.Hints.Count == 0)
                r.Hints.Add($"No single culprit; size is spread over {goCount:N0} objects (~{Fmt(perObject)} each). Check the per-type table for the heaviest component types.");
        }

        public static string Fmt(long bytes)
        {
            if (bytes >= 1L << 30) return $"{bytes / (double)(1L << 30):0.00} GB";
            if (bytes >= 1L << 20) return $"{bytes / (double)(1L << 20):0.00} MB";
            if (bytes >= 1L << 10) return $"{bytes / 1024.0:0.0} KB";
            return bytes + " B";
        }
    }

    // ------------------------------------------------------------------ Writers

    public static class SceneSizeReportWriter
    {
        static string Fmt(long b) => SceneSizeReport.Fmt(b);
        static string Pct(long b, long total) => total > 0 ? $"{100.0 * b / total:0.0}%" : "0%";

        public static string ToText(SceneSizeReport r)
        {
            var sb = new StringBuilder();
            long t = r.FileBytes;
            sb.AppendLine($"Scene: {r.ScenePath}");
            sb.AppendLine($"Size:  {Fmt(t)} ({t:N0} bytes)  |  {r.Docs.Count:N0} objects  |  {r.Docs.Count(d => d.ClassId == 1):N0} GameObjects");
            sb.AppendLine();

            if (r.Hints.Count > 0)
            {
                sb.AppendLine("HINTS");
                foreach (var h in r.Hints) sb.AppendLine("  * " + h);
                sb.AppendLine();
            }
            if (!r.IsText) return sb.ToString();

            sb.AppendLine("BY TYPE");
            foreach (var e in r.ByType.Take(r.TopN))
                sb.AppendLine($"  {Fmt(e.Bytes),10}  {Pct(e.Bytes, t),6}  x{e.Count,-7} {e.Key}");
            sb.AppendLine();

            sb.AppendLine("BY FIELD (Type.field, summed over all objects)");
            foreach (var e in r.ByField)
                sb.AppendLine($"  {Fmt(e.Bytes),10}  {Pct(e.Bytes, t),6}  x{e.Count,-7} {e.Key}");
            sb.AppendLine();

            sb.AppendLine("LARGEST SINGLE OBJECTS");
            foreach (var d in r.TopDocs)
            {
                sb.AppendLine($"  {Fmt(d.Bytes),10}  {Pct(d.Bytes, t),6}  {d.DisplayType} '{d.Name}' &{d.FileId}");
                foreach (var f in d.Fields.OrderByDescending(f => f.Value).Take(3))
                    sb.AppendLine($"  {"",10}  {"",6}    {f.Key}: {Fmt(f.Value)}");
            }
            sb.AppendLine();

            sb.AppendLine("LARGEST GAMEOBJECTS (object + own components)");
            foreach (var n in r.TopObjects)
                sb.AppendLine($"  {Fmt(n.OwnBytes),10}  {Pct(n.OwnBytes, t),6}  {n.Path}");
            sb.AppendLine();

            sb.AppendLine("LARGEST HIERARCHY SUBTREES");
            foreach (var n in r.TopSubtrees)
                sb.AppendLine($"  {Fmt(n.SubtreeBytes),10}  {Pct(n.SubtreeBytes, t),6}  {n.Path}  ({CountNodes(n):N0} objects)");
            sb.AppendLine();

            sb.AppendLine($"Scene settings / unowned: {Fmt(r.UnownedBytes)}   YAML header: {Fmt(r.HeaderBytes)}");
            return sb.ToString();
        }

        public static string ToHtml(SceneSizeReport r)
        {
            long t = r.FileBytes;
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><head><meta charset='utf-8'><title>Scene Size Report</title><style>");
            sb.Append("body{font:13px/1.4 -apple-system,Segoe UI,Roboto,sans-serif;background:#1b1d21;color:#d8dbe0;margin:24px}");
            sb.Append("h1{font-size:20px;margin:0 0 4px}h2{font-size:15px;margin:28px 0 8px;color:#f0b232;border-bottom:1px solid #333;padding-bottom:4px}");
            sb.Append(".meta{color:#8a8f98;margin-bottom:12px}table{border-collapse:collapse;width:100%}");
            sb.Append("td,th{padding:4px 8px;text-align:left;border-bottom:1px solid #2a2d33;white-space:nowrap;vertical-align:top}th{color:#8a8f98;font-weight:normal}");
            sb.Append("td.n{text-align:right;font-variant-numeric:tabular-nums}td.w{white-space:normal;width:100%}");
            sb.Append(".bar{display:inline-block;height:9px;background:#f0b232;border-radius:2px;vertical-align:middle}");
            sb.Append(".sub{color:#8a8f98;font-size:11px}ul{margin:0;padding-left:18px}li{margin:4px 0}");
            sb.Append("</style></head><body>");

            sb.Append($"<h1>{H(Path.GetFileName(r.ScenePath))}</h1>");
            sb.Append($"<div class='meta'>{H(r.ScenePath)} &nbsp;|&nbsp; {Fmt(t)} ({t:N0} bytes) &nbsp;|&nbsp; {r.Docs.Count:N0} objects &nbsp;|&nbsp; {r.Docs.Count(d => d.ClassId == 1):N0} GameObjects &nbsp;|&nbsp; {DateTime.Now:yyyy-MM-dd HH:mm}</div>");

            if (r.Hints.Count > 0)
            {
                sb.Append("<h2>Hints</h2><ul>");
                foreach (var h in r.Hints) sb.Append("<li>" + H(h) + "</li>");
                sb.Append("</ul>");
            }
            if (!r.IsText) return sb.Append("</body></html>").ToString();

            Table(sb, "By type", "Type", r.ByType.Take(r.TopN).Select(e => (e.Key, e.Count, e.Bytes, (string)null)), t);
            Table(sb, "By field (Type.field, summed over all objects)", "Field", r.ByField.Select(e => (e.Key, e.Count, e.Bytes, (string)null)), t);
            Table(sb, "Largest single objects", "Object", r.TopDocs.Select(d => (
                $"{d.DisplayType} '{d.Name}' &{d.FileId}", 1, d.Bytes,
                string.Join(", ", d.Fields.OrderByDescending(f => f.Value).Take(4).Select(f => $"{f.Key} {Fmt(f.Value)}")))), t);
            Table(sb, "Largest GameObjects (object + own components)", "Path", r.TopObjects.Select(n => (n.Path, 1, n.OwnBytes, (string)null)), t);
            Table(sb, "Largest hierarchy subtrees", "Path", r.TopSubtrees.Select(n => (n.Path, CountNodes(n), n.SubtreeBytes, (string)null)), t);

            sb.Append($"<h2>Other</h2><div class='meta'>Scene settings / unowned: {Fmt(r.UnownedBytes)} &nbsp;|&nbsp; YAML header: {Fmt(r.HeaderBytes)}</div>");
            return sb.Append("</body></html>").ToString();
        }

        static void Table(StringBuilder sb, string title, string col, IEnumerable<(string key, int count, long bytes, string sub)> rows, long total)
        {
            sb.Append($"<h2>{H(title)}</h2><table><tr><th>Size</th><th>%</th><th></th><th>Count</th><th>{H(col)}</th></tr>");
            foreach (var (key, count, bytes, sub) in rows)
            {
                double pct = total > 0 ? 100.0 * bytes / total : 0;
                sb.Append($"<tr><td class='n'>{Fmt(bytes)}</td><td class='n'>{pct:0.0}%</td>");
                sb.Append($"<td><span class='bar' style='width:{Math.Max(1, pct * 1.6):0}px'></span></td>");
                sb.Append($"<td class='n'>{count:N0}</td><td class='w'>{H(key)}");
                if (sub != null) sb.Append($"<div class='sub'>{H(sub)}</div>");
                sb.Append("</td></tr>");
            }
            sb.Append("</table>");
        }

        static int CountNodes(GoNode n) => 1 + n.Children.Sum(CountNodes);

        static string H(string s) => string.IsNullOrEmpty(s) ? "" :
            s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&#39;");
    }
}
