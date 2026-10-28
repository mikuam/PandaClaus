using System.ComponentModel.DataAnnotations;
using PandaClaus.Web.Pages;

namespace PantaClaus.Web.Tests;

[TestClass]
public class LetterFormLossQuestionsTests
{
    private static bool IsValid(string propertyName, string? value)
    {
        var model = new LetterFormModel(null!, null!, null!, null!, null!, null!);
        var property = typeof(LetterFormModel).GetProperty(propertyName);
        Assert.IsNotNull(property, $"Form is missing {propertyName}");
        property.SetValue(model, value);
        var context = new ValidationContext(model) { MemberName = propertyName };
        return Validator.TryValidateProperty(value, context, new List<ValidationResult>());
    }

    [TestMethod]
    public void RelationshipAcceptsOnlyRegulationCategories()
    {
        foreach (var relation in new[] { "Mama", "Tata", "Brat", "Siostra", "Opiekun prawny" })
            Assert.IsTrue(IsValid("DeceasedRelation", relation), relation);
        foreach (var relation in new string?[] { null, "", "Babcia", "Inna osoba", "Mama<script>" })
            Assert.IsFalse(IsValid("DeceasedRelation", relation), relation);
    }
}
