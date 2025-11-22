using UnityEditor;
using UnityEngine;

namespace BTools.BComponentComparator.Editor
{
    /// <summary>
    /// Adds context menu integration for Components to quickly add them to the Comparator
    /// </summary>
    public static class ComponentContextMenu
    {
        private const string menuPath = "CONTEXT/Component/Add to Comparator";
        private const int menuPriority = 1500;

        [MenuItem(menuPath, false, menuPriority)]
        private static void AddToComparator(MenuCommand command)
        {
            var component = command.context as Component;
            if (component == null)
            {
                return;
            }

            // Open or focus the Comparator window
            var window = EditorWindow.GetWindow<BComponentComparatorWindow>();
            window.titleContent = new GUIContent("BComponentComparator");
            window.Show();
            window.Focus();

            // Add the component to the window
            window.AddComponentFromContextMenu(component);
        }

        [MenuItem(menuPath, true)]
        private static bool ValidateAddToComparator(MenuCommand command)
        {
            // Always enable the menu item for any Component
            return command.context is Component;
        }
    }
}