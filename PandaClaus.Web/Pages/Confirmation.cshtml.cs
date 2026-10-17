using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PandaClaus.Web.Pages;
public class ConfirmationModel : BasePageModel
{
    private readonly GoogleSheetsClient _client;
    public Letter Letter { get; set; }
    public ConfirmationModel(GoogleSheetsClient client) => _client = client;

    public async Task<IActionResult> OnGetAsync(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash) ||
            !string.Equals(HttpContext.Session.GetString("ReservedLetterHash"), hash, StringComparison.OrdinalIgnoreCase))
            return NotFound();
        Letter = await _client.FetchLetterByHashAsync(hash);
        return Letter == null ? NotFound() : Page();
    }
}
