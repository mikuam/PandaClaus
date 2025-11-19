using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PandaClaus.Web.Pages;
public class IndexModel : BasePageModel
{
    private const string AdminParameter = "adminCode";
    private const string EarlyAccessParameter = "earlyAccess";
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

    public async Task OnGetAsync()
    {
        var isAdminFromQuery = Request.Query.ContainsKey(AdminParameter) && Request.Query[AdminParameter] == _adminCode;
        if (isAdminFromQuery)
        {
            Request.HttpContext.Session.SetString("IsAdmin", "true");
        }

        Request.HttpContext.Session.SetString("IsAdmin", IsAdmin ? "true" : "false");

        var isEarlyAccess = IsEarlyAccess();
        Request.HttpContext.Session.SetString("IsEarlyAccess", isEarlyAccess ? "true" : "false");

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

        if (!IsAdmin && !isEarlyAccess)
        {
            result = result.Where(l => l.IsVisible).ToList();
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
    }

    private bool IsEarlyAccess()
    {
        var isEarlAccessValue = Request.HttpContext.Session.GetString("IsEarlyAccess");
        var isEarlyAccessFromSession = isEarlAccessValue is not null && isEarlAccessValue == "true";

        return isEarlyAccessFromSession ||
               (Request.Query.ContainsKey(EarlyAccessParameter) && Request.Query[EarlyAccessParameter] == "true");
    }

    private string TrimLength(string text)
    {
        if (text.Length > 130)
            return text.Substring(0, 130) + "...";

        return text;
    }
}
