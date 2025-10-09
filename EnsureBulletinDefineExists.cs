#if UNITY_EDITOR

using System;
using System.Linq;
using UnityEditor;


#pragma warning disable IDE0130
namespace UnityBulletin
{
    /// <summary>
    /// Ensures that the Bulletin scripting define symbol exists in the project.
    /// This is just so that you can conditionally enable/disable code related to Bulletin in other libraries with #if BULLETIN.
    /// </summary>
    internal static class EnsureBulletinDefineExists
    {
        private static readonly string[] DEFINES = new string[] { "BULLETIN" };

        [InitializeOnLoadMethod]
        private static void EnsureScriptingDefineSymbol()
        {
            var currentTarget = EditorUserBuildSettings.selectedBuildTargetGroup;

            if (currentTarget == BuildTargetGroup.Unknown)
            {
                return;
            }

            #pragma warning disable CS0618 // PlayerSettings.GetScriptingDefineSymbolsForGroup is obsolete
            var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentTarget).Trim();
            #pragma warning restore CS0618

            var defines = definesString.Split(';');

            bool changed = false;

            foreach (var define in DEFINES)
            {
                if (defines.Contains(define) == false)
                {
                    if (definesString.EndsWith(";", StringComparison.InvariantCulture) == false)
                    {
                        definesString += ";";
                    }

                    definesString += define;
                    changed = true;
                }
            }

            if (changed)
            {
                #pragma warning disable CS0618 // PlayerSettings.SetScriptingDefineSymbolsForGroup is obsolete
                PlayerSettings.SetScriptingDefineSymbolsForGroup(currentTarget, definesString);
                #pragma warning restore CS0618
            }
        }
    }
}
#endif
