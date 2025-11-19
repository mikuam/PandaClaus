using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace PandaClaus.Web.Pages;
public class LetterFormModel : BasePageModel
{
    private readonly GoogleSheetsClient _sheetsClient;
    private readonly BlobClient _blobClient;
    private readonly EmailSender _emailSender;
    private readonly IConfiguration _configuration;

    [BindProperty]
    [Required]
    public string ParentName { get; set; }

    [BindProperty]
    [Required]
    public string ParentSurname { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Numer telefonu jest wymagany")]
    [RegularExpression(@"^[4-8]\d{8}$", ErrorMessage = "Numer telefonu musi być polskim numerem telefonu składającym się z 9 cyfr (bez +48), zaczynającym się od cyfry 4-8")]
    [StringLength(9, MinimumLength = 9, ErrorMessage = "Numer telefonu musi składać się z dokładnie 9 cyfr")]
    public string PhoneNumber { get; set; }

    [BindProperty]
    [Required]
    public string Email { get; set; }

    [BindProperty]
    [Required]
    public string Street { get; set; }

    [BindProperty]
    [Required]
    public string HouseNumber { get; set; }

    [BindProperty]
    public string? ApartmentNumber { get; set; }

    [BindProperty]
    [Required]
    public string City { get; set; }

    [BindProperty]
    [Required]
    public string PostalCode { get; set; }

    [BindProperty]
    [Required]
    public string PaczkomatCode { get; set; }

    [BindProperty]
    [Required]
    public string ChildName { get; set; }

    [BindProperty]
    [Required]
    public int ChildAge { get; set; }

    [BindProperty]
    [Required]
    public string Description { get; set; }

    [BindProperty]
    [Required]
    public string Presents { get; set; }

    [BindProperty]
    [Required]
    public List<IFormFile> LetterPhotos { get; set; }

    public string InPostGeoWidgetToken => _configuration["InPostGeoWidgetToken"] ?? string.Empty;

    public LetterFormModel(GoogleSheetsClient sheetsClient, BlobClient blobClient, EmailSender emailSender, IConfiguration configuration)
    {
        _sheetsClient = sheetsClient;
        _blobClient = blobClient;
        _emailSender = emailSender;
        _configuration = configuration;
    }

    public async Task OnGetAsync()
    {
        // Letter = await _client.FetchLetterAsync(rowNumber);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Validate file types
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff" };
        var allowedMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/bmp", "image/tiff", "image/x-tiff" };
        
        foreach (var file in LetterPhotos)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError(nameof(LetterPhotos), 
                    $"Plik '{file.FileName}' ma niedozwolone rozszerzenie. Dozwolone formaty: JPG, JPEG, PNG, BMP, TIF, TIFF");
                return Page();
            }
            
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                ModelState.AddModelError(nameof(LetterPhotos), 
                    $"Plik '{file.FileName}' ma niedozwolony typ pliku. Proszę przesłać tylko pliki graficzne.");
                return Page();
            }
        }

        // Check if letter already exists
        var existingLetters = await _sheetsClient.FetchLetters();
        var isDuplicate = existingLetters.Any(l => 
            string.Equals(l.Email, Email, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(l.ChildName, ChildName, StringComparison.OrdinalIgnoreCase) &&
            l.ChildAge == ChildAge);

        if (isDuplicate)
        {
            ModelState.AddModelError(string.Empty, "List dla tego dziecka został już dodany. Jeśli masz pytania, skontaktuj się z nami: pandaclaus@pandateam.pl");
            return Page();
        }

        var photoIds = await _blobClient.UploadPhotos(LetterPhotos);

        var letter = new Letter
        {
            RowNumber = 0, // Will be set by Google Sheets
            Number = "X", // Temporary, will be updated by the service
            ParentName = ParentName.Trim(),
            ParentSurname = ParentSurname.Trim(),
            PhoneNumber = PhoneNumber.Trim(),
            Email = Email.Trim(),
            Street = Street.Trim(),
            HouseNumber = HouseNumber.Trim(),
            ApartmentNumber = ApartmentNumber ?? string.Empty,
            City = City.Trim(),
            PostalCode = PostalCode,
            PaczkomatCode = PaczkomatCode.Trim(),
            ChildName = ChildName.Trim(),
            ChildAge = ChildAge,
            Description = Description,
            Presents = Presents,
            ImageIds = [],
            ImageUrls = photoIds,
            Added = DateTime.Now,
            IsVisible = false,
            IsAssigned = false
        };

        var rowNumber = await _sheetsClient.AddLetter(letter);

        await _emailSender.SendLetterAdded(rowNumber);

        return RedirectToPage("./LetterAddedConfirmation", new { rowNumber });
    }
}

