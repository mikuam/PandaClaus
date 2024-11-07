using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PandaClaus.Web.Pages;
public class LetterModel : PageModel
{
    private readonly GoogleSheetsClient _client;
    private readonly EmailSender _emailSender;

    [BindProperty]
    public Letter Letter { get; set; }

    [BindProperty]
    public bool IsAdmin { get; set; }

    public LetterModel(GoogleSheetsClient client, EmailSender emailSender)
    {
        _emailSender = emailSender;
        _client = client;
    }

    public async Task OnGetAsync(int rowNumber)
    {
        var isAdminValue = Request.HttpContext.Session.GetString("IsAdmin");
        IsAdmin = isAdminValue is not null && isAdminValue == "true";

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
                Name = Letter.AssignedTo,
                CompanyName = Letter.AssignedToCompanyName,
                Email = Letter.AssignedToEmail,
                PhoneNumber = Letter.AssignedToPhone,
                Info = Letter.AssignedToInfo
            });
        }

        await _emailSender.SendConfirmationEmail(rowNumber);
        
        return RedirectToPage($"./Confirmation", new { rowNumber});
    }
}

