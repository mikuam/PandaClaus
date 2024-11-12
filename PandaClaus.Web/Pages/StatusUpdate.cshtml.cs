using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics.Metrics;

namespace PandaClaus.Web.Pages;
public class StatusUpdateModel : PageModel
{
    private readonly GoogleSheetsClient _client;
    private readonly EmailSender _emailSender;
    private readonly BlobClient _blobClient;

    [BindProperty]
    public Letter Letter { get; set; }

    [BindProperty]
    public string Status { get; set; }

    [BindProperty]
    public string Uwagi { get; set; }

    [BindProperty]
    public string Gabaryt { get; set; }

    public StatusUpdateModel(GoogleSheetsClient client, EmailSender emailSender, BlobClient blobClient)
    {
        _emailSender = emailSender;
        _client = client;
        _blobClient = blobClient;
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

        /*
        if (!letter.IsAssigned)
        {
            await _client.AssignLetterAsync(new LetterAssignment
            {
                RowNumber = letter.RowNumber,
                Name = Letter.AssignedTo,
                CompanyName = Letter.AssignedToCompanyName,
                Email = Letter.AssignedToEmail,
                PhoneNumber = Letter.AssignedToPhone,
                Info = Letter.AssignedToInfo
            });
        }

        await _emailSender.SendConfirmationEmail(rowNumber);
        */
        return RedirectToPage("./StatusUpdate", new { number = letter.RowNumber });
    }

    public async Task<IActionResult> OnPostSetStatusAsync()
    {
        if (int.TryParse(Request.Form["number"].ToString(), out var number))
        {
            var status = Request.Query["status"].ToString();
            var uwagi = Request.Form["uwagi"].ToString();
            var gabaryt = Request.Form["gabaryt"].ToString();

            await _client.UpdateStatus(number, status, uwagi, gabaryt);
        }

        return RedirectToPage("./StatusUpdate");
    }
}

