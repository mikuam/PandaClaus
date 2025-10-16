using Azure;
using Azure.Communication.Email;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PandaClaus.Web;

public class EmailSender
{
    private const string PageUrl = "https://pandaclaus.pl/";
    private const string EmailFrom = "DoNotReply@\r\n3e425720-d311-4859-9dbd-725f2a71aad6.azurecomm.net";

    private readonly EmailClient _emailClient;
    private readonly GoogleSheetsClient _googleSheetsClient;

    public EmailSender(IConfiguration configuration, GoogleSheetsClient googleSheetsClient)
    {
        _googleSheetsClient = googleSheetsClient;
        _emailClient = new EmailClient(configuration["EmailConnectionString"]!);
        
        // Set QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task SendConfirmationEmail(int rowNumber)
    {
        var letter = await _googleSheetsClient.FetchLetterAsync(rowNumber);

        var subject = "Potwierdzenie rezerwacji listu oraz ważne informacje";
        var plainTextContent = $@"Drodzy PANDAstyczni Darczyńcy!
Bardzo dziękujemy za Wasze zaangażowanie w akcję Panda Claus 
Gratulujemy {letter.AssignedTo}! Potwierdzamy realizowanie wybranego przez Ciebie listu nr {letter.Number}. List znajdziesz pod tym adresem: {GetLetterUrl(letter)}
Jak wiecie, w tym roku mieliśmy rekordową liczbę zgłoszonych listów — dokładnie 240! Akcja z roku na rok rośnie i jest coraz większym wyzwaniem logistycznym. W związku z tym przypominamy najważniejsze zasady i dziękujemy Wam za wyrozumiałość.
Paczki muszą być oznaczone numerami listów — to BARDZO ważne i pozwoli nam uniknąć pomyłek.
Proszę o wysyłanie skompletowanych paczek. Tylko w taki sposób jesteśmy w stanie uniknąć pomyłek. Bardzo złym pomysłem jest zamawianie zabawek bezpośrednio ze sklepu do nas i dosyłanie osobno innych upominków. To prowadzi do błędów i innych trudności organizacyjnych. Takie paczki nie będą niestety przyjmowane.
Jeżeli realizujesz kilka listów, paczkę dla każdego dziecka spakuj osobno i umieść wszystkie możliwie w jednym kartonie.
Do listu możesz dołączyć upominek dla rodziców dziecka, list lub kartkę oraz słodycze itd.
W tym roku naszym Partnerem jest InPost, dlatego paczki muszą mieć rozmiary zgodne z wymaganiami Paczkomatów InPost.
Paczkę wyślij Paczkomatem InPost pod numer POZ82M.
(jeżeli paczka nie zmieści się do Paczkomatu Inpost — daj nam znać na wyżej podany adres mailowy. Prosimy, nie nadawaj paczki kurierem bez informowania nas o tym)
Nadając przesyłkę podaj e-mail pandaclaus@pandateam.pl oraz numer telefonu +48 534 897 105
Jeżeli chcesz dostarczyć paczkę osobiście:
Przywieź paczkę do naszego magazynu na ul. Głogowskiej 16 w Poznaniu (naprzeciwko Dworca Zachodniego, hala numer 2 MTP) w dniach:
- 5-6 grudnia od 17:30 do 20:30
- 7 grudnia od 10:30 do 14:30
Z góry BARDZO dziękujemy za dostosowanie się do naszych szczegółowych wytycznych. Pomoże nam to sprawnie przeprowadzić akcję i dostarczyć prezenty do naszych podopiecznych :)
Już teraz zapraszamy Was do zgłaszania się do wolontariatu podczas finału, który odbędzie się 13 i 14 grudnia (pt-sob) w hali nr 10 Międzynarodowych Targów Poznańskich. Chęć pomocy można zgłosić poprzez formularz dostępny na stronie https://forms.gle/xn8UMCst3uSTwoeV9 (wolontariaty pracownicze proszę zgłaszać e-mailowo). Będzie nam miło, jak pomożecie nam w finale! :) 
Z całym #pandateam życzymy wszystkiego PANDAstycznego!
";

        var receiveConfirmationUrl = CreateReceiveConfirmationUrl(letter);
        var qrCodeBase64 = GenerateQRCodeBase64(receiveConfirmationUrl);

        var htmlContent = $@"<h1>Drodzy PANDAstyczni Darczyńcy!</h1>
<p>Bardzo dziękujemy za Wasze zaangażowanie w akcję Panda Claus.</p>
<p>Gratulujemy {letter.AssignedTo}! Potwierdzamy realizowanie wybranego przez Ciebie listu nr {letter.Number}. List znajdziesz pod tym adresem: <a href=\""{GetLetterUrl(letter)}\"">{GetLetterUrl(letter)}</a></p>
<p>Jak wiecie, w tym roku mieliśmy rekordową liczbę zgłoszonych listów — dokładnie 240! Akcja z roku na rok rośnie i jest coraz większym wyzwaniem logistycznym. W związku z tym przypominamy najważniejsze zasady i dziękujemy Wam za wyrozumiałość:</p>
<p>Paczki muszą być oznaczone numerami listów — to BARDZO ważne i pozwoli nam uniknąć pomyłek.</p>
<p><b>Proszę o wysyłanie skompletowanych paczek. Tylko w taki sposób jesteśmy w stanie uniknąć pomyłek.</b> Bardzo złym pomysłem jest zamawianie zabawek bezpośrednio ze sklepu do nas i dosyłanie osobno innych upominków. To prowadzi do błędów i innych trudności organizacyjnych. Takie paczki nie będą niestety przyjmowane.</p>
<p>Jeżeli realizujesz kilka listów, paczkę dla każdego dziecka spakuj osobno i umieść wszystkie możliwie w jednym kartonie.</p>
<p>Do listu możesz dołączyć upominek dla rodziców dziecka, list lub kartkę oraz słodycze itd.</p>
<p>W tym roku naszym Partnerem jest InPost, dlatego paczki muszą mieć rozmiary zgodne z wymaganiami <b>Paczkomatów InPost</b>.</p>
<p><b>Paczkę wyślij Paczkomatem InPost</b> pod numer <b>POZ82M</b>. (jeżeli paczka nie zmieści się do Paczkomatu Inpost — daj nam znać na wyżej podany adres mailowy. Prosimy, <p>nie nadawaj</p> paczki kurierem bez informowania nas o tym)</p>
<p>Nadając przesyłkę, podaj e-mail: <a href=""mailto:pandaclaus@pandateam.pl"">pandaclaus@pandateam.pl</a> oraz numer telefonu: +48 534 897 105.</p>
<h2>Jeżeli chcesz dostarczyć paczkę osobiście:</h2>
<p>Przywieź paczkę do naszego magazynu na ul. Głogowskiej 16 w Poznaniu (naprzeciwko Dworca Zachodniego, hala numer 2 MTP) w dniach:</p>
<ul>
    <li>5-6 grudnia od 17:30 do 20:30</li>
    <li>7 grudnia od 10:30 do 14:30</li>
</ul>
<p>Z góry BARDZO dziękujemy za dostosowanie się do naszych szczegółowych wytycznych. Pomoże nam to sprawnie przeprowadzić akcję i dostarczyć prezenty do naszych podopiecznych :)</p>
<p>Już teraz zapraszamy Was do zgłaszania się do wolontariatu podczas finału, który odbędzie się 13 i 14 grudnia (pt-sob) w hali nr 10 Międzynarodowych Targów Poznańskich. Chęć pomocy można zgłosić poprzez formularz dostępny na stronie <a href=""https://forms.gle/xn8UMCst3uSTwoeV9"" target=""_blank"">https://forms.gle/xn8UMCst3uSTwoeV9</a> (wolontariaty pracownicze proszę zgłaszać e-mailowo). Będzie nam miło, jak pomożecie nam w finale! :) </p>
<p>Z całym #pandateam życzymy wszystkiego PANDAstycznego!</p>
<hr/>
<h2>Potwierdzenie odbioru paczki</h2>
<p><b>Aby potwierdzić odbiór paczki, zeskanuj poniższy kod QR lub kliknij w link:</b></p>
<p style=""text-align: center;"">
    <a href=\""{receiveConfirmationUrl}\""><img src=""data:image/png;base64,{qrCodeBase64}"" alt=""QR Code - Potwierdzenie odbioru paczki"" style=""max-width: 300px;"" /></a>
</p>
<p><a href=\""{receiveConfirmationUrl}\"">{receiveConfirmationUrl}</a></p>
<p><b>Kod QR do wydruku</b> znajduje się również w załączonym pliku PDF.</p>";

