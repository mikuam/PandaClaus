using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PandaClaus.Web.Pages;
public class ConfirmationModel : BasePageModel
{
    private readonly GoogleSheetsClient _client;

    public Letter Letter { get; set; }
    
    public ConfirmationModel(GoogleSheetsClient client)
    {
        _client = client;
    }

    public async Task OnGetAsync(int rowNumber)
    {
        Letter = await _client.FetchLetterAsync(rowNumber);
    }
}

