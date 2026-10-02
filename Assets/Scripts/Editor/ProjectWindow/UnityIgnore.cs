// UnityIgnore.cs - put this in any "Editor" folder.
//
// Hides files and folders from the Project window, driven by a gitignore-style
// ".unityignore" file in the project root (next to Assets/).
//
// Display only: hidden assets are still imported, referenced and built as usual.
// Toggle: Assets > Unity Ignore > Hide Ignored Files (Ctrl+Alt+H), also in the Project right-click menu.
//
// Unity has no public API for this, so the script filters the Project Browser's internal
// row lists through reflection. Member names were checked against the UnityCsReference
// source for 6000.0 - 6000.7. If they ever change, it logs one warning and switches itself off.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UnityLibrary.UnityIgnore
{
    [InitializeOnLoad]
    static class UnityIgnore
    {
        const string IgnoreFile = ".unityignore";
        const string PrefKey = "UnityIgnore.Enabled";
        const string ToggleName = "Assets/Unity Ignore/Hide Ignored Files";
        const string ToggleMenu = ToggleName + " %&h";
        const string EditMenu = "Assets/Unity Ignore/Edit .unityignore";

        const string Template =
            "# .unityignore - hides matching files and folders from the Project window.\n" +
            "# gitignore syntax, paths relative to the project root, not case sensitive.\n" +
            "# Hidden assets are still imported and usable.\n" +
            "#\n" +
            "# desktop.ini\n" +
            "# *.psd\n" +
            "# /Assets/ScriptTemplates/\n";

        const BindingFlags Declared =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        struct Rule
        {
            public Regex regex;
            public bool negate, dirOnly;
        }

        static readonly List<Rule> s_Rules = new List<Rule>();
        static readonly Dictionary<string, bool> s_Cache = new Dictionary<string, bool>();

        // Objects already handled: filtered row lists / result arrays, and data sources that carry our hook.
        static readonly ConditionalWeakTable<object, object> s_Seen = new ConditionalWeakTable<object, object>();

        static IList s_Browsers;
        static MethodInfo s_ResetViews;
        static bool s_Enabled, s_Broken;
        static DateTime s_FileTime;
        static double s_NextFileCheck;

        static UnityIgnore()
        {
            s_Enabled = EditorUserSettings.GetConfigValue(PrefKey) != "0";
            LoadRules();
            EditorApplication.update += Tick;
            EditorApplication.projectChanged += s_Cache.Clear;
        }

        // ------------------------------------------------------------------ menu

        [MenuItem(ToggleMenu, false, 2000)]
        static void Toggle()
        {
            s_Enabled = !s_Enabled;
            EditorUserSettings.SetConfigValue(PrefKey, s_Enabled ? "1" : "0");
            Refresh();
        }

        [MenuItem(ToggleMenu, true)]
        static bool ToggleValidate()
        {
            Menu.SetChecked(ToggleName, s_Enabled);
            return true;
        }

        [MenuItem(EditMenu, false, 2001)]
        static void Edit()
        {
            if (!File.Exists(IgnoreFile))
                File.WriteAllText(IgnoreFile, Template);
            InternalEditorUtility.OpenFileAtLineExternal(Path.GetFullPath(IgnoreFile), 1);
        }

        // ------------------------------------------------------------------ rules

        static void LoadRules()
        {
            string[] lines;
            try
            {
                s_FileTime = File.GetLastWriteTimeUtc(IgnoreFile); // a missing file has a stable timestamp too
                lines = File.Exists(IgnoreFile) ? File.ReadAllLines(IgnoreFile) : new string[0];
            }
            catch (IOException)
            {
                s_FileTime = DateTime.MinValue; // file is mid-save, try again on the next check
                return;
            }

            s_Rules.Clear();
            s_Cache.Clear();

            for (int n = 0; n < lines.Length; n++)
            {
                string line = lines[n].Trim();
                if (line.Length == 0 || line[0] == '#')
                    continue;

                Rule rule = new Rule();
                rule.negate = line[0] == '!';
                if (rule.negate)
                    line = line.Substring(1);

                rule.dirOnly = line.EndsWith("/");
                line = line.TrimEnd('/');
                bool anchored = line.Contains("/"); // "/x" or "a/x" starts at the project root, plain "x" matches at any depth
                line = line.TrimStart('/');
                if (line.Length == 0)
                    continue;

                try
                {
                    rule.regex = new Regex(GlobToRegex(line, anchored), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    s_Rules.Add(rule);
                }
                catch (ArgumentException)
                {
                    Debug.LogWarning("[UnityIgnore] Skipping bad pattern on line " + (n + 1) + ": " + lines[n]);
                }
            }
        }

        static string GlobToRegex(string glob, bool anchored)
        {
            StringBuilder sb = new StringBuilder(anchored ? "^" : "^(?:.*/)?");

            for (int i = 0; i < glob.Length; i++)
            {
                char c = glob[i];
                int close = c == '[' && i + 2 < glob.Length ? glob.IndexOf(']', i + 2) : -1;

                if (c == '*' && i + 1 < glob.Length && glob[i + 1] == '*')
                {
                    i++;
                    if (i + 1 < glob.Length && glob[i + 1] == '/')
                    {
                        i++;
                        sb.Append("(?:.*/)?"); // "**/" = any number of folders, including none
                    }
                    else
                    {
                        sb.Append(".*");
                    }
                }
                else if (c == '*')
                {
                    sb.Append("[^/]*");
                }
                else if (c == '?')
                {
                    sb.Append("[^/]");
                }
                else if (close > 0)
                {
                    string set = glob.Substring(i + 1, close - i - 1);
                    bool not = set[0] == '!' || set[0] == '^';
                    if (not)
                        set = set.Substring(1);
                    set = set.Replace("\\", "\\\\").Replace("[", "\\[").Replace("]", "\\]").Replace("^", "\\^");
                    sb.Append(not ? "[^" : "[").Append(set).Append(']');
                    i = close;
                }
                else
                {
                    if (c == '\\' && i + 1 < glob.Length)
                        c = glob[++i];
                    sb.Append(Regex.Escape(c.ToString()));
                }
            }

            return sb.Append('$').ToString();
        }

        static bool IsHidden(string guid)
        {
            return !string.IsNullOrEmpty(guid) && IsIgnored(AssetDatabase.GUIDToAssetPath(guid));
        }

        static bool IsIgnored(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            bool ignored;
            if (s_Cache.TryGetValue(path, out ignored))
                return ignored;

            // A hidden folder hides everything inside it. Otherwise the last matching rule wins.
            int slash = path.LastIndexOf('/');
            ignored = slash > 0 && IsIgnored(path.Substring(0, slash));
            if (!ignored)
            {
                bool isDir = AssetDatabase.IsValidFolder(path);
                foreach (Rule rule in s_Rules)
                    if ((isDir || !rule.dirOnly) && rule.regex.IsMatch(path))
                        ignored = !rule.negate;
            }

            s_Cache[path] = ignored;
            return ignored;
        }

        // ------------------------------------------------------------------ project window

        static void Tick()
        {
            try
            {
                // Pick up edits to .unityignore (checked once a second).
                if (EditorApplication.timeSinceStartup >= s_NextFileCheck)
                {
                    s_NextFileCheck = EditorApplication.timeSinceStartup + 1;
                    if (File.GetLastWriteTimeUtc(IgnoreFile) != s_FileTime)
                    {
                        LoadRules();
                        if (s_Enabled)
                            Refresh();
                        return;
                    }
                }

                FilterAll();
            }
            catch (Exception e)
            {
                Fail(e);
            }
        }

        static bool Active
        {
            get { return s_Enabled && !s_Broken && s_Rules.Count > 0; }
        }

        static IList Browsers()
        {
            if (s_Browsers == null)
            {
                Type type = typeof(EditorWindow).Assembly.GetType("UnityEditor.ProjectBrowser", true);
                s_ResetViews = type.GetMethod("ResetViews", BindingFlags.Instance | BindingFlags.NonPublic);
                s_Browsers = (IList)type.GetMethod("GetAllProjectBrowsers").Invoke(null, null);
            }
            return s_Browsers;
        }

        static void FilterAll()
        {
            if (!Active)
                return;

            foreach (object browser in Browsers())
            {
                FilterTree(Get(browser, "m_AssetTree"));  // one column layout
                FilterTree(Get(browser, "m_FolderTree")); // two column layout: folder tree
                FilterList(browser);                      // two column layout: right pane, and search results
            }
        }

        // Rebuilds every Project window, the same way Unity does after an asset change.
        static void Refresh()
        {
            s_Cache.Clear();
            try
            {
                foreach (EditorWindow browser in Browsers())
                {
                    s_ResetViews.Invoke(browser, null);
                    browser.Repaint();
                }
                FilterAll();
            }
            catch (Exception e)
            {
                Fail(e);
            }
        }

        static void FilterTree(object tree)
        {
            object data = Get(tree, "data");
            if (data == null)
                return;

            if (FirstTime(data))
            {
                // Expanding or collapsing a folder rebuilds the rows lazily inside OnGUI.
                // This callback fires right after that, so nothing flashes.
                Action hook = delegate { FilterRows(data); };
                Set(data, "onVisibleRowsChanged", Delegate.Combine((Delegate)Get(data, "onVisibleRowsChanged"), hook));
            }

            FilterRows(data); // covers the rebuilds that skip the callback (ReloadData and friends)
        }

        static void FilterRows(object data)
        {
            if (!Active)
                return;

            try
            {
                IList rows = Get(data, "m_Rows") as IList;
                if (rows == null || !FirstTime(rows))
                    return;

                for (int i = rows.Count - 1; i >= 0; i--)
                {
                    object item = rows[i];
                    if (!IsHidden(Get(item, "Guid", true) as string))
                        continue;

                    rows.RemoveAt(i);

                    // Also detach it from its parent, so a folder with nothing visible left loses its foldout arrow.
                    IList siblings = Get(Get(item, "parent"), "children") as IList;
                    if (siblings != null)
                        siblings.Remove(item);
                }
            }
            catch (Exception e)
            {
                Fail(e);
            }
        }

        static void FilterList(object browser)
        {
            object hierarchy = Get(Get(Get(browser, "m_ListArea"), "m_LocalAssets"), "m_FilteredHierarchy");
            if (hierarchy == null)
                return;

            // Single "|" on purpose: both arrays have to be stripped.
            if (Strip(hierarchy, "m_Results") | Strip(hierarchy, "m_VisibleItems"))
                ((EditorWindow)browser).Repaint();
        }

        static bool Strip(object hierarchy, string field)
        {
            Array items = Get(hierarchy, field) as Array;
            if (items == null || !FirstTime(items))
                return false;

            List<object> kept = new List<object>(items.Length);
            foreach (object item in items)
                if (!IsHidden(Get(item, "guid") as string))
                    kept.Add(item);

            if (kept.Count == items.Length)
                return false;

            Array filtered = Array.CreateInstance(items.GetType().GetElementType(), kept.Count);
            for (int i = 0; i < kept.Count; i++)
                filtered.SetValue(kept[i], i);

            FirstTime(filtered);
            Set(hierarchy, field, filtered);
            return true;
        }

        // ------------------------------------------------------------------ helpers

        static bool FirstTime(object obj)
        {
            object unused;
            if (s_Seen.TryGetValue(obj, out unused))
                return false;
            s_Seen.Add(obj, null);
            return true;
        }

        // Reads a field or property by name, searching base classes too. Null in, null out.
        static object Get(object obj, string name, bool optional = false)
        {
            if (obj == null)
                return null;

            for (Type type = obj.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, Declared);
                if (field != null)
                    return field.GetValue(obj);

                PropertyInfo property = type.GetProperty(name, Declared);
                if (property != null)
                    return property.GetValue(obj, null);
            }

            if (optional)
                return null;
            throw new MissingMemberException(obj.GetType().Name, name);
        }

        static void Set(object obj, string name, object value)
        {
            for (Type type = obj.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, Declared);
                if (field != null)
                {
                    field.SetValue(obj, value);
                    return;
                }
            }
            throw new MissingMemberException(obj.GetType().Name, name);
        }

        static void Fail(Exception e)
        {
            if (s_Broken)
                return;

            s_Broken = true;
            EditorApplication.update -= Tick;
            Debug.LogWarning("[UnityIgnore] The Project window internals look different in this Unity version, so hiding is switched off.\n" + e);
        }
    }
}
