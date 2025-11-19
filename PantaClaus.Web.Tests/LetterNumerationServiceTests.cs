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
    [DataRow("1Z", 1)] // Changed: 1Z is parsed as 1 with suffix Z
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
    public void GetNextLetterNumber_WhenExistingLettersWithoutSameFamily_ShouldReturnNextNumber()
    {
        // Arrange
        var letters = new List<Letter> { CreateTestLetter(new { Number = "1", Email = "other@example.com" }) };
        var newLetter = CreateTestLetter(new { Email = "test@example.com" });

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("2", result);
    }

    [TestMethod]
    public void GetNextLetterNumber_WhenExistingLetterWithDifferentEmail_ShouldReturnNextNumber()
    {
        // Arrange
        var letters = new List<Letter> { CreateTestLetter(new { Number = "1", Email = "em1@example.com" }) };
        var newLetter = CreateTestLetter(new { Email = "em2@example.com" });

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("2", result);
    }

    [TestMethod]
    public void GetNextLetterNumber_WhenExistingLettersWithVariousNumbers_ShouldReturnNextAvailableNumber()
    {
        // Arrange
        var letters = new List<Letter> { 
            CreateTestLetter(new { Number = "1", Email = "em1@example.com" }), 
            CreateTestLetter(new { Number = "X", Email = "em2@example.com" }) 
        };
        var newLetter = CreateTestLetter(new { Email = "em3@example.com" });

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("2", result);
    }

    [TestMethod]
    public void GetNextLetterNumber_WhenExistingLetterWithSameFamily_ShouldReturnSameNumberWithSuffixB()
    {
        // Arrange - Same ParentName, ParentSurname, and Email = same family
        var letters = new List<Letter>
        {
            CreateTestLetter(new { Number = "1", ParentName = "John", ParentSurname = "Doe", Email = "john.doe@example.com" }),
            CreateTestLetter(new { Number = "X" }),
            CreateTestLetter(new { Number = "2" })
        };

        var newLetter = CreateTestLetter(new { ParentName = "John", ParentSurname = "Doe", Email = "john.doe@example.com" });

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("1B", result);
    }

    [TestMethod]
    public void GetNextLetterNumber_WhenExistingLetterWithSameFamilyAndNoBaseLetter_ShouldReturnBaseNumber()
    {
        // Arrange - Letter with "X" number should not affect family numbering
        var letters = new List<Letter>
        {
            CreateTestLetter(new { Number = "X", ParentName = "John", ParentSurname = "Doe", Email = "john.doe@example.com" }),
            CreateTestLetter(new { Number = "X" }),
            CreateTestLetter(new { Number = "2" })
        };

        var newLetter = CreateTestLetter(new { ParentName = "John", ParentSurname = "Doe", Email = "john.doe@example.com" });

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("3", result); // Next available number since no valid letter for this family
    }

    [TestMethod]
    public void GetNextLetterNumber_WhenExistingLetterWithSameFamilyAndSuffixA_ShouldReturnSuffixB()
    {
        // Arrange
        var letters = new List<Letter>
        {
            CreateTestLetter(new { Number = "1A", ParentName = "John", ParentSurname = "Doe", Email = "john.doe@example.com" }),
            CreateTestLetter(new { Number = "X" }),
            CreateTestLetter(new { Number = "5" })
        };

        var newLetter = CreateTestLetter(new { ParentName = "John", ParentSurname = "Doe", Email = "john.doe@example.com" });

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("1B", result);
    }

    [TestMethod]
    public void GetNextLetterNumber_WhenHighestNumberIs123_AndSameFamily_ShouldReturn123C()
    {
        // Arrange
        var letters = new List<Letter>
        {
            CreateTestLetter(new { Number = "X" }),
            CreateTestLetter(new { Number = "1", Email = "em2@example.com" }),
            CreateTestLetter(new { Number = "123", ParentName = "John", ParentSurname = "Doe", Email = "john.doe@example.com" }),
            CreateTestLetter(new { Number = "123B", ParentName = "John", ParentSurname = "Doe", Email = "john.doe@example.com" }),
        };

        var newLetter = CreateTestLetter(new { ParentName = "John", ParentSurname = "Doe", Email = "john.doe@example.com" });

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("123C", result);
    }

    [TestMethod]
    public void GetNextLetterNumber_WhenHighestNumberIs123_AndDifferentFamily_ShouldReturn124()
    {
        // Arrange
        var letters = new List<Letter>
        {
            CreateTestLetter(new { Number = "X" }),
            CreateTestLetter(new { Number = "1", Email = "em2@example.com" }),
            CreateTestLetter(new { Number = "123", ParentName = "John", ParentSurname = "Doe", Email = "john.doe@example.com" }),
            CreateTestLetter(new { Number = "123B", ParentName = "John", ParentSurname = "Doe", Email = "john.doe@example.com" }),
        };

        var newLetter = CreateTestLetter(new { ParentName = "Jane", ParentSurname = "Smith", Email = "jane.smith@example.com" });

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("124", result);
    }

    [TestMethod]
    public void GetNextLetterNumber_WhenSameAddressButDifferentEmail_ShouldReturnNextNumber()
    {
        // Arrange - Different email = different family even with same address
        var letters = new List<Letter>
        {
            CreateTestLetter(new { Number = "1", Street = "Test St", HouseNumber = "1", Email = "parent1@example.com" })
        };

        var newLetter = CreateTestLetter(new { Street = "Test St", HouseNumber = "1", Email = "parent2@example.com" });

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("2", result); // Different family despite same address
    }

    [TestMethod]
    public void GetNextLetterNumber_WhenSameEmailButDifferentNames_ShouldReturnNextNumber()
    {
        // Arrange - Same email but different parent names = different family
        var letters = new List<Letter>
        {
            CreateTestLetter(new { Number = "1", ParentName = "John", ParentSurname = "Doe", Email = "family@example.com" })
        };

        var newLetter = CreateTestLetter(new { ParentName = "Jane", ParentSurname = "Doe", Email = "family@example.com" });

        // Act
        var result = LetterNumerationServiceInstance().GetNextLetterNumber(letters, newLetter);

        // Assert
        Assert.AreEqual("2", result); // Different family despite same email
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
            ApartmentNumber = "",
            PostalCode = "12-345",
            ChildName = "Test Child",
            ChildAge = 5,
            Description = "Test Description",
            Presents = "Test Presents",
            ImageIds = new List<string>(),
            ImageUrls = new List<string>(),
            Added = DateTime.Now,
            IsDeleted = false,
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
