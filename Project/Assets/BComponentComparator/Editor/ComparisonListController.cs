using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace BComponentComparator.Editor
{
    /// <summary>
    /// Controller for managing the comparison list (left panel).
    /// Handles ListView data binding, item operations, and drag-drop.
    /// </summary>
    public class ComparisonListController
    {
        private List<ComparisonItem> items;
        private ListView listView;
        private Type requiredType;

        /// <summary>
        /// Event raised when the list changes (items added/removed/cleared)
        /// </summary>
        public event Action OnItemsChanged;

        /// <summary>
        /// Event fired when ListView selection changes
        /// </summary>
        public event Action<List<UnityEngine.Object>> OnSelectionChangedEvent;

        /// <summary>
        /// Constructor creates ListView and initializes item list
        /// </summary>
        public ComparisonListController(VisualElement container, int insertIndex = -1)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            items = new List<ComparisonItem>();

            // Create ListView
            listView = new ListView();
            listView.AddToClassList("comparison-list");
            listView.style.flexGrow = 1;
            listView.selectionType = SelectionType.Multiple;
            listView.reorderable = true;

            // Set up ListView callbacks
            listView.makeItem = MakeItem;
            listView.bindItem = BindItem;
            listView.itemsSource = items;
            listView.selectionChanged += OnSelectionChanged;

            // Insert at specific index or add to end
            if (insertIndex >= 0 && insertIndex < container.childCount)
            {
                container.Insert(insertIndex, listView);
            }
            else
            {
                container.Add(listView);
            }

            // Register drag-drop callbacks for adding items to list
            RegisterDragDropCallbacks();
        }

        /// <summary>
        /// Handle ListView selection changed
        /// </summary>
        private void OnSelectionChanged(IEnumerable<object> selectedItems)
        {
            var selectedObjects = new List<UnityEngine.Object>();
            
            foreach (var item in selectedItems)
            {
                if (item is ComparisonItem compItem && compItem.IsValid())
                {
                    selectedObjects.Add(compItem.TargetObject);
                }
            }

            // Sync Unity Selection
            if (selectedObjects.Count > 0)
            {
                Selection.objects = selectedObjects.ToArray();
                
                // Ping the first selected object to highlight it in Project window
                if (selectedObjects.Count == 1)
                {
                    EditorGUIUtility.PingObject(selectedObjects[0]);
                }
            }

            // Notify for Inspector highlight update
            OnSelectionChangedEvent?.Invoke(selectedObjects);
        }

        /// <summary>
        /// Register drag-drop callbacks for ListView
        /// </summary>
        private void RegisterDragDropCallbacks()
        {
            DragDropHandler.RegisterDragDropCallbacks(
                listView,
                obj => requiredType != null && DragDropHandler.ValidateObject(obj, requiredType),
                OnObjectDropped
            );
        }

        /// <summary>
        /// Handle objects dropped on ListView
        /// </summary>
        private void OnObjectDropped(UnityEngine.Object obj)
        {
            if (requiredType == null)
            {
                return;
            }

            // Get all dragged objects
            var draggedObjects = DragAndDrop.objectReferences;
            var validObjects = new List<UnityEngine.Object>();

            foreach (var draggedObj in draggedObjects)
            {
                if (draggedObj != null && DragDropHandler.ValidateObject(draggedObj, requiredType))
                {
                    validObjects.Add(draggedObj);
                }
            }

            if (validObjects.Count > 0)
            {
                AddItems(validObjects);
            }
        }

        /// <summary>
        /// Set the required Component/asset type for validation
        /// </summary>
        public void SetRequiredType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            // If type changes, clear existing list and prompt user
            if (requiredType != null && requiredType != type)
            {
                if (items.Count > 0)
                {
                    if (EditorUtility.DisplayDialog(
                        "Component Type Changed",
                        $"Changing type from '{requiredType.Name}' to '{type.Name}' will clear the current list. Continue?",
                        "Yes", "No"))
                    {
                        ClearItems();
                    }
                    else
                    {
                        // User cancelled, keep old type
                        return;
                    }
                }
            }

            requiredType = type;
        }

        /// <summary>
        /// Add a single item to the list
        /// </summary>
        public void AddItem(UnityEngine.Object obj)
        {
            if (obj == null || requiredType == null)
            {
                return;
            }

            // Check for duplicates
            if (items.Exists(item => item.TargetObject == obj))
            {
                UnityEngine.Debug.LogWarning($"{obj.name} already in comparison list");
                return;
            }

            try
            {
                var comparisonItem = new ComparisonItem(obj, requiredType);
                items.Add(comparisonItem);
                RebuildListView();
                OnItemsChanged?.Invoke();
            }
            catch (ArgumentException ex)
            {
                UnityEngine.Debug.LogWarning(ex.Message);
            }
        }

        /// <summary>
        /// Add multiple items in batch (more efficient than calling AddItem repeatedly)
        /// </summary>
        public void AddItems(IEnumerable<UnityEngine.Object> objects)
        {
            if (objects == null || requiredType == null)
            {
                return;
            }

            bool anyAdded = false;
            foreach (var obj in objects)
            {
                if (obj == null || items.Exists(item => item.TargetObject == obj))
                {
                    continue;
                }

                try
                {
                    var comparisonItem = new ComparisonItem(obj, requiredType);
                    items.Add(comparisonItem);
                    anyAdded = true;
                }
                catch (ArgumentException ex)
                {
                    UnityEngine.Debug.LogWarning(ex.Message);
                }
            }

            if (anyAdded)
            {
                RebuildListView();
                OnItemsChanged?.Invoke();
            }
        }

        /// <summary>
        /// Remove item at specified index
        /// </summary>
        public void RemoveItem(int index)
        {
            if (index < 0 || index >= items.Count)
            {
                return;
            }

            items[index].Dispose();
            items.RemoveAt(index);
            RebuildListView();
            OnItemsChanged?.Invoke();
        }

        /// <summary>
        /// Clear all items from the list
        /// </summary>
        public void ClearItems()
        {
            foreach (var item in items)
            {
                item.Dispose();
            }

            items.Clear();
            RebuildListView();
            OnItemsChanged?.Invoke();
        }

        /// <summary>
        /// Get read-only view of current items
        /// </summary>
        public IReadOnlyList<ComparisonItem> GetItems()
        {
            return items.AsReadOnly();
        }

        /// <summary>
        /// Dispose of resources when controller is destroyed
        /// </summary>
        public void Dispose()
        {
            // Unregister drag-drop callbacks
            if (listView != null)
            {
                DragDropHandler.UnregisterDragDropCallbacks(listView);
                listView.selectionChanged -= OnSelectionChanged;
            }

            // Dispose all items
            if (items != null)
            {
                foreach (var item in items)
                {
                    item?.Dispose();
                }
                items.Clear();
            }
        }

        /// <summary>
        /// Refresh ListView display
        /// </summary>
        private void RebuildListView()
        {
            listView.itemsSource = items;
            listView.Rebuild();
        }

        /// <summary>
        /// ListView callback: Create visual element for list item
        /// </summary>
        private VisualElement MakeItem()
        {
            var itemElement = new VisualElement();
            itemElement.AddToClassList("list-item");
            itemElement.style.flexDirection = FlexDirection.Row;
            itemElement.style.alignItems = Align.Center;
            itemElement.style.position = Position.Relative;

            var label = new Label();
            label.style.flexGrow = 1;
            label.style.paddingRight = 25; // Space for button
            itemElement.Add(label);

            // Remove button with × symbol (absolutely positioned, initially hidden)
            var removeButton = new Button();
            removeButton.text = "×";
            removeButton.AddToClassList("remove-button");
            removeButton.style.position = Position.Absolute;
            removeButton.style.right = 5;
            removeButton.style.top = new StyleLength(new Length(50, LengthUnit.Percent));
            removeButton.style.translate = new Translate(0, new Length(-50, LengthUnit.Percent));
            removeButton.style.width = 20;
            removeButton.style.height = 20;
            removeButton.style.visibility = Visibility.Hidden;
            itemElement.Add(removeButton);

            // Show/hide button on hover
            itemElement.RegisterCallback<MouseEnterEvent>(evt =>
            {
                removeButton.style.visibility = Visibility.Visible;
            });

            itemElement.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                removeButton.style.visibility = Visibility.Hidden;
            });

            return itemElement;
        }

        /// <summary>
        /// ListView callback: Bind data to visual element
        /// </summary>
        private void BindItem(VisualElement element, int index)
        {
            if (index < 0 || index >= items.Count)
            {
                return;
            }

            var label = element.Q<Label>();
            if (label != null)
            {
                label.text = items[index].DisplayName;
            }

            // Bind remove button click event
            var removeButton = element.Q<Button>();
            if (removeButton != null)
            {
                // Remove old click handlers to avoid duplicates
                removeButton.clicked -= () => { };
                removeButton.clicked += () => RemoveItem(index);
            }
        }
    }
}
