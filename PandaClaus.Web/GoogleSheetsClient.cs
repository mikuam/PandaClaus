﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using PandaClaus.Web.Core;
using PandaClaus.Web.Core.DTOs;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace PandaClaus.Web;

public class GoogleSheetsClient
{
    private readonly string _sheetName;
    private readonly string _packageSheetName;
    private readonly string _spreadsheetId;
    private readonly string _googleSheetsCredentials;
    private readonly string _blobUrl;
    private readonly SheetsService _sheetsService;
    private readonly LetterNumerationService _letterNumerationService;

    public GoogleSheetsClient(IConfiguration configuration, LetterNumerationService letterNumerationService)
    {
        _letterNumerationService = letterNumerationService;

        _spreadsheetId = configuration["SpreadsheetId"]!;
        _sheetName = configuration["SheetName"]!;
        _packageSheetName = configuration["PackageSheetName"]!;
        _blobUrl = configuration["BlobUrl"]!;

        _googleSheetsCredentials = configuration["GoogleSheetsCredentials"]!;

        var credential = GetCredentialsFromFile();
        _sheetsService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "Panda Claus"
        });
    }

    public async Task<List<Letter>> FetchLetters()
    {
        var range = $"{_sheetName}!A:Z";
        var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
        var response = await request.ExecuteAsync();

        return MapToLetters(response.Values);
    }

    public async Task<Letter> FetchLetterAsync(int rowNumber)
    {
        var range = $"{_sheetName}!A{rowNumber}:Z{rowNumber}";
        var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
        var response = await request.ExecuteAsync();

        var letter = MapLetter(rowNumber, response.Values.First());
        return letter;
    }

    public async Task<int> AddLetter(Letter letter)
    {
        var letters = await FetchLetters();
        var firstEmptyRow = letters.Count + 2;

        var letterNumber = _letterNumerationService.GetNextLetterNumber(letters, letter);
        var range = $"{_sheetName}!A{firstEmptyRow}:S{firstEmptyRow}";
        var valuesToAdd = new List<object>
        {
            letterNumber,
            letter.ParentName,
            letter.ParentSurname,
            letter.PhoneNumber,
            letter.Email,
            letter.Street,
            letter.HouseNumber,
            letter.ApartmentNumber,
            letter.City,
            letter.PostalCode,
            letter.PaczkomatCode,
            letter.ChildName,
            letter.ChildAge,
            letter.Description,
            letter.Presents,
            string.Join(",", letter.ImageUrls),
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
            ParentSurname = row[2].ToString() ?? string.Empty,
            PhoneNumber = row[3].ToString() ?? string.Empty,
            Email = row[4].ToString() ?? string.Empty,
            Street = row[5].ToString() ?? string.Empty,
            HouseNumber = row[6].ToString() ?? string.Empty,
            ApartmentNumber = row[7].ToString() ?? string.Empty,
            City = row[8].ToString() ?? string.Empty,
            PostalCode = row[9].ToString() ?? string.Empty,
            PaczkomatCode = row[10].ToString() ?? string.Empty,
            ChildName = row[11].ToString() ?? string.Empty,
            ChildAge = string.IsNullOrWhiteSpace(row[12].ToString()) ? 0 : int.Parse(row[12].ToString()!),
            Description = row[13].ToString() ?? string.Empty,
            Presents = row[14].ToString() ?? string.Empty,
            ImageIds = string.IsNullOrWhiteSpace(row[15].ToString())
                ? new List<string>()
                : row[15].ToString()!.Split(',').ToList(),
            ImageUrls = string.IsNullOrWhiteSpace(row[15].ToString())
                ? new List<string>()
                : row[15].ToString()!.Split(',').Select(url => $"{_blobUrl}/{url}").ToList(),
            ImageThumbnailId = string.IsNullOrWhiteSpace(row[15].ToString())
                ? string.Empty
                : row[15].ToString()!.Split(',').First(),
            ImageThumbnailUrl = string.IsNullOrWhiteSpace(row[15].ToString())
                ? string.Empty
                : $"{_blobUrl}/{row[15].ToString()!.Split(',').First()}_thumbnail.jpg",
            Added = DateTime.Parse(row[16].ToString() ?? string.Empty),
            IsVisible = string.Equals(row[17].ToString() ?? string.Empty, "tak", StringComparison.OrdinalIgnoreCase), // "tak" or "nie"
            IsAssigned = string.Equals(row[18].ToString() ?? string.Empty, "tak", StringComparison.OrdinalIgnoreCase), // "tak" or "nie"
            
            // optional cells
            AssignedTo = GetCellOrEmptyString(row, 19),
            AssignedToCompanyName = GetCellOrEmptyString(row, 20),
            AssignedToEmail = GetCellOrEmptyString(row, 21),
            AssignedToPhone = GetCellOrEmptyString(row, 22),
            AssignedToInfo = GetCellOrEmptyString(row, 23),
            Uwagi = GetCellOrEmptyString(row, 24),
            Status = string.IsNullOrWhiteSpace(GetCellOrEmptyString(row, 25)) ? LetterStatus.NIE_WIADOMO : Enum.Parse<LetterStatus>(GetCellOrEmptyString(row, 25), true)
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
        var data = Convert.FromBase64String(_googleSheetsCredentials);
        var credentialsJson = System.Text.Encoding.UTF8.GetString(data);
        
        return GoogleCredential.FromJson(credentialsJson).CreateScoped(SheetsService.Scope.Spreadsheets);
    }

    public async Task AssignLetterAsync(LetterAssignment assignment)
    {
        var range = $"{_sheetName}!R{assignment.RowNumber}:W{assignment.RowNumber}";

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

    internal async Task UpdateImageIds(Letter letter)
    {
        var range = $"{_sheetName}!O{letter.RowNumber}:O{letter.RowNumber}";

        var valuesToUpdate = new List<object> { string.Join(",", letter.ImageIds) };

        var valueRange = new ValueRange
        {
            Values = new List<IList<object>> { valuesToUpdate }
        };
        var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
        updateRequest.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;
        await updateRequest.ExecuteAsync();
    }

    public async Task UpdateStatus(int rowNumber, LetterStatus status, string uwagi)
    {
        var range = $"{_sheetName}!X{rowNumber}:Y{rowNumber}";

        var valuesToUpdate = new List<object>
        {
            uwagi,
            status.ToString()
        };
        var valueRange = new ValueRange
        {
            Values = new List<IList<object>> { valuesToUpdate }
        };
        var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
        updateRequest.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;
        await updateRequest.ExecuteAsync();
    }

    public async Task UpdateLetterDetailsWithChild(int rowNumber, string childName, int childAge, string description, string presents)
    {
        var range = $"{_sheetName}!L{rowNumber}:O{rowNumber}";

        var valuesToUpdate = new List<object>
        {
            childName,
            childAge,
            description,
            presents
        };
        var valueRange = new ValueRange
        {
            Values = new List<IList<object>> { valuesToUpdate }
        };
        var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
        updateRequest.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;
        await updateRequest.ExecuteAsync();
    }

    public async Task CreatePackages(string letterNumber, IEnumerable<Gabaryt> sizes)
    {
        var packages = await FetchPackages();
        var firstEmptyRow = packages.Count + 2;

        var rowNumber = firstEmptyRow;
        foreach (var size in sizes)
        {
            var range = $"{_packageSheetName}!A{rowNumber}:D{rowNumber}";

            var valuesToUpdate = new List<object>
            {
                letterNumber,
                rowNumber - firstEmptyRow + 1,
                sizes.Count(),
                size.ToString()
            };
            var valueRange = new ValueRange
            {
                Values = new List<IList<object>> { valuesToUpdate }
            };
            var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
            updateRequest.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;
            await updateRequest.ExecuteAsync();

            rowNumber++;
        }
    }

    public async Task<List<Package>> FetchPackages()
    {
        var range = $"{_packageSheetName}!A:D";
        var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
        var response = await request.ExecuteAsync();
        return MapToPackages(response.Values);
    }

    private List<Package> MapToPackages(IList<IList<object>> values)
    {
        var packages = new List<Package>();
        var rowNumber = 2;
        foreach (var row in values.Skip(1))
        {
            if (row.Count < 4)
                continue;

            var package = new Package
            {
                RowNumber = rowNumber,
                LetterNumber = row[0].ToString() ?? string.Empty,
                PackageNumber = string.IsNullOrWhiteSpace(row[1].ToString()) ? 0 : int.Parse(row[1].ToString()!),
                TotalPackages = string.IsNullOrWhiteSpace(row[2].ToString()) ? 0 : int.Parse(row[2].ToString()!),
                Size = string.IsNullOrWhiteSpace(row[3].ToString()) ? Gabaryt.A : Enum.Parse<Gabaryt>(row[3].ToString()!, true)
            };

            rowNumber++;
            packages.Add(package);
        }
        return packages;
    }
}