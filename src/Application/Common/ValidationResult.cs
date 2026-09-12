namespace CleanMinimalApi.Application.Common;

public sealed record ValidationResult
{
    public bool IsValid { get; init; }
    public bool IsBusinessRule { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? EntityId { get; init; }

    public static ValidationResult Success(string message, string? entityId = null)
    {
        return new()
        {
            IsValid = true,
            IsBusinessRule = false,
            Message = message,
            EntityId = entityId
        };
    }

    public static ValidationResult Warning(string message, string? entityId = null)
    {
        return new()
        {
            IsValid = true,
            IsBusinessRule = true,
            Message = message,
            EntityId = entityId
        };
    }

    public static ValidationResult Failure(string message, string? entityId = null)
    {
        return new()
        {
            IsValid = false,
            IsBusinessRule = false,
            Message = message,
            EntityId = entityId
        };
    }
}
