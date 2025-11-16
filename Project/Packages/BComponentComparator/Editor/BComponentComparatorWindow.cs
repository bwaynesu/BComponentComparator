using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BTools.BComponentComparator.Editor
{
    /// <summary>
    /// Main EditorWindow for Component Comparator tool.
    /// Provides UI to specify Component type, build comparison list, and view side-by-side Inspectors.
    /// </summary>
    public class BComponentComparatorWindow : EditorWindow
    {
        // --- Left Panel Class Names ---
        private static readonly string controlPanelClassName = "control-panel";
        private static readonly string componentTypeFieldClassName = "component-type-field";
        private static readonly string displayModeRowClassName = "display-mode-row";
        private static readonly string displayModeLabelClassName = "display-mode-label";
        private static readonly string widthControlRowClassName = "width-control-row";
        private static readonly string widthLabelClassName = "width-label";
        private static readonly string widthSliderClassName = "width-slider";
        private static readonly string comparisonListClassName = "comparison-list";
        private static readonly string buttonContainerClassName = "button-container";

        // --- Left Panel Element Names ---
        private static readonly string listPlaceholderName = "list-placeholder";

        // --- Right Panel Class Names ---
        private static readonly string comparisonPanelClassName = "comparison-panel";
        private static readonly string comparisonScrollViewClassName = "comparison-scroll-view";
        private static readonly string emptyStateClassName = "empty-state";

        // --- Right Panel Element Names ---
        private static readonly string scrollViewName = "inspector-scroll-view";

        // --- Serialized Fields (persist across recompilation) ---
        [SerializeField]
        private string componentTypeAssemblyQualifiedName;

        [SerializeField]
        private List<UnityEngine.Object> serializedObjects = new();

        [SerializeField]
        private int inspectorWidth = 300;

        // --- Fields ---
        private Type currentComponentType;
        private ComparisonListController listController;
        private InspectorColumnController inspectorController;

        // --- UI Elements ---
        private TwoPaneSplitView splitView;
        private VisualElement componentTypeField;
        private Label componentTypeLabel;
        private Button clearListButton;
        private SliderInt widthSlider;
        private EnumField displayModeField;

        [MenuItem("Window/BTools/BComponentComparator")]
        public static void ShowWindow()
        {
            var window = GetWindow<BComponentComparatorWindow>();
            window.titleContent = new GUIContent("BComponentComparator");
            window.minSize = new Vector2(800, 400);
        }

        public void CreateGUI()
        {
            // Load USS stylesheet using relative path from script location
            var scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            var scriptDirectory = Path.GetDirectoryName(scriptPath);
            var styleSheetPath = Path.Combine(scriptDirectory, "Styles", $"{nameof(BComponentComparatorWindow)}.uss");
            styleSheetPath = styleSheetPath.Replace("\\", "/"); // Normalize path separators

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            if (styleSheet != null)
            {
                rootVisualElement.styleSheets.Add(styleSheet);
            }

            // Create split view layout
            splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            splitView.AddToClassList("b-component-comparator-window");
            rootVisualElement.Add(splitView);

            CreateLeftPanel();
            CreateRightPanel();

            InitializeControllers();
            RegisterEventHandlers();

            // Monitor geometry changes (e.g., window docking) to re-register callbacks
            rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            RestoreState();
        }

        private void OnDisable()
        {
            UnregisterEventHandlers();
        }

        private void OnDestroy()
        {
            listController?.Dispose();
            inspectorController?.Dispose();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            // Re-register drag-drop callbacks when window is docked/undocked
            if (componentTypeField != null)
            {
                DragDropHandler.RegisterDragDropCallbacks(
                    componentTypeField,
                    obj => DragDropHandler.TryExtractedType(obj, out _),
                    OnComponentTypeDrop
                );
            }

            if (currentComponentType != null && listController != null)
            {
                listController.RefreshDragDropCallbacks();
            }
        }

        private void CreateLeftPanel()
        {
            var leftPanel = new VisualElement();
            leftPanel.AddToClassList(controlPanelClassName);

            // Component type field
            componentTypeField = new VisualElement();
            componentTypeField.AddToClassList(componentTypeFieldClassName);
            componentTypeLabel = new Label("Drag Component type here...");
            componentTypeLabel.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
            componentTypeField.Add(componentTypeLabel);
            leftPanel.Add(componentTypeField);

            // Display Mode controls
            var displayModeRow = new VisualElement();
            displayModeRow.AddToClassList(displayModeRowClassName);

            var displayModeLabel = new Label("Display Mode:");
            displayModeLabel.AddToClassList(displayModeLabelClassName);
            displayModeRow.Add(displayModeLabel);

            displayModeField = new EnumField("", InspectorDisplayMode.Element);
            displayModeField.style.flexGrow = 1;
            displayModeField.SetEnabled(false);
            displayModeField.RegisterValueChangedCallback(OnDisplayModeChanged);
            displayModeRow.Add(displayModeField);

            leftPanel.Add(displayModeRow);

            // Column width controls - label and slider in one row
            var widthRow = new VisualElement();
            widthRow.AddToClassList(widthControlRowClassName);

            var widthLabel = new Label("Inspector Width:");
            widthLabel.AddToClassList(widthLabelClassName);
            widthRow.Add(widthLabel);

            widthSlider = new SliderInt(200, 600) { value = 300 };
            widthSlider.AddToClassList(widthSliderClassName);
            widthRow.Add(widthSlider);

            // Slider updates inspector width
            widthSlider.RegisterValueChangedCallback(evt =>
            {
                inspectorWidth = evt.newValue;
                inspectorController?.SetColumnWidth(evt.newValue);
            });

            leftPanel.Add(widthRow);

            // List view placeholder (will be created by ComparisonListController)
            var listPlaceholder = new VisualElement { name = listPlaceholderName };
            listPlaceholder.AddToClassList(comparisonListClassName);
            listPlaceholder.style.flexGrow = 1;
            leftPanel.Add(listPlaceholder);

            // Clear button below list
            var buttonContainer = new VisualElement();
            buttonContainer.AddToClassList(buttonContainerClassName);

            clearListButton = new Button { text = "Clear List" };
            clearListButton.SetEnabled(false);
            buttonContainer.Add(clearListButton);

            leftPanel.Add(buttonContainer);

            splitView.Add(leftPanel);
        }

        private void CreateRightPanel()
        {
            var rightPanel = new VisualElement();
            rightPanel.AddToClassList(comparisonPanelClassName);

            // ScrollView for Inspector columns (will be managed by InspectorColumnController)
            var scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            scrollView.AddToClassList(comparisonScrollViewClassName);
            scrollView.name = scrollViewName;
            scrollView.style.flexGrow = 1;

            // Empty state
            var emptyState = new VisualElement();
            emptyState.AddToClassList(emptyStateClassName);
            emptyState.Add(new Label("Add items to the list to compare"));
            scrollView.Add(emptyState);

            rightPanel.Add(scrollView);
            splitView.Add(rightPanel);
        }

        private void InitializeControllers()
        {
            // Initialize ComparisonListController
            var listPlaceholder = rootVisualElement.Q(listPlaceholderName);
            if (listPlaceholder != null)
            {
                var parent = listPlaceholder.parent;
                int placeholderIndex = parent.IndexOf(listPlaceholder);

                listController = new ComparisonListController(parent, placeholderIndex);
                listPlaceholder.RemoveFromHierarchy(); // Remove placeholder

                // Subscribe to list changes
                listController.OnItemsChanged += RefreshInspectorColumns;

                // Subscribe to selection changes
                listController.OnSelectionChangedEvent += OnListSelectionChanged;
            }

            // Initialize InspectorColumnController
            var scrollView = rootVisualElement.Q<ScrollView>(scrollViewName);
            if (scrollView != null)
            {
                inspectorController = new InspectorColumnController(scrollView);
                inspectorController.OnRemoveItemRequested += OnRemoveItemFromInspector;
            }
        }

        private void OnRemoveItemFromInspector(ComparisonItem item)
        {
            if (listController == null || item == null)
            {
                return;
            }

            var items = listController.GetItems();
            var index = -1;
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i] == item)
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                listController.RemoveItem(index);
            }
        }

        private void RegisterEventHandlers()
        {
            // Register drag-drop for component type field
            if (componentTypeField != null)
            {
                DragDropHandler.RegisterDragDropCallbacks(
                    componentTypeField,
                    obj => DragDropHandler.TryExtractedType(obj, out _),
                    OnComponentTypeDrop
                );
            }

            // Register button click events
            if (clearListButton != null)
            {
                clearListButton.clicked += OnClearListButtonClick;
            }
        }

        private void UnregisterEventHandlers()
        {
            // Unregister geometry change callback
            if (rootVisualElement != null)
            {
                rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            }

            // Unregister drag-drop
            if (componentTypeField != null)
            {
                DragDropHandler.UnregisterDragDropCallbacks(componentTypeField);
            }

            // Unregister button click events
            if (clearListButton != null)
            {
                clearListButton.clicked -= OnClearListButtonClick;
            }

            // Unsubscribe from list changes
            if (listController != null)
            {
                listController.OnItemsChanged -= RefreshInspectorColumns;
                listController.OnSelectionChangedEvent -= OnListSelectionChanged;
            }
        }

        private void OnComponentTypeDrop(UnityEngine.Object droppedObject)
        {
            if (!DragDropHandler.TryExtractedType(droppedObject, out Type componentType))
            {
                return;
            }

            currentComponentType = componentType;
            componentTypeLabel.text = componentType.Name;
            componentTypeLabel.style.color = Color.white;

            listController?.SetRequiredType(componentType);
            UpdateDisplayModeField();
            listController?.AddItem(droppedObject);
            ValidateButtonStates();

            SaveState();
        }

        private void OnClearListButtonClick()
        {
            if (listController == null || listController.GetItems().Count <= 0)
            {
                return;
            }

            listController.ClearItems();
            SaveState();
        }

        private void RefreshInspectorColumns()
        {
            if (inspectorController != null && listController != null)
            {
                inspectorController.RebuildColumns(listController.GetItems());
            }

            ValidateButtonStates();
            SaveState();
        }

        private void OnListSelectionChanged(List<UnityEngine.Object> selectedObjects)
        {
            inspectorController?.HighlightColumns(selectedObjects);
            UpdateDisplayModeField();
        }

        private void UpdateDisplayModeField()
        {
            // If no component type is specified, disable the field
            if (currentComponentType == null)
            {
                displayModeField.SetEnabled(false);
                displayModeField.SetValueWithoutNotify(InspectorDisplayMode.Element);
                return;
            }

            // Enable the field and show current mode for the current component type
            displayModeField.SetEnabled(true);
            var currentMode = InspectorDisplaySettings.instance.GetDisplayMode(currentComponentType);
            displayModeField.SetValueWithoutNotify(currentMode);
        }

        private void OnDisplayModeChanged(ChangeEvent<Enum> evt)
        {
            if (currentComponentType == null)
                return;

            var newMode = (InspectorDisplayMode)evt.newValue;

            // Save the new mode for the current component type
            InspectorDisplaySettings.instance.SetDisplayMode(currentComponentType, newMode);

            // Rebuild the inspector columns to apply the new mode
            RefreshInspectorColumns();
        }

        private void ValidateButtonStates()
        {
            var hasItems = listController != null && listController.GetItems().Count > 0;
            clearListButton?.SetEnabled(hasItems);
        }

        private void SaveState()
        {
            // Save component type
            componentTypeAssemblyQualifiedName = currentComponentType?.AssemblyQualifiedName;

            // Save objects from list
            serializedObjects.Clear();

            if (listController != null)
            {
                var items = listController.GetItems();
                foreach (var item in items)
                {
                    if (item.IsValid())
                    {
                        serializedObjects.Add(item.TargetObject);
                    }
                }
            }
        }

        private void RestoreState()
        {
            // Restore component type
            if (!string.IsNullOrEmpty(componentTypeAssemblyQualifiedName))
            {
                currentComponentType = Type.GetType(componentTypeAssemblyQualifiedName);
                if (currentComponentType != null)
                {
                    componentTypeLabel.text = currentComponentType.Name;
                    componentTypeLabel.style.color = Color.white;

                    listController?.SetRequiredType(currentComponentType);
                }
            }

            // Restore inspector width
            if (widthSlider != null && inspectorWidth != 300)
            {
                widthSlider.SetValueWithoutNotify(inspectorWidth);
                inspectorController?.SetColumnWidth(inspectorWidth);
            }

            // Restore objects
            if (serializedObjects != null &&
                serializedObjects.Count > 0 &&
                currentComponentType != null &&
                listController != null)
            {
                var validObjects = new List<UnityEngine.Object>();
                foreach (var obj in serializedObjects)
                {
                    if (obj != null && DragDropHandler.IsValidObject(obj, currentComponentType))
                    {
                        validObjects.Add(obj);
                    }
                }

                if (validObjects.Count > 0)
                {
                    listController.AddItems(validObjects);
                }
            }

            ValidateButtonStates();
        }
    }
}