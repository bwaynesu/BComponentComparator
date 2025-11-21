using System;
using UnityEditor;
using UnityEngine;

namespace BTools.BComponentComparator.Editor
{
    /// <summary>
    /// Data model wrapping a Unity Object with metadata for comparison.
    /// Represents an item in the comparison list.
    /// </summary>
    public class ComparisonItem : IDisposable
    {
        /// <summary>
        /// The original dropped object (GameObject, ScriptableObject, or Material)
        /// </summary>
        public UnityEngine.Object TargetObject { get; private set; }

        /// <summary>
        /// If TargetObject is GameObject, this is the specific Component instance being compared
        /// </summary>
        public Component TargetComponent { get; private set; }

        /// <summary>
        /// User-friendly name shown in ListView
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Unity's wrapper for safe Inspector editing with Undo/Redo
        /// </summary>
        public SerializedObject SerializedObject { get; private set; }

        /// <summary>
        /// Constructor creates SerializedObject for Inspector binding
        /// </summary>
        /// <param name="targetObject">GameObject, ScriptableObject, or Material</param>
        /// <param name="componentType">Type of Component to extract (if GameObject)</param>
        public ComparisonItem(UnityEngine.Object targetObject, Type componentType)
        {
            if (targetObject == null)
            {
                throw new ArgumentNullException(nameof(targetObject));
            }

            if (componentType == null)
            {
                throw new ArgumentNullException(nameof(componentType));
            }

            TargetObject = targetObject;

            if (targetObject is GameObject go && typeof(Component).IsAssignableFrom(componentType))
            {
                // Extract Component from GameObject (only if componentType is actually a Component)
                TargetComponent = go.GetComponent(componentType);

                if (TargetComponent == null)
                {
                    throw new ArgumentException($"GameObject '{go.name}' does not have Component of type '{componentType.Name}'");
                }

                SerializedObject = new SerializedObject(TargetComponent);
                DisplayName = go.name;
            }
            else if (componentType.IsAssignableFrom(targetObject.GetType()))
            {
                // ScriptableObject, Material, or other assets - compare the asset itself
                SerializedObject = new SerializedObject(targetObject);
                DisplayName = targetObject.name;
            }
            else if (typeof(AssetImporter).IsAssignableFrom(componentType))
            {
                // For AssetImporter types (e.g., ModelImporter for FBX)
                // Create SerializedObject from the importer

                var path = AssetDatabase.GetAssetPath(targetObject);
                if (!string.IsNullOrEmpty(path))
                {
                    var importer = AssetImporter.GetAtPath(path);
                    if (importer != null && componentType.IsAssignableFrom(importer.GetType()))
                    {
                        SerializedObject = new SerializedObject(importer);
                        DisplayName = targetObject.name;
                    }
                    else
                    {
                        throw new ArgumentException($"Asset '{targetObject.name}' does not have importer of type '{componentType.Name}'");
                    }
                }
                else
                {
                    throw new ArgumentException($"Asset '{targetObject.name}' has no valid asset path");
                }
            }
            else
            {
                throw new ArgumentException($"Object '{targetObject.name}' is not of type '{componentType.Name}'");
            }
        }

        /// <summary>
        /// Check if the target object still exists (not destroyed)
        /// </summary>
        public bool IsValid()
        {
            if (TargetObject == null)
            {
                return false;
            }

            // For GameObjects, also check if Component still exists
            if (TargetComponent != null && TargetComponent == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Cleanup SerializedObject resources
        /// </summary>
        public void Dispose()
        {
            if (SerializedObject != null)
            {
                SerializedObject.Dispose();
                SerializedObject = null;
            }
        }
    }
}