using PandaClaus.Web;

namespace PantaClaus.Web.Tests;

[TestClass]
public class LocalSheetWritePlanTests
{
    [TestMethod]
    public void AddsOnlySevenExistingOptionalCellsAndTwoNewAnswersAfterLegacyColumns()
    {
        var oldValues = Enumerable.Range(0, 20).Select(i => (object)$"old-{i}").ToList(); // B:U
        var letter = new Letter
        {
            RowNumber = 0, Number = "X", ParentName = "Test", ParentSurname = "Opiekun",
            PhoneNumber = "500000000", Email = "test@example.invalid", Street = "Testowa",
            HouseNumber = "1", City = "Test", PostalCode = "00-000", ChildName = "Dziecko",
            ChildAge = 12, Description = "Fikcyjna historia", Presents = "Fikcyjny prezent",
            ImageIds = [], ImageUrls = [], Added = new DateTime(2026, 10, 14),
            IsDeleted = false, IsVisible = false, IsAssigned = false,
            DeceasedRelation = "Mama", LossTime = "09.2024"
        };

        var values = LetterSheetWritePlan.Extend(oldValues, letter);

        Assert.AreSame(oldValues, values);
        Assert.AreEqual(29, values.Count); // B:AD
        Assert.AreEqual("old-0", values[0]);
        Assert.AreEqual("old-19", values[19]);
        for (var index = 20; index < 27; index++) Assert.IsNull(values[index]); // V:AB are skipped, not cleared
        Assert.AreEqual("Mama", values[27]); // AC
        Assert.AreEqual("'09.2024", values[28]); // AD stays text under USER_ENTERED
    }
}
