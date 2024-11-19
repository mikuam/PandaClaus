using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaClaus.Web.Core;

namespace PandaClaus.Web.Pages;

public interface ICsvExporter
{
    string Export(IEnumerable<Letter> enumerable);
}

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
            var letters = (await _client.FetchLetters()).Where(l => l.Status == LetterStatus.UDEKOROWANY);
            var csvExport = _csvExporter.Export(letters.Take(letterCount));

            var byteArray = System.Text.Encoding.UTF8.GetBytes(csvExport);
            var stream = new MemoryStream(byteArray);
            var fileName = $"letters_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(stream, "text/csv", fileName);
        }

        return RedirectToPage("./Index");
    }

    public async Task<IActionResult> OnPostExportFromNumbersAsync(string letterNumbers)
    {
        if (CheckIsAdmin())
        {
            var letterNumbersSeparate = letterNumbers.Split(',').Select(l => l.Trim().ToLowerInvariant());

            var letters = (await _client.FetchLetters()).Where(l => letterNumbersSeparate.Contains(l.Number.ToLowerInvariant()));
            var csvExport = _csvExporter.Export(letters);

            var byteArray = System.Text.Encoding.UTF8.GetBytes(csvExport);
            var stream = new MemoryStream(byteArray);
            var fileName = $"letters_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            foreach (var letter in letters)
            {
                _client.UpdateStatus(letter.RowNumber, LetterStatus.ZAADRESOWANY, letter.Uwagi, letter.Gabaryt);
            }

            return File(stream, "text/csv", fileName);
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

    private bool CheckIsAdmin()
    {
        var isAdminValue = Request.HttpContext.Session.GetString("IsAdmin");
        return isAdminValue is not null && isAdminValue == "true";
    }
}

