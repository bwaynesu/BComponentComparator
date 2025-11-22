using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Get the inheritance chain of a type, from the type itself up to (but excluding) System.Object/UnityEngine.Object.
        /// </summary>
        /// <param name="type">The starting type.</param>
        /// <returns>List of types in the inheritance chain.</returns>
        public static List<Type> GetInheritanceChain(Type type)
        {
            var chain = new List<Type>();
            if (type == null)
            {
                return chain;
            }

            var curType = type;
            while (curType != null)
            {
                // Filter out System.Object, UnityEngine.Object, and System.* namespace types
                if (curType == typeof(object) ||
                    curType == typeof(UnityEngine.Object) ||
                    (curType.Namespace != null && curType.Namespace.StartsWith("System")))
                {
                    break;
                }

                chain.Add(curType);
                curType = curType.BaseType;
            }

            return chain;
        }
    }
}