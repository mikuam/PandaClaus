using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PandaClaus.Web.Pages;
public class LetterAddedConfirmationModel : PageModel
{
    private readonly GoogleSheetsClient _client;

    public Letter Letter { get; set; }
    
    public LetterAddedConfirmationModel(GoogleSheetsClient client)
    {
        _client = client;
    }

    public async Task OnGetAsync(string hash)
    {
        Letter = await _client.FetchLetterByHashAsync(hash);
    }
}

