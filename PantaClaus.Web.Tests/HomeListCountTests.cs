using PandaClaus.Web.Pages;

namespace PantaClaus.Web.Tests;

[TestClass]
public class HomeListCountTests
{
    [TestMethod]
    [DataRow("all", "Wszystkie listy")]
    [DataRow("available", "Dostępne listy")]
    [DataRow("reserved", "Zarezerwowane listy")]
    [DataRow("", "Wszystkie listy")]
    public void CountLabelMatchesSelectedFilter(string filter, string label)
    {
        Assert.AreEqual(label, IndexModel.GetListCountLabel(filter));
    }

    [TestMethod]
    public void HomepageCounterCountsDisplayedLettersInsteadOfAvailableOnly()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "PandaClaus.Web", "Pages", "Index.cshtml")))
            directory = directory.Parent;

        Assert.IsNotNull(directory);
        var source = File.ReadAllText(Path.Combine(directory.FullName, "PandaClaus.Web", "Pages", "Index.cshtml"));
        StringAssert.Contains(source, "@Model.Letters.Count</strong>");
        Assert.IsFalse(source.Contains("@Model.Letters.Count(l => l is { IsAssigned: false, IsVisible: true })"));
        StringAssert.Contains(source, "GetListCountLabel");
    }
}
