using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PandaClaus.Web.Pages;
public class StatusUpdateModel : PageModel
{
    private readonly GoogleSheetsClient _client;

    [BindProperty]
    public Letter Letter { get; set; }

    public StatusUpdateModel(GoogleSheetsClient client)
    {
        _client = client;
    }

    public async Task OnGetAsync()
    {
        if (int.TryParse(Request.Query["number"].ToString(), out var number))
        {
            Letter = await _client.FetchLetterAsync(number);
        }
    }

    public async Task<IActionResult> OnPostSetLetterNumberAsync()
    {
        var letterNumber = Request.Form["letterNumber"].ToString();
        var allLetters = await _client.FetchLetters();
        var letter = allLetters.FirstOrDefault(l => l.Number.Equals(letterNumber, StringComparison.CurrentCultureIgnoreCase));

        if (letter == null)
        {
            return RedirectToPage("./StatusUpdate");
        }

        Letter = letter;

        return RedirectToPage("./StatusUpdate", new { number = letter.RowNumber });
    }

    public async Task<IActionResult> OnPostSetStatusAsync()
    {
        if (int.TryParse(Request.Form["number"].ToString(), out var rowNumber))
        {
            var status = Request.Query["status"].ToString();
            var uwagi = Request.Form["uwagi"].ToString();
            var gabaryt = string.IsNullOrWhiteSpace(Request.Form["gabaryt"].ToString())
                ? Letter.Gabaryt.ToString()
                : Request.Form["gabaryt"].ToString();

            await _client.UpdateStatus(rowNumber, status, uwagi, gabaryt);
        }

        return RedirectToPage("./StatusUpdate", new { message = "Status zaktualizowany pomyślnie" });
    }
}