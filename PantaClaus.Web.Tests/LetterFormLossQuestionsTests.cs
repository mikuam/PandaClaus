using System.ComponentModel.DataAnnotations;
using PandaClaus.Web.Pages;
using PandaClaus.Web.Validation;

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

    [TestMethod]
    public void LossTimeAllowsYearOrMonthAndYearButNotSpecificDay()
    {
        var year = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, "Europe/Warsaw").Year;
        foreach (var date in new[] { year.ToString(), $"01.{year}" })
            Assert.IsTrue(IsValid("LossTime", date), date);
        foreach (var date in new string?[] { null, "", $"13.{year}", $"00.{year}", $"5.{year}", $"{year}-05-01", "abc" })
            Assert.IsFalse(IsValid("LossTime", date), date);
    }

    [TestMethod]
    public void RejectsYearsBeforeTheTwoPreviousCalendarYearsOrInTheFuture()
    {
        var submitted = new DateOnly(2026, 10, 14);
        foreach (var date in new[] { "2023", "12.2023", "11.2026", "2027" })
            Assert.IsFalse(LossTimeEligibility.IsEligibleYear(date, submitted), date);
    }

    [TestMethod]
    public void MonthIsOptionalAndDoesNotChangeEligibilityOfYear()
    {
        var submitted = new DateOnly(2026, 10, 14);
        foreach (var date in new[] { "2024", "01.2024", "09.2024", "12.2024", "2025", "2026", "10.2026" })
            Assert.IsTrue(LossTimeEligibility.IsEligibleYear(date, submitted), date);
    }

    [TestMethod]
    public void FormValidationRejectsClearlyIneligibleYear()
    {
        Assert.IsFalse(IsValid("LossTime", "2020"));
        Assert.IsFalse(IsValid("LossTime", "9999"));
    }
}
