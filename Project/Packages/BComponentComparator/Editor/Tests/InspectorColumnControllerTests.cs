using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BTools.BComponentComparator.Editor.Tests
{
    /// <summary>
    /// Tests for InspectorColumnController
    /// </summary>
    public class InspectorColumnControllerTests
    {
        private UnityEngine.UIElements.ScrollView scrollView;
        private InspectorColumnController controller;
        private GameObject testObject1;
        private GameObject testObject2;

        [SetUp]
        public void SetUp()
        {
            scrollView = new UnityEngine.UIElements.ScrollView();
            controller = new InspectorColumnController(scrollView);

            testObject1 = new GameObject("TestObject1");
            testObject1.AddComponent<Rigidbody>();

            testObject2 = new GameObject("TestObject2");
            testObject2.AddComponent<Rigidbody>();
        }

        [TearDown]
        public void TearDown()
        {
            controller?.Dispose();

            if (testObject1 != null)
            {
                Object.DestroyImmediate(testObject1);
            }

            if (testObject2 != null)
            {
                Object.DestroyImmediate(testObject2);
            }
        }

        [Test]
        public void Constructor_WithValidScrollView_CreatesController()
        {
            // Assert
            Assert.IsNotNull(controller);
        }

        [Test]
        public void Constructor_WithNullScrollView_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() =>
            {
                new InspectorColumnController(null);
            });
        }

        [Test]
        public void RebuildColumns_WithEmptyList_ShowsEmptyState()
        {
            // Arrange
            var emptyList = new List<ComparisonItem>();

            // Act
            controller.RebuildColumns(emptyList);

            // Assert
            var emptyState = scrollView.Q(className: "empty-state");
            Assert.IsNotNull(emptyState, "Empty state should be displayed");
        }

        [Test]
        public void RebuildColumns_WithValidItems_CreatesInspectorColumns()
        {
            // Arrange
            var items = new List<ComparisonItem>
            {
                new ComparisonItem(testObject1, typeof(Rigidbody)),
                new ComparisonItem(testObject2, typeof(Rigidbody))
            };

            // Act
            controller.RebuildColumns(items);

            // Assert
            var columns = scrollView.Query(className: "inspector-column").ToList();
            Assert.AreEqual(2, columns.Count, "Should create 2 Inspector columns");

            // Cleanup
            foreach (var item in items)
            {
                item.Dispose();
            }
        }

        [Test]
        public void RebuildColumns_WithInvalidItem_SkipsInvalidItem()
        {
            // Arrange
            var item1 = new ComparisonItem(testObject1, typeof(Rigidbody));
            var item2 = new ComparisonItem(testObject2, typeof(Rigidbody));

            // Destroy testObject2 to make item2 invalid
            Object.DestroyImmediate(testObject2);
            testObject2 = null;

            var items = new List<ComparisonItem> { item1, item2 };

            // Act
            controller.RebuildColumns(items);

            // Assert
            var columns = scrollView.Query(className: "inspector-column").ToList();
            Assert.AreEqual(1, columns.Count, "Should only create 1 column for valid item");

            // Cleanup
            item1.Dispose();
            item2.Dispose();
        }

        [Test]
        public void Clear_RemovesAllColumns()
        {
            // Arrange
            var items = new List<ComparisonItem>
            {
                new ComparisonItem(testObject1, typeof(Rigidbody)),
                new ComparisonItem(testObject2, typeof(Rigidbody))
            };
            controller.RebuildColumns(items);

            // Act
            controller.Clear();

            // Assert
            var columns = scrollView.Query(className: "inspector-column").ToList();
            Assert.AreEqual(0, columns.Count, "All columns should be removed");

            // Cleanup
            foreach (var item in items)
            {
                item.Dispose();
            }
        }

        [Test]
        public void Dispose_CleansUpResources()
        {
            // Arrange
            var items = new List<ComparisonItem>
            {
                new ComparisonItem(testObject1, typeof(Rigidbody))
            };
            controller.RebuildColumns(items);

            // Act
            controller.Dispose();

            // Assert
            var columns = scrollView.Query(className: "inspector-column").ToList();
            Assert.AreEqual(0, columns.Count, "Dispose should clear all columns");

            // Cleanup
            foreach (var item in items)
            {
                item.Dispose();
            }
        }

        [Test]
        public void RebuildColumns_WithNullList_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() =>
            {
                controller.RebuildColumns(null);
            });
        }

        [Test]
        public void SetRowCount_DistributesItemsToRows_Correctly()
        {
            // Arrange
            // Prepare 4 test objects
            var items = new List<ComparisonItem>();
            var gameObjects = new List<GameObject>();
            for (int i = 0; i < 4; i++)
            {
                var go = new GameObject($"TestObject{i}");
                go.AddComponent<Rigidbody>();
                gameObjects.Add(go);
                items.Add(new ComparisonItem(go, typeof(Rigidbody)));
            }

            // Act
            // Set to 2 Rows
            controller.SetRowCount(2);
            controller.RebuildColumns(items);

            // Assert
            // 1. Get main container
            var container = scrollView.Q(className: "inspector-columns-container");
            Assert.IsNotNull(container);

            // 2. Verify Row count (Should be 2)
            Assert.AreEqual(2, container.childCount, "Should create 2 row containers");

            // 3. Verify Item distribution
            // Expected logic (i % 2):
            // Row 0: Index 0, 2
            // Row 1: Index 1, 3
            
            var row0 = container.ElementAt(0);
            var row1 = container.ElementAt(1);

            Assert.AreEqual(2, row0.childCount, "Row 0 should have 2 items");
            Assert.AreEqual(2, row1.childCount, "Row 1 should have 2 items");

            // Verify Row 0 content (Item 0, Item 2)
            var col0_0 = row0.ElementAt(0).Q<Label>(className: "inspector-column-header-label");
            var col0_1 = row0.ElementAt(1).Q<Label>(className: "inspector-column-header-label");
            Assert.AreEqual("TestObject0", col0_0.text);
            Assert.AreEqual("TestObject2", col0_1.text);

            // Verify Row 1 content (Item 1, Item 3)
            var col1_0 = row1.ElementAt(0).Q<Label>(className: "inspector-column-header-label");
            var col1_1 = row1.ElementAt(1).Q<Label>(className: "inspector-column-header-label");
            Assert.AreEqual("TestObject1", col1_0.text);
            Assert.AreEqual("TestObject3", col1_1.text);

            // Cleanup
            foreach (var item in items)
            {
                item.Dispose();
            }
            foreach (var go in gameObjects)
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}