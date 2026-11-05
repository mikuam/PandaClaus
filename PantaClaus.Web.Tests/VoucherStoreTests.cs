using PandaClaus.Web.Core;

namespace PandaClaus.Web.Tests;

[TestClass]
public class VoucherStoreTests
{
    private sealed class FakeSheet : IVoucherSheet
    {
        public List<(string Number, string Used)> Rows { get; } = new();
        public int Writes { get; private set; }
        public bool IgnoreWrites { get; set; }
        public bool ConflictingWriter { get; set; }
        public bool ThrowAfterWrite { get; set; }
        public bool ThrowBeforeWrite { get; set; }
        public Task<IReadOnlyList<VoucherRow>> ReadAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<VoucherRow>>(Rows.Select((x, i) => new VoucherRow(i + 2, x.Number, x.Used)).ToList());
        public Task WriteUsedAsync(int row, string letter, CancellationToken ct = default)
        {
            Writes++;
            if (ThrowBeforeWrite) throw new InvalidOperationException("synthetic transport failure");
            if (ConflictingWriter) Rows[row - 2] = (Rows[row - 2].Number, "other-letter");
            else if (!IgnoreWrites) Rows[row - 2] = (Rows[row - 2].Number, letter);
            if (ThrowAfterWrite) throw new InvalidOperationException("synthetic lost acknowledgement");
            return Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task FirstUnusedRowAssignedAndResendReusesIt()
    {
        var sheet = new FakeSheet();
        sheet.Rows.AddRange([("SYNTH-A", "letter-old"), ("SYNTH-B", ""), ("SYNTH-C", "")]);
        var store = new VoucherStore(sheet);
        Assert.AreEqual("SYNTH-B", await store.GetOrAllocateAsync("letter-new"));
        Assert.AreEqual("SYNTH-B", await new VoucherStore(sheet).GetOrAllocateAsync("letter-new"));
        Assert.AreEqual("SYNTH-C", await store.GetOrAllocateAsync("letter-next"));
        Assert.AreEqual(2, sheet.Writes);
    }

    [TestMethod]
    public async Task ReadBackFailureDoesNotReturnCode()
    {
        var sheet = new FakeSheet { IgnoreWrites = true };
        sheet.Rows.Add(("SYNTH-A", ""));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => new VoucherStore(sheet).GetOrAllocateAsync("letter-new"));
    }

    [TestMethod]
    public async Task InterveningWriterDetectedBeforeMail()
    {
        var sheet = new FakeSheet { ConflictingWriter = true };
        sheet.Rows.Add(("SYNTH-A", ""));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => new VoucherStore(sheet).GetOrAllocateAsync("letter-new"));
    }

    [TestMethod]
    public async Task DuplicateCodesAndAssignmentsFailClosed()
    {
        var sheet = new FakeSheet();
        sheet.Rows.AddRange([("SYNTH-A", ""), ("SYNTH-A", "")]);
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => new VoucherStore(sheet).GetOrAllocateAsync("letter-new"));
        sheet.Rows.Clear();
        sheet.Rows.AddRange([("SYNTH-A", "letter-old"), ("SYNTH-B", "letter-old")]);
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => new VoucherStore(sheet).GetOrAllocateAsync("letter-old"));
    }

    [TestMethod]
    public async Task ExhaustionFailsWithoutWrite()
    {
        var sheet = new FakeSheet();
        sheet.Rows.Add(("SYNTH-A", "letter-old"));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => new VoucherStore(sheet).GetOrAllocateAsync("letter-new"));
        Assert.AreEqual(0, sheet.Writes);
    }

    [TestMethod]
    public async Task ParallelRequestsWithinOneStoreSerialize()
    {
        var sheet = new FakeSheet();
        sheet.Rows.AddRange(Enumerable.Range(1, 20).Select(i => ($"SYNTH-{i}", "")));
        var store = new VoucherStore(sheet);
        var results = await Task.WhenAll(Enumerable.Range(1, 20).Select(i => store.GetOrAllocateAsync($"letter-{i}")));
        Assert.AreEqual(20, results.Distinct().Count());
        Assert.AreEqual(20, sheet.Writes);
    }

    [TestMethod]
    public async Task LostAcknowledgementRereadsAssignmentWithoutSecondWrite()
    {
        var sheet = new FakeSheet { ThrowAfterWrite = true };
        sheet.Rows.Add(("SYNTH-A", ""));
        Assert.AreEqual("SYNTH-A", await new VoucherStore(sheet).GetOrAllocateAsync("letter-new"));
        Assert.AreEqual(1, sheet.Writes);
    }

    [TestMethod]
    public async Task FailedWriteWithoutAssignmentReportsStageButNeverCode()
    {
        var sheet = new FakeSheet { ThrowBeforeWrite = true };
        sheet.Rows.Add(("SYNTH-A", ""));
        var error = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => new VoucherStore(sheet).GetOrAllocateAsync("letter-new"));
        StringAssert.Contains(error.Message, "write");
        Assert.IsFalse(error.ToString().Contains("SYNTH-A", StringComparison.Ordinal));
        Assert.AreEqual(1, sheet.Writes);
    }

    [TestMethod]
    public async Task UnverifiedWriteReportsStageButNeverCode()
    {
        var sheet = new FakeSheet { IgnoreWrites = true };
        sheet.Rows.Add(("SYNTH-A", ""));
        var error = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => new VoucherStore(sheet).GetOrAllocateAsync("letter-new"));
        StringAssert.Contains(error.Message, "verification");
        Assert.IsFalse(error.ToString().Contains("SYNTH-A", StringComparison.Ordinal));
    }
}
