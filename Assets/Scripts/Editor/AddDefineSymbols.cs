using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Adds the given define symbols to PlayerSettings define symbols.
/// Just add your own define symbols to the Symbols property at the below.
/// </summary>
[InitializeOnLoad]
public class AddDefineSymbols : Editor
{

    /// <summary>
    /// Symbols that will be added to the editor
    /// </summary>
    public static readonly string [] Symbols = new string[] {
        "MYCOMPANY",
        "MYCOMPANY_MYPACKAGE"
    };

    /// <summary>
    /// Add define symbols as soon as Unity gets done compiling.
    /// </summary>
    static AddDefineSymbols ()
    {
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup ( EditorUserBuildSettings.selectedBuildTargetGroup );
        List<string> allDefines = definesString.Split ( ';' ).ToList ();
        allDefines.AddRange ( Symbols.Except ( allDefines ) );
        PlayerSettings.SetScriptingDefineSymbolsForGroup (
            EditorUserBuildSettings.selectedBuildTargetGroup,
            string.Join ( ";", allDefines.ToArray () ) );
    }

}
