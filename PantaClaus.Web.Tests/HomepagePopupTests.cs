namespace PandaClaus.Web.Tests;

[TestClass]
public class HomepagePopupTests
{
    [TestMethod]
    public void HomepageHasNoRegulationChangePopupButKeepsCountdown()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "PandaClaus.Web", "Pages", "Index.cshtml")))
            directory = directory.Parent;
        var source = File.ReadAllText(Path.Combine(directory!.FullName, "PandaClaus.Web", "Pages", "Index.cshtml"));

        Assert.IsFalse(source.Contains("regulation-change-modal", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("pandaClaus2026NoLossTimeLimitNoticeSeen", StringComparison.Ordinal));
        StringAssert.Contains(source, "campaign-countdown");
    }
}
