using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ComponentComparator.Editor.Tests
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
    }
}
