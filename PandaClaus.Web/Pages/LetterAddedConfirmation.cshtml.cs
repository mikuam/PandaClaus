using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PandaClaus.Web.Pages;
public class LetterAddedConfirmationModel : PageModel
{
    private readonly GoogleSheetsClient _client;
    public Letter Letter { get; set; }
    public LetterAddedConfirmationModel(GoogleSheetsClient client) => _client = client;

    public async Task<IActionResult> OnGetAsync(string hash)
    {
        if (string.IsNullOrEmpty(hash) ||
            !string.Equals(HttpContext.Session.GetString("SubmittedLetterHash"), hash, StringComparison.OrdinalIgnoreCase))
            return NotFound();
        Letter = await _client.FetchLetterByHashAsync(hash);
        return Letter == null ? NotFound() : Page();
    }
}
