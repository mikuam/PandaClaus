﻿using System.Text.RegularExpressions;

namespace PandaClaus.Web.Core;

public class LetterNumerationService
{
    public string GetNextLetterNumber(List<Letter> letters, Letter newLetter)
    {
        var lettersWithNumbers = letters.Where(l => l.Number != "X");
        var letterNumbers = lettersWithNumbers.Select(GetNumber);

        var lastLetterWithTheSameAddress = lettersWithNumbers
            .LastOrDefault(l => l.Street == newLetter.Street
                                && l.HouseNumber == newLetter.HouseNumber
                                && l.ApartmentNumber == (newLetter.ApartmentNumber ?? string.Empty)
                                && l.Email == newLetter.Email);

        if (lastLetterWithTheSameAddress != null)
        {
            var lastFamilyLetterNumber = GetNumber(lastLetterWithTheSameAddress);
            return $"{lastFamilyLetterNumber.Number}{(char)((lastFamilyLetterNumber?.Letter ?? 'A') + 1)}";
        }

        var lastLetterNumber = letterNumbers
            .OrderByDescending(n => n.Number)
            .FirstOrDefault();

        return $"{(lastLetterNumber?.Number ?? 0) + 1}";
    }

    public static ParsedLetterNumber GetNumber(Letter t)
    {
        var match = Regex.Match(t.Number, @"^(?<number>\d+)(?<suffix>[A-Z]?)$");
        if (match.Success)
        {
            int number = int.Parse(match.Groups["number"].Value);
            char? suffix = string.IsNullOrEmpty(match.Groups["suffix"].Value) ? (char?)null : match.Groups["suffix"].Value[0];
            return new ParsedLetterNumber(number, suffix);
        }

        return new ParsedLetterNumber(0, null);
    }
}
