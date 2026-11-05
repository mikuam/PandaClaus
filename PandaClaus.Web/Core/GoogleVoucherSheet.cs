using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace PandaClaus.Web.Core;

// Separate voucher spreadsheet, never the letter/family spreadsheet.
public sealed class GoogleVoucherSheet : IVoucherSheet
{
    private readonly SheetsService _service;
    private readonly string _id;
    private readonly string _tab;

    public GoogleVoucherSheet(IConfiguration config)
    {
        _id = config["InPostVoucherSpreadsheetId"] ?? "";
        _tab = config["InPostVoucherSheetName"] ?? "";
        if (string.IsNullOrWhiteSpace(_id) || _id == config["SpreadsheetId"] ||
            string.IsNullOrWhiteSpace(_tab) || _tab.Contains('!') || _tab.Contains('\''))
            throw new InvalidOperationException("A distinct voucher spreadsheet ID and safe tab name are required.");
        var encoded = config["GoogleSheetsCredentials"];
        if (string.IsNullOrWhiteSpace(encoded)) throw new InvalidOperationException("Voucher Sheets credentials missing.");
        var credential = GoogleCredential.FromJson(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encoded)))
            .CreateScoped(SheetsService.Scope.Spreadsheets);
        _service = new SheetsService(new BaseClientService.Initializer { HttpClientInitializer = credential, ApplicationName = "Panda Claus vouchers" });
    }

    private string Range(string cells) => $"'{_tab}'!{cells}";

    public async Task<IReadOnlyList<VoucherRow>> ReadAsync(CancellationToken ct = default)
    {
        var request = _service.Spreadsheets.Values.Get(_id, Range("A:B"));
        request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMATTEDVALUE;
        var values = (await request.ExecuteAsync(ct)).Values ?? throw new InvalidOperationException("Voucher sheet is empty.");
        if (values.Count == 0 || values[0].Count < 2 || values[0][0]?.ToString() != "Numer" || values[0][1]?.ToString() != "Used")
            throw new InvalidOperationException("Voucher sheet headers must be Numer and Used.");
        // The approved production inventory has exactly 250 rows. Do not silently allocate
        // from a truncated or extended sheet (values.get omits trailing empty rows).
        if (values.Count != 251)
            throw new InvalidOperationException("Voucher sheet inventory must contain 250 codes.");
        return values.Skip(1).Select((row, index) => new VoucherRow(index + 2,
            row.Count > 0 ? row[0]?.ToString() ?? "" : "",
            row.Count > 1 ? row[1]?.ToString() ?? "" : "")).ToList();
    }

    public async Task WriteUsedAsync(int row, string letterHash, CancellationToken ct = default)
    {
        var range = Range($"B{row}");
        var body = new ValueRange { Values = new List<IList<object>> { new List<object> { letterHash } } };
        var request = _service.Spreadsheets.Values.Update(body, _id, range);
        request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
        var result = await request.ExecuteAsync(ct);
        if (result.UpdatedCells != 1) throw new InvalidOperationException("Voucher sheet write was not acknowledged.");
    }
}
