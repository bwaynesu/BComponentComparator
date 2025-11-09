using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace ComponentComparator.Editor.Tests
{
    /// <summary>
    /// Tests for DragDropHandler validation logic
    /// </summary>
    public class DragDropHandlerTests
    {
        private GameObject testGameObject;
        private GameObject testPrefab;

        [SetUp]
        public void SetUp()
        {
            // Create test GameObject
            testGameObject = new GameObject("TestObject");
            testGameObject.AddComponent<Rigidbody>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }

            if (testPrefab != null)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(testPrefab));
            }
        }

        [Test]
        public void ValidateComponentType_WithComponentInstance_ReturnsTrue()
        {
            // Arrange
            var component = testGameObject.GetComponent<Transform>();

            // Act
            bool isValid = DragDropHandler.ValidateComponentType(component, out System.Type componentType);

            // Assert
            Assert.IsTrue(isValid);
            Assert.AreEqual(typeof(Transform), componentType);
        }

        [Test]
        public void ValidateComponentType_WithScriptableObject_ReturnsTrue()
        {
            // Arrange
            var scriptableObject = ScriptableObject.CreateInstance<TestScriptableObject>();

            // Act
            bool isValid = DragDropHandler.ValidateComponentType(scriptableObject, out System.Type componentType);

            // Assert
            Assert.IsTrue(isValid);
            Assert.AreEqual(typeof(TestScriptableObject), componentType);

            // Cleanup
            Object.DestroyImmediate(scriptableObject);
        }

        [Test]
        public void ValidateComponentType_WithMaterial_ReturnsTrue()
        {
            // Arrange
            var material = new Material(Shader.Find("Standard"));

            // Act
            bool isValid = DragDropHandler.ValidateComponentType(material, out System.Type componentType);

            // Assert
            Assert.IsTrue(isValid);
            Assert.AreEqual(typeof(Material), componentType);

            // Cleanup
            Object.DestroyImmediate(material);
        }

        [Test]
        public void ValidateComponentType_WithNullObject_ReturnsFalse()
        {
            // Act
            bool isValid = DragDropHandler.ValidateComponentType(null, out System.Type componentType);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsNull(componentType);
        }

        [Test]
        public void ValidateObject_WithValidGameObject_ReturnsTrue()
        {
            // Act
            bool isValid = DragDropHandler.ValidateObject(testGameObject, typeof(Rigidbody));

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void ValidateObject_WithGameObjectMissingComponent_ReturnsFalse()
        {
            // Act
            bool isValid = DragDropHandler.ValidateObject(testGameObject, typeof(BoxCollider));

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void ValidateObject_WithPrefabAsset_ReturnsFalse()
        {
            // Arrange - Create a temporary prefab
            testPrefab = new GameObject("TestPrefab");
            testPrefab.AddComponent<Rigidbody>();
            
            string prefabPath = "Assets/TestPrefab.prefab";
            testPrefab = PrefabUtility.SaveAsPrefabAsset(testPrefab, prefabPath);

            // Act
            bool isValid = DragDropHandler.ValidateObject(testPrefab, typeof(Rigidbody));

            // Assert
            Assert.IsFalse(isValid);

            // Cleanup
            AssetDatabase.DeleteAsset(prefabPath);
            testPrefab = null;
        }

        [Test]
        public void ValidateObject_WithScriptableObject_ReturnsTrue()
        {
            // Arrange
            var scriptableObject = ScriptableObject.CreateInstance<TestScriptableObject>();

            // Act
            bool isValid = DragDropHandler.ValidateObject(scriptableObject, typeof(TestScriptableObject));

            // Assert
            Assert.IsTrue(isValid);

            // Cleanup
            Object.DestroyImmediate(scriptableObject, true);
        }

        [Test]
        public void ValidateObject_WithMaterial_ReturnsTrue()
        {
            // Arrange
            var material = new Material(Shader.Find("Standard"));

            // Act
            bool isValid = DragDropHandler.ValidateObject(material, typeof(Material));

            // Assert
            Assert.IsTrue(isValid);

            // Cleanup
            Object.DestroyImmediate(material, true);
        }

        [Test]
        public void ValidateObject_WithNullObject_ReturnsFalse()
        {
            // Act
            bool isValid = DragDropHandler.ValidateObject(null, typeof(Transform));

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void ValidateObject_WithNullType_ReturnsFalse()
        {
            // Act
            bool isValid = DragDropHandler.ValidateObject(testGameObject, null);

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void IsValidGameObject_WithValidComponent_ReturnsTrue()
        {
            // Act
            bool isValid = DragDropHandler.IsValidGameObject(testGameObject, typeof(Rigidbody));

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void IsValidGameObject_WithMissingComponent_ReturnsFalse()
        {
            // Act
            bool isValid = DragDropHandler.IsValidGameObject(testGameObject, typeof(BoxCollider));

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void IsValidAsset_WithMatchingType_ReturnsTrue()
        {
            // Arrange
            var scriptableObject = ScriptableObject.CreateInstance<TestScriptableObject>();

            // Act
            bool isValid = DragDropHandler.IsValidAsset(scriptableObject, typeof(ScriptableObject));

            // Assert
            Assert.IsTrue(isValid);

            // Cleanup
            Object.DestroyImmediate(scriptableObject, true);
        }

        [Test]
        public void IsValidAsset_WithNonMatchingType_ReturnsFalse()
        {
            // Arrange
            var scriptableObject = ScriptableObject.CreateInstance<TestScriptableObject>();

            // Act
            bool isValid = DragDropHandler.IsValidAsset(scriptableObject, typeof(Material));

            // Assert
            Assert.IsFalse(isValid);

            // Cleanup
            Object.DestroyImmediate(scriptableObject, true);
        }
    }
}
