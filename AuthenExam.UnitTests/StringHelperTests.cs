using NUnit.Framework;
using AuthenExamCompareFaceExam.Utils;
using AuthenExamCompareFaceExam;

namespace AuthenExam.UnitTests
{
    [TestFixture]
    public class StringHelperTests
    {
        [Test]
        public void IsNotNullOrEmpty_InputIsNull_ReturnsFalse()
        {
            // Arrange
            object input = null;

            // Act
            var result = input.IsNotNullOrEmpty();

            // Assert
            Assert.IsFalse(result, "Expected IsNotNullOrEmpty to return false for null input.");
        }

        [Test]
        public void IsNotNullOrEmpty_InputIsEmptyString_ReturnsFalse()
        {
            // Arrange
            object input = "";

            // Act
            var result = input.IsNotNullOrEmpty();

            // Assert
            Assert.IsFalse(result, "Expected IsNotNullOrEmpty to return false for empty string.");
        }

        [Test]
        public void IsNotNullOrEmpty_InputIsWhitespace_ReturnsFalse()
        {
            // Arrange
            object input = "   ";

            // Act
            var result = input.IsNotNullOrEmpty();

            // Assert
            Assert.IsFalse(result, "Expected IsNotNullOrEmpty to return false for whitespace string.");
        }

        [Test]
        public void IsNotNullOrEmpty_InputIsValidString_ReturnsTrue()
        {
            // Arrange
            object input = "Hello, World!";

            // Act
            var result = input.IsNotNullOrEmpty();

            // Assert
            Assert.IsTrue(result, "Expected IsNotNullOrEmpty to return true for a valid string.");
        }

        [Test]
        public void IsNullOrEmpty_InputIsNull_ReturnsTrue()
        {
            // Arrange
            object input = null;

            // Act
            var result = input.IsNullOrEmpty();

            // Assert
            Assert.IsTrue(result, "Expected IsNullOrEmpty to return true for null input.");
        }

        [Test]
        public void IsNullOrEmpty_InputIsEmptyString_ReturnsTrue()
        {
            // Arrange
            object input = "";

            // Act
            var result = input.IsNullOrEmpty();

            // Assert
            Assert.IsTrue(result, "Expected IsNullOrEmpty to return true for empty string.");
        }

        [Test]
        public void IsNullOrEmpty_InputIsWhitespace_ReturnsTrue()
        {
            // Arrange
            object input = "   ";

            // Act
            var result = input.IsNullOrEmpty();

            // Assert
            Assert.IsTrue(result, "Expected IsNullOrEmpty to return true for whitespace string.");
        }

        [Test]
        public void IsNullOrEmpty_InputIsValidString_ReturnsFalse()
        {
            // Arrange
            object input = "Hello, World!";

            // Act
            var result = input.IsNullOrEmpty();

            // Assert
            Assert.IsFalse(result, "Expected IsNullOrEmpty to return false for a valid string.");
        }
    }
}
