using Application.Core;

namespace Application.Errors;

public static class ImageErrors
{
    public static Error ImageIsAlreadyDeleted() => new Error("Image is alreayd deleted", 400);
    public static Error ImageUploadUnsuccessful() => new Error("Image upload was unsuccessful", 500);
}