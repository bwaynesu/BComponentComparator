using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ComponentComparator.Editor
{
    /// <summary>
    /// Static utility class for handling drag-drop operations and validation.
    /// Provides validation for Component types and objects (GameObjects, ScriptableObjects, Materials).
    /// Rejects Prefab assets per requirements.
    /// </summary>
    public static class DragDropHandler
    {
        /// <summary>
        /// Validate if the dropped object is a valid Component type or MonoScript.
        /// </summary>
        /// <param name="obj">Dropped object</param>
        /// <param name="componentType">Extracted Component type</param>
        /// <returns>True if valid Component type</returns>
        public static bool ValidateComponentType(UnityEngine.Object obj, out Type componentType)
        {
            componentType = null;

            if (obj == null)
            {
                return false;
            }

            // Direct Component instance
            if (obj is Component component)
            {
                componentType = component.GetType();
                return true;
            }

            // MonoScript (drag .cs file from Project window)
            if (obj is MonoScript monoScript)
            {
                Type scriptType = monoScript.GetClass();
                if (scriptType != null && typeof(Component).IsAssignableFrom(scriptType))
                {
                    componentType = scriptType;
                    return true;
                }
            }

            // ScriptableObject or Material type (direct asset)
            if (obj is ScriptableObject || obj is Material)
            {
                componentType = obj.GetType();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Validate if the dropped object matches the required Component type.
        /// Rejects Prefab assets.
        /// </summary>
        /// <param name="obj">Dropped object</param>
        /// <param name="requiredType">Required Component/asset type</param>
        /// <returns>True if valid and matches type</returns>
        public static bool ValidateObject(UnityEngine.Object obj, Type requiredType)
        {
            if (obj == null || requiredType == null)
            {
                return false;
            }

            // Reject Prefab assets (no warning here, will show on actual drop)
            if (PrefabUtility.IsPartOfPrefabAsset(obj))
            {
                return false;
            }

            // GameObject: must have required Component
            if (obj is GameObject go)
            {
                return IsValidGameObject(go, requiredType);
            }

            // ScriptableObject or Material: type must match
            if (obj is ScriptableObject || obj is Material)
            {
                return IsValidAsset(obj, requiredType);
            }

            return false;
        }

        /// <summary>
        /// Check if GameObject has the required Component type.
        /// </summary>
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

            Component component = go.GetComponent(componentType);
            if (component == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if asset (ScriptableObject/Material) matches the required type.
        /// </summary>
        public static bool IsValidAsset(UnityEngine.Object asset, Type requiredType)
        {
            if (asset == null || requiredType == null)
            {
                return false;
            }

            if (!requiredType.IsAssignableFrom(asset.GetType()))
            {
                return false;
            }

            return true;
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
                throw new ArgumentNullException("Target, validator, and callback must not be null");
            }

            target.RegisterCallback<DragUpdatedEvent>(evt => OnDragUpdated(evt, target, validator));
            target.RegisterCallback<DragPerformEvent>(evt => OnDragPerform(evt, target, validator, onDropAccepted));
            target.RegisterCallback<DragLeaveEvent>(evt => OnDragLeave(evt, target));
            target.RegisterCallback<DragExitedEvent>(evt => OnDragExited(evt, target));
        }

        /// <summary>
        /// Unregister drag-drop event callbacks from a VisualElement.
        /// </summary>
        public static void UnregisterDragDropCallbacks(VisualElement target)
        {
            if (target == null)
            {
                return;
            }

            target.UnregisterCallback<DragUpdatedEvent>(evt => { });
            target.UnregisterCallback<DragPerformEvent>(evt => { });
            target.UnregisterCallback<DragLeaveEvent>(evt => { });
            target.UnregisterCallback<DragExitedEvent>(evt => { });
        }

        private static void OnDragUpdated(DragUpdatedEvent evt, VisualElement target, Func<UnityEngine.Object, bool> validator)
        {
            if (DragAndDrop.objectReferences.Length == 0)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                target.RemoveFromClassList("drag-hover");
                target.RemoveFromClassList("drag-rejected");
                return;
            }

            UnityEngine.Object draggedObject = DragAndDrop.objectReferences[0];
            bool isValid = validator(draggedObject);

            DragAndDrop.visualMode = isValid ? DragAndDropVisualMode.Link : DragAndDropVisualMode.Rejected;

            // Visual feedback - remove old classes first
            target.RemoveFromClassList("drag-hover");
            target.RemoveFromClassList("drag-rejected");
            
            if (isValid)
            {
                target.AddToClassList("drag-hover");
            }
            else
            {
                target.AddToClassList("drag-rejected");
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
                target.RemoveFromClassList("drag-hover");
                target.RemoveFromClassList("drag-rejected");
                return;
            }

            UnityEngine.Object draggedObject = DragAndDrop.objectReferences[0];
            
            // Check for Prefab and show warning on actual drop
            if (PrefabUtility.IsPartOfPrefabAsset(draggedObject))
            {
                Debug.LogWarning("Prefabs are not supported. Please drag the Prefab instance to the scene first.");
                // Remove visual feedback
                target.RemoveFromClassList("drag-hover");
                target.RemoveFromClassList("drag-rejected");
                evt.StopPropagation();
                return;
            }
            
            bool isValid = validator(draggedObject);

            if (isValid)
            {
                DragAndDrop.AcceptDrag();
                onDropAccepted(draggedObject);
            }

            // Remove visual feedback
            target.RemoveFromClassList("drag-hover");
            target.RemoveFromClassList("drag-rejected");

            evt.StopPropagation();
        }

        private static void OnDragLeave(DragLeaveEvent evt, VisualElement target)
        {
            // Remove visual feedback
            target.RemoveFromClassList("drag-hover");
            target.RemoveFromClassList("drag-rejected");

            evt.StopPropagation();
        }

        private static void OnDragExited(DragExitedEvent evt, VisualElement target)
        {
            // Ensure visual feedback is removed when drag operation fully ends
            target.RemoveFromClassList("drag-hover");
            target.RemoveFromClassList("drag-rejected");

            evt.StopPropagation();
        }
    }
}
