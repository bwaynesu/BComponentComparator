using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BTools.BComponentComparator.Editor
{
    /// <summary>
    /// Static utility class for handling drag-drop operations and validation.
    /// Provides validation for Component types and objects (GameObjects, ScriptableObjects, Materials, etc).
    /// </summary>
    public static class DragDropHandler
    {
        private class DragCallbackStorage
        {
            public EventCallback<DragUpdatedEvent> OnDragUpdated;
            public EventCallback<DragPerformEvent> OnDragPerform;
            public EventCallback<DragLeaveEvent> OnDragLeave;
            public EventCallback<DragExitedEvent> OnDragExited;
        }

        private static readonly HashSet<string> excludedImporterNames = new()
        {
            "PrefabImporter",
        };

        private static readonly string dragHoverClassName = "drag-hover";
        private static readonly string dragRejectedClassName = "drag-rejected";
        private static readonly ConditionalWeakTable<VisualElement, DragCallbackStorage> elementDragCallbacksMap = new();

        /// <summary>
        /// Try to extract the type of the dropped object.
        /// </summary>
        /// <param name="obj">Dropped object</param>
        /// <param name="type">Extracted type</param>
        /// <returns>True if type extracted successfully</returns>
        public static bool TryExtractedType(UnityEngine.Object obj, out Type type)
        {
            type = (obj != null) ? obj.GetType() : null;

            if (obj == null)
            {
                return false;
            }

            // MonoScript (drag .cs file from Project window)
            if (obj is MonoScript monoScript)
            {
                var scriptType = monoScript.GetClass();
                if (scriptType != null && typeof(Component).IsAssignableFrom(scriptType))
                {
                    type = scriptType;
                    return true;
                }
            }

            // Component - use the component type directly
            if (obj is Component component)
            {
                type = component.GetType();
                return true;
            }

            // Check if it's a GameObject before checking for importer
            // GameObject needs special handling (either Component or Importer)
            if (typeof(GameObject).IsAssignableFrom(type))
            {
                // For GameObject, check if it has an inherited AssetImporter (e.g., ModelImporter for FBX)
                if (Utility.TryGetInheritedImporter(obj, out var importerType))
                {
                    type = importerType;
                    return !excludedImporterNames.Contains(type.Name);
                }
                
                // Regular GameObject without valid importer
                type = null;
                return false;
            }

            // For other assets (Mesh, Material, Texture, etc.), use their own type
            // Don't check for importer as these are valid comparison types themselves
            return true;
        }

        /// <summary>
        /// Validate if the dropped object matches the required Component type.
        /// </summary>
        /// <param name="obj">Dropped object</param>
        /// <param name="requiredType">Required Component/asset type</param>
        /// <returns>True if valid and matches type</returns>
        public static bool IsValidObject(UnityEngine.Object obj, Type requiredType)
        {
            if (obj == null || requiredType == null)
            {
                return false;
            }

            if (IsValidAsset(obj, requiredType))
            {
                return true;
            }

            // If required type is an AssetImporter, validate by importer type
            if (typeof(AssetImporter).IsAssignableFrom(requiredType))
            {
                if (Utility.TryGetInheritedImporter(obj, out Type importerType))
                {
                    return !excludedImporterNames.Contains(importerType.Name) && 
                        requiredType.IsAssignableFrom(importerType);
                }

                return false;
            }

            // GameObject: must have required Component
            if (obj is GameObject go)
            {
                return IsValidGameObject(go, requiredType);
            }

            return false;
        }

        /// <summary>
        /// Check if GameObject has the required Component type.
        /// </summary>
        /// <param name="go">GameObject to check</param>
        /// <param name="componentType">Required Component type</param>
        /// <returns>True if GameObject has the Component</returns>
        public static bool IsValidGameObject(GameObject go, Type componentType)
        {
            if (go == null || componentType == null)
            {
                return false;
            }

            // Only try GetComponent if type is actually a Component
            if (!typeof(Component).IsAssignableFrom(componentType))
            {
                return false;
            }

            return go.TryGetComponent(componentType, out _);
        }

        /// <summary>
        /// Check if asset matches the required type.
        /// </summary>
        /// <param name="asset">Asset to check</param>
        /// <param name="requiredType">Required type</param>
        /// <returns>True if asset matches type</returns>
        public static bool IsValidAsset(UnityEngine.Object asset, Type requiredType)
        {
            if (asset == null || requiredType == null)
            {
                return false;
            }

            return requiredType.IsAssignableFrom(asset.GetType());
        }

        /// <summary>
        /// Register drag-drop event callbacks on a VisualElement.
        /// Provides visual feedback and validation during drag operations.
        /// </summary>
        /// <param name="target">Target VisualElement</param>
        /// <param name="validator">Validation function</param>
        /// <param name="onDropAccepted">Callback when valid drop accepted</param>
        public static void RegisterDragDropCallbacks(
            VisualElement target,
            Func<UnityEngine.Object, bool> validator,
            Action<UnityEngine.Object> onDropAccepted)
        {
            if (target == null || validator == null || onDropAccepted == null)
            {
                throw new ArgumentNullException($"{nameof(target)}, {nameof(validator)}, and {nameof(onDropAccepted)} must not be null");
            }

            // Unregister old callbacks if they exist
            UnregisterDragDropCallbacks(target);

            // Create new callbacks
            var storage = new DragCallbackStorage
            {
                OnDragUpdated = evt => OnDragUpdated(evt, target, validator),
                OnDragPerform = evt => OnDragPerform(evt, target, validator, onDropAccepted),
                OnDragLeave = evt => OnDragLeave(evt, target),
                OnDragExited = evt => OnDragExited(evt, target)
            };

            // Register callbacks
            target.RegisterCallback(storage.OnDragUpdated);
            target.RegisterCallback(storage.OnDragPerform);
            target.RegisterCallback(storage.OnDragLeave);
            target.RegisterCallback(storage.OnDragExited);

            // Store callbacks for later unregistration
            elementDragCallbacksMap.AddOrUpdate(target, storage);
        }

        /// <summary>
        /// Unregister drag-drop event callbacks from a VisualElement.
        /// </summary>
        /// <param name="target">Target VisualElement</param>
        public static void UnregisterDragDropCallbacks(VisualElement target)
        {
            if (target == null)
            {
                return;
            }

            if (!elementDragCallbacksMap.TryGetValue(target, out var storage))
            {
                return;
            }

            target.UnregisterCallback(storage.OnDragUpdated);
            target.UnregisterCallback(storage.OnDragPerform);
            target.UnregisterCallback(storage.OnDragLeave);
            target.UnregisterCallback(storage.OnDragExited);

            elementDragCallbacksMap.Remove(target);
        }

        private static void OnDragUpdated(
            DragUpdatedEvent evt,
            VisualElement target,
            Func<UnityEngine.Object, bool> validator)
        {
            if (DragAndDrop.objectReferences.Length == 0)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                target.RemoveFromClassList(dragHoverClassName);
                target.RemoveFromClassList(dragRejectedClassName);
                return;
            }

            var draggedObject = DragAndDrop.objectReferences[0];
            var isValid = validator(draggedObject);

            DragAndDrop.visualMode = isValid ? DragAndDropVisualMode.Link : DragAndDropVisualMode.Rejected;

            // Visual feedback - remove old classes first
            target.RemoveFromClassList(dragHoverClassName);
            target.RemoveFromClassList(dragRejectedClassName);

            if (isValid)
            {
                target.AddToClassList(dragHoverClassName);
            }
            else
            {
                target.AddToClassList(dragRejectedClassName);
            }

            evt.StopPropagation();
        }

        private static void OnDragPerform(
            DragPerformEvent evt,
            VisualElement target,
            Func<UnityEngine.Object, bool> validator,
            Action<UnityEngine.Object> onDropAccepted)
        {
            if (DragAndDrop.objectReferences.Length == 0)
            {
                // Remove visual feedback
                target.RemoveFromClassList(dragHoverClassName);
                target.RemoveFromClassList(dragRejectedClassName);
                return;
            }

            var draggedObject = DragAndDrop.objectReferences[0];

            var isValid = validator(draggedObject);
            if (isValid)
            {
                DragAndDrop.AcceptDrag();
                onDropAccepted(draggedObject);
            }

            // Remove visual feedback
            target.RemoveFromClassList(dragHoverClassName);
            target.RemoveFromClassList(dragRejectedClassName);

            evt.StopPropagation();
        }

        private static void OnDragLeave(DragLeaveEvent evt, VisualElement target)
        {
            // Remove visual feedback
            target.RemoveFromClassList(dragHoverClassName);
            target.RemoveFromClassList(dragRejectedClassName);

            evt.StopPropagation();
        }

        private static void OnDragExited(DragExitedEvent evt, VisualElement target)
        {
            // Ensure visual feedback is removed when drag operation fully ends
            target.RemoveFromClassList(dragHoverClassName);
            target.RemoveFromClassList(dragRejectedClassName);

            evt.StopPropagation();
        }
    }
}