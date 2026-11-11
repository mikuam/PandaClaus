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
    public void CheckedStatusIsRemovedWithoutRenumberingRemainingStatuses()
    {
        Assert.IsFalse(Enum.TryParse<LetterStatus>("SPRAWDZONE", out _));
        Assert.AreEqual(5, (int)LetterStatus.SPAKOWANE);
        Assert.AreEqual(6, (int)LetterStatus.ZAADRESOWANE);
        Assert.IsTrue(StatusHelper.IsSelectableStatus(LetterStatus.SPAKOWANE));
        Assert.IsFalse(StatusHelper.IsSelectableStatus((LetterStatus)4));
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
