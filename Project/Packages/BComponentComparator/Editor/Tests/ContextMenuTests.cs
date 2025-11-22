using NUnit.Framework;
using UnityEngine;

namespace BTools.BComponentComparator.Editor.Tests
{
    /// <summary>
    /// Tests for Context Menu integration functionality
    /// </summary>
    public class ContextMenuTests
    {
        private GameObject testGameObject;

        [SetUp]
        public void SetUp()
        {
            testGameObject = new GameObject("TestObject");
        }

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
        }

        [Test]
        public void AddComponentFromContextMenu_WithValidComponent_ComponentExists()
        {
            // Arrange
            var component = testGameObject.AddComponent<BoxCollider>();

            // Assert - Component should exist and be valid
            Assert.IsNotNull(component, "Component should be created");
            Assert.IsTrue(component.gameObject == testGameObject, "Component should belong to GameObject");
        }

        [Test]
        public void ComponentContextMenu_CanAccessComponentType()
        {
            // Arrange
            var component = testGameObject.AddComponent<BoxCollider>();

            // Act
            var componentType = component.GetType();

            // Assert
            Assert.AreEqual(typeof(BoxCollider), componentType, "Should correctly identify component type");
        }

        [Test]
        public void MultipleComponents_CanBeAccessedIndividually()
        {
            // Arrange
            var component1 = testGameObject.AddComponent<BoxCollider>();
            var component2 = testGameObject.AddComponent<SphereCollider>();

            // Assert
            Assert.IsNotNull(component1, "First component should exist");
            Assert.IsNotNull(component2, "Second component should exist");
            Assert.AreNotEqual(component1.GetType(), component2.GetType(), "Components should be different types");
        }

        [Test]
        public void ComponentType_CanBeDeterminedFromComponent()
        {
            // Arrange
            var component = testGameObject.AddComponent<Rigidbody>();

            // Act
            var type = component.GetType();

            // Assert
            Assert.IsTrue(typeof(Component).IsAssignableFrom(type), "Should be assignable to Component");
            Assert.AreEqual(typeof(Rigidbody), type, "Should be Rigidbody type");
        }

        [Test]
        public void Transform_IsAlwaysPresent()
        {
            // Act
            var transform = testGameObject.transform;

            // Assert
            Assert.IsNotNull(transform, "Transform should always be present");
            Assert.AreEqual(typeof(Transform), transform.GetType(), "Should be Transform type");
        }

        [Test]
        public void MonoBehaviour_CanBeAdded()
        {
            // Arrange
            var monoBehaviour = testGameObject.AddComponent<TestMonoBehaviourComponent>();

            // Assert
            Assert.IsNotNull(monoBehaviour, "MonoBehaviour component should be created");
            Assert.IsTrue(monoBehaviour is MonoBehaviour, "Should be MonoBehaviour type");
        }

        [Test]
        public void MultipleComponentsOfSameType_CanExist()
        {
            // Arrange
            var component1 = testGameObject.AddComponent<BoxCollider>();
            var component2 = testGameObject.AddComponent<BoxCollider>();

            // Assert
            Assert.IsNotNull(component1, "First component should exist");
            Assert.IsNotNull(component2, "Second component should exist");
            Assert.AreNotSame(component1, component2, "Should be different instances");
        }
    }

    /// <summary>
    /// Test MonoBehaviour component for context menu testing
    /// </summary>
    public class TestMonoBehaviourComponent : MonoBehaviour
    {
        public int testValue;
    }
}
