using PandaClaus.Web.Core;
using PandaClaus.Web.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using System.Reflection;

namespace PantaClaus.Web.Tests;

[TestClass]
public class CombinedStatusTests
{
    [TestMethod]
    public void PackedStatusNamesBothSteps()
    {
        Assert.AreEqual("Paczki sprawdzone i spakowane", StatusHelper.GetStatusDescription(LetterStatus.SPAKOWANE));
    }

    [TestMethod]
    public void LegacyCheckedStatusRemainsReadableButCannotBeSelected()
    {
        Assert.IsTrue(Enum.TryParse<LetterStatus>("SPRAWDZONE", out var legacy));
        Assert.AreEqual(4, (int)legacy);
        Assert.AreEqual("Paczki sprawdzone (do spakowania)", StatusHelper.GetStatusDescription(legacy));
        Assert.IsFalse(StatusHelper.IsSelectableStatus(legacy));
        Assert.IsTrue(StatusHelper.IsSelectableStatus(LetterStatus.SPAKOWANE));
    }

    [TestMethod]
    public void BlankOptionalPackageSizesDoNotReuseTheFirstPackageSize()
    {
        var context = new DefaultHttpContext();
        context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
        {
            ["package1Size"] = "A",
            ["package2Size"] = "",
            ["package3Size"] = ""
        });
        var page = new StatusUpdateModel(null!, null!)
        {
            PageContext = new PageContext { HttpContext = context },
            Package1Size = Gabaryt.A
        };
        var getSize = typeof(StatusUpdateModel).GetMethod("GetPackageSize", BindingFlags.Instance | BindingFlags.NonPublic)!;
        Assert.AreEqual(Gabaryt.A, getSize.Invoke(page, ["package1Size"]));
        Assert.AreEqual(Gabaryt.None, getSize.Invoke(page, ["package2Size"]));
        Assert.AreEqual(Gabaryt.None, getSize.Invoke(page, ["package3Size"]));
    }
}
