using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PandaClaus.Web.Pages;
public class IndexModel : BasePageModel
{
    private const string AdminParameter = "adminCode";
    private const string ActiveButton = "btn-primary";
    private const string InActiveButton = "btn-outline-primary";

    private readonly GoogleSheetsClient _client;
    private readonly string _adminCode;

    public List<Letter> Letters { get; set; } = new();
    public string ButtonFiltersAll = "btn-primary";
    public string ButtonFiltersReserved = "btn-outline-primary";
    public string ButtonFiltersAvailable = "btn-outline-primary";

    public IndexModel(GoogleSheetsClient client, IConfiguration configuration)
    {
        _client = client;
        _adminCode = configuration["AdminCode"]!;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        Response.Headers.CacheControl = "private, no-store";
        var isAdminFromQuery = !string.IsNullOrWhiteSpace(_adminCode) &&
            Request.Query.ContainsKey(AdminParameter) && Request.Query[AdminParameter] == _adminCode;
        if (isAdminFromQuery)
        {
            Request.HttpContext.Session.SetString("IsAdmin", "true");
            var pendingHash = HttpContext.Session.GetString("PendingReceiptHash");
            var pendingCode = HttpContext.Session.GetString("PendingReceiptCode");
            HttpContext.Session.Remove("PendingReceiptHash");
            HttpContext.Session.Remove("PendingReceiptCode");
            if (!string.IsNullOrEmpty(pendingHash) && !string.IsNullOrEmpty(pendingCode))
                return RedirectToPage("./ReceiveConfirmation", new { hash = pendingHash, code = pendingCode });
        }

        Request.HttpContext.Session.SetString("IsAdmin", IsAdmin ? "true" : "false");

        var result = (await _client.FetchLetters()).Where(l => !l.IsDeleted).ToList();
        var filter = Request.Query["filter"].ToString();
        switch (filter)
        {
            case "available":
                result = result.Where(l => !l.IsAssigned).ToList();

                ButtonFiltersAll = InActiveButton;
                ButtonFiltersReserved = InActiveButton;
                ButtonFiltersAvailable = ActiveButton;

                break;
            case "reserved":
                result = result.Where(l => l.IsAssigned).ToList();

                ButtonFiltersAll = InActiveButton;
                ButtonFiltersReserved = ActiveButton;
                ButtonFiltersAvailable = InActiveButton;

                break;
            default:

                ButtonFiltersAll = ActiveButton;
                ButtonFiltersReserved = InActiveButton;
                ButtonFiltersAvailable = InActiveButton;
                break;
        }

        if (!IsAdmin)
        {
            result = result.Where(l => LetterAccessPolicy.CanReadPublicly(l.IsVisible, l.IsDeleted)).ToList();
        }

        var search = Request.Query["search"].ToString();
        if (!string.IsNullOrEmpty(search))
        {
            result = result.Where(l =>
                l.Description.Contains(search) 
                || l.ChildName.Contains(search)
                || l.Number.Contains(search)).ToList();
        }

        result.ForEach(l => l.Description = TrimLength(l.Description));
        Letters = result.OrderBy(l => l.Number, new CustomComparer()).ToList();
        return Page();
    }

    private string TrimLength(string text)
    {
        if (text.Length > 130)
            return text.Substring(0, 130) + "...";

        return text;
    }
}
