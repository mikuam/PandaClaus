using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaClaus.Web.Core;

namespace PandaClaus.Web.Pages;

public class AdminModel : PageModel
{
    private readonly GoogleSheetsClient _client;
    private readonly ICsvExporter _csvExporter;
    private readonly BlobClient _blobClient;

    [BindProperty]
    public string LetterNumbers { get; set; }

    [BindProperty]
    public bool IsAdmin { get; set; }

    [BindProperty]
    public int LetterCount { get; set; }

    public AdminModel(GoogleSheetsClient client, ICsvExporter csvExporter, BlobClient blobClient)
    {
        _client = client;
        _csvExporter = csvExporter;
        _blobClient = blobClient;
    }

    public async Task<IActionResult> OnGetAsync(int rowNumber)
    
    {
        IsAdmin = CheckIsAdmin();

        if (!IsAdmin)
        {
            return RedirectToPage("./Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostExportNextAsync(int letterCount)
    {
        if (CheckIsAdmin())
        {
            var letters = (await _client.FetchLetters())
                .Where(l => l.Status == LetterStatus.SPAKOWANE)
                .Take(letterCount);
            return await GenerateFileAndUpdateStatus(letters);
        }

        return RedirectToPage("./Index");
    }

    public async Task<IActionResult> OnPostExportFromNumbersAsync(string letterNumbers)
    {
        if (CheckIsAdmin())
        {
            var letterNumbersSeparate = letterNumbers.Split(',').Select(l => l.Trim().ToLowerInvariant());

            var letters = (await _client.FetchLetters()).Where(l => letterNumbersSeparate.Contains(l.Number.ToLowerInvariant()));
            return await GenerateFileAndUpdateStatus(letters);
        }

        return RedirectToPage("./Index");
    }

    public async Task<IActionResult> OnPostUpdateContentTypeAsync()
    {
        if (CheckIsAdmin())
        {
            var letters = await _client.FetchLetters();
            var images = letters.SelectMany(l => l.ImageIds).Distinct().ToList();

            foreach (var image in images)
            {
                var contentType = ImageHelper.GetContentType(image);

                if (!string.IsNullOrWhiteSpace(contentType))
                {
                    await _blobClient.UpdateContentType(image, contentType);
                }
                else
                {
                    
                }
            }
        }

        return RedirectToPage("./Index");
    }

    private async Task<IActionResult> GenerateFileAndUpdateStatus(IEnumerable<Letter> letters)
    {
        var letterNumbers = letters.Select(l => l.Number).ToList();
        var packages = (await _client.FetchPackages()).Where(p => letterNumbers.Contains(p.LetterNumber));

        var lettersWithPackages = letters.ToDictionary(l => l, l => packages.Where(p => p.LetterNumber == l.Number));
        var csvExport = _csvExporter.Export(lettersWithPackages);

        foreach (var letter in letters)
        {
            _client.UpdateStatus(letter.RowNumber, LetterStatus.ZAADRESOWANE, letter.Uwagi);
        }

        var byteArray = System.Text.Encoding.UTF8.GetBytes(csvExport);
        var stream = new MemoryStream(byteArray);
        var fileName = $"letters_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

        return File(stream, "text/csv", fileName);
    }

    private bool CheckIsAdmin()
    {
        var isAdminValue = Request.HttpContext.Session.GetString("IsAdmin");
        return isAdminValue is not null && isAdminValue == "true";
    }
}

