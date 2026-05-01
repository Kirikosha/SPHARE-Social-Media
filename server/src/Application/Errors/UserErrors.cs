using Application.Core;

namespace Application.Errors;

public static class UserErrors
{
    public static Error UserNotFound() => new("User was not found", 404);
    public static Error UserMainInfoUpdateFailed() => new("User update of main information was unsuccessful. Please " +
                                                          "try again later", 500);
    public static Error ErrorDuringUserReceiving() => new("User fetching was unsuccessful", 500);
}