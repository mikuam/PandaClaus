using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PandaClaus.Web.Pages;
public class IndexModel : PageModel
{
    private readonly GoogleSheetsClient _client;
    public List<Letter> Letters { get; set; } = new();

    public IndexModel(GoogleSheetsClient client)
    {
        _client = client;
    }

    public async Task OnGetAsync()
    {
        Letters = await _client.FetchLetters();
    }
}
