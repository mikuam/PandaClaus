using PandaClaus.Web;
using PandaClaus.Web.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PantaClaus.Web.Tests;

[TestClass]
public class LetterNumerationServiceTests
{
    [TestMethod]
    [DataRow("X", 0)]
    [DataRow("1", 1)]
    [DataRow("123", 123)]
    [DataRow("1Z", 0)]
    [DataRow("123A", 123)]
    [DataRow("", 0)]
    public void GetNumber_ShouldReturnExpectedValue(string input, int expected)
    {
        // Arrange & Act
        var result = LetterNumerationService.GetNumber(CreateTestLetter(new { Number = input }));

        // Assert
        Assert.AreEqual(expected, result.Number);
    }

    [TestMethod]
    public void GetNextLetterNumber_WhenNoExistingLetters_ShouldReturn1()
    {
        // Arrange
        var letters = new List<Letter>();
        var newLetter = CreateTestLetter();

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("1", result);
    }

    [TestMethod]
    public void GetNextLetterNumber_WhenExistingLettersWithoutSameAddress_ShouldReturnNextNumber()
    {
        // Arrange
        var letters = new List<Letter> { CreateTestLetter(new { Number = "X" }) };
        var newLetter = CreateTestLetter();

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("1", result);
    }

    [TestMethod]
    public void GetNextLetterNumber_WhenExistingLetterWithSameEmail_ShouldReturnSameNumber()
    {
        // Arrange
        var letters = new List<Letter> { CreateTestLetter(new { Number = "1", Email = "em1" }) };
        var newLetter = CreateTestLetter(new { Email = "em2" });

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("2", result);
    }

    [TestMethod]
    public void GetNextLetterNumber_WhenExistingLettersWithVariousNumbers_ShouldReturnNextAvailableNumber()
    {
        // Arrange
        var letters = new List<Letter> { CreateTestLetter(new { Number = "1", Email = "em1" }), CreateTestLetter(new { Number = "X", Email = "em2" }) };
        var newLetter = CreateTestLetter(new { Email = "em3" });

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("2", result);
    }

    [TestMethod]
    public void GetNextLetterNumber_WhenExistingLetterWithSameAddress_ShouldReturnSameNumberWithSuffix()
    {
        // Arrange
        var letters = new List<Letter>
        {
            CreateTestLetter(new { Number = "1", Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" }),
            CreateTestLetter(new { Number = "X" }),
            CreateTestLetter(new { Number = "2" })
        };

        var newLetter = CreateTestLetter(new { Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" });

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("1A", result);
    }

    [TestMethod]
    public void GetNextLetterNumber_WhenExistingLetterWithSameAddressAndMultipleSuffixes_ShouldReturnNextSuffix()
    {
        // Arrange
        var letters = new List<Letter>
        {
            CreateTestLetter(new { Number = "X", Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" }),
            CreateTestLetter(new { Number = "X" }),
            CreateTestLetter(new { Number = "2" })
        };

        var newLetter = CreateTestLetter(new { Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" });

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("1", result);
    }

    [TestMethod]
    public void GetNextLetterNumber_WhenExistingLetterWithSameAddressAndSuffixA_ShouldReturnSuffixB()
    {
        // Arrange
        var letters = new List<Letter>
        {
            CreateTestLetter(new { Number = "1A", Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" }),
            CreateTestLetter(new { Number = "X" }),
            CreateTestLetter(new { Number = "5" })
        };

        var newLetter = CreateTestLetter(new { Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" });

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("1B", result);
    }

    [TestMethod]
    public void GetNextLetterNumber_WhenHighestNumberIs123_ShouldReturn124()
    {
        // Arrange
        var letters = new List<Letter>
        {
            CreateTestLetter(new { Number = "X" }),
            CreateTestLetter(new { Number = "1", Email = "em2" }),
            CreateTestLetter(new { Number = "123", Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" }),
            CreateTestLetter(new { Number = "123B", Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" }),
        };

        var newLetter = CreateTestLetter(new { Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" });

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("123C", result);
    }

    [TestMethod]
    public void GetNextLetterNumber_WhenHighestNumberIs123_ShouldReturn124_ButNoSameAddress()
    {
        // Arrange
        var letters = new List<Letter>
        {
            CreateTestLetter(new { Number = "X" }),
            CreateTestLetter(new { Number = "1", Email = "em2" }),
            CreateTestLetter(new { Number = "123", Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" }),
            CreateTestLetter(new { Number = "123B", Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" }),
        };

        var newLetter = CreateTestLetter(new { Email = "em4" });

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("124", result);
    }

    private static Letter CreateTestLetter(object? properties = null)
    {
        var letter = new Letter
        {
            RowNumber = 1,
            Number = "X",
            ParentName = "Test Parent",
            ParentSurname = "Test Surname",
            PhoneNumber = "123456789",
            Email = "test@example.com",
            Street = "Test Street",
            City = "Test City",
            HouseNumber = "1",
            PostalCode = "12-345",
            ChildName = "Test Child",
            ChildAge = 5,
            Description = "Test Description",
            Presents = "Test Presents",
            ImageIds = new List<string>(),
            ImageUrls = new List<string>(),
            Added = DateTime.Now,
            IsVisible = true,
            IsAssigned = false
        };

        if (properties != null)
        {
            var props = properties.GetType().GetProperties();
            foreach (var prop in props)
            {
                var letterProp = typeof(Letter).GetProperty(prop.Name);
                if (letterProp != null && letterProp.CanWrite)
                {
                    letterProp.SetValue(letter, prop.GetValue(properties));
                }
            }
        }

        return letter;
    }

    private static LetterNumerationService LetterNumerationServiceInstance()
    {
        return new LetterNumerationService();
    }
}
