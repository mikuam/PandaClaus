using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace PandaClaus.Web;

public class GoogleSheetsClient
{
    private readonly string _sheetName;
    private readonly string _spreadsheetId;
    private readonly string _googleSeetsCredentials;
    private readonly SheetsService _sheetsService;

    public GoogleSheetsClient(IConfiguration configuration)
    {
        _spreadsheetId = configuration["SpreadsheetId"]!;
        _sheetName = configuration["SheetName"]!;

        _googleSeetsCredentials = configuration["GoogleSheetsCredentials"]!;

        var credential = GetCredentialsFromFile();
        _sheetsService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "Panda Claus"
        });
    }

    public async Task<List<Letter>> FetchLetters()
    {
        var range = $"{_sheetName}!A:Q";
        var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
        var response = await request.ExecuteAsync();

        return MapToLetters(response.Values);
    }

    public async Task<Letter> FetchLetterAsync(int rowNumber)
    {
        var range = $"{_sheetName}!A{rowNumber}:Q{rowNumber}";
        var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
        var response = await request.ExecuteAsync();

        var letter = MapLetter(rowNumber, response.Values.First());
        return letter;
    }

    public async Task AddLetter(Letter letter)
    {
        var letters = await FetchLetters();
        var firstEmptyRow = letters.Count + 2;

        var range = $"{_sheetName}!A{firstEmptyRow}:L{firstEmptyRow}";
        var valuesToUpdate = new List<object>
        {
            letter.Number,
            letter.ParentName,
            letter.PhoneNumber,
            letter.Email,
            letter.Address,
            letter.PaczkomatCode,
            letter.ChildAge,
            letter.Description,
            string.Join(",", letter.ImageIds),
            letter.Added.ToString("yyyy-MM-dd HH:mm:ss"),
            letter.IsVisible ? "tak" : "nie",
            "nie"
        };
        var valueRange = new ValueRange
        {
            Values = new List<IList<object>> { valuesToUpdate }
        };

        var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
        updateRequest.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;
        await updateRequest.ExecuteAsync();
    }

    private List<Letter> MapToLetters(IList<IList<object>> values)
    {
        var letters = new List<Letter>();
        var rowNumber = 2;

        foreach (var row in values.Skip(1))
        {
            if (row.Count < 12)
                continue;

            var letter = MapLetter(rowNumber, row);

            rowNumber++;
            letters.Add(letter);
        }

        return letters;
    }

    private static Letter MapLetter(int rowNumber, IList<object> row)
    {
        var letter = new Letter
        {
            RowNumber = rowNumber,
            Number = row[0].ToString() ?? string.Empty,
            ParentName = row[1].ToString() ?? string.Empty,
            PhoneNumber = row[2].ToString() ?? string.Empty,
            Email = row[3].ToString() ?? string.Empty,
            Address = row[4].ToString() ?? string.Empty,
            PaczkomatCode = row[5].ToString() ?? string.Empty,
            ChildAge = row[6].ToString() ?? string.Empty,
            Description = row[7].ToString() ?? string.Empty,
            ImageIds = string.IsNullOrWhiteSpace(row[8].ToString())
                ? new List<string>()
                : row[8].ToString()!.Split(',').Select(url => url.Trim().Replace("open?", "uc?")).ToList(),
            Added = DateTime.Parse(row[9].ToString() ?? string.Empty),
            IsVisible = string.Equals(row[10].ToString() ?? string.Empty, "tak", StringComparison.OrdinalIgnoreCase), // "tak" or "nie"
            IsAssigned = string.Equals(row[11].ToString() ?? string.Empty, "tak", StringComparison.OrdinalIgnoreCase), // "tak" or "nie"
            
            // optional cells
            AssignedTo = GetCellOrEmptyString(row, 12),
            AssignedToCompanyName = GetCellOrEmptyString(row, 13),
            AssignedToEmail = GetCellOrEmptyString(row, 14),
            AssignedToPhone = GetCellOrEmptyString(row, 15),
            AssignedToDescription = GetCellOrEmptyString(row, 16)
        };

        return letter;
    }

    private static string GetCellOrEmptyString(IList<object> row, int index)
    {
        if (row.Count >= index + 1)
        {
            return row[index].ToString()!;
        }

        return string.Empty;
    }

    private GoogleCredential GetCredentialsFromFile()
    {
        var data = Convert.FromBase64String(_googleSeetsCredentials);
        var credentialsJson = System.Text.Encoding.UTF8.GetString(data);
        
        return GoogleCredential.FromJson(credentialsJson).CreateScoped(SheetsService.Scope.Spreadsheets);
    }

    public async Task AssignLetterAsync(LetterAssignment assignment)
    {
        var range = $"{_sheetName}!N{assignment.RowNumber}:R{assignment.RowNumber}";

        var valuesToUpdate = new List<object> { "tak", assignment.Name, assignment.Email, assignment.PhoneNumber, assignment.Description };
        var valueRange = new ValueRange
        {
            Values = new List<IList<object>> { valuesToUpdate }
        };
        var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
        updateRequest.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;
        await updateRequest.ExecuteAsync();
    }
}