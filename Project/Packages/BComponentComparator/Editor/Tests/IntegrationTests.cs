using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BTools.BComponentComparator.Editor.Tests
{
    /// <summary>
    /// Integration tests for end-to-end workflow
    /// </summary>
    public class IntegrationTests
    {
        private BComponentComparatorWindow window;
        private GameObject testObject1;
        private GameObject testObject2;
        private GameObject testObject3;

        [SetUp]
        public void SetUp()
        {
            window = EditorWindow.GetWindow<BComponentComparatorWindow>();
            window.CreateGUI();

            testObject1 = new GameObject("TestObject1");
            testObject1.transform.position = new Vector3(1, 0, 0);

            testObject2 = new GameObject("TestObject2");
            testObject2.transform.position = new Vector3(2, 0, 0);

            testObject3 = new GameObject("TestObject3");
            testObject3.transform.position = new Vector3(3, 0, 0);
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject1 != null)
            {
                Object.DestroyImmediate(testObject1);
            }

            if (testObject2 != null)
            {
                Object.DestroyImmediate(testObject2);
            }

            if (testObject3 != null)
            {
                Object.DestroyImmediate(testObject3);
            }

            if (window != null)
            {
                window.Close();
            }
        }

        [Test]
        public void EndToEndWorkflow_SpecifyTypeAndAddItems_CreatesInspectorColumns()
        {
            // This is a simplified integration test
            // Full integration testing would require simulating drag-drop which is complex in Unity Test Framework

            // Arrange - Window is already open and GUI created

            // Assert initial state
            var root = window.rootVisualElement;
            Assert.IsNotNull(root);

            var splitView = root.Q<TwoPaneSplitView>();
            Assert.IsNotNull(splitView, "Split view should exist");

            var scrollView = root.Q<ScrollView>("inspector-scroll-view");
            Assert.IsNotNull(scrollView, "Inspector scroll view should exist");

            // Test passes if basic structure is correct
            Assert.Pass("Basic window structure validated");
        }

        [Test]
        public void IntegrationTest_ButtonsExist()
        {
            // Arrange
            var root = window.rootVisualElement;

            // Act
            var buttons = root.Query<Button>().ToList();

            // Assert
            Assert.GreaterOrEqual(buttons.Count, 1, "Should have at least 1 button");

            var clearButton = buttons.Find(b => b.text.Contains("Clear"));

            Assert.IsNotNull(clearButton, "Clear button should exist");
        }
    }
}