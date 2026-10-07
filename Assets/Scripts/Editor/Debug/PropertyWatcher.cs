// Property Watcher - "who changed this?" for Inspector properties.
// Put this file in any Editor folder (Unity 2022.1 or newer).
//
//   1. Right-click a property in the Inspector > Watch Changes.
//   2. Enter Play mode. Window > Analysis > Property Watcher lists every change and its source.
//
// The value is compared at checkpoints in the player loop, so every change is caught and pinned
// to a loop stage (scripts, physics, animation...). 
// If 0Harmony.dll (Harmony 2.2 or newer, the net48 build from the Harmony-Fat zip, set to Editor only) is in the project,
// download dll https://github.com/pardeike/Harmony/releases (copy to Assets/Plugins/PropertyWatcher or so)
// the code that can write the value is hooked as well, and the entry names the calling script with a call stack
// (click a row for the stack, double-click to open the file):
//   - built-in components: the component's own void methods, e.g. Transform.set_position
//   - script fields:       every method whose IL writes the field

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public class PropertyWatcher : EditorWindow
{
    const int MaxHistory = 30;
    const string HarmonyId = "PropertyWatcher";
    const BindingFlags Declared = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    [Serializable]
    class Watch
    {
        public Object target;                       // serialized by the window, survives domain reloads
        public string path;
        [NonSerialized] public SerializedObject so;
        [NonSerialized] public uint hash;
        [NonSerialized] public string label, value;
        [NonSerialized] public bool hooked, byInstance;
        [NonSerialized] public List<Entry> history;
    }

    class Entry
    {
        public string time, source, from, to, stack, file;
        public int line, count = 1;
    }

    [Serializable] class Ref { public Object obj; }

    struct Checkpoint { }                           // marks our systems in the player loop

    [SerializeField] List<Watch> watches = new List<Watch>();
    [SerializeField] bool pauseOnChange;
    Vector2 scroll;
    Entry selected;

    static PropertyWatcher instance;
    static int mainThread;
    static bool busy, armed;

    // ---------------------------------------------------------------- setup

    [InitializeOnLoadMethod]
    static void Init()
    {
        EditorApplication.contextualPropertyMenu += OnPropertyMenu;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        SceneManager.sceneLoaded += (scene, mode) => Rebind();
    }

    [MenuItem("Window/Analysis/Property Watcher")]
    static void Open() => GetWindow<PropertyWatcher>("Property Watcher");

    void OnEnable()
    {
        instance = this;
        mainThread = Thread.CurrentThread.ManagedThreadId;
        // After the domain reload that starts Play mode this runs before any game code, so the hooks
        // are in place before the game's methods get JIT-compiled (the JIT can inline small setters).
        if (EditorApplication.isPlayingOrWillChangePlaymode) { Arm(); return; }
        watches.RemoveAll(w => !w.target);                 // e.g. left over from a closed scene
        Rebind();
    }

    void OnDisable()
    {
        Disarm();
        instance = null;
    }

    static void OnPropertyMenu(GenericMenu menu, SerializedProperty property)
    {
        var targets = property.serializedObject.targetObjects;
        string path = property.propertyPath;
        bool watched = instance && instance.watches.Any(w => w.target == targets[0] && w.path == path);

        menu.AddItem(new GUIContent(watched ? "Stop Watching Changes" : "Watch Changes"), false, () =>
        {
            var window = GetWindow<PropertyWatcher>("Property Watcher");
            foreach (var t in targets)
            {
                foreach (var old in window.watches.Where(w => w.target == t && w.path == path).ToArray()) Remove(old);
                if (watched) continue;
                var watch = new Watch { target = t, path = path };
                window.watches.Add(watch);
                Bind(watch);
            }
        });
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!instance) return;
        if (state == PlayModeStateChange.ExitingEditMode)
            foreach (var w in instance.watches) w.history?.Clear();
        else if (state == PlayModeStateChange.EnteredPlayMode) Arm();
        else if (state == PlayModeStateChange.ExitingPlayMode) Disarm();
        else if (state == PlayModeStateChange.EnteredEditMode) Rebind();   // history stays readable
    }

    static void Arm()
    {
        armed = true;
        SetCheckpoints(true);
        Rebind();
    }

    static void Disarm()
    {
        armed = false;
        SetCheckpoints(false);
        Unhook();
        if (instance) foreach (var w in instance.watches) w.hooked = false;
    }

    static void Rebind()
    {
        if (instance) foreach (var w in instance.watches) Bind(w);
    }

    static void Bind(Watch w)
    {
        w.history ??= new List<Entry>();
        w.target = Refresh(w.target);
        w.so = w.target ? new SerializedObject(w.target) : null;
        if (w.so == null) return;

        var p = w.so.FindProperty(w.path);
        string component = ObjectNames.NicifyVariableName(w.target.GetType().Name);
        w.label = w.target.name + "  >  " + component + "  >  " + (p != null ? p.displayName : w.path);
        if (p != null) { w.hash = p.contentHash; w.value = Format(p); }

        if (armed && !w.hooked) { w.hooked = true; Hook(w); }
    }

    // Entering and exiting Play mode reloads the scene: the C# wrapper can go stale while the
    // instance ID lives on. A JSON round trip resolves the reference again by ID.
    static Object Refresh(Object obj)
    {
        var r = new Ref { obj = obj };
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(r), r);
        return r.obj ? r.obj : obj;
    }

    static void Remove(Watch w)
    {
        if (instance) instance.watches.Remove(w);
        foreach (var list in hooks.Values) list.Remove(w);
    }

    // ---------------------------------------------------------------- change detection

    static readonly string[] Stages = { "Script", "Physics", "Director", "Animation", "AI", "Constraint", "Particle", "Cloth" };

    // Adds (or strips) a value check after every interesting player-loop system and at the end of each phase.
    static void SetCheckpoints(bool install)
    {
        var loop = PlayerLoop.GetCurrentPlayerLoop();
        var phases = loop.subSystemList;
        if (phases == null) return;

        for (int i = 0; i < phases.Length; i++)
        {
            string phase = phases[i].type?.Name;
            var list = new List<PlayerLoopSystem>();
            if (install && i == 0) list.Add(MakeCheckpoint("Editor: Inspector or editor code, between frames"));

            foreach (var system in phases[i].subSystemList ?? new PlayerLoopSystem[0])
            {
                if (system.type == typeof(Checkpoint)) continue;
                list.Add(system);
                string name = system.type?.Name ?? "";
                if (install && Stages.Any(k => name.Contains(k)))
                    list.Add(MakeCheckpoint((name.Contains("Script") ? "Scripts: " : "Engine: ") + phase + "." + name));
            }

            if (install && (list.Count == 0 || list[list.Count - 1].type != typeof(Checkpoint)))
                list.Add(MakeCheckpoint("Other: " + phase));
            phases[i].subSystemList = list.ToArray();
        }
        PlayerLoop.SetPlayerLoop(loop);
    }

    static PlayerLoopSystem MakeCheckpoint(string label) => new PlayerLoopSystem
    {
        type = typeof(Checkpoint),
        updateDelegate = () =>
        {
            if (instance) foreach (var w in instance.watches) Check(w, null, false, label);
        }
    };

    // via == null: player-loop checkpoint.  Otherwise called right before / after a hooked method.
    // Runs inside game code when hooked, so it must never throw.
    static void Check(Watch w, MethodBase via, bool after, string label)
    {
        if (busy || w.so == null || !w.target) return;
        busy = true;                                // so.Update() can run user serialization callbacks
        try
        {
            w.so.Update();
            var p = w.so.FindProperty(w.path);
            if (p == null || p.contentHash == w.hash) return;

            var e = via == null ? new Entry { source = label }
                  : after ? Describe(via)
                  : new Entry { source = "Unknown (changed before " + Pretty(via) + " ran)" };
            e.time = DateTime.Now.ToString("HH:mm:ss") + "  #" + Time.frameCount;
            e.from = w.value;
            e.to = w.value = Format(p);
            w.hash = p.contentHash;

            var last = w.history.Count > 0 ? w.history[0] : null;
            if (last != null && last.source == e.source && last.line == e.line)
            {
                last.count++;                       // same source again: update the row instead of adding one
                last.time = e.time; last.from = e.from; last.to = e.to; last.stack = e.stack;
            }
            else
            {
                w.history.Insert(0, e);
                if (w.history.Count > MaxHistory) w.history.RemoveAt(MaxHistory);
            }

            if (instance && instance.pauseOnChange) Debug.Break();
        }
        catch (Exception) { }                       // e.g. target destroyed mid-frame
        finally { busy = false; }
    }

    static string Format(SerializedProperty p)
    {
        try
        {
            switch (p.propertyType)
            {
                case SerializedPropertyType.Quaternion: return Text(p.quaternionValue.eulerAngles);
                case SerializedPropertyType.ObjectReference: return p.objectReferenceValue ? p.objectReferenceValue.name : "None";
                case SerializedPropertyType.Enum: return p.enumValueIndex >= 0 ? p.enumDisplayNames[p.enumValueIndex] : p.intValue.ToString();
                case SerializedPropertyType.Generic: return (p.isArray ? p.arraySize + " items" : p.type) + "  #" + p.contentHash.ToString("X8");
                default: return Text(p.boxedValue);
            }
        }
        catch { return "#" + p.contentHash.ToString("X8"); }
    }

    static string Text(object v) => v is IFormattable f ? f.ToString("0.###", CultureInfo.InvariantCulture) : v?.ToString() ?? "null";

    // ---------------------------------------------------------------- attribution (optional Harmony)

    static Type harmonyType = FindType("HarmonyLib.Harmony");
    static object harmony, prefix, postfix;
    static MethodInfo patchMethod, originalOf;
    static readonly Dictionary<IntPtr, List<Watch>> hooks = new Dictionary<IntPtr, List<Watch>>();   // by method handle
    static readonly string[] NotUser = { "UnityEngine", "UnityEditor", "Unity.Mathematics", "System", "mscorlib", "netstandard", "Mono.", "0Harmony", "MonoMod" };

    static Type FindType(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).FirstOrDefault(t => t != null);

    static bool IsUserCode(Type t)
    {
        string assembly = t.Assembly.GetName().Name;
        return !NotUser.Any(n => assembly.StartsWith(n));
    }

    // Harmony is used through reflection so the tool compiles with or without the DLL.
    static bool InitHarmony()
    {
        if (harmony != null) return true;
        if (harmonyType == null) return false;
        try
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            var wrap = harmonyType.Assembly.GetType("HarmonyLib.HarmonyMethod").GetConstructor(new[] { typeof(MethodInfo) });
            prefix = wrap.Invoke(new object[] { typeof(PropertyWatcher).GetMethod(nameof(Pre), flags) });
            postfix = wrap.Invoke(new object[] { typeof(PropertyWatcher).GetMethod(nameof(Post), flags) });
            patchMethod = harmonyType.GetMethods().First(m => m.Name == "Patch" && m.GetParameters().Length >= 3);
            originalOf = harmonyType.GetMethod("GetOriginalMethodFromStackframe", new[] { typeof(StackFrame) });
            harmony = Activator.CreateInstance(harmonyType, HarmonyId);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Property Watcher: Harmony was found but could not be set up: " + e.Message);
            harmonyType = null;
            return false;
        }
    }

    static void Hook(Watch w)
    {
        if (!InitHarmony()) return;
        var field = FindUserField(w.target.GetType(), w.path);
        w.byInstance = field == null;

        if (field != null)
        {
            // Script field: hook every method that writes it; any instance may be the one written.
            foreach (var m in FindWriters(field)) Patch(m, w);
            return;
        }

        // Engine property: hook the component's own void methods (setters, Translate, SetParent...).
        for (var t = w.target.GetType(); t != null && t != typeof(Component) && t != typeof(Object); t = t.BaseType)
        {
            if (IsUserCode(t)) continue;
            foreach (var m in t.GetMethods(Declared))
                if (m.ReturnType == typeof(void) && !m.ContainsGenericParameters && !m.Name.StartsWith("Get") && IL(m) != null)
                    Patch(m, w);
        }
    }

    static void Patch(MethodBase m, Watch w)
    {
        if (!hooks.TryGetValue(m.MethodHandle.Value, out var list))
        {
            try
            {
                var args = new object[patchMethod.GetParameters().Length];   // (original, prefix, postfix, transpiler, finalizer)
                args[0] = m; args[1] = prefix; args[2] = postfix;
                patchMethod.Invoke(harmony, args);
            }
            catch { return; }                       // not patchable, skip
            hooks[m.MethodHandle.Value] = list = new List<Watch>();
        }
        if (!list.Contains(w)) list.Add(w);
    }

    static void Unhook()
    {
        if (harmony != null && hooks.Count > 0)
        {
            var unpatch = harmonyType.GetMethod("UnpatchSelf", Type.EmptyTypes) ?? harmonyType.GetMethod("UnpatchAll", new[] { typeof(string) });
            unpatch?.Invoke(harmony, unpatch.GetParameters().Length == 0 ? null : new object[] { HarmonyId });
        }
        hooks.Clear();
    }

    // Harmony injects these parameters by name.
    static void Pre(object __instance, MethodBase __originalMethod) => OnHook(__instance, __originalMethod, false);
    static void Post(object __instance, MethodBase __originalMethod) => OnHook(__instance, __originalMethod, true);

    static void OnHook(object target, MethodBase method, bool after)
    {
        if (busy || Thread.CurrentThread.ManagedThreadId != mainThread) return;
        if (!hooks.TryGetValue(method.MethodHandle.Value, out var list)) return;
        for (int i = 0; i < list.Count; i++)
        {
            var w = list[i];
            if (!w.byInstance || (target is Object o && o == w.target)) Check(w, method, after, null);
        }
    }

    // Builds the "who" text and call stack for a change that a hooked method just made.
    static Entry Describe(MethodBase via)
    {
        var e = new Entry();
        var lines = new List<string>();
        string who = null, first = null;

        foreach (var frame in new StackTrace(true).GetFrames() ?? new StackFrame[0])
        {
            var m = FrameMethod(frame);
            if (m == null || m.DeclaringType == null || m.DeclaringType == typeof(PropertyWatcher)) continue;
            if (m.DeclaringType == via.DeclaringType && m.Name == via.Name) continue;      // the hooked method itself
            if (m.DeclaringType.Name.StartsWith("DynamicMethodDefinition")) continue;      // unresolved Harmony frame

            string name = Pretty(m), file = frame.GetFileName();
            bool hasFile = !string.IsNullOrEmpty(file);
            lines.Add(name + (hasFile ? "    " + Path.GetFileName(file) + ":" + frame.GetFileLineNumber() : ""));
            first ??= name;

            if (!IsUserCode(m.DeclaringType)) continue;
            who ??= name;
            if (e.file == null && hasFile) { e.file = file; e.line = frame.GetFileLineNumber(); }   // hooked methods have no line info
        }

        who ??= first;
        string writer = Pretty(via);
        e.source = !IsUserCode(via.DeclaringType) ? (who ?? "Unity") + "   ->  " + writer   // script, then the engine setter it called
                 : who == null || who == writer ? writer
                 : writer + "   <-  " + who;                                              // field writer, then its caller
        e.stack = string.Join("\n", lines);
        return e;
    }

    // A hooked method shows up on the stack as Harmony's replacement; map it back to the original.
    static MethodBase FrameMethod(StackFrame frame)
    {
        MethodBase m = null;
        try { m = originalOf?.Invoke(null, new object[] { frame }) as MethodBase; } catch { }
        return m ?? frame.GetMethod();
    }

    static string Pretty(MethodBase m)
    {
        var t = m.DeclaringType;
        string name = m.Name;
        while (t != null && t.IsNested && t.Name.StartsWith("<"))            // lambda, coroutine, async
        {
            if (name == "MoveNext") name = t.Name;
            t = t.DeclaringType;
        }
        int end = name.IndexOf('>');
        if (name.StartsWith("<") && end > 1) name = name.Substring(1, end - 1);
        return (t != null ? t.Name + "." : "") + name;
    }

    // Deepest field on the property path that is declared in project code (null for native properties).
    static FieldInfo FindUserField(Type type, string path)
    {
        FieldInfo best = null;
        foreach (string part in path.Replace(".Array.data[", "[").Split('.'))
        {
            int bracket = part.IndexOf('[');
            string name = bracket < 0 ? part : part.Substring(0, bracket);

            FieldInfo field = null;
            for (var t = type; t != null && field == null; t = t.BaseType) field = t.GetField(name, Declared);
            if (field == null) break;

            if (IsUserCode(field.DeclaringType)) best = field;
            type = field.FieldType;
            if (bracket >= 0) type = type.IsArray ? type.GetElementType() : type.IsGenericType ? type.GetGenericArguments()[0] : type;
        }
        return best;
    }

    static IEnumerable<MethodBase> FindWriters(FieldInfo field)
    {
        var home = field.DeclaringType.Assembly;
        string homeName = home.GetName().Name;

        // A private field can only be written by its own class (and the lambdas/coroutines nested in it).
        IEnumerable<Type> types = field.IsPrivate
            ? WithNested(field.DeclaringType)
            : AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && (a == home || (!field.IsAssembly && a.GetReferencedAssemblies().Any(r => r.Name == homeName))))
                .SelectMany(SafeTypes);

        foreach (var t in types)
        {
            if (t.IsValueType) continue;            // struct methods are left alone, they fall back to the loop stage
            foreach (var m in t.GetMethods(Declared | BindingFlags.Static))
                if (!m.ContainsGenericParameters && Writes(m, field)) yield return m;
        }
    }

    static IEnumerable<Type> WithNested(Type t) =>
        new[] { t }.Concat(t.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).SelectMany(WithNested));

    static IEnumerable<Type> SafeTypes(Assembly a)
    {
        try { return a.GetTypes(); }
        catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
    }

    static byte[] IL(MethodBase m)
    {
        try { return m.GetMethodBody()?.GetILAsByteArray(); }
        catch { return null; }
    }

    // True if the method contains stfld / ldflda for the field (ldflda covers "field.x = ..." and ref use).
    static bool Writes(MethodBase m, FieldInfo field)
    {
        var il = IL(m);
        if (il == null) return false;

        for (int i = 0; i + 4 < il.Length; i++)
        {
            if (il[i] != 0x7D && il[i] != 0x7C) continue;                   // stfld, ldflda
            int token = BitConverter.ToInt32(il, i + 1);
            int table = token >> 24;
            if (table != 0x04 && table != 0x0A) continue;                   // FieldDef, MemberRef
            try
            {
                var f = m.Module.ResolveField(token);
                if (f.Module == field.Module && f.MetadataToken == field.MetadataToken) return true;
            }
            catch { }                                                       // the bytes were not a field token
        }
        return false;
    }

    // ---------------------------------------------------------------- window

    void OnInspectorUpdate() => Repaint();

    void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            pauseOnChange = GUILayout.Toggle(pauseOnChange, "Pause on change", EditorStyles.toolbarButton);
            if (GUILayout.Button("Clear history", EditorStyles.toolbarButton))
                foreach (var w in watches) w.history?.Clear();
            GUILayout.FlexibleSpace();
            GUILayout.Label(harmonyType != null ? "Harmony: on" : "Harmony: not found", EditorStyles.miniLabel);
        }

        if (harmonyType == null)
            EditorGUILayout.HelpBox("Without Harmony only the player-loop stage of a change is known. Add 0Harmony.dll (Harmony 2.2+, net48 build) to the project to see the calling script.", MessageType.Info);
        if (watches.Count == 0)
            EditorGUILayout.HelpBox("Right-click a property in the Inspector and choose \"Watch Changes\".", MessageType.Info);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var w in watches.ToArray()) DrawWatch(w);
        EditorGUILayout.EndScrollView();
    }

    void DrawWatch(Watch w)
    {
        w.history ??= new List<Entry>();
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(w.label ?? w.path, EditorStyles.boldLabel)) EditorGUIUtility.PingObject(w.target);
                if (GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(60))) Remove(w);
            }

            if (!w.target || w.so == null)
            {
                EditorGUILayout.LabelField("Object is gone.");
                return;
            }

            if (!EditorApplication.isPlaying)       // outside Play mode just mirror the Inspector
            {
                w.so.Update();
                var p = w.so.FindProperty(w.path);
                if (p != null) w.value = Format(p);
            }

            EditorGUILayout.LabelField("Current", w.value);
            EditorGUILayout.LabelField("Last changed by", w.history.Count > 0 ? w.history[0].source : "-");

            var ev = Event.current;
            foreach (var e in w.history)
            {
                string row = e.time + "    " + e.source + (e.count > 1 ? "  x" + e.count : "") + "    " + e.from + " -> " + e.to;
                var style = e == selected ? EditorStyles.boldLabel : EditorStyles.label;
                var rect = GUILayoutUtility.GetRect(new GUIContent(row), style);

                // click: show the stack, double-click: open the script
                if (ev.type == EventType.MouseDown && rect.Contains(ev.mousePosition))
                {
                    selected = e;
                    if (ev.clickCount == 2 && !string.IsNullOrEmpty(e.file))
                        InternalEditorUtility.OpenFileAtLineExternal(e.file, e.line);
                    ev.Use();
                }
                GUI.Label(rect, row, style);
            }

            if (selected != null && w.history.Contains(selected) && !string.IsNullOrEmpty(selected.stack))
                EditorGUILayout.HelpBox(selected.stack, MessageType.None);
        }
    }
}
