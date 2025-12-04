namespace TAABP.Application.Common.Errors;

public static class ValidationErrors
{
    public static Error InvalidInput(string description) =>
        new("Validation.InvalidInput", description);

    public static Error Required(string fieldName) =>
        new("Validation.Required", $"'{fieldName}' is required.");

    public static Error InvalidFormat(string fieldName) =>
        new("Validation.InvalidFormat", $"'{fieldName}' has an invalid format.");
}
