using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PandaClaus.Web.Validation;

public static class LossTimeEligibility
{
    // The organizer counts calendar years, not the exact two-year anniversary.
    public static bool IsEligibleYear(string value, DateOnly submittedOn)
    {
        if (!Regex.IsMatch(value, @"^(?:[0-9]{4}|(?:0[1-9]|1[0-2])\.[0-9]{4})$"))
            return false;

        var parts = value.Split('.');
        var year = int.Parse(parts[^1], CultureInfo.InvariantCulture);
        if (year < 1 || year < submittedOn.Year - 2 || year > submittedOn.Year)
            return false;
        // A month is optional and does not narrow the eligible years, but cannot be in the future.
        return parts.Length == 1 || year < submittedOn.Year
            || int.Parse(parts[0], CultureInfo.InvariantCulture) <= submittedOn.Month;
    }
}

public sealed class LossWithinTwoYearsAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
            return true; // [Required] reports missing values.
        var todayInPoland = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, "Europe/Warsaw").Date);
        return LossTimeEligibility.IsEligibleYear(text, todayInPoland);
    }
}
