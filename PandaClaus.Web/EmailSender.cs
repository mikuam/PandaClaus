using Azure;
using Azure.Communication.Email;

namespace PandaClaus.Web;

public class EmailSender
{
    private const string PageUrl = "https://pandaclaus.pandateam.pl/";
    //private const string EmailFrom = "DoNotReply@pandaclausmail.pandateam.pl";
    private const string EmailFrom = "DoNotReply@\r\n3e425720-d311-4859-9dbd-725f2a71aad6.azurecomm.net";

    private readonly EmailClient _emailClient;
    private readonly GoogleSheetsClient _googleSheetsClient;

    public EmailSender(IConfiguration configuration, GoogleSheetsClient googleSheetsClient)
    {
        _googleSheetsClient = googleSheetsClient;
        _emailClient = new EmailClient(configuration["EmailConnectionString"]!);
    }

    public async Task SendConfirmationEmail(int rowNumber)
    {
        var letter = await _googleSheetsClient.FetchLetterAsync(rowNumber);

        var subject = "Panda Claus - potwierdzenie przypisania listu";
        var plainTextContent = $"Cześć {letter.AssignedTo}!\n\n" +
                               $"Potwierdzamy przypisanie listu do Ciebie. " +
                               $"Oto link do listu: {GetLetterUrl(letter)} " +
                               $"W razie pytań prosimy o kontakt na adres e-mail pandaclaus@pandateam.pl.\n\n";

        await _emailClient.SendAsync(
            WaitUntil.Completed,
            EmailFrom,
            letter.AssignedToEmail,
            subject,
            string.Empty,
            plainTextContent);
    }

    private string GetLetterUrl(Letter letter)
    {
        return PageUrl + $"Letter/?rowNumber={letter.RowNumber}";
    }

    public async Task SendLetterAdded(int rowNumber)
    {
        var letter = await _googleSheetsClient.FetchLetterAsync(rowNumber);

        var subject = "Panda Claus - potwierdzenie dodania listu";

        var plainTextContent = $"Cześć {letter.ParentName}!\n\n" +
                               $"Potwierdzamy dodanie przez Ciebie listu dla: {letter.ChildAge}\n\n" +
                               $"List wymaga jeszcze weryfikacji przez naszych wolontariuszy. Jeżeli wszystko będzie w porządku, list zostanie opublikowany na naszej stronie internetowej." +
                               $"Wszelkie pytania prosimy kierować na adres e - mail pandaclaus @pandateam.pl.\n\n";

        await _emailClient.SendAsync(
            WaitUntil.Completed,
            EmailFrom,
            letter.Email,
            subject,
            string.Empty,
            plainTextContent);
    }
}
