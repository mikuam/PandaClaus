using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PandaClaus.Web.Pages;
public class LetterModel : PageModel
{
    private readonly GoogleSheetsClient _client;
    private readonly EmailSender _emailSender;

    public Letter Letter { get; set; }

    public LetterModel(GoogleSheetsClient client, EmailSender emailSender)
    {
        _emailSender = emailSender;
        _client = client;
    }

    public async Task OnGetAsync(int rowNumber)
    {
        Letter = await _client.FetchLetterAsync(rowNumber);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var rowNumber = int.Parse(Request.Form["RowNumber"]!);

        var letter = await _client.FetchLetterAsync(rowNumber);
        if (!letter.IsAssigned)
        {
            await _client.AssignLetterAsync(new LetterAssignment
            {
                RowNumber = rowNumber,
                Name = Request.Form["Name"],
                Email = Request.Form["Email"],
                PhoneNumber = Request.Form["Phone"],
            });
        }

        await _emailSender.SendConfirmationEmail(rowNumber);
        
        return RedirectToPage($"./Confirmation", new { rowNumber});
    }
}

