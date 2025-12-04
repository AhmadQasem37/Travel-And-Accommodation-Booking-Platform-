namespace TAABP.Application.Common.Errors;

public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        new("User.NotFound", $"User with ID '{id}' was not found.");

    public static Error NotFoundByEmail(string email) =>
        new("User.NotFoundByEmail", $"User with email '{email}' was not found.");

    public static Error InvalidCredentials =>
        new("User.InvalidCredentials", "Invalid email or password.");

    public static Error EmailAlreadyExists(string email) =>
        new("User.EmailAlreadyExists", $"Email '{email}' is already registered.");

    public static Error UsernameAlreadyExists(string username) =>
        new("User.UsernameAlreadyExists", $"Username '{username}' is already taken.");

    public static Error Unauthorized =>
        new("User.Unauthorized", "User is not authenticated.");
}
