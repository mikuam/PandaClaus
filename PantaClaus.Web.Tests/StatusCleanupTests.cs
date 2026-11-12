namespace PantaClaus.Web.Tests;

[TestClass]
public class StatusCleanupTests
{
    private static string Source(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "PandaClaus.Web", "Pages")))
            directory = directory.Parent;
        Assert.IsNotNull(directory);
        return File.ReadAllText(Path.Combine(directory.FullName, "PandaClaus.Web", relativePath));
    }

    [TestMethod]
    public void StatusChoicesUseEnumWithoutRedundantSelector()
    {
        Assert.IsFalse(Source("Core/StatusHelper.cs").Contains("IsSelectableStatus"));
        Assert.IsFalse(Source("Pages/StatusUpdate.cshtml").Contains("IsSelectableStatus"));
        var handler = Source("Pages/StatusUpdate.cshtml.cs");
        Assert.IsFalse(handler.Contains("IsSelectableStatus"));
        StringAssert.Contains(handler, "Enum.IsDefined(status)");
    }

    [TestMethod]
    public void RemovedCheckOnlyMetricsHaveNoReferencesAndStatsStayPublic()
    {
        var model = Source("Core/Statistics.cs");
        Assert.IsFalse(model.Contains("Sprawdzone {"));
        Assert.IsFalse(model.Contains("SprawdzonePercentage {"));
        Assert.IsFalse(model.Contains("DoSprawdzenia {"));

        var handler = Source("Pages/Stats.cshtml.cs");
        Assert.IsFalse(handler.Contains("var sprawdzone ="));
        Assert.IsFalse(handler.Contains("var doSprawdzenia ="));
        Assert.IsFalse(handler.Contains("Sprawdzone ="));
        Assert.IsFalse(handler.Contains("DoSprawdzenia ="));
        StringAssert.Contains(handler, "class StatsModel : PageModel");

        var view = Source("Pages/Stats.cshtml");
        Assert.IsFalse(view.Contains("Listy sprawdzone:"));
        Assert.IsFalse(view.Contains("Listów do sprawdzenia"));
        StringAssert.Contains(view, "Listy spakowane:");
        StringAssert.Contains(view, "@page");
    }
}
