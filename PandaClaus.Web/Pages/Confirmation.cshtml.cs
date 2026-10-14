using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PandaClaus.Web.Pages;
public class ConfirmationModel : BasePageModel
{
    private readonly GoogleSheetsClient _client;
    public Letter Letter { get; set; }
    public ConfirmationModel(GoogleSheetsClient client) => _client = client;

    public async Task<IActionResult> OnGetAsync(int rowNumber)
    {
        if (rowNumber < 2 || HttpContext.Session.GetInt32("ReservedRowNumber") != rowNumber)
            return NotFound();
        Letter = await _client.FetchLetterAsync(rowNumber);
        return Letter == null ? NotFound() : Page();
    }
}
