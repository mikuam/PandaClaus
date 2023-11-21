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
    private readonly string _blobUrl;
    private readonly SheetsService _sheetsService;

    public GoogleSheetsClient(IConfiguration configuration)
    {
        _spreadsheetId = configuration["SpreadsheetId"]!;
        _sheetName = configuration["SheetName"]!;
        _blobUrl = configuration["BlobUrl"]!;

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
        var range = $"{_sheetName}!A:S";
        var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
        var response = await request.ExecuteAsync();

        return MapToLetters(response.Values);
    }

    public async Task<Letter> FetchLetterAsync(int rowNumber)
    {
        var range = $"{_sheetName}!A{rowNumber}:S{rowNumber}";
        var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
        var response = await request.ExecuteAsync();

        var letter = MapLetter(rowNumber, response.Values.First());
        return letter;
    }

    public async Task<int> AddLetter(Letter letter)
    {
        var letters = await FetchLetters();
        var firstEmptyRow = letters.Count + 2;

        var range = $"{_sheetName}!A{firstEmptyRow}:N{firstEmptyRow}";
        var valuesToAdd = new List<object>
        {
            letter.Number,
            letter.ParentName,
            letter.PhoneNumber,
            letter.Email,
            letter.Street,
            letter.City,
            letter.PostalCode,
            letter.ChildName,
            letter.ChildAge,
            letter.Description,
            string.Join(",", letter.ImageIds),
            letter.Added.ToString("yyyy-MM-dd HH:mm:ss"),
            letter.IsVisible ? "tak" : "nie",
            "nie"
        };
        var valueRange = new ValueRange
        {
            Values = new List<IList<object>> { valuesToAdd }
        };

        var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
        updateRequest.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;
        await updateRequest.ExecuteAsync();

        return firstEmptyRow;
    }

    private List<Letter> MapToLetters(IList<IList<object>> values)
    {
        var letters = new List<Letter>();
        var rowNumber = 2;

        foreach (var row in values.Skip(1))
        {
            if (row.Count < 14)
                continue;

            var letter = MapLetter(rowNumber, row);

            rowNumber++;
            letters.Add(letter);
        }

        return letters;
    }

    private Letter MapLetter(int rowNumber, IList<object> row)
    {
        var letter = new Letter
        {
            RowNumber = rowNumber,
            Number = row[0].ToString() ?? string.Empty,
            ParentName = row[1].ToString() ?? string.Empty,
            PhoneNumber = row[2].ToString() ?? string.Empty,
            Email = row[3].ToString() ?? string.Empty,
            Street = row[4].ToString() ?? string.Empty,
            City = row[5].ToString() ?? string.Empty,
            PostalCode = row[6].ToString() ?? string.Empty,
            ChildName = row[7].ToString() ?? string.Empty,
            ChildAge = row[8].ToString() ?? string.Empty,
            Description = row[9].ToString() ?? string.Empty,
            ImageIds = string.IsNullOrWhiteSpace(row[10].ToString())
                ? new List<string>()
                : row[10].ToString()!.Split(',').Select(url => $"{_blobUrl}/{url}").ToList(),
            Added = DateTime.Parse(row[11].ToString() ?? string.Empty),
            IsVisible = string.Equals(row[12].ToString() ?? string.Empty, "tak", StringComparison.OrdinalIgnoreCase), // "tak" or "nie"
            IsAssigned = string.Equals(row[13].ToString() ?? string.Empty, "tak", StringComparison.OrdinalIgnoreCase), // "tak" or "nie"
            
            // optional cells
            AssignedTo = GetCellOrEmptyString(row, 14),
            AssignedToCompanyName = GetCellOrEmptyString(row, 15),
            AssignedToEmail = GetCellOrEmptyString(row, 16),
            AssignedToPhone = GetCellOrEmptyString(row, 17),
            AssignedToInfo = GetCellOrEmptyString(row, 18)
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
        var range = $"{_sheetName}!N{assignment.RowNumber}:S{assignment.RowNumber}";

        var valuesToUpdate = new List<object>
        {
            "tak",
            assignment.Name,
            assignment.CompanyName,
            assignment.Email,
            assignment.PhoneNumber,
            assignment.Info
        };
        var valueRange = new ValueRange
        {
            Values = new List<IList<object>> { valuesToUpdate }
        };
        var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
        updateRequest.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;
        await updateRequest.ExecuteAsync();
    }
}