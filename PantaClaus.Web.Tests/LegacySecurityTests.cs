using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using PandaClaus.Web;
using PandaClaus.Web.Pages;

namespace PantaClaus.Web.Tests;

[TestClass]
public class LegacySecurityTests
{
    private sealed class Session : ISession
    {
        private readonly Dictionary<string, byte[]> _items = new();
        public bool IsAvailable => true;
        public string Id => "synthetic";
        public IEnumerable<string> Keys => _items.Keys;
        public void Clear() => _items.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _items.Remove(key);
        public void Set(string key, byte[] value) => _items[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _items.TryGetValue(key, out value!);
    }
    private sealed class SessionFeature : ISessionFeature { public ISession Session { get; set; } = new Session(); }
    private static DefaultHttpContext Context()
    {
        var context = new DefaultHttpContext();
        context.Features.Set<ISessionFeature>(new SessionFeature());
        return context;
    }
    private static GoogleSheetsClient UnreachableSheets() =>
        (GoogleSheetsClient)RuntimeHelpers.GetUninitializedObject(typeof(GoogleSheetsClient));

    [TestMethod]
    public async Task AnonymousStatusReadAndWritesAreDeniedBeforeSheetsAccess()
    {
        var page = new StatusUpdateModel(UnreachableSheets(), null!)
        {
            PageContext = new PageContext { HttpContext = Context() }
        };
        Assert.IsInstanceOfType<RedirectToPageResult>(await page.OnGetAsync());
        Assert.IsInstanceOfType<RedirectToPageResult>(await page.OnPostSetLetterNumberAsync());
        Assert.IsInstanceOfType<RedirectToPageResult>(await page.OnPostSetStatusAsync());
    }

    [TestMethod]
    public void PublicVisibilityAndReservationDenyHiddenOrDeletedRecords()
    {
        Assert.IsFalse(LetterAccessPolicy.CanReadPublicly(false, false));
        Assert.IsFalse(LetterAccessPolicy.CanReadPublicly(true, true));
        Assert.IsTrue(LetterAccessPolicy.CanReadPublicly(true, false));
        Assert.IsFalse(LetterAccessPolicy.CanReserve(false, false, false));
        Assert.IsFalse(LetterAccessPolicy.CanReserve(true, true, false));
        Assert.IsFalse(LetterAccessPolicy.CanReserve(true, false, true));
    }

    [TestMethod]
    public void LegacyIsAdminSessionIsStillAccepted()
    {
        var context = Context();
        var model = new BasePageModel { PageContext = new PageContext { HttpContext = context } };
        Assert.IsFalse(model.IsAdmin);
        context.Session.SetString("IsAdmin", "true");
        Assert.IsTrue(model.IsAdmin);
        context.Session.SetString("IsEarlyAccess", "true");
        Assert.IsTrue(model.IsAdmin);
    }

    [TestMethod]
    public async Task ReceiptQrDestinationSurvivesLegacyAdminLinkLogin()
    {
        var context = Context();
        var receipt = new ReceiveConfirmationModel(UnreachableSheets(), null!, new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build())
        {
            PageContext = new PageContext { HttpContext = context }
        };
        Assert.IsInstanceOfType<RedirectToPageResult>(await receipt.OnGetAsync("FAKE99", "123A"));
        Assert.AreEqual("FAKE99", context.Session.GetString("PendingReceiptHash"));
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AdminCode"] = "synthetic-admin" }).Build();
        context.Request.QueryString = new QueryString("?adminCode=synthetic-admin");
        var index = new IndexModel(UnreachableSheets(), configuration)
        {
            PageContext = new PageContext { HttpContext = context }
        };
        var resumed = (RedirectToPageResult)await index.OnGetAsync();
        Assert.AreEqual("./ReceiveConfirmation", resumed.PageName);
        Assert.AreEqual("FAKE99", resumed.RouteValues!["hash"]);
        Assert.AreEqual("123A", resumed.RouteValues["code"]);
        Assert.IsNull(context.Session.GetString("PendingReceiptHash"));
    }

    [TestMethod]
    public async Task ConfirmationPagesRequireTheirOperationSessionBeforeSheetsAccess()
    {
        var confirmation = new ConfirmationModel(UnreachableSheets())
        {
            PageContext = new PageContext { HttpContext = Context() }
        };
        Assert.IsInstanceOfType<NotFoundResult>(await confirmation.OnGetAsync(3));
        var added = new LetterAddedConfirmationModel(UnreachableSheets())
        {
            PageContext = new PageContext { HttpContext = Context() }
        };
        Assert.IsInstanceOfType<NotFoundResult>(await added.OnGetAsync("FAKE99"));
    }

    [TestMethod]
    public async Task AnonymousReceiptIsDeniedBeforeSheetsAccess()
    {
        var page = new ReceiveConfirmationModel(UnreachableSheets(), null!, new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build())
        {
            PageContext = new PageContext { HttpContext = Context() }
        };
        Assert.IsInstanceOfType<RedirectToPageResult>(await page.OnGetAsync("FAKE", "1"));
        Assert.IsInstanceOfType<RedirectToPageResult>(await page.OnPostConfirmAsync(2, "1"));
    }
}
