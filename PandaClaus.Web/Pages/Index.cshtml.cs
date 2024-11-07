using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PandaClaus.Web.Pages;
public class IndexModel : PageModel
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

    public async Task OnGetAsync()
    {
        var isAdmin = IsAdmin();
        Request.HttpContext.Session.SetString("IsAdmin", isAdmin ? "true" : "false");

        var result = (await _client.FetchLetters()).ToList();
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

        if (!isAdmin)
        {
            result = result.Where(l => l.IsVisible).ToList();
        }

        result.ForEach(l => l.Description = TrimLength(l.Description));
        Letters = result.OrderBy(l => l.Number, new CustomComparer()).ToList();
    }

    private bool IsAdmin()
    {
        var isAdminValue = Request.HttpContext.Session.GetString("IsAdmin");
        var isAdminFromSession = isAdminValue is not null && isAdminValue == "true";

        return isAdminFromSession ||
               (Request.Query.ContainsKey(AdminParameter) && Request.Query[AdminParameter] == _adminCode);
    }

    private string TrimLength(string text)
    {
        if (text.Length > 130)
            return text.Substring(0, 130) + "...";

        return text;
    }
}
