using FluentAssertions;
using PandaClaus.Web.Core;

namespace PandaClaus.Web.Tests;

[TestClass]
public class LetterNumerationServiceTests
{
    [DataTestMethod]
    [DataRow("0", 0, null)]
    [DataRow("0A", 0, 'A')]
    [DataRow("X", 0, null)]
    [DataRow("123", 123, null)]
    [DataRow("123B", 123, 'B')]
    [DataRow("456C", 456, 'C')]
    [DataRow("789", 789, null)]
    [DataRow("0Z", 0, 'Z')]
    [DataRow("999Y", 999, 'Y')]
    [DataRow("", 0, null)]
    public void GetNumber_ShouldReturnExpectedResult(string input, int expectedNumber, char? expectedLetter)
    {
        // Arrange & Act
        var result = LetterNumerationService.GetNumber(new Letter { Number = input });

        // Assert
        result.Number.Should().Be(expectedNumber);
        result.Letter.Should().Be(expectedLetter);
    }

    [TestMethod]
    public void GetNextLetterNumber_ShouldReturn1_WhenNoLetters()
    {
        // Arrange
        var service = new LetterNumerationService();
        var letters = new List<Letter>();
        var newLetter = new Letter();

        // Act
        var result = service.GetNextLetterNumber(letters, newLetter);

        // Assert
        result.Should().Be("1");
    }

    [TestMethod]
    public void GetNextLetterNumber_ShouldReturn1_WhenNoLettersWithNumber()
    {
        // Arrange
        var service = new LetterNumerationService();
        var letters = new List<Letter> { new Letter { Number = "X" } };
        var newLetter = new Letter();

        // Act
        var result = service.GetNextLetterNumber(letters, newLetter);

        // Assert
        result.Should().Be("1");
    }

    [TestMethod]
    public void GetNextLetterNumber_ShouldReturn2_WhenOneLetterWithNumber()
    {
        // Arrange
        var service = new LetterNumerationService();
        var letters = new List<Letter> { new Letter { Number = "1", Email = "em1" } };
        var newLetter = new Letter { Email = "em2" };

        // Act
        var result = service.GetNextLetterNumber(letters, newLetter);

        // Assert
        result.Should().Be("2");
    }

    [TestMethod]
    public void GetNextLetterNumber_ShouldReturn2_WhenOneLetterWithNumberAndOneWithout()
    {
        // Arrange
        var service = new LetterNumerationService();
        var letters = new List<Letter> { new Letter { Number = "1", Email = "em1" }, new Letter { Number = "X", Email = "em2" } };
        var newLetter = new Letter { Email = "em3" };

        // Act
        var result = service.GetNextLetterNumber(letters, newLetter);

        // Assert
        result.Should().Be("2");
    }

    [TestMethod]
    public void GetNextLetterNumber_ShouldReturn1B_WheFirstLetterAlreadyExists()
    {
        // Arrange
        var service = new LetterNumerationService();
        var letters = new List<Letter>
        {
            new Letter { Number = "1", Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" },
            new Letter { Number = "X" },
            new Letter { Number = "2" }
        };

        var newLetter = new Letter { Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" };

        // Act
        var result = service.GetNextLetterNumber(letters, newLetter);

        // Assert
        result.Should().Be("1B");
    }

    [TestMethod]
    public void GetNextLetterNumber_ShouldReturn3B_WheFirstLetterAlreadyExistsButWithoutANumber()
    {
        // Arrange
        var service = new LetterNumerationService();
        var letters = new List<Letter>
        {
            new Letter { Number = "X", Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" },
            new Letter { Number = "X" },
            new Letter { Number = "2" }
        };

        var newLetter = new Letter { Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" };

        // Act
        var result = service.GetNextLetterNumber(letters, newLetter);

        // Assert
        result.Should().Be("3");
    }

    [TestMethod]
    public void GetNextLetterNumber_ShouldReturn1A_WhenOneLetterWithNumberAndOneWithoutAndOneWithSameAddress()
    {
        // Arrange
        var service = new LetterNumerationService();
        var letters = new List<Letter>
        {
            new Letter { Number = "1A", Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" },
            new Letter { Number = "X" },
            new Letter { Number = "5" }
        };

        var newLetter = new Letter { Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" };

        // Act
        var result = service.GetNextLetterNumber(letters, newLetter);

        // Assert
        result.Should().Be("1B");
    }

    [TestMethod]
    public void GetNextLetterNumber_ShouldReturn123C_WhenTwoLettersWithTheSameAddressExists()
    {
        // Arrange
        var service = new LetterNumerationService();
        var letters = new List<Letter>
        {
            new Letter { Number = "X" },
            new Letter { Number = "1", Email = "em2" },
            new Letter { Number = "123", Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" },
            new Letter { Number = "123B", Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" },
        };

        var newLetter = new Letter { Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" };

        // Act
        var result = service.GetNextLetterNumber(letters, newLetter);

        // Assert
        result.Should().Be("123C");
    }

    [TestMethod]
    public void GetNextLetterNumber_ShouldReturn124_When123BExists()
    {
        // Arrange
        var service = new LetterNumerationService();
        var letters = new List<Letter>
        {
            new Letter { Number = "X" },
            new Letter { Number = "1", Email = "em2" },
            new Letter { Number = "123", Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" },
            new Letter { Number = "123B", Street = "Test", HouseNumber = "1", ApartmentNumber = "1", Email = "email" },
        };

        var newLetter = new Letter { Email = "em4" };

        // Act
        var result = service.GetNextLetterNumber(letters, newLetter);

        // Assert
        result.Should().Be("124");
    }
}
