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
    }

    [TestMethod]
    public async Task ConfirmationPagesRequireTheirOperationSessionBeforeSheetsAccess()
    {
        var confirmation = new ConfirmationModel(UnreachableSheets())
        {
            PageContext = new PageContext { HttpContext = Context() }
        };
        Assert.IsInstanceOfType<NotFoundResult>(await confirmation.OnGetAsync("fakehash"));
        var added = new LetterAddedConfirmationModel(UnreachableSheets())
        {
            PageContext = new PageContext { HttpContext = Context() }
        };
        Assert.IsInstanceOfType<NotFoundResult>(await added.OnGetAsync("FAKE99"));
    }
}
