using UnityEditor;
using UnityEngine;

namespace BTools.BComponentComparator.Editor
{
    /// <summary>
    /// Adds context menu integration for Components to quickly add them to the Comparator
    /// </summary>
    public static class ComponentContextMenu
    {
        private const string menuPath = "CONTEXT/Object/Add to Comparator";
        private const int menuPriority = 49;

        [MenuItem(menuPath, false, menuPriority)]
        private static void AddToComparator(MenuCommand command)
        {
            if (!TryExtractedObjectAndType(command.context, out var targetObject, out _))
            {
                return;
            }

            var window = EditorWindow.GetWindow<BComponentComparatorWindow>();

            window.titleContent = new GUIContent("BComponentComparator");

            window.Show();
            window.Focus();

            // Check if the current window type is compatible with the target object
            if (window.CurrentComponentType != null && 
                DragDropHandler.IsValidObject(targetObject, window.CurrentComponentType))
            {
                // If compatible, just add to the list
                window.AddObjectToList(targetObject);
            }
            else
            {
                // Otherwise, reset the type to the new object's type
                window.AssignTypeFromObject(targetObject);
            }
        }

        [MenuItem(menuPath, true, menuPriority)]
        private static bool ValidateAddToComparator(MenuCommand command)
        {
            return TryExtractedObjectAndType(command.context, out _, out _);
        }

        private static bool TryExtractedObjectAndType(
            Object obj,
            out Object targetObject,
            out System.Type componentType)
        {
            targetObject = null;
            componentType = null;

            if (obj == null)
            {
                return false;
            }

            targetObject = (obj is AssetImporter importer) ? AssetDatabase.LoadAssetAtPath<Object>(importer.assetPath) : obj;

            return DragDropHandler.TryExtractedType(targetObject, out componentType);
        }
    }
}