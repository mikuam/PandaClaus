using PandaClaus.Web.Core;

namespace PandaClaus.Web.Tests;

[TestClass]
public class VoucherEmailTests
{
    [TestMethod]
    public void VoucherAppearsInBothEmailFormatsAsTextNotWarehouseQr()
    {
        var (plain, html) = VoucherEmailContent.Render("SYNTH-ONE");
        StringAssert.Contains(plain, "SYNTH-ONE");
        StringAssert.Contains(html, "SYNTH-ONE");
        StringAssert.Contains(plain, "InPost");
        StringAssert.Contains(plain, "bezpłatnie");
        StringAssert.Contains(plain, "aplikacji InPost");
        StringAssert.Contains(html, "jednego listu");
        Assert.IsFalse(html.Contains("<img", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void VoucherHtmlIsEncoded()
    {
        var (_, html) = VoucherEmailContent.Render("SYNTH-<tag>");
        Assert.IsFalse(html.Contains("<tag>"));
        StringAssert.Contains(html, "&lt;tag&gt;");
    }
}
