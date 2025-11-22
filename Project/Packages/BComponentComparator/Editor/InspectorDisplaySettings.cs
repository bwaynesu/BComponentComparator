using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BTools.BComponentComparator.Editor
{
    /// <summary>
    /// ScriptableSingleton that stores custom display mode settings for object types.
    /// Only stores non-default modes to keep the file clean.
    /// Data is automatically persisted in UserSettings folder.
    /// </summary>
    [FilePath("UserSettings/BComponentComparator/InspectorDisplaySettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class InspectorDisplaySettings : ScriptableSingleton<InspectorDisplaySettings>
    {
        [Serializable]
        public class TypeDisplayModeEntry
        {
            public string typeFullName;
            public InspectorDisplayMode displayMode;

            public TypeDisplayModeEntry(string typeFullName, InspectorDisplayMode displayMode)
            {
                this.typeFullName = typeFullName;
                this.displayMode = displayMode;
            }
        }

        [SerializeField]
        private List<TypeDisplayModeEntry> customModes = null;

        /// <summary>
        /// Get the display mode for a specific type
        /// </summary>
        /// <param name="type">The object type</param>
        /// <returns>The display mode, defaults to Default if not found</returns>
        public InspectorDisplayMode GetDisplayMode(Type type)
        {
            if (type == null)
            {
                return InspectorDisplayMode.Element;
            }

            var typeFullName = GetTypeFullName(type);
            var entry = customModes.FirstOrDefault(e => e.typeFullName == typeFullName);

            return entry?.displayMode ?? InspectorDisplayMode.Element;
        }

        /// <summary>
        /// Set the display mode for a specific type
        /// </summary>
        /// <param name="type">The object type</param>
        /// <param name="mode">The display mode to set</param>
        public void SetDisplayMode(Type type, InspectorDisplayMode mode)
        {
            if (type == null)
            {
                return;
            }

            var typeFullName = GetTypeFullName(type);

            // Remove existing entry for this type
            customModes.RemoveAll(e => e.typeFullName == typeFullName);

            // Only store non-default modes
            if (mode != InspectorDisplayMode.Element)
            {
                customModes.Add(new TypeDisplayModeEntry(typeFullName, mode));
            }

            // Save changes
            SaveSettings(true);
        }

        /// <summary>
        /// Save settings to disk
        /// </summary>
        /// <param name="saveAsText">Whether to save as text asset</param>
        public void SaveSettings(bool saveAsText) => base.Save(saveAsText);

        /// <summary>
        /// Clear all custom display modes
        /// </summary>
        public void ClearAll()
        {
            customModes.Clear();
            SaveSettings(true);
        }

        /// <summary>
        /// Reset to default display mode configurations
        /// </summary>
        public void ResetToDefaults()
        {
            customModes = GetDefaultModes();
            SaveSettings(true);
        }

        private static string GetTypeFullName(Type type)
        {
            if (type == null)
            {
                return string.Empty;
            }

            return $"{type.FullName}, {type.Assembly.GetName().Name}";
        }

        private static List<TypeDisplayModeEntry> GetDefaultModes()
        {
            return new List<TypeDisplayModeEntry>
            {
                new (GetTypeFullName(typeof(DefaultAsset)), InspectorDisplayMode.Editor),
                new (GetTypeFullName(typeof(Texture2D)), InspectorDisplayMode.Editor),
                new (GetTypeFullName(typeof(Font)), InspectorDisplayMode.Editor),
                new (GetTypeFullName(typeof(AssemblyDefinitionAsset)), InspectorDisplayMode.Editor),
                new ("UnityEngine.InputSystem.InputActionAsset, Unity.InputSystem", InspectorDisplayMode.Editor),
                new (GetTypeFullName(typeof(Shader)), InspectorDisplayMode.EditorThenElement),
            };
        }

        private void OnEnable()
        {
            if (customModes == null)
            {
                customModes = GetDefaultModes();
                SaveSettings(true);
            }
        }
    }
}