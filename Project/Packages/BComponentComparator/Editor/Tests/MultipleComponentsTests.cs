using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace BTools.BComponentComparator.Editor.Tests
{
    /// <summary>
    /// Tests for adding multiple components of the same type from a single GameObject
    /// </summary>
    public class MultipleComponentsTests
    {
        private GameObject testGameObject;
        private ComparisonListController listController;
        private UnityEngine.UIElements.VisualElement container;

        [SetUp]
        public void SetUp()
        {
            testGameObject = new GameObject("TestObject");
            container = new UnityEngine.UIElements.VisualElement();
            listController = new ComparisonListController(container);
        }

        [TearDown]
        public void TearDown()
        {
            listController?.Dispose();

            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
        }

        #region Single Component Tests

        [Test]
        public void AddItem_GameObjectWithSingleComponent_AddsOneItem()
        {
            // Arrange
            testGameObject.AddComponent<BoxCollider>();
            listController.SetRequiredType(typeof(BoxCollider));

            // Act
            listController.AddItem(testGameObject);

            // Assert
            var items = listController.GetItems();
            Assert.AreEqual(1, items.Count, "Should add exactly one component");
            Assert.IsInstanceOf<BoxCollider>(items[0].TargetObject);
        }

        #endregion

        #region Multiple Components Tests

        [Test]
        public void AddItem_GameObjectWithMultipleComponents_AddsAllComponents()
        {
            // Arrange
            testGameObject.AddComponent<BoxCollider>();
            testGameObject.AddComponent<BoxCollider>();
            testGameObject.AddComponent<BoxCollider>();
            listController.SetRequiredType(typeof(BoxCollider));

            // Act
            listController.AddItem(testGameObject);

            // Assert
            var items = listController.GetItems();
            Assert.AreEqual(3, items.Count, "Should add all three BoxColliders");
            Assert.IsTrue(items.All(item => item.TargetObject is BoxCollider));
        }

        [Test]
        public void AddItem_GameObjectWithMultipleDerivedComponents_AddsAllMatchingComponents()
        {
            // Arrange - Add multiple colliders (all derive from Collider)
            testGameObject.AddComponent<BoxCollider>();
            testGameObject.AddComponent<SphereCollider>();
            testGameObject.AddComponent<CapsuleCollider>();
            listController.SetRequiredType(typeof(Collider));

            // Act
            listController.AddItem(testGameObject);

            // Assert
            var items = listController.GetItems();
            Assert.AreEqual(3, items.Count, "Should add all three Collider components");
            Assert.IsTrue(items.All(item => item.TargetObject is Collider));
        }

        [Test]
        public void AddItem_GameObjectWithMixedComponents_AddsOnlyMatchingType()
        {
            // Arrange
            testGameObject.AddComponent<BoxCollider>();
            testGameObject.AddComponent<BoxCollider>();
            testGameObject.AddComponent<Rigidbody>(); // Different type
            listController.SetRequiredType(typeof(BoxCollider));

            // Act
            listController.AddItem(testGameObject);

            // Assert
            var items = listController.GetItems();
            Assert.AreEqual(2, items.Count, "Should add only the two BoxColliders");
            Assert.IsTrue(items.All(item => item.TargetObject is BoxCollider));
        }

        #endregion

        #region Multiple GameObjects with Multiple Components

        [Test]
        public void AddItems_MultipleGameObjectsWithMultipleComponents_AddsAllComponents()
        {
            // Arrange
            var go1 = new GameObject("GO1");
            go1.AddComponent<BoxCollider>();
            go1.AddComponent<BoxCollider>();

            var go2 = new GameObject("GO2");
            go2.AddComponent<BoxCollider>();
            go2.AddComponent<BoxCollider>();
            go2.AddComponent<BoxCollider>();

            listController.SetRequiredType(typeof(BoxCollider));

            try
            {
                // Act
                listController.AddItems(new[] { go1, go2 });

                // Assert
                var items = listController.GetItems();
                Assert.AreEqual(5, items.Count, "Should add all 5 BoxColliders (2 from GO1, 3 from GO2)");
                Assert.IsTrue(items.All(item => item.TargetObject is BoxCollider));
            }
            finally
            {
                Object.DestroyImmediate(go1);
                Object.DestroyImmediate(go2);
            }
        }

        #endregion

        #region Duplicate Prevention Tests

        [Test]
        public void AddItem_SameGameObjectTwice_DoesNotAddDuplicates()
        {
            // Arrange
            testGameObject.AddComponent<BoxCollider>();
            testGameObject.AddComponent<BoxCollider>();
            listController.SetRequiredType(typeof(BoxCollider));

            // Act
            listController.AddItem(testGameObject);
            listController.AddItem(testGameObject); // Add same GameObject again

            // Assert
            var items = listController.GetItems();
            Assert.AreEqual(2, items.Count, "Should not add duplicates, still only 2 components");
        }

        [Test]
        public void AddItems_GameObjectArrayWithDuplicates_AddsEachComponentOnce()
        {
            // Arrange
            testGameObject.AddComponent<BoxCollider>();
            testGameObject.AddComponent<BoxCollider>();
            listController.SetRequiredType(typeof(BoxCollider));

            // Act - Pass same GameObject twice
            listController.AddItems(new[] { testGameObject, testGameObject });

            // Assert
            var items = listController.GetItems();
            Assert.AreEqual(2, items.Count, "Should add each component only once despite duplicate GameObjects");
        }

        #endregion

        #region Edge Cases

        [Test]
        public void AddItem_GameObjectWithNoMatchingComponents_AddsNothing()
        {
            // Arrange - No components added
            listController.SetRequiredType(typeof(BoxCollider));

            // Act
            listController.AddItem(testGameObject);

            // Assert
            var items = listController.GetItems();
            Assert.AreEqual(0, items.Count, "Should not add anything when no matching components found");
        }

        [Test]
        public void AddItem_GameObjectWithTransform_AddsTransform()
        {
            // Arrange - Every GameObject has Transform
            listController.SetRequiredType(typeof(Transform));

            // Act
            listController.AddItem(testGameObject);

            // Assert
            var items = listController.GetItems();
            Assert.AreEqual(1, items.Count, "Should add the Transform component");
            Assert.IsInstanceOf<Transform>(items[0].TargetObject);
        }

        [Test]
        public void AddItem_GameObjectWithMonoBehaviourBaseType_AddsAllDerivedComponents()
        {
            // Arrange
            testGameObject.AddComponent<TestMonoBehaviourComponent1>();
            testGameObject.AddComponent<TestMonoBehaviourComponent2>();
            listController.SetRequiredType(typeof(MonoBehaviour));

            // Act
            listController.AddItem(testGameObject);

            // Assert
            var items = listController.GetItems();
            Assert.AreEqual(2, items.Count, "Should add all MonoBehaviour-derived components");
            Assert.IsTrue(items.All(item => item.TargetObject is MonoBehaviour));
        }

        #endregion

        #region Non-Component Asset Tests

        [Test]
        public void AddItem_NonGameObjectAsset_AddsDirectly()
        {
            // Arrange
            var scriptableObject = ScriptableObject.CreateInstance<TestScriptableObject>();
            listController.SetRequiredType(typeof(TestScriptableObject));

            try
            {
                // Act
                listController.AddItem(scriptableObject);

                // Assert
                var items = listController.GetItems();
                Assert.AreEqual(1, items.Count, "Should add ScriptableObject directly");
                Assert.AreEqual(scriptableObject, items[0].TargetObject);
            }
            finally
            {
                Object.DestroyImmediate(scriptableObject);
            }
        }

        [Test]
        public void AddItems_MixedGameObjectsAndAssets_HandlesCorrectly()
        {
            // Arrange
            testGameObject.AddComponent<BoxCollider>();
            testGameObject.AddComponent<BoxCollider>();

            var go2 = new GameObject("GO2");
            go2.AddComponent<BoxCollider>();

            listController.SetRequiredType(typeof(BoxCollider));

            try
            {
                // Act
                listController.AddItems(new UnityEngine.Object[] { testGameObject, go2 });

                // Assert
                var items = listController.GetItems();
                Assert.AreEqual(3, items.Count, "Should add 2 from first GO and 1 from second GO");
                Assert.IsTrue(items.All(item => item.TargetObject is BoxCollider));
            }
            finally
            {
                Object.DestroyImmediate(go2);
            }
        }

        #endregion
    }

    #region Test Helper Components

    public class TestMonoBehaviourComponent1 : MonoBehaviour
    {
        public int value1;
    }

    public class TestMonoBehaviourComponent2 : MonoBehaviour
    {
        public int value2;
    }

    #endregion
}
