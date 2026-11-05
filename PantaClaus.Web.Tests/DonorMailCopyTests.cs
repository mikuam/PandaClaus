namespace PandaClaus.Web.Tests;

[TestClass]
public class DonorMailCopyTests
{
    private static string Source()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "PandaClaus.Web", "Core", "EmailSender.cs")))
            directory = directory.Parent;
        return File.ReadAllText(Path.Combine(directory!.FullName, "PandaClaus.Web", "Core", "EmailSender.cs"));
    }

    [TestMethod]
    public void ApprovedDeadlineAddressAndFinaleAppearInBothMailFormats()
    {
        var source = Source();
        Assert.AreEqual(2, source.Split("27.11.2026", StringSplitOptions.None).Length - 1);
        Assert.AreEqual(2, source.Split("Biuro Spectre Cars", StringSplitOptions.None).Length - 1);
        Assert.IsFalse(source.Contains("Siedziba Spectre Cars", StringComparison.Ordinal));
        Assert.AreEqual(2, source.Split("3 i 4 grudnia 2026 roku", StringSplitOptions.None).Length - 1);
        Assert.AreEqual(2, source.Split("około 200 listów", StringSplitOptions.None).Length - 1);
        Assert.IsFalse(source.Contains("ul. Opłotki 23", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("Jeżeli chcesz dostarczyć paczkę osobiście", StringComparison.Ordinal));
    }

    [TestMethod]
    public void LetterNumberAndLinkStayDynamicInBothFormats()
    {
        var source = Source();
        Assert.IsTrue(source.Split("{letter.Number}", StringSplitOptions.None).Length - 1 >= 2);
        Assert.IsTrue(source.Split("{GetLetterUrl(letter)}", StringSplitOptions.None).Length - 1 >= 2);
        Assert.IsTrue(source.Split("{voucherContent.Plain}", StringSplitOptions.None).Length - 1 >= 1);
        Assert.IsTrue(source.Split("{voucherContent.Html}", StringSplitOptions.None).Length - 1 >= 1);
    }

    [TestMethod]
    public void DonorConfirmationAwaitsProviderInsteadOfLaunchingBackgroundTask()
    {
        var source = Source();
        var start = source.IndexOf("public async Task SendConfirmationEmail", StringComparison.Ordinal);
        var end = source.IndexOf("private byte[] GenerateQRCodePdf", start, StringComparison.Ordinal);
        var confirmation = source[start..end];
        StringAssert.Contains(confirmation, "await SendCheckedAsync(email)");
        Assert.IsFalse(confirmation.Contains("Task.Run", StringComparison.Ordinal));
    }

    [TestMethod]
    public void OtherEmailPathsAlsoAwaitProviderFailures()
    {
        var source = Source();
        Assert.IsFalse(source.Contains("Task.Run(() => _emailClient.Send", StringComparison.Ordinal));
        Assert.AreEqual(3, source.Split("await SendCheckedAsync(email)", StringSplitOptions.None).Length - 1);
    }

    [TestMethod]
    public void DonorMailChecksCompletedProviderStatusBeforeReturning()
    {
        var source = Source();
        StringAssert.Contains(source, "operation.Value.Status != EmailSendStatus.Succeeded");
        StringAssert.Contains(source, "throw new InvalidOperationException(\"Email provider did not accept delivery.\")");
    }

    [TestMethod]
    public void ReservationSeparatesVoucherAndLetterWriteForSupport()
    {
        var source = File.ReadAllText(Path.Combine(SourceRoot(), "PandaClaus.Web", "Pages", "Letter.cshtml.cs"));
        var reservation = source[source.IndexOf("public async Task<IActionResult> OnPostAsync(string hash)", StringComparison.Ordinal)..];
        Assert.IsTrue(reservation.IndexOf("await _vouchers.GetOrAllocateAsync(letter.Hash)", StringComparison.Ordinal)
            < reservation.IndexOf("await _client.AssignLetterAsync(", StringComparison.Ordinal));
        StringAssert.Contains(reservation, "Voucher assigned but letter reservation write failed");
    }

    [TestMethod]
    public void NewsletterFailureCannotPreventConfirmation()
    {
        var source = File.ReadAllText(Path.Combine(SourceRoot(), "PandaClaus.Web", "Pages", "Letter.cshtml.cs"));
        var reservation = source[source.IndexOf("public async Task<IActionResult> OnPostAsync(string hash)", StringComparison.Ordinal)..];
        var confirmation = reservation.IndexOf("await _emailSender.SendConfirmationEmail(letter.Hash)", StringComparison.Ordinal);
        var newsletter = reservation.IndexOf("await _getResponseClient.SubscribeAsync(", StringComparison.Ordinal);
        Assert.IsTrue(confirmation >= 0 && newsletter > confirmation);
        StringAssert.Contains(reservation, "Newsletter subscription failed after reservation confirmation");
    }

    private static string SourceRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "PandaClaus.Web", "Pages", "Letter.cshtml.cs")))
            directory = directory.Parent;
        return directory!.FullName;
    }
}
