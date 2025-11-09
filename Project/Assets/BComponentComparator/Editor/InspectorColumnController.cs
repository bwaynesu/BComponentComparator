using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ComponentComparator.Editor
{
    /// <summary>
    /// Controller for managing Inspector columns (right panel).
    /// Creates and manages InspectorElement instances for side-by-side comparison.
    /// </summary>
    public class InspectorColumnController : IDisposable
    {
        private ScrollView scrollView;
        private VisualElement columnsContainer;
        private List<VisualElement> inspectorColumns;
        private List<ComparisonItem> currentItems;
        private int columnWidth = 300;

        /// <summary>
        /// Event fired when remove button is clicked in Inspector column
        /// </summary>
        public event Action<ComparisonItem> OnRemoveItemRequested;

        /// <summary>
        /// Constructor initializes the controller with a ScrollView
        /// </summary>
        public InspectorColumnController(ScrollView scrollView)
        {
            if (scrollView == null)
            {
                throw new ArgumentNullException(nameof(scrollView));
            }

            this.scrollView = scrollView;
            this.inspectorColumns = new List<VisualElement>();
            this.currentItems = new List<ComparisonItem>();

            // Create container for Inspector columns
            columnsContainer = new VisualElement();
            columnsContainer.AddToClassList("inspector-columns-container");
            columnsContainer.style.flexDirection = FlexDirection.Row;
            columnsContainer.style.alignItems = Align.FlexStart;

            // Clear any existing content
            scrollView.Clear();
            scrollView.Add(columnsContainer);
        }

        /// <summary>
        /// Rebuild all Inspector columns based on current comparison items
        /// </summary>
        public void RebuildColumns(IReadOnlyList<ComparisonItem> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            // Clear existing columns and all container content
            Clear();
            columnsContainer.Clear();
            currentItems.Clear();

            // If no items, show empty state
            if (items.Count == 0)
            {
                ShowEmptyState();
                return;
            }

            // Create Inspector column for each valid item
            foreach (var item in items)
            {
                if (!item.IsValid())
                {
                    UnityEngine.Debug.LogWarning($"Skipping invalid item: {item.DisplayName}");
                    continue;
                }

                CreateInspectorColumn(item);
                currentItems.Add(item);
            }
        }

        /// <summary>
        /// Clear all Inspector columns
        /// </summary>
        public void Clear()
        {
            // Unbind and remove all Inspector columns
            foreach (var column in inspectorColumns)
            {
                var inspector = column.Q<InspectorElement>();
                if (inspector != null)
                {
                    inspector.Unbind();
                }
                columnsContainer.Remove(column);
            }

            inspectorColumns.Clear();
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Clear();
        }

        /// <summary>
        /// Highlight Inspector columns for selected objects
        /// </summary>
        public void HighlightColumns(List<UnityEngine.Object> selectedObjects)
        {
            if (selectedObjects == null)
            {
                return;
            }

            // Remove all highlights first
            foreach (var column in inspectorColumns)
            {
                var header = column.Q(className: "inspector-column-header");
                if (header != null)
                {
                    header.RemoveFromClassList("inspector-column-header-selected");
                }
            }

            // Add highlight to selected columns
            for (int i = 0; i < inspectorColumns.Count && i < currentItems.Count; i++)
            {
                var column = inspectorColumns[i];
                var item = currentItems[i];
                var header = column.Q(className: "inspector-column-header");
                
                if (header != null && item.IsValid())
                {
                    // Check if this column's object is in selection
                    if (selectedObjects.Contains(item.TargetObject))
                    {
                        header.AddToClassList("inspector-column-header-selected");
                    }
                }
            }
        }

        /// <summary>
        /// Set the width of Inspector columns
        /// </summary>
        public void SetColumnWidth(int width)
        {
            columnWidth = width;
            
            // Update all existing columns
            foreach (var column in inspectorColumns)
            {
                column.style.width = columnWidth;
                column.style.minWidth = columnWidth;
            }
        }

        /// <summary>
        /// Create an Inspector column for a comparison item
        /// </summary>
        private void CreateInspectorColumn(ComparisonItem item)
        {
            // Create column container
            var column = new VisualElement();
            column.AddToClassList("inspector-column");
            column.style.width = columnWidth;
            column.style.minWidth = columnWidth;

            // Header container with name and remove button
            var headerContainer = new VisualElement();
            headerContainer.AddToClassList("inspector-column-header");
            headerContainer.style.position = Position.Relative;

            var headerLabel = new Label(item.DisplayName);
            headerLabel.style.flexGrow = 1;
            headerLabel.style.overflow = Overflow.Hidden;
            headerLabel.style.paddingRight = 25; // Space for button
            headerLabel.AddToClassList("inspector-column-header-label");
            headerContainer.Add(headerLabel);

            // Remove button with × symbol (absolutely positioned, initially hidden)
            var removeButton = new Button();
            removeButton.text = "×";
            removeButton.AddToClassList("remove-button");
            removeButton.style.position = Position.Absolute;
            removeButton.style.right = 8;
            removeButton.style.top = 5;
            removeButton.style.width = 20;
            removeButton.style.height = 20;
            removeButton.style.visibility = Visibility.Hidden;
            removeButton.clicked += () => OnRemoveColumn(item);
            headerContainer.Add(removeButton);

            // Show/hide button on header hover
            headerContainer.RegisterCallback<MouseEnterEvent>(evt =>
            {
                removeButton.style.visibility = Visibility.Visible;
            });

            headerContainer.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                removeButton.style.visibility = Visibility.Hidden;
            });

            column.Add(headerContainer);

            // Inspector content container
            var contentContainer = new VisualElement();
            contentContainer.AddToClassList("inspector-content");

            // Create InspectorElement
            var inspector = new InspectorElement();
            inspector.Bind(item.SerializedObject);
            contentContainer.Add(inspector);

            column.Add(contentContainer);

            // Add to container
            columnsContainer.Add(column);
            inspectorColumns.Add(column);
        }

        /// <summary>
        /// Handle remove column button click
        /// </summary>
        private void OnRemoveColumn(ComparisonItem item)
        {
            // Trigger removal through event
            OnRemoveItemRequested?.Invoke(item);
        }

        /// <summary>
        /// Show empty state message
        /// </summary>
        private void ShowEmptyState()
        {
            var emptyState = new VisualElement();
            emptyState.AddToClassList("empty-state");
            emptyState.Add(new Label("Add items to the list to compare"));
            columnsContainer.Add(emptyState);
        }
    }
}