        var email = new EmailMessage(
            EmailFrom,
            letter.AssignedToEmail,
            new EmailContent(subject) { PlainText = plainTextContent, Html = htmlContent });
        email.Headers.Add("Message-ID", "<do-not-reply@example.com>");

        // Generate PDF with QR code and attach it
        var pdfBytes = GenerateQRCodePdf(receiveConfirmationUrl, letter.Number);
        var pdfAttachment = new EmailAttachment($"PandaClaus_QRCode_List_{letter.Number}.pdf", "application/pdf", new BinaryData(pdfBytes));
        email.Attachments.Add(pdfAttachment);

        // run in a background thread, don't wait for it to finish
        _ = Task.Run(() => _emailClient.Send(WaitUntil.Completed, email));
    }

    private string GenerateQRCodeBase64(string url)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(20);
        return Convert.ToBase64String(qrCodeBytes);
    }

    private byte[] GenerateQRCodePdf(string url, string letterNumber)
    {
        var qrCodeBytes = GenerateQRCodeBytes(url);
        
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(14));

                page.Content()
                    .Column(column =>
                    {
                        column.Spacing(20);

                        column.Item().AlignCenter().Text("Panda Claus 2025")
                            .FontSize(28).Bold().FontColor(Colors.Blue.Medium);

                        column.Item().AlignCenter().Text("Potwierdzenie dostarczenia paczki")
                            .FontSize(20).SemiBold();

                        column.Item().AlignCenter().Text($"List numer: {letterNumber}")
                            .FontSize(18).Bold();

                        column.Item().PaddingTop(20).AlignCenter()
                            .Image(qrCodeBytes)
                            .FitWidth();

                        column.Item().PaddingTop(20).AlignCenter().Text("Zeskanuj kod QR, aby potwierdzić dostarczenie paczki")
                            .FontSize(16);

                        column.Item().PaddingTop(10).AlignCenter().Text(url)
                            .FontSize(10).Italic();

                        column.Item().PaddingTop(30).Text("Dziękujemy za wsparcie akcji Panda Claus!")
                            .FontSize(14).SemiBold().AlignCenter();
                    });
            });
        });

        return document.GeneratePdf();
    }

    private byte[] GenerateQRCodeBytes(string url)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }

    private string GetLetterUrl(Letter letter)
    {
        return PageUrl + $"Letter?rowNumber={letter.RowNumber}";
    }

    public async Task SendLetterAdded(int rowNumber)
    {
        var letter = await _googleSheetsClient.FetchLetterAsync(rowNumber);

        var subject = "Panda Claus - potwierdzenie dodania listu";

        var plainTextContent = $"Cześć {letter.ParentName}!\n\n" +
                               $"Potwierdzamy dodanie przez Ciebie listu dla: {letter.ChildName}\n\n" +
                               $"List wymaga jeszcze weryfikacji przez naszych wolontariuszy. Jeżeli wszystko będzie w porządku, list zostanie opublikowany na naszej stronie internetowej." +
                               $"Wszelkie pytania prosimy kierować na adres e - mail pandaclaus@pandateam.pl.\n\n" +
                               $"Pozdrawiamy,\n" +
                               $"Zespół Panda Team";

        var email = new EmailMessage(
            EmailFrom,
            letter.Email,
            new EmailContent(subject) { PlainText = plainTextContent });
        email.Headers.Add("Message-ID", "<do-not-reply@example.com>");

        _ = Task.Run(() => _emailClient.Send(WaitUntil.Completed, email));
    }

    public async Task SendPackageReceived(int rowNumber)
    {
        var letter = await _googleSheetsClient.FetchLetterAsync(rowNumber);

        var subject = "Panda Claus - potwierdzenie dostarczenia paczek dla listu";

        var plainTextContent = $"Cześć {letter.AssignedTo}!\n\n" +
                               $"Potwierdzamy dostarczenie przez Ciebie paczek do listu numer {letter.Number} dla: {letter.ChildName}\n\n" +
                               $"Paczka znajduje się teraz w naszym magazynie i podczas finału akcji zostanie przygotowana do wysłania do potrzebującego dziecka." +
                               $"Wszelkie pytania prosimy kierować na adres e - mail pandaclaus@pandateam.pl.\n\n" +
                               $"Pozdrawiamy,\n" +
                               $"Zespół Panda Team";

        var email = new EmailMessage(
            EmailFrom,
            letter.AssignedToEmail,
            new EmailContent(subject) { PlainText = plainTextContent });
        email.Headers.Add("Message-ID", "<do-not-reply@example.com>");
    }

    private string CreateReceiveConfirmationUrl(Letter letter)
    {
        return PageUrl + $"ReceiveConfirmation?rowNumber={letter.RowNumber}&code={letter.Number}";
    }
}
