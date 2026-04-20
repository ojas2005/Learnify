namespace Learnify.Core.Core;

// Wraps any domain operation outcome, forcing callers to explicitly handle
// success/failure paths rather than relying on exceptions.
// Keeps the call stack clean and makes error handling predictable.
public sealed class OperationResult<T>
{
    public bool Succeeded { get; private init; }
    public T? Payload { get; private init; }
    public string? FailureReason { get; private init; }
    public FailureKind Kind { get; private init; }

    private OperationResult() { }

    public static OperationResult<T> Ok(T payload)
    {
        return new OperationResult<T> { Succeeded = true, Payload = payload };
    }

    public static OperationResult<T> NotFound(string reason)
    {
        return new OperationResult<T> { Succeeded = false, FailureReason = reason, Kind = FailureKind.NotFound };
    }

    public static OperationResult<T> Conflict(string reason)
    {
        return new OperationResult<T> { Succeeded = false, FailureReason = reason, Kind = FailureKind.Conflict };
    }

    public static OperationResult<T> AccessDenied(string reason)
    {
        return new OperationResult<T> { Succeeded = false, FailureReason = reason, Kind = FailureKind.AccessDenied };
    }

    public static OperationResult<T> BusinessRuleViolation(string reason)
    {
        return new OperationResult<T> { Succeeded = false, FailureReason = reason, Kind = FailureKind.BusinessRuleViolation };
    }

// Maps the payload to a different type while preserving failure state.
    public OperationResult<TOut> Map<TOut>(Func<T, TOut> transform)
    {
        if (!Succeeded)
            return new OperationResult<TOut> { Succeeded = false, FailureReason = FailureReason, Kind = Kind };

        return OperationResult<TOut>.Ok(transform(Payload!));
    }
}

// Non-generic variant for operations that don't return a payload (void-style).
public sealed class OperationResult
{
    public bool Succeeded { get; private init; }
    public string? FailureReason { get; private init; }
    public FailureKind Kind { get; private init; }

    public static readonly OperationResult Done = new() { Succeeded = true };

    public static OperationResult NotFound(string reason)
    {
        return new OperationResult { Succeeded = false, FailureReason = reason, Kind = FailureKind.NotFound };
    }

    public static OperationResult Conflict(string reason)
    {
        return new OperationResult { Succeeded = false, FailureReason = reason, Kind = FailureKind.Conflict };
    }

    public static OperationResult AccessDenied(string reason)
    {
        return new OperationResult { Succeeded = false, FailureReason = reason, Kind = FailureKind.AccessDenied };
    }

    public static OperationResult BusinessRuleViolation(string reason)
    {
        return new OperationResult { Succeeded = false, FailureReason = reason, Kind = FailureKind.BusinessRuleViolation };
    }
}

public enum FailureKind
{
    NotFound,
    Conflict,
    AccessDenied,
    BusinessRuleViolation
}
