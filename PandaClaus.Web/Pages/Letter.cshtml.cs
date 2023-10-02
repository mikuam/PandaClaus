using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PandaClaus.Web.Pages;
public class LetterModel : PageModel
{
    private readonly GoogleSheetsClient _client;

    public Letter Letter { get; set; }

    public LetterModel(GoogleSheetsClient client)
    {
        _client = client;
    }

    public async Task OnGetAsync(int rowNumber)
    {
        Letter = await _client.FetchLetter(rowNumber);
    }
}

