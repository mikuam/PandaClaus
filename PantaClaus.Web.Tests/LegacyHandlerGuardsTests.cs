using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using PandaClaus.Web;
using PandaClaus.Web.Core;
using PandaClaus.Web.Pages;
using PandaClaus.Web.Services;

namespace PantaClaus.Web.Tests;

// An uninitialized Sheets client throws on ANY access. An anonymous handler that
// reads even once therefore fails the test instead of silently contacting Google.
[TestClass]
public class LegacyHandlerGuardsTests
{
    private sealed class Session : ISession
    {
        private readonly Dictionary<string, byte[]> _items = new();
        public bool IsAvailable => true;
        public string Id => "synthetic-guard-test";
        public IEnumerable<string> Keys => _items.Keys;
        public void Clear() => _items.Clear();
        public Task CommitAsync(CancellationToken token = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken token = default) => Task.CompletedTask;
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

    private static T Unreachable<T>() where T : class => (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
    private static T Setup<T>(T model, DefaultHttpContext context) where T : PageModel
    {
        model.PageContext = new PageContext { HttpContext = context };
        return model;
    }

    private static AdminModel Admin(DefaultHttpContext context) => Setup(
        new AdminModel(Unreachable<GoogleSheetsClient>(), null!, null!, null!, null!, null!), context);

    private static LetterModel Letter(DefaultHttpContext context) => Setup(
        new LetterModel(Unreachable<GoogleSheetsClient>(), null!, null!, new ConfigurationBuilder().Build(), null!), context);

    [DataTestMethod]
    [DataRow("index")]
    [DataRow("exportNext")]
    [DataRow("exportFromNumbers")]
    [DataRow("sendToInpost")]
    [DataRow("updateContentType")]
    [DataRow("downloadCsv")]
    [DataRow("toggleAddLetter")]
    public async Task AnonymousAdminHandlerDeniesBeforeReadOrEffect(string handler)
    {
        var page = Admin(Context());
        IActionResult result = handler switch
        {
            "index" => await page.OnGetAsync(2),
            "exportNext" => await page.OnPostExportNextAsync(1),
            "exportFromNumbers" => await page.OnPostExportFromNumbersAsync("FAKE"),
            "sendToInpost" => await page.OnPostSendToInPostAsync("FAKE"),
            "updateContentType" => await page.OnPostUpdateContentTypeAsync(),
            "downloadCsv" => await page.OnGetDownloadCsvAsync("fake.csv"),
            "toggleAddLetter" => page.OnPostToggleAddLetterLink(true),
            _ => throw new ArgumentOutOfRangeException(nameof(handler))
        };
        Assert.IsInstanceOfType<RedirectToPageResult>(result);
    }

    [DataTestMethod]
    [DataRow("deleteImage")]
    [DataRow("uploadImage")]
    [DataRow("updateDetails")]
    [DataRow("deleteLetter")]
    [DataRow("sendConfirmationEmail")]
    [DataRow("confirmDelivery")]
    public async Task AnonymousLetterMutationDeniesBeforeReadUploadEmailOrWrite(string handler)
    {
        var page = Letter(Context());
        IActionResult result = handler switch
        {
            "deleteImage" => await page.OnPostDeleteImageAsync(2, "synthetic-image"),
            "uploadImage" => await page.OnPostUploadImageAsync(2),
            "updateDetails" => await page.OnPostUpdateDetailsAsync(2),
            "deleteLetter" => await page.OnPostDeleteLetterAsync(2),
            "sendConfirmationEmail" => await page.OnPostSendConfirmationEmailAsync(2),
            "confirmDelivery" => await page.OnPostConfirmDeliveryAsync(2, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(handler))
        };
        Assert.IsInstanceOfType<RedirectToPageResult>(result);
    }

    [TestMethod]
    public async Task ForgedReservationWithInvalidRowIsRejectedBeforeSheetsAccess()
    {
        var context = Context();
        context.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["RowNumber"] = "0"
        });
        Assert.IsInstanceOfType<BadRequestResult>(await Letter(context).OnPostAsync());
    }

    [TestMethod]
    public async Task AnonymousReceiptPostIsDeniedBeforeSheetsAccess()
    {
        var page = Setup(new ReceiveConfirmationModel(Unreachable<GoogleSheetsClient>(), null!,
            new ConfigurationBuilder().Build()), Context());
        Assert.IsInstanceOfType<RedirectToPageResult>(await page.OnPostConfirmAsync(2, "FAKE"));
    }

    [TestMethod]
    public async Task LaterInvalidQrClearsOlderPendingDestination()
    {
        var context = Context();
        var page = Setup(new ReceiveConfirmationModel(Unreachable<GoogleSheetsClient>(), null!,
            new ConfigurationBuilder().Build()), context);
        Assert.IsInstanceOfType<RedirectToPageResult>(await page.OnGetAsync("FAKE", "1"));
        Assert.AreEqual("FAKE", context.Session.GetString("PendingReceiptHash"));
        Assert.IsInstanceOfType<RedirectToPageResult>(await page.OnGetAsync(new string('X', 65), "1"));
        Assert.IsNull(context.Session.GetString("PendingReceiptHash"));
        Assert.IsNull(context.Session.GetString("PendingReceiptCode"));
    }
}
