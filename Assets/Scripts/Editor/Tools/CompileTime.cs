// compile time tracking tool (and quickly enable disable settings, and scene autosave option)
// original source: https://gist.github.com/spajus/72a74146b1bbeddd44a66e1b8a3c829c
// created by https://github.com/spajus twitch https://www.twitch.tv/dev_spajus
// 02.07.2020 added unity_2018_4 check and set autosave off by default - unitycoder.com

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityLibrary
{
    class CompileTime : EditorWindow
    {
        bool allowProfiler = false;
        bool isTrackingTime;
        bool isLockReload;
        bool isAutoSave = false; // autosave default off
        bool isLocked;
        bool restartAfterCompile;
        bool memoryWarned;
        string lastReloadTime = "";
        string lastCompileTime = "";
        string lastAssCompileTime = "";
        double startTime, finishTime, compileTime, reloadTime;
        double assStartTime, assFinishTime, assCompileTime;
        Dictionary<string, DateTime> startTimes = new Dictionary<string, DateTime>();
        List<string> lastAssCompile;

        [MenuItem("Tools/UnityLibrary/Compile Time")]
        public static void Init()
        {
            EditorWindow.GetWindow(typeof(CompileTime));
        }

        void Update()
        {
            if (isLockReload) { return; }
            if (EditorApplication.isCompiling && !isTrackingTime)
            {
                if (EditorApplication.isPlaying)
                {
                    restartAfterCompile = true;
                    EditorApplication.isPlaying = false;
                }
                startTime = EditorApplication.timeSinceStartup;
                lastReloadTime = "Reloading now";
                lastCompileTime = "Compiling now";
                lastAssCompile = new List<string>();
                Debug.Log("Started compiling scripts");
                isTrackingTime = true;
            }
            else if (!EditorApplication.isCompiling && isTrackingTime)
            {
                finishTime = EditorApplication.timeSinceStartup;
                isTrackingTime = false;
                EditorApplication.Beep();
                reloadTime = finishTime - startTime;
                lastReloadTime = reloadTime.ToString("0.000") + "s";
                compileTime = reloadTime - assCompileTime;
                lastCompileTime = compileTime.ToString("0.000") + "s";
                if (isAutoSave && !EditorApplication.isPlaying)
                {
                    Debug.Log("Auto Saving Scene");
                    EditorSceneManager.SaveOpenScenes();
                }
                if (restartAfterCompile)
                {
                    restartAfterCompile = false;
                    EditorApplication.isPlaying = true;
                }
            }
        }

        void OnGUI()
        {
#if UNITY_2018_4_OR_NEWER
        // Toggle domain reload
        var playModeOptsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
        playModeOptsEnabled = EditorGUILayout.Toggle("Disable Domain Reload", playModeOptsEnabled);
        EditorSettings.enterPlayModeOptionsEnabled = playModeOptsEnabled;
#endif

            if (UnityEngine.Profiling.Profiler.enabled)
            {
                EditorGUILayout.LabelField("PROFILER ENABLED");
            }
            allowProfiler = EditorGUILayout.Toggle("Allow profiler", allowProfiler);
            if (!allowProfiler && UnityEngine.Profiling.Profiler.enabled)
            {
                UnityEngine.Profiling.Profiler.enabled = false;
            }
            EditorGUILayout.LabelField("Time", Time.realtimeSinceStartup.ToString());
            float m1 = (UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024f / 1024f);
            float m2 = (UnityEngine.Profiling.Profiler.GetAllocatedMemoryForGraphicsDriver() / 1024f / 1024f);
            float m3 = (UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / 1024f / 1024f);
            float m = m1 + m2 + m3;
            if (m > 10000 && !memoryWarned)
            {
                memoryWarned = true;
                EditorApplication.Beep();
                Debug.LogError("Memory over 10000MB!");
                allowProfiler = false;
                UnityEngine.Profiling.Profiler.enabled = false;
            }
            if (m < 8000 && memoryWarned)
            {
                memoryWarned = false;
            }
            EditorGUILayout.LabelField("Memory used:", m.ToString("0.00") + " MB");

            isLockReload = EditorGUILayout.Toggle("Lock assembly reload", isLockReload);
            isAutoSave = EditorGUILayout.Toggle("Auto Save", isAutoSave);
            EditorGUILayout.LabelField("Full reload", lastReloadTime);
            EditorGUILayout.LabelField("Compile", lastCompileTime);
            if (lastAssCompileTime != null)
            {
                EditorGUILayout.LabelField("Assembly reload", lastAssCompileTime);
            }
            // For mysterious reason, iterating over a dictionary doesn't work, it gets empty!
            if (lastAssCompile != null)
            {
                foreach (string s in lastAssCompile)
                {
                    var ss = s.Split(':');
                    EditorGUILayout.LabelField(ss[0], ss[1]);
                }
            }

            if (isLockReload)
            {
                if (!isLocked)
                {
                    Debug.Log("Locking reload of assemblies");
                    EditorApplication.LockReloadAssemblies();
                    isLocked = true;
                }
            }
            else
            {
                if (isLocked)
                {
                    Debug.Log("Unlocking reload of assemblies");
                    EditorApplication.UnlockReloadAssemblies();
                    isLocked = false;
                }
            }
        }

        void OnBeforeAssemblyReload()
        {
            assStartTime = EditorApplication.timeSinceStartup;
            this.ShowNotification(new GUIContent("Started assembly reload"));
        }

        void OnAfterAssemblyReload()
        {
            assFinishTime = EditorApplication.timeSinceStartup;
            assCompileTime = assFinishTime - assStartTime;
            lastAssCompileTime = assCompileTime.ToString("0.000") + "s";
        }

        void CompilationPipelineOnAssemblyCompilationStarted(string assembly)
        {
            Debug.Log("Assembly compile started: " + assembly);
            startTimes[assembly] = DateTime.UtcNow;
        }

        void CompilationPipelineOnAssemblyCompilationFinished(string assembly, CompilerMessage[] arg2)
        {
            var time = startTimes[assembly];
            var timeSpan = DateTime.UtcNow - startTimes[assembly];
            var bt = string.Format("{0:0.00}s", timeSpan.TotalMilliseconds / 1000f);
            var cleanAss = assembly.Replace("Library/ScriptAssemblies/", "");

            if (lastAssCompile != null)
            {
                lastAssCompile.Add(cleanAss + ":" + bt);
            }
            Debug.Log("Assembly compile ended: " + assembly + " in " + bt);
        }

        void OnEnable()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            CompilationPipeline.assemblyCompilationStarted += CompilationPipelineOnAssemblyCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += CompilationPipelineOnAssemblyCompilationFinished;
        }

        void OnDisable()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
            CompilationPipeline.assemblyCompilationStarted -= CompilationPipelineOnAssemblyCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished -= CompilationPipelineOnAssemblyCompilationFinished;
        }
    }
}
