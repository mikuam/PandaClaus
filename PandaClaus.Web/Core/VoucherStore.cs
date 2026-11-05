namespace PandaClaus.Web.Core;

public sealed record VoucherRow(int Row, string Number, string Used);

public interface IVoucherSheet
{
    Task<IReadOnlyList<VoucherRow>> ReadAsync(CancellationToken ct = default);
    Task WriteUsedAsync(int row, string letterHash, CancellationToken ct = default);
}

// Google Sheets is the sole inventory and assignment store. The lock only protects one process;
// Sheets values.update has no compare-and-swap, so multiple processes/editors remain unsafe.
public sealed class VoucherStore(IVoucherSheet sheet)
{
    private static readonly SemaphoreSlim ProcessLock = new(1, 1);

    public async Task<string> GetOrAllocateAsync(string letterHash, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(letterHash)) throw new ArgumentException("Letter identity required.", nameof(letterHash));
        await ProcessLock.WaitAsync(ct);
        try
        {
            var rows = await sheet.ReadAsync(ct);
            if (rows.Count == 0 || rows.Any(r => r.Row < 2 || string.IsNullOrWhiteSpace(r.Number)) ||
                rows.Select(r => r.Number).Distinct(StringComparer.Ordinal).Count() != rows.Count ||
                rows.Where(r => !string.IsNullOrWhiteSpace(r.Used)).Select(r => r.Used).Distinct(StringComparer.Ordinal).Count() != rows.Count(r => !string.IsNullOrWhiteSpace(r.Used)))
                throw new InvalidOperationException("Voucher sheet inventory is invalid.");
            var existing = rows.FirstOrDefault(r => r.Used == letterHash);
            if (existing != null) return existing.Number;
            var free = rows.FirstOrDefault(r => string.IsNullOrWhiteSpace(r.Used));
            if (free == null) throw new InvalidOperationException("Voucher sheet inventory exhausted.");
            var writeFailed = false;
            try { await sheet.WriteUsedAsync(free.Row, letterHash, ct); }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception) { writeFailed = true; } // An acknowledgement failure may follow a committed write.
            // Re-read entire inventory to detect altered row, lost write, or duplicate assignment.
            IReadOnlyList<VoucherRow> after;
            try { after = await sheet.ReadAsync(ct); }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception) { throw new InvalidOperationException("Voucher write verification read failed; outcome unknown; do not send mail."); }
            if (after.Count(r => r.Used == letterHash) != 1 ||
                after.FirstOrDefault(r => r.Row == free.Row) is not { } actual ||
                actual.Number != free.Number || actual.Used != letterHash ||
                after.Count(r => r.Number == free.Number) != 1)
                throw new InvalidOperationException(writeFailed
                    ? "Voucher write failed or was not committed; do not send mail."
                    : "Voucher write verification failed; do not send mail.");
            return free.Number;
        }
        finally { ProcessLock.Release(); }
    }
}
