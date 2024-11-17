using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PandaClaus.Web.Pages;

public interface ICsvExporter
{
    string Export(IEnumerable<Letter> enumerable);
}

public class AdminModel : PageModel
{
    private readonly GoogleSheetsClient _client;
    private readonly ICsvExporter _csvExporter;

    [BindProperty]
    public string LetterNumbers { get; set; }

    [BindProperty]
    public bool IsAdmin { get; set; }

    [BindProperty]
    public int LetterCount { get; set; }

    public AdminModel(GoogleSheetsClient client, ICsvExporter csvExporter)
    {
        _client = client;
        _csvExporter = csvExporter;
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
            var letters = await _client.FetchLetters();
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

            return File(stream, "text/csv", fileName);
        }

        return RedirectToPage("./Index");
    }

    private bool CheckIsAdmin()
    {
        var isAdminValue = Request.HttpContext.Session.GetString("IsAdmin");
        return isAdminValue is not null && isAdminValue == "true";
    }
}

