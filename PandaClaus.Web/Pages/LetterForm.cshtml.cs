using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace PandaClaus.Web.Pages;
public class LetterFormModel : PageModel
{
    private readonly GoogleSheetsClient _sheetsClient;

    [BindProperty]
    [Required]
    public string ParentName { get; set; }

    [BindProperty]
    public string CompanyName { get; set; }

    [BindProperty]
    [Required]
    public string PhoneNumber { get; set; }

    [BindProperty]
    [Required]
    public string Email { get; set; }

    [BindProperty]
    [Required]
    public string Address { get; set; }

    [BindProperty]
    [Required]
    public string PaczkomatCode { get; set; }

    [BindProperty]
    [Required]
    public string ChildAge { get; set; }

    [BindProperty]
    [Required]
    public string Description { get; set; }

    [BindProperty]
    [Required]
    public List<IFormFile> LetterPhotos { get; set; }

    public LetterFormModel(GoogleSheetsClient sheetsClient)
    {
        _sheetsClient = sheetsClient;
    }

    public async Task OnGetAsync()
    {
        // Letter = await _client.FetchLetterAsync(rowNumber);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var letter = new Letter
        {
            Number = "X",
            ParentName = ParentName,
            PhoneNumber = PhoneNumber,
            Email = Email,
            Address = Address,
            PaczkomatCode = PaczkomatCode,
            ChildAge = ChildAge,
            Description = Description,
            ImageIds = LetterPhotos.Select(p => p.FileName).ToList(),
            Added = DateTime.Now,
            IsVisible = false
        };

        await _sheetsClient.AddLetter(letter);

        /*
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
        */

        return RedirectToPage($"./");
    }
}

