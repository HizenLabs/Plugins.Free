using HizenLabs.FluentUI.Extensions;

namespace FluentUI.Tests.Extensions
{
    [TestFixture]
    public class DictionaryExtensionsTests
    {
        private Dictionary<string, int> _dictionary;

        [SetUp]
        public void Setup()
        {
            _dictionary = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 },
                { "three", 3 }
            };
        }

        #region GetOrAdd Tests

        [Test]
        public void GetOrAdd_KeyExists_ReturnsExistingValue()
        {
            // Arrange
            int factoryCallCount = 0;
            Func<int> valueFactory = () =>
            {
                factoryCallCount++;
                return 99;
            };

            // Act
            int result = _dictionary.GetOrAdd("two", valueFactory);

            // Assert
            Assert.That(result, Is.EqualTo(2));
            Assert.That(factoryCallCount, Is.EqualTo(0), "Factory should not be called when key exists");
            Assert.That(_dictionary, Has.Count.EqualTo(3));
        }

        [Test]
        public void GetOrAdd_KeyDoesNotExist_AddsNewValueAndReturnsIt()
        {
            // Arrange
            int factoryCallCount = 0;
            Func<int> valueFactory = () =>
            {
                factoryCallCount++;
                return 99;
            };

            // Act
            int result = _dictionary.GetOrAdd("four", valueFactory);

            // Assert
            Assert.That(result, Is.EqualTo(99));
            Assert.That(factoryCallCount, Is.EqualTo(1), "Factory should be called exactly once");
            Assert.That(_dictionary.Count, Is.EqualTo(4));
            Assert.IsTrue(_dictionary.ContainsKey("four"));
            Assert.That(_dictionary["four"], Is.EqualTo(99));
        }

        [Test]
        public void GetOrAdd_NullKeyWithKeyType_ThrowsArgumentNullException()
        {
            // Arrange
            var dict = new Dictionary<string, int>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => dict.GetOrAdd(null!, () => 42));
        }

        [Test]
        public void GetOrAdd_NullValueFactory_ThrowsArgumentNullException()
        {
            // Arrange
            Func<int> valueFactory = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _dictionary.GetOrAdd("key", valueFactory));
        }

        #endregion

        #region TryRemove Tests

        [Test]
        public void TryRemove_KeyExists_RemovesKeyAndReturnsTrue()
        {
            // Arrange
            bool keyExistsBefore = _dictionary.ContainsKey("two");

            // Act
            bool result = _dictionary.TryRemove("two", out int value);

            // Assert
            Assert.IsTrue(keyExistsBefore, "Key should exist before removal");
            Assert.IsTrue(result, "TryRemove should return true when key exists");
            Assert.That(value, Is.EqualTo(2), "TryRemove should return the correct value");
            Assert.That(_dictionary.Count, Is.EqualTo(2), "Dictionary should have one less item");
            Assert.IsFalse(_dictionary.ContainsKey("two"), "Key should be removed from dictionary");
        }

        [Test]
        public void TryRemove_KeyDoesNotExist_ReturnsFalseWithDefaultValue()
        {
            // Act
            bool result = _dictionary.TryRemove("nonexistent", out int value);

            // Assert
            Assert.IsFalse(result, "TryRemove should return false when key doesn't exist");
            Assert.That(value, Is.EqualTo(default(int)), "Value should be default when key doesn't exist");
            Assert.That(_dictionary.Count, Is.EqualTo(3), "Dictionary count should remain unchanged");
        }

        [Test]
        public void TryRemove_NullKey_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _dictionary.TryRemove(null!, out int value));
        }

        [Test]
        public void TryRemove_EmptyDictionary_ReturnsFalse()
        {
            // Arrange
            var emptyDict = new Dictionary<string, int>();

            // Act
            bool result = emptyDict.TryRemove("any", out int value);

            // Assert
            Assert.IsFalse(result, "TryRemove on empty dictionary should return false");
            Assert.That(value, Is.EqualTo(default(int)));
        }

        [Test]
        public void TryRemove_ReferenceTypeValue_ReturnsNullForNonExistentKey()
        {
            // Arrange
            var stringDict = new Dictionary<int, string> { { 1, "one" }, { 2, "two" } };

            // Act
            bool result = stringDict.TryRemove(3, out string value);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(value, "Reference type default value should be null");
        }

        #endregion
    }
}