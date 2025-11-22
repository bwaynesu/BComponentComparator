using NUnit.Framework;
using UnityEngine;

namespace BTools.BComponentComparator.Editor.Tests
{
    /// <summary>
    /// Tests for inheritance support in DragDropHandler
    /// </summary>
    public class InheritanceTests
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

        #region Component Inheritance Tests

        [Test]
        public void ValidateObject_WithDerivedComponent_ReturnsTrue()
        {
            // Arrange - BoxCollider derives from Collider
            testGameObject.AddComponent<BoxCollider>();

            // Act - Validate with base type
            bool isValid = DragDropHandler.IsValidObject(testGameObject, typeof(Collider));

            // Assert
            Assert.IsTrue(isValid, "GameObject with BoxCollider should be valid for Collider type");
        }

        [Test]
        public void ValidateObject_WithMultipleDerivedComponents_ReturnsTrue()
        {
            // Arrange - Add multiple colliders (all derive from Collider)
            testGameObject.AddComponent<BoxCollider>();
            testGameObject.AddComponent<SphereCollider>();

            // Act - Validate with base type
            bool isValid = DragDropHandler.IsValidObject(testGameObject, typeof(Collider));

            // Assert
            Assert.IsTrue(isValid, "GameObject with multiple derived components should be valid for base type");
        }

        [Test]
        public void ValidateObject_WithBaseComponentType_ReturnsTrue()
        {
            // Arrange - Transform is the base Component type
            // Every GameObject has a Transform

            // Act - Validate with Component base type
            bool isValid = DragDropHandler.IsValidObject(testGameObject, typeof(Component));

            // Assert
            Assert.IsTrue(isValid, "GameObject should be valid for Component base type");
        }

        [Test]
        public void ValidateObject_WithUnrelatedComponent_ReturnsFalse()
        {
            // Arrange - Add BoxCollider
            testGameObject.AddComponent<BoxCollider>();

            // Act - Validate with unrelated type (Rigidbody)
            bool isValid = DragDropHandler.IsValidObject(testGameObject, typeof(Rigidbody));

            // Assert
            Assert.IsFalse(isValid, "GameObject with BoxCollider should not be valid for unrelated Rigidbody type");
        }

        [Test]
        public void ValidateObject_WithDerivedMonoBehaviour_ReturnsTrue()
        {
            // Arrange
            testGameObject.AddComponent<DerivedTestComponent>();

            // Act - Validate with base MonoBehaviour type
            bool isValid = DragDropHandler.IsValidObject(testGameObject, typeof(MonoBehaviour));

            // Assert
            Assert.IsTrue(isValid, "GameObject with derived MonoBehaviour should be valid for MonoBehaviour type");
        }

        [Test]
        public void ValidateObject_WithBaseMonoBehaviourComponent_ReturnsTrue()
        {
            // Arrange
            testGameObject.AddComponent<DerivedTestComponent>();

            // Act - Validate with custom base type
            bool isValid = DragDropHandler.IsValidObject(testGameObject, typeof(BaseTestComponent));

            // Assert
            Assert.IsTrue(isValid, "GameObject with derived component should be valid for custom base type");
        }

        #endregion

        #region ScriptableObject Inheritance Tests

        [Test]
        public void ValidateObject_WithDerivedScriptableObject_ReturnsTrue()
        {
            // Arrange
            var derivedSO = ScriptableObject.CreateInstance<DerivedTestScriptableObject>();

            // Act - Validate with base ScriptableObject type
            bool isValid = DragDropHandler.IsValidObject(derivedSO, typeof(BaseTestScriptableObject));

            // Assert
            Assert.IsTrue(isValid, "Derived ScriptableObject should be valid for base type");

            // Cleanup
            Object.DestroyImmediate(derivedSO);
        }

        [Test]
        public void ValidateObject_WithDerivedScriptableObject_AsScriptableObject_ReturnsTrue()
        {
            // Arrange
            var derivedSO = ScriptableObject.CreateInstance<DerivedTestScriptableObject>();

            // Act - Validate with ScriptableObject base type
            bool isValid = DragDropHandler.IsValidObject(derivedSO, typeof(ScriptableObject));

            // Assert
            Assert.IsTrue(isValid, "Derived ScriptableObject should be valid for ScriptableObject base type");

            // Cleanup
            Object.DestroyImmediate(derivedSO);
        }

        [Test]
        public void ValidateObject_WithUnrelatedScriptableObject_ReturnsFalse()
        {
            // Arrange
            var derivedSO = ScriptableObject.CreateInstance<DerivedTestScriptableObject>();

            // Act - Validate with unrelated type
            bool isValid = DragDropHandler.IsValidObject(derivedSO, typeof(TestScriptableObject));

            // Assert
            Assert.IsFalse(isValid, "Derived ScriptableObject should not be valid for unrelated type");

            // Cleanup
            Object.DestroyImmediate(derivedSO);
        }

        #endregion

        #region Asset Inheritance Tests

        [Test]
        public void IsValidAsset_WithDerivedType_ReturnsTrue()
        {
            // Arrange
            var derivedSO = ScriptableObject.CreateInstance<DerivedTestScriptableObject>();

            // Act
            bool isValid = DragDropHandler.IsValidAsset(derivedSO, typeof(BaseTestScriptableObject));

            // Assert
            Assert.IsTrue(isValid, "IsValidAsset should return true for derived types");

            // Cleanup
            Object.DestroyImmediate(derivedSO);
        }

        [Test]
        public void IsValidGameObject_WithDerivedComponent_ReturnsTrue()
        {
            // Arrange
            testGameObject.AddComponent<BoxCollider>();

            // Act
            bool isValid = DragDropHandler.IsValidGameObject(testGameObject, typeof(Collider));

            // Assert
            Assert.IsTrue(isValid, "IsValidGameObject should return true for base component types");
        }

        #endregion

        #region Utility Inheritance Chain Tests

        [Test]
        public void GetInheritanceChain_WithBoxCollider_ReturnsCorrectChain()
        {
            // Arrange
            var type = typeof(BoxCollider);

            // Act
            var chain = Utility.GetInheritanceChain(type);

            // Assert
            // Expected: BoxCollider -> Collider -> Component -> Object (UnityEngine.Object) -> Object (System.Object)
            // But Utility filters out UnityEngine.Object and System.Object
            // So expected: BoxCollider -> Collider -> Component -> Behaviour -> Component? 
            // Wait, let's check the hierarchy:
            // BoxCollider : Collider
            // Collider : Component
            // Component : Object (UnityEngine)
            // Actually: BoxCollider -> Collider -> Component -> Object
            // Wait, Collider inherits from Component directly?
            // BoxCollider -> Collider -> Component -> Object
            // Let's check Behaviour.
            // BoxCollider -> Collider -> Component
            // Wait, is Collider a Behaviour? No.
            // Let's check MonoBehaviour.
            // MonoBehaviour -> Behaviour -> Component -> Object
            
            // Let's verify BoxCollider chain:
            // BoxCollider -> Collider -> Component -> Object
            // Filtered: BoxCollider, Collider, Component.
            
            Assert.IsNotNull(chain);
            Assert.AreEqual(3, chain.Count);
            Assert.AreEqual(typeof(BoxCollider), chain[0]);
            Assert.AreEqual(typeof(Collider), chain[1]);
            Assert.AreEqual(typeof(Component), chain[2]);
        }

        [Test]
        public void GetInheritanceChain_WithMonoBehaviour_ReturnsCorrectChain()
        {
            // Arrange
            var type = typeof(DerivedTestComponent);

            // Act
            var chain = Utility.GetInheritanceChain(type);

            // Assert
            // DerivedTestComponent -> BaseTestComponent -> MonoBehaviour -> Behaviour -> Component -> Object
            // Filtered: DerivedTestComponent, BaseTestComponent, MonoBehaviour, Behaviour, Component
            
            Assert.IsNotNull(chain);
            Assert.AreEqual(5, chain.Count);
            Assert.AreEqual(typeof(DerivedTestComponent), chain[0]);
            Assert.AreEqual(typeof(BaseTestComponent), chain[1]);
            Assert.AreEqual(typeof(MonoBehaviour), chain[2]);
            Assert.AreEqual(typeof(Behaviour), chain[3]);
            Assert.AreEqual(typeof(Component), chain[4]);
        }

        [Test]
        public void GetInheritanceChain_WithNull_ReturnsEmptyList()
        {
            // Act
            var chain = Utility.GetInheritanceChain(null);

            // Assert
            Assert.IsNotNull(chain);
            Assert.AreEqual(0, chain.Count);
        }

        [Test]
        public void GetInheritanceChain_WithSystemType_ReturnsEmptyOrFiltered()
        {
            // Arrange
            var type = typeof(string); // System.String -> System.Object

            // Act
            var chain = Utility.GetInheritanceChain(type);

            // Assert
            // Should be empty because it starts with System namespace
            Assert.AreEqual(0, chain.Count);
        }

        [Test]
        public void GetInheritanceChain_WithUnityEngineObject_ReturnsEmpty()
        {
            // Arrange
            var type = typeof(UnityEngine.Object);

            // Act
            var chain = Utility.GetInheritanceChain(type);

            // Assert
            Assert.AreEqual(0, chain.Count);
        }

        #endregion
    }

    #region Test Helper Classes

    /// <summary>
    /// Base test component for inheritance testing
    /// </summary>
    public class BaseTestComponent : MonoBehaviour
    {
        public int baseValue;
    }

    /// <summary>
    /// Derived test component for inheritance testing
    /// </summary>
    public class DerivedTestComponent : BaseTestComponent
    {
        public int derivedValue;
    }

    /// <summary>
    /// Base test ScriptableObject for inheritance testing
    /// </summary>
    public class BaseTestScriptableObject : ScriptableObject
    {
        public int baseValue;
    }

    /// <summary>
    /// Derived test ScriptableObject for inheritance testing
    /// </summary>
    public class DerivedTestScriptableObject : BaseTestScriptableObject
    {
        public int derivedValue;
    }

    #endregion
}
