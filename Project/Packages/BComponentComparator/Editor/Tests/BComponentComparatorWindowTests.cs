using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace BTools.BComponentComparator.Editor.Tests
{
    /// <summary>
    /// Tests for BComponentComparatorWindow
    /// </summary>
    public class BComponentComparatorWindowTests
    {
        private BComponentComparatorWindow window;

        [SetUp]
        public void SetUp()
        {
            // Open window for testing
            BComponentComparatorWindow.ShowWindow();
            window = EditorWindow.GetWindow<BComponentComparatorWindow>();
        }

        [TearDown]
        public void TearDown()
        {
            if (window != null)
            {
                window.Close();
            }
        }

        [Test]
        public void Window_CanBeOpened()
        {
            // Assert
            Assert.IsNotNull(window);
            Assert.IsTrue(window.titleContent.text.Contains("BComponentComparator"));
        }

        [Test]
        public void Window_HasMinimumSize()
        {
            // Assert
            Assert.GreaterOrEqual(window.minSize.x, 800);
            Assert.GreaterOrEqual(window.minSize.y, 400);
        }

        [Test]
        public void CreateGUI_CreatesUIStructure()
        {
            // Act
            window.CreateGUI();

            // Assert
            var root = window.rootVisualElement;
            Assert.IsNotNull(root);

            // Check for split view
            var splitView = root.Q<TwoPaneSplitView>();
            Assert.IsNotNull(splitView, "Split view should exist");

            // Check for component type field
            var typeField = root.Q(className: "component-type-field");
            Assert.IsNotNull(typeField, "Component type field should exist");

            // Check for buttons
            var buttons = root.Query<Button>().ToList();
            Assert.GreaterOrEqual(buttons.Count, 1, "Should have at least 1 button");

            // Check for scroll view
            var scrollView = root.Q<ScrollView>("inspector-scroll-view");
            Assert.IsNotNull(scrollView, "Inspector scroll view should exist");
        }
    }
}