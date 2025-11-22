using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BTools.BComponentComparator.Editor
{
    /// <summary>
    /// Provides a settings UI in Project Settings for Inspector Display Modes
    /// </summary>
    public class InspectorDisplaySettingsProvider : SettingsProvider
    {
        private const string SettingsPath = "Project/BComponentComparator";

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new InspectorDisplaySettingsProvider(SettingsPath, SettingsScope.Project, new[] { "BComponentComparator", "Inspector", "Display", "Mode" });
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            // Settings are automatically loaded by ScriptableSingleton
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Inspector Display Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure how different object types are displayed in the Component Comparator Inspector.\n\n" +
                "These settings are stored per-project and control the rendering mode for each type.",
                MessageType.Info);

            EditorGUILayout.Space();

            var settings = InspectorDisplaySettings.instance;

            // Get serialized object for proper undo/redo support
            var serializedObject = new SerializedObject(settings);
            var customModesProperty = serializedObject.FindProperty("customModes");

            EditorGUILayout.LabelField("Custom Display Modes", EditorStyles.boldLabel);

            if (customModesProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No custom display modes configured yet.\n\nUse the Display Mode dropdown in the BComponentComparator window to configure modes for specific types.", MessageType.Info);
            }
            else
            {
                EditorGUI.BeginChangeCheck();

                for (int i = 0; i < customModesProperty.arraySize; i++)
                {
                    var entryProperty = customModesProperty.GetArrayElementAtIndex(i);
                    var typeNameProperty = entryProperty.FindPropertyRelative("typeFullName");
                    var displayModeProperty = entryProperty.FindPropertyRelative("displayMode");

                    EditorGUILayout.BeginHorizontal();

                    // Show simplified type name
                    var typeName = typeNameProperty.stringValue;
                    var shortTypeName = typeName.Contains(",") ? typeName.Substring(0, typeName.IndexOf(",")) : typeName;
                    if (shortTypeName.Contains("."))
                    {
                        var parts = shortTypeName.Split('.');
                        shortTypeName = parts[parts.Length - 1];
                    }

                    EditorGUILayout.LabelField(shortTypeName, GUILayout.Width(200));
                    EditorGUILayout.PropertyField(displayModeProperty, GUIContent.none, GUILayout.Width(150));

                    if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    {
                        customModesProperty.DeleteArrayElementAtIndex(i);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    settings.SaveSettings(true);
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            
            if ((customModesProperty.arraySize > 0) && GUILayout.Button("Clear All Custom Modes"))
            {
                if (EditorUtility.DisplayDialog(
                        "Clear All Custom Modes",
                        "Are you sure you want to clear all custom display modes? This cannot be undone.",
                        "Clear All",
                        "Cancel"))
                {
                    settings.ClearAll();
                }
            }

            if (GUILayout.Button("Reset to Defaults"))
            {
                if (EditorUtility.DisplayDialog(
                        "Reset to Defaults",
                        "Are you sure you want to reset all display modes to default values? This will overwrite your current settings.",
                        "Reset",
                        "Cancel"))
                {
                    settings.ResetToDefaults();
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        public InspectorDisplaySettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
        }
    }
}