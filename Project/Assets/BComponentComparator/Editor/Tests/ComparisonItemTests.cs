using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace ComponentComparator.Editor.Tests
{
    /// <summary>
    /// Tests for ComparisonItem data model
    /// </summary>
    public class ComparisonItemTests
    {
        private GameObject testGameObject;
        private Transform testTransform;

        [SetUp]
        public void SetUp()
        {
            // Create test GameObject with Transform
            testGameObject = new GameObject("TestObject");
            testTransform = testGameObject.transform;
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
        public void Constructor_WithGameObjectAndComponentType_CreatesValidItem()
        {
            // Arrange & Act
            var item = new ComparisonItem(testGameObject, typeof(Transform));

            // Assert
            Assert.IsNotNull(item);
            Assert.AreEqual(testGameObject, item.TargetObject);
            Assert.AreEqual(testTransform, item.TargetComponent);
            Assert.IsNotNull(item.SerializedObject);
            Assert.IsTrue(item.DisplayName.Contains("TestObject"));

            // Cleanup
            item.Dispose();
        }

        [Test]
        public void Constructor_WithNullObject_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() =>
            {
                new ComparisonItem(null, typeof(Transform));
            });
        }

        [Test]
        public void Constructor_WithNullType_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() =>
            {
                new ComparisonItem(testGameObject, null);
            });
        }

        [Test]
        public void Constructor_WithMissingComponent_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentException>(() =>
            {
                new ComparisonItem(testGameObject, typeof(Rigidbody));
            });
        }

        [Test]
        public void Constructor_WithScriptableObject_CreatesValidItem()
        {
            // Arrange
            var scriptableObject = ScriptableObject.CreateInstance<TestScriptableObject>();
            scriptableObject.name = "TestSO";

            // Act
            var item = new ComparisonItem(scriptableObject, typeof(TestScriptableObject));

            // Assert
            Assert.IsNotNull(item);
            Assert.AreEqual(scriptableObject, item.TargetObject);
            Assert.IsNull(item.TargetComponent); // ScriptableObject has no Component
            Assert.IsNotNull(item.SerializedObject);
            Assert.IsTrue(item.DisplayName.Contains("TestSO"));

            // Cleanup
            item.Dispose();
            Object.DestroyImmediate(scriptableObject);
        }

        [Test]
        public void IsValid_WithExistingObject_ReturnsTrue()
        {
            // Arrange
            var item = new ComparisonItem(testGameObject, typeof(Transform));

            // Act
            bool isValid = item.IsValid();

            // Assert
            Assert.IsTrue(isValid);

            // Cleanup
            item.Dispose();
        }

        [Test]
        public void IsValid_AfterObjectDestroyed_ReturnsFalse()
        {
            // Arrange
            var item = new ComparisonItem(testGameObject, typeof(Transform));
            Object.DestroyImmediate(testGameObject);
            testGameObject = null;

            // Act
            bool isValid = item.IsValid();

            // Assert
            Assert.IsFalse(isValid);

            // Cleanup
            item.Dispose();
        }

        [Test]
        public void Dispose_ReleasesSerializedObject()
        {
            // Arrange
            var item = new ComparisonItem(testGameObject, typeof(Transform));

            // Act
            item.Dispose();

            // Assert
            Assert.IsNull(item.SerializedObject);
        }

        [Test]
        public void DisplayName_FormattedCorrectly()
        {
            // Arrange & Act
            var item = new ComparisonItem(testGameObject, typeof(Transform));

            // Assert
            Assert.AreEqual("TestObject", item.DisplayName);

            // Cleanup
            item.Dispose();
        }
    }

    /// <summary>
    /// Test ScriptableObject for testing
    /// </summary>
    public class TestScriptableObject : ScriptableObject
    {
        public int testValue;
    }
}
