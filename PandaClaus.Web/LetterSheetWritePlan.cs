namespace PandaClaus.Web;

// Extend the existing B:U write to B:AD without changing its first 20 cells.
public static class LetterSheetWritePlan
{
    public static List<object> Extend(List<object> legacyValues, Letter letter)
    {
        legacyValues.AddRange(Enumerable.Repeat<object>(null!, 7)); // V:AB: Sheets skips null cells
        legacyValues.Add(letter.DeceasedRelation); // AC
        legacyValues.Add($"'{letter.LossTime}"); // AD: literal text under USER_ENTERED
        return legacyValues;
    }
}
