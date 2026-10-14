using System.Reflection;
using System.Runtime.CompilerServices;
using PandaClaus.Web;

namespace PantaClaus.Web.Tests;

[TestClass]
public class LegacyLetterRowsTests
{
    // Synthetic rows only. Never connect the tests to the foundation's sheet.
    private static Letter Map(IList<object> row)
    {
        var client = (GoogleSheetsClient)RuntimeHelpers.GetUninitializedObject(typeof(GoogleSheetsClient));
        var method = typeof(GoogleSheetsClient).GetMethod("MapLetter", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (Letter)method.Invoke(client, new object[] { 2, row })!;
    }

    private static List<object> OldRow()
    {
        var cells = Enumerable.Repeat<object>(string.Empty, 28).ToList(); // A:AB
        cells[0] = "synthetic-hash";
        cells[1] = "42";
        cells[2] = "Testowa";
        cells[12] = "Dziecko";
        cells[13] = "12";
        cells[14] = "Przykładowy opis";
        cells[15] = "Przykładowy prezent";
        cells[17] = "2026-10-14 10:00:00";
        cells[18] = "nie";
        return cells;
    }

    [TestMethod]
    public void Old28ColumnRowStillMapsWithoutLossAnswers()
    {
        var letter = Map(OldRow());
        Assert.AreEqual("synthetic-hash", letter.Hash);
        Assert.AreEqual("Dziecko", letter.ChildName);
        Assert.AreEqual(12, letter.ChildAge);
        Assert.AreEqual("Przykładowy opis", letter.Description);
        Assert.AreEqual("", letter.DeceasedRelation);
        Assert.AreEqual("", letter.LossTime);
    }

    [TestMethod]
    public void NewColumnsDoNotShiftExistingFields()
    {
        var cells = OldRow();
        cells.Add("Mama"); // AC
        cells.Add("09.2024"); // AD
        var letter = Map(cells);
        Assert.AreEqual("Dziecko", letter.ChildName);
        Assert.AreEqual("Przykładowy prezent", letter.Presents);
        Assert.AreEqual("Mama", letter.DeceasedRelation);
        Assert.AreEqual("09.2024", letter.LossTime);
    }

}
