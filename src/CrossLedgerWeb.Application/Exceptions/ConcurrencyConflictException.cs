namespace CrossLedgerWeb.Application.Exceptions;

/// <summary>Two requests raced to modify the same aggregate (specification 2.4's
/// RowVersion optimistic concurrency). IUnitOfWork's implementation throws this instead
/// of leaking its own ORM's concurrency exception type - Application has no reference to
/// EF Core and cannot catch Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException
/// directly, so Infrastructure translates it into this at the one seam Application
/// actually depends on.</summary>
public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(Exception innerException)
        : base("The record was modified by another request before this change could be saved.", innerException)
    {
    }
}
