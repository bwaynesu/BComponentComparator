using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BTools.BComponentComparator.Editor
{
    /// <summary>
    /// Controller for managing Inspector columns (right panel).
    /// Creates and manages InspectorElement instances for side-by-side comparison.
    /// </summary>
    public class InspectorColumnController : IDisposable
    {
        private static readonly string inspectorColumnClassName = "inspector-column";
        private static readonly string inspectorColumnsContainerClassName = "inspector-columns-container";
        private static readonly string inspectorColumnHeaderClassName = "inspector-column-header";
        private static readonly string inspectorColumnHeaderSelectedClassName = "inspector-column-header-selected";
        private static readonly string inspectorColumnHeaderLabelClassName = "inspector-column-header-label";
        private static readonly string removeButtonClassName = "remove-button";
        private static readonly string inspectorContentClassName = "inspector-content";
        private static readonly string emptyStateClassName = "empty-state";

        private readonly ScrollView scrollView;
        private readonly VisualElement columnsContainer;
        private readonly List<VisualElement> inspectorColumns;
        private readonly List<ComparisonItem> currentItems;
        private readonly Dictionary<ComparisonItem, UnityEditor.Editor> itemEditorMap;

        private int columnWidth = 300;
        private int rowCount = 1;

        /// <summary>
        /// Event fired when remove button is clicked in Inspector column
        /// </summary>
        public event Action<ComparisonItem> OnRemoveItemRequested;

        /// <summary>
        /// Constructor initializes the controller with a ScrollView
        /// </summary>
        /// <param name="scrollView">ScrollView to host the Inspector columns</param>
        public InspectorColumnController(ScrollView scrollView)
        {
            this.scrollView = scrollView ?? throw new ArgumentNullException(nameof(scrollView));
            this.inspectorColumns = new List<VisualElement>();
            this.currentItems = new List<ComparisonItem>();
            this.itemEditorMap = new Dictionary<ComparisonItem, UnityEditor.Editor>();

            // Create container for Inspector columns
            columnsContainer = new VisualElement();
            columnsContainer.AddToClassList(inspectorColumnsContainerClassName);
            columnsContainer.style.flexDirection = FlexDirection.Column;
            columnsContainer.style.alignItems = Align.FlexStart;

            // Clear any existing content
            scrollView.Clear();
            scrollView.Add(columnsContainer);
        }

        /// <summary>
        /// Rebuild all Inspector columns based on current comparison items
        /// </summary>
        /// <param name="items">List of ComparisonItem to display</param>
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

            // Create row containers
            var rowContainers = new List<VisualElement>();
            for (var i = 0; i < rowCount; i++)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.FlexStart;
                columnsContainer.Add(row);
                rowContainers.Add(row);
            }

            // Create Inspector column for each valid item
            var validItemIndex = 0;
            foreach (var item in items)
            {
                if (!item.IsValid())
                {
                    Debug.LogWarning($"Skipping invalid item: {item.DisplayName}");
                    continue;
                }

                var column = CreateInspectorColumn(item);
                
                // Distribute to rows: i % rowCount
                var rowId = validItemIndex % rowCount;
                if (rowId < rowContainers.Count)
                {
                    rowContainers[rowId].Add(column);
                }

                inspectorColumns.Add(column);
                currentItems.Add(item);
                validItemIndex++;
            }
        }

        /// <summary>
        /// Clear all Inspector columns
        /// </summary>
        public void Clear()
        {
            // Destroy cached editors
            foreach ((_, var editor) in itemEditorMap)
            {
                if (editor != null)
                {
                    UnityEngine.Object.DestroyImmediate(editor);
                }
            }

            itemEditorMap.Clear();

            // Unbind and remove all Inspector columns
            foreach (var column in inspectorColumns)
            {
                var inspector = column.Q<InspectorElement>();
                inspector?.Unbind();
                column.RemoveFromHierarchy();
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
        /// <param name="selectedObjects">List of selected Object</param>
        public void HighlightColumns(List<UnityEngine.Object> selectedObjects)
        {
            if (selectedObjects == null)
            {
                return;
            }

            var selectedIndices = new List<int>();

            // Remove all highlights first
            foreach (var column in inspectorColumns)
            {
                var header = column.Q(className: inspectorColumnHeaderClassName);
                header?.RemoveFromClassList(inspectorColumnHeaderSelectedClassName);
            }

            // Add highlight to selected columns
            for (var i = 0; i < inspectorColumns.Count && i < currentItems.Count; i++)
            {
                var column = inspectorColumns[i];
                var item = currentItems[i];
                var header = column.Q(className: inspectorColumnHeaderClassName);

                if (header != null && item.IsValid())
                {
                    // Check if this column's object is in selection
                    if (selectedObjects.Contains(item.TargetObject))
                    {
                        header.AddToClassList(inspectorColumnHeaderSelectedClassName);
                        selectedIndices.Add(i);
                    }
                }
            }

            // Scroll to selection
            if (selectedIndices.Count > 0)
            {
                ScrollToIndices(selectedIndices);
            }
        }

        /// <summary>
        /// Set the width of Inspector columns
        /// </summary>
        /// <param name="width">Width in pixels</param>
        public void SetColumnWidth(int width)
        {
            columnWidth = width;

            // Update all existing columns and their content containers
            foreach (var column in inspectorColumns)
            {
                column.style.width = columnWidth;
                column.style.minWidth = columnWidth;
                column.style.maxWidth = columnWidth;

                // Update content container width
                var contentContainer = column.Q(className: inspectorContentClassName);
                if (contentContainer != null)
                {
                    contentContainer.style.width = columnWidth;
                    contentContainer.style.maxWidth = columnWidth;
                }
            }
        }

        /// <summary>
        /// Set the number of rows for the layout
        /// </summary>
        /// <param name="count">Number of rows</param>
        public void SetRowCount(int count)
        {
            if (rowCount == count)
            {
                return;
            }

            rowCount = Mathf.Max(1, count);

            RebuildColumns(new List<ComparisonItem>(currentItems));
        }

        /// <summary>
        /// Create an Inspector column for a comparison item
        /// </summary>
        /// <param name="item">ComparisonItem to display</param>
        private VisualElement CreateInspectorColumn(ComparisonItem item)
        {
            // Create column container
            var column = new VisualElement();
            column.AddToClassList(inspectorColumnClassName);
            column.style.width = columnWidth;
            column.style.minWidth = columnWidth;
            column.style.maxWidth = columnWidth;

            // Header container with name and remove button
            var headerContainer = new VisualElement();
            headerContainer.AddToClassList(inspectorColumnHeaderClassName);

            var headerLabel = new Label(item.DisplayName);
            headerLabel.AddToClassList(inspectorColumnHeaderLabelClassName);
            headerContainer.Add(headerLabel);

            // Remove button with × symbol
            var removeButton = new Button { text = "×" };
            removeButton.AddToClassList(removeButtonClassName);
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
            contentContainer.AddToClassList(inspectorContentClassName);
            contentContainer.style.width = columnWidth;
            contentContainer.style.maxWidth = columnWidth;

            CreateInspector(item, contentContainer);

            column.Add(contentContainer);

            return column;
        }

        /// <summary>
        /// Handle remove column button click
        /// </summary>
        /// <param name="item">ComparisonItem to remove</param>
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
            emptyState.AddToClassList(emptyStateClassName);
            emptyState.Add(new Label("Add items to the list to compare"));
            columnsContainer.Add(emptyState);
        }

        /// <summary>
        /// Create InspectorElement for a ComparisonItem
        /// </summary>
        /// <param name="item">ComparisonItem to display</param>
        /// <param name="container">Container to add the inspector to</param>
        private void CreateInspector(ComparisonItem item, VisualElement container)
        {
            // Destroy previous editor if exists
            if (itemEditorMap.TryGetValue(item, out var existingEditor) && existingEditor != null)
            {
                UnityEngine.Object.DestroyImmediate(existingEditor);
            }

            // Get display mode from settings
            var targetObj = item.SerializedObject.targetObject;
            var displayMode = InspectorDisplaySettings.instance.GetDisplayMode(targetObj.GetType());

            UnityEditor.Editor editor = null;
            IMGUIContainer imguiContainer = null;
            InspectorElement inspectorElement = null;

            if (displayMode == InspectorDisplayMode.Editor)
            {
                CreateEditorContainer(targetObj, out editor, out imguiContainer);
            }
            else if (displayMode == InspectorDisplayMode.EditorThenElement)
            {
                // First use Editor, then InspectorElement for remaining properties
                CreateEditorContainer(targetObj, out editor, out imguiContainer, bottomPadding: 10f);
                inspectorElement = new InspectorElement();
                inspectorElement.Bind(item.SerializedObject);
            }
            else
            {
                // Default mode: Use InspectorElement

                // Draw header using Editor
                editor = UnityEditor.Editor.CreateEditor(targetObj);
                imguiContainer = editor ? new IMGUIContainer(editor.DrawHeader) : null;

                // Create InspectorElement
                inspectorElement = new InspectorElement();
                inspectorElement.Bind(item.SerializedObject);
            }

            if (imguiContainer != null)
            {
                container.Add(imguiContainer);
            }

            if (inspectorElement != null)
            {
                container.Add(inspectorElement);
            }

            itemEditorMap[item] = editor;
        }

        /// <summary>
        /// Create Editor and IMGUIContainer for an object
        /// </summary>
        /// <param name="targetObj">Target Unity Object</param>
        /// <param name="editor">Output Editor instance</param>
        /// <param name="imguiContainer">Output IMGUIContainer instance</param>
        /// <param name="bottomPadding">Optional bottom padding</param>
        private static void CreateEditorContainer(
            UnityEngine.Object targetObj,
            out UnityEditor.Editor editor,
            out IMGUIContainer imguiContainer,
            float bottomPadding = 0)
        {
            var path = AssetDatabase.GetAssetPath(targetObj);
            var importer = AssetImporter.GetAtPath(path);

            // If the importer is a derived type, use it; otherwise use the target object
            var isInheritedImporter = (importer != null) && (importer.GetType() != typeof(AssetImporter));
            var newEditor = UnityEditor.Editor.CreateEditor(isInheritedImporter ? importer : targetObj);

            imguiContainer = new IMGUIContainer(() =>
            {
                if (newEditor == null || newEditor.target == null)
                {
                    return;
                }

                newEditor.DrawHeader();

                EditorGUILayout.BeginVertical();
                {
                    newEditor.OnInspectorGUI();

                    if (bottomPadding > 0)
                    {
                        EditorGUILayout.Space(bottomPadding);
                    }
                }
                EditorGUILayout.EndVertical();
            });

            editor = newEditor;
        }

        /// <summary>
        /// Scroll to center the selected indices and reset vertical scroll
        /// </summary>
        /// <param name="indices">List of selected column indices</param>
        private void ScrollToIndices(List<int> indices)
        {
            if (indices.Count == 0)
            {
                return;
            }

            // Calculate average index
            var sum = 0;
            foreach (var index in indices)
            {
                // Adjust index to represent visual column index
                // Item index i is at visual column (i / rowCount)
                sum += (index / rowCount);
            }

            var avgIndex = sum / indices.Count;

            // Calculate target position
            // Column width + margins (5px left + 5px right = 10px)
            // Note: Margin is defined in USS (.inspector-column { margin: 5px; })
            var itemWidth = columnWidth + 10f;
            var startOffset = 5f; // First item left margin

            var targetCenterX = startOffset + (avgIndex * itemWidth) + (columnWidth / 2f);
            var viewportWidth = scrollView.contentViewport.layout.width;

            // If viewport layout is not ready, try main element layout
            if (float.IsNaN(viewportWidth) || viewportWidth == 0)
            {
                viewportWidth = scrollView.layout.width;
            }

            // Calculate scroll X to center the target
            var scrollX = targetCenterX - (viewportWidth / 2f);

            // Clamp to 0 (max clamping is handled by ScrollView automatically)
            if (scrollX < 0)
            {
                scrollX = 0;
            }

            // Reset vertical scroll to 0 as requested
            scrollView.scrollOffset = new Vector2(scrollX, 0);
        }
    }
}