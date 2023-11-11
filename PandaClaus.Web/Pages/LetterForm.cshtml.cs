using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PandaClaus.Web.Pages;
public class LetterFormModel : PageModel
{
    private readonly GoogleSheetsClient _sheetsClient;
    private readonly BlobClient _blobClient;
    private readonly EmailSender _emailSender;

    [BindProperty]
    [Required]
    public string ParentName { get; set; }

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

    public LetterFormModel(GoogleSheetsClient sheetsClient, BlobClient blobClient, EmailSender emailSender)
    {
        _sheetsClient = sheetsClient;
        _blobClient = blobClient;
        _emailSender = emailSender;
    }

    public async Task OnGetAsync()
    {
        // Letter = await _client.FetchLetterAsync(rowNumber);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var photoIds = await _blobClient.UploadPhotos(LetterPhotos);

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
            ImageIds = photoIds,
            Added = DateTime.Now,
            IsVisible = false
        };

        var rowNumber = await _sheetsClient.AddLetter(letter);

        await _emailSender.SendLetterAdded(rowNumber);

        return RedirectToPage($"./LetterAddedConfirmation", new { rowNumber });
    }
}

