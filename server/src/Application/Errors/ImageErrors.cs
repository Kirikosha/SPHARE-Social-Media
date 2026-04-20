using Application.Core;

namespace Application.Errors;

public static class ImageErrors
{
    public static Error ImageIsAlreadyDeleted() => new Error("Image is alreayd deleted", 400);
}