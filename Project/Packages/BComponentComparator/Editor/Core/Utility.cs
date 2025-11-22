using System;
using UnityEditor;

namespace BTools.BComponentComparator.Editor
{
    /// <summary>
    /// General utility methods for the BComponentComparator tool.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Try to get the type of the inherited AssetImporter for the given object.
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <param name="importerType">Output importer type</param>
        /// <returns>True if inherited importer type found</returns>
        public static bool TryGetInheritedImporter(UnityEngine.Object obj, out Type importerType)
        {
            importerType = null;

            if (obj == null)
            {
                return false;
            }

            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            var importer = AssetImporter.GetAtPath(path);
            var isInheritedImporter = (importer != null) && (importer.GetType() != typeof(AssetImporter));

            if (!isInheritedImporter)
            {
                return false;
            }

            importerType = importer.GetType();
            return true;
        }
    }
}