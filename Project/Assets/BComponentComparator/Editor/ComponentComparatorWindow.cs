using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ComponentComparator.Editor
{
    /// <summary>
    /// Main EditorWindow for Component Comparator tool.
    /// Provides UI to specify Component type, build comparison list, and view side-by-side Inspectors.
    /// </summary>
    public class ComponentComparatorWindow : EditorWindow
    {
        // --- Fields ---
        private Type currentComponentType;
        private ComparisonListController listController;
        private InspectorColumnController inspectorController;

        // UI Elements
        private TwoPaneSplitView splitView;
        private VisualElement componentTypeField;
        private Label componentTypeLabel;
        private Button clearListButton;

        // --- EditorWindow Menu Item ---
        [MenuItem("Window/Component Comparator")]
        public static void ShowWindow()
        {
            ComponentComparatorWindow window = GetWindow<ComponentComparatorWindow>();
            window.titleContent = new GUIContent("Component Comparator");
            window.minSize = new Vector2(800, 400);
        }

        // --- UI Toolkit Lifecycle ---
        public void CreateGUI()
        {
            // Load USS stylesheet using relative path from script location
            var scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            var scriptDirectory = System.IO.Path.GetDirectoryName(scriptPath);
            var styleSheetPath = System.IO.Path.Combine(scriptDirectory, "Styles", "ComponentComparatorWindow.uss");
            styleSheetPath = styleSheetPath.Replace("\\", "/"); // Normalize path separators
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            if (styleSheet != null)
            {
                rootVisualElement.styleSheets.Add(styleSheet);
            }

            // Create split view layout
            splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            splitView.AddToClassList("component-comparator-window");
            rootVisualElement.Add(splitView);

            // Create left panel (control area)
            CreateLeftPanel();

            // Create right panel (comparison view)
            CreateRightPanel();

            // Initialize controllers
            InitializeControllers();

            // Register drag-drop and event handlers
            RegisterEventHandlers();
        }

        private void CreateLeftPanel()
        {
            var leftPanel = new VisualElement();
            leftPanel.AddToClassList("control-panel");

            // Component type field
            componentTypeField = new VisualElement();
            componentTypeField.AddToClassList("component-type-field");
            componentTypeLabel = new Label("Drag Component type here...");
            componentTypeLabel.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
            componentTypeField.Add(componentTypeLabel);
            leftPanel.Add(componentTypeField);

            // Column width controls
            var widthSliderContainer = new VisualElement();
            widthSliderContainer.style.marginTop = 5;
            widthSliderContainer.style.marginBottom = 5;
            
            // Label and input field row
            var widthInputRow = new VisualElement();
            widthInputRow.style.flexDirection = FlexDirection.Row;
            widthInputRow.style.alignItems = Align.Center;
            widthInputRow.style.marginBottom = 3;
            
            var widthLabel = new Label("Inspector Width:");
            widthLabel.style.marginRight = 5;
            widthInputRow.Add(widthLabel);
            
            var widthField = new IntegerField();
            widthField.value = 300;
            widthField.style.width = 60;
            widthInputRow.Add(widthField);
            
            widthSliderContainer.Add(widthInputRow);
            
            // Slider
            var widthSlider = new SliderInt(200, 600) { value = 300 };
            widthSlider.style.flexGrow = 1;
            widthSliderContainer.Add(widthSlider);
            
            // Sync on Enter key for field (avoid immediate clamping)
            widthField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    int clampedValue = Math.Max(200, Math.Min(600, widthField.value));
                    widthField.value = clampedValue;
                    widthSlider.SetValueWithoutNotify(clampedValue);
                    if (inspectorController != null)
                    {
                        inspectorController.SetColumnWidth(clampedValue);
                    }
                }
            });
            
            // Slider updates field and width immediately
            widthSlider.RegisterValueChangedCallback(evt =>
            {
                widthField.SetValueWithoutNotify(evt.newValue);
                if (inspectorController != null)
                {
                    inspectorController.SetColumnWidth(evt.newValue);
                }
            });
            
            leftPanel.Add(widthSliderContainer);

            // List view placeholder (will be created by ComparisonListController)
            var listPlaceholder = new VisualElement();
            listPlaceholder.name = "list-placeholder";
            listPlaceholder.AddToClassList("comparison-list");
            listPlaceholder.style.flexGrow = 1;
            leftPanel.Add(listPlaceholder);

            // Clear button below list
            var buttonContainer = new VisualElement();
            buttonContainer.AddToClassList("button-container");

            clearListButton = new Button { text = "Clear List" };
            clearListButton.SetEnabled(false);
            buttonContainer.Add(clearListButton);

            leftPanel.Add(buttonContainer);

            splitView.Add(leftPanel);
        }

        private void CreateRightPanel()
        {
            var rightPanel = new VisualElement();
            rightPanel.AddToClassList("comparison-panel");

            // ScrollView for Inspector columns (will be managed by InspectorColumnController)
            var scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            scrollView.AddToClassList("comparison-scroll-view");
            scrollView.name = "inspector-scroll-view";
            scrollView.style.flexGrow = 1;

            // Empty state
            var emptyState = new VisualElement();
            emptyState.AddToClassList("empty-state");
            emptyState.Add(new Label("Add items to the list to compare"));
            scrollView.Add(emptyState);

            rightPanel.Add(scrollView);
            splitView.Add(rightPanel);
        }

        private void InitializeControllers()
        {
            // Initialize ComparisonListController
            var listPlaceholder = rootVisualElement.Q("list-placeholder");
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
            var scrollView = rootVisualElement.Q<ScrollView>("inspector-scroll-view");
            if (scrollView != null)
            {
                inspectorController = new InspectorColumnController(scrollView);
                inspectorController.OnRemoveItemRequested += OnRemoveItemFromInspector;
            }
        }

        private void OnRemoveItemFromInspector(ComparisonItem item)
        {
            if (listController != null && item != null)
            {
                var items = listController.GetItems();
                int index = -1;
                for (int i = 0; i < items.Count; i++)
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
        }

        private void RegisterEventHandlers()
        {
            // Register drag-drop for component type field
            if (componentTypeField != null)
            {
                DragDropHandler.RegisterDragDropCallbacks(
                    componentTypeField,
                    obj => DragDropHandler.ValidateComponentType(obj, out _),
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
            if (DragDropHandler.ValidateComponentType(droppedObject, out Type componentType))
            {
                currentComponentType = componentType;
                componentTypeLabel.text = componentType.Name;
                componentTypeLabel.style.color = Color.white;

                // Set required type for list controller
                if (listController != null)
                {
                    listController.SetRequiredType(componentType);
                }

                // Enable buttons
                ValidateButtonStates();
            }
        }

        private void OnClearListButtonClick()
        {
            if (listController != null && listController.GetItems().Count > 0)
            {
                listController.ClearItems();
            }
        }

        private void RefreshInspectorColumns()
        {
            if (inspectorController != null && listController != null)
            {
                inspectorController.RebuildColumns(listController.GetItems());
            }

            ValidateButtonStates();
        }

        private void OnListSelectionChanged(List<UnityEngine.Object> selectedObjects)
        {
            if (inspectorController != null)
            {
                inspectorController.HighlightColumns(selectedObjects);
            }
        }

        private void ValidateButtonStates()
        {
            bool hasItems = listController != null && listController.GetItems().Count > 0;
            clearListButton?.SetEnabled(hasItems);
        }

        public void OnEnable()
        {
            // Event handlers are registered in CreateGUI() after UI elements are created
        }

        public void OnDisable()
        {
            UnregisterEventHandlers();
        }

        public void OnDestroy()
        {
            // Cleanup controllers
            if (listController != null)
            {
                listController.Dispose();
            }

            if (inspectorController != null)
            {
                inspectorController.Dispose();
            }
        }
    }
}
