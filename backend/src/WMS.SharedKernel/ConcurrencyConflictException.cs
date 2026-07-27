namespace WMS.SharedKernel;

/// <summary>
/// Infrastructure-agnostic signal that an optimistic-concurrency check failed (e.g. Postgres xmin
/// mismatch). Infrastructure repositories catch the underlying ORM exception and rethrow this, so
/// Application-layer command handlers can react without depending on EF Core.
/// </summary>
public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
