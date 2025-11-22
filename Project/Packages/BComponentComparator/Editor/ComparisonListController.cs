using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BTools.BComponentComparator.Editor
{
    /// <summary>
    /// Controller for managing the comparison list (left panel).
    /// Handles ListView data binding, item operations, and drag-drop.
    /// </summary>
    public class ComparisonListController
    {
        private readonly List<ComparisonItem> items;
        private readonly ListView listView;
        private readonly Dictionary<Button, Action> removeButtonClickActionMap;

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
        /// <param name="container">Parent VisualElement to host the ListView</param>
        /// <param name="insertIndex">Index to insert ListView into container, defaults to -1 for appending</param>
        public ComparisonListController(VisualElement container, int insertIndex = -1)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            items = new List<ComparisonItem>();
            removeButtonClickActionMap = new Dictionary<Button, Action>();

            // Create ListView
            listView = new ListView();
            listView.AddToClassList("comparison-list");
            listView.style.flexGrow = 1;
            listView.selectionType = SelectionType.Multiple;
            listView.reorderable = true;
            listView.reorderMode = ListViewReorderMode.Animated;
            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

            // Set up ListView callbacks
            listView.makeItem = MakeItem;
            listView.bindItem = BindItem;
            listView.itemsSource = items;
            listView.selectionChanged += OnSelectionChanged;
            listView.itemIndexChanged += OnItemIndexChanged;

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
        /// Refresh drag-drop callbacks (useful when window is re-docked)
        /// </summary>
        public void RefreshDragDropCallbacks()
        {
            if (requiredType != null)
            {
                RegisterDragDropCallbacks();
            }
        }

        /// <summary>
        /// Set the required Component/asset type for validation
        /// </summary>
        /// <param name="type">Type of Component or asset</param>
        public void SetRequiredType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            // If type changes, clear existing list and prompt user
            if (requiredType != null &&
                requiredType != type &&
                items.Count > 0)
            {
                ClearItems();
            }

            requiredType = type;
        }

        /// <summary>
        /// Add a single item to the list
        /// </summary>
        /// <param name="obj">The object to add (GameObject, ScriptableObject, or Material, etc.)</param>
        public void AddItem(UnityEngine.Object obj)
        {
            if (obj == null || requiredType == null)
            {
                return;
            }

            // If GameObject and required type is Component, extract ALL matching Components
            var isGoComponent = (obj is GameObject) && typeof(Component).IsAssignableFrom(requiredType);
            if (isGoComponent)
            {
                var components = (obj as GameObject).GetComponents(requiredType);
                if (components != null && components.Length > 0)
                {
                    AddItems(components);
                    return;
                }

                // No matching components found
                return;
            }

            // Check for duplicates
            if (items.Exists(item => item.TargetObject == obj))
            {
                Debug.LogWarning($"{obj.name} already in comparison list");
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
                Debug.LogWarning(ex.Message);
            }
        }

        /// <summary>
        /// Add multiple items in batch (more efficient than calling AddItem repeatedly)
        /// </summary>
        /// <param name="objects">Collection of objects to add</param>
        public void AddItems(IEnumerable<UnityEngine.Object> objects)
        {
            if (objects == null || requiredType == null)
            {
                return;
            }

            var anyAdded = false;
            foreach (var obj in objects)
            {
                // If GameObject and required type is Component, extract ALL matching Components
                var isGoComponent = (obj is GameObject) && typeof(Component).IsAssignableFrom(requiredType);
                if (isGoComponent)
                {
                    var components = (obj as GameObject).GetComponents(requiredType);
                    if (components == null || components.Length <= 0)
                    {
                        continue;
                    }

                    foreach (var component in components)
                    {
                        if (component == null || items.Exists(item => item.TargetObject == component))
                        {
                            continue;
                        }

                        try
                        {
                            var comparisonItem = new ComparisonItem(component, requiredType);
                            items.Add(comparisonItem);
                            anyAdded = true;
                        }
                        catch (ArgumentException ex)
                        {
                            Debug.LogWarning(ex.Message);
                        }
                    }

                    continue;
                }

                // For non-GameObject objects, add directly
                if (obj != null && !items.Exists(item => item.TargetObject == obj))
                {
                    try
                    {
                        var comparisonItem = new ComparisonItem(obj, requiredType);
                        items.Add(comparisonItem);
                        anyAdded = true;
                    }
                    catch (ArgumentException ex)
                    {
                        Debug.LogWarning(ex.Message);
                    }
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
        /// <param name="index">Index of item to remove</param>
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
                listView.itemIndexChanged -= OnItemIndexChanged;
            }

            // Clear button click handlers
            if (removeButtonClickActionMap != null)
            {
                foreach ((var btn, var clickAction) in removeButtonClickActionMap)
                {
                    if (btn != null)
                    {
                        btn.clicked -= clickAction;
                    }
                }

                removeButtonClickActionMap.Clear();
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
        /// Handle ListView selection changed
        /// </summary>
        /// <param name="selectedItems">Currently selected items</param>
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
                // Convert Components to their GameObjects for Selection
                var compToGoSelectedObjects = new List<UnityEngine.Object>();
                foreach (var obj in selectedObjects)
                {
                    if (obj is Component comp)
                    {
                        compToGoSelectedObjects.Add(comp.gameObject);
                    }
                    else
                    {
                        compToGoSelectedObjects.Add(obj);
                    }
                }

                Selection.objects = compToGoSelectedObjects.ToArray();

                // Ping the first selected object to highlight it in Project window
                if (compToGoSelectedObjects.Count == 1)
                {
                    EditorGUIUtility.PingObject(compToGoSelectedObjects[0]);
                }
            }

            // Notify for Inspector highlight update
            OnSelectionChangedEvent?.Invoke(selectedObjects);
        }

        /// <summary>
        /// Handle ListView item reorder
        /// </summary>
        /// <param name="oldIndex">Old index of moved item</param>
        /// <param name="newIndex">New index of moved item</param>
        private void OnItemIndexChanged(int oldIndex, int newIndex)
        {
            OnItemsChanged?.Invoke();
        }

        /// <summary>
        /// Register drag-drop callbacks for ListView
        /// </summary>
        private void RegisterDragDropCallbacks()
        {
            DragDropHandler.RegisterDragDropCallbacks(
                listView,
                obj => requiredType != null && DragDropHandler.IsValidObject(obj, requiredType),
                OnObjectDropped
            );
        }

        /// <summary>
        /// Handle objects dropped on ListView
        /// </summary>
        /// <param name="droppedObjects">Array of all dropped objects</param>
        private void OnObjectDropped(UnityEngine.Object[] droppedObjects)
        {
            if (requiredType == null || droppedObjects == null || droppedObjects.Length == 0)
            {
                return;
            }

            AddItems(droppedObjects);
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
            var removeButton = new Button { text = "×" };
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
            itemElement.RegisterCallback<MouseEnterEvent>(evt => { removeButton.style.visibility = Visibility.Visible; });
            itemElement.RegisterCallback<MouseLeaveEvent>(evt => { removeButton.style.visibility = Visibility.Hidden; });

            return itemElement;
        }

        /// <summary>
        /// ListView callback: Bind data to visual element
        /// </summary>
        /// <param name="element">The visual element to bind</param>
        /// <param name="index">Index of the item in the list</param>
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
                // Store the item reference to find correct index when clicked
                var currentItem = items[index];

                // Remove old handler if exists
                if (removeButtonClickActionMap.TryGetValue(removeButton, out var oldAction))
                {
                    removeButton.clicked -= oldAction;
                }

                // Create and store new handler
                var clickEvent = new Action(() =>
                {
                    // Find current index of the item (in case list was reordered)
                    int currentIndex = items.IndexOf(currentItem);
                    if (currentIndex >= 0)
                    {
                        RemoveItem(currentIndex);
                    }
                });

                removeButtonClickActionMap[removeButton] = clickEvent;
                removeButton.clicked += clickEvent;
            }
        }
    }
}