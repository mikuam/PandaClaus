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
        var result = (await _client.FetchLetters()).Where(l => l.IsVisible).ToList();
        result.ForEach(l => l.Description = TrimLength(l.Description));
        Letters = result;
    }

    private string TrimLength(string text)
    {
        if (text.Length > 130)
            return text.Substring(0, 130) + "...";

        return text;
    }
}
