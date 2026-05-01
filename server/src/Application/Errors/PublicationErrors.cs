using Application.Core;

namespace Application.Errors;

public static class PublicationErrors
{
    public static Error NotAuthorised() => new Error("You are not authorised for this action", 403);
    public static Error FetchingUnsuccessful() => new("Publication fetching was unsuccessful", 400);
    public static Error UpdateUnsuccessful() => new("Publication update was unsuccessful", 500);
}