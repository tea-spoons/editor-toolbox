
namespace TeaSpoons.EditorToolbox.Editor
{
    using UnityEngine;
    using System.IO;
    using System.Reflection;
    using UnityEditor;

    /// <summary>
    /// Miscellaneous methods and properties for working with paths in the editor.
    /// </summary>
    public static class ProjectPathUtility
    {
        /// <summary>
        /// The root path of the project, containing Assets, Library, Packages, etc.
        /// </summary>
        public static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

        /// <summary>
        /// Returns the project-relative path (<c>Assets/...</c>) that's currently open in the project view.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the path could be found, <c>false</c> if there was a problem finding it.
        /// </returns>
        public static bool TryGetActiveFolderPath(out string path)
        {
            path = null;

            var tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod("TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

            object[] args = new object[] { null };
            bool found = (bool)tryGetActiveFolderPath.Invoke(null, args);
            if (found)
            {
                path = (string)args[0] + "/";
            }
            return found;
        }

        /// <summary>
        /// Returns the full path (<c>C:/.../Assets/...</c>) that's currently open in the project view.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the path could be found, <c>false</c> if there was a problem finding it.
        /// </returns>
        public static bool TryGetFullActiveFolderPath(out string path)
        {
            path = null;

            var found = TryGetActiveFolderPath(out var relativePath);
            if (found)
            { 
                path = Path.Combine(ProjectRoot, relativePath);
            }
            return found;
        }
    }
}
