using Application.Core;
using Application.DTOs.UserDTOs;
using Application.Interfaces.Services;
using FluentValidation;

namespace Application.Features.Users.Commands;

public class UpdateUserProfileImage
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string UserId { get; set; }
        public required UpdateUserProfileImageDto UpdateModel { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.UpdateModel.ProfileImage)
                .NotNull().WithMessage("Profile image is required.")
                .Must(file => file.Length > 0).WithMessage("The selected file is empty.")
                .Must(file => file.Length <= 5 * 1024 * 1024).WithMessage("File size cannot exceed 5MB.")
                .Must(file => IsValidImageExtension(file.FileName)).WithMessage("Invalid file format. Only .jpg, .jpeg, and .png are allowed.")
                .Must(file => IsValidContentType(file.ContentType)).WithMessage("Invalid file type.");
        }
        
        private bool IsValidImageExtension(string fileName)
        {
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(fileName).ToLower();
            return allowedExtensions.Contains(extension);
        }

        private bool IsValidContentType(string contentType)
        {
            string[] allowedContentTypes = { "image/jpeg", "image/png" };
            return allowedContentTypes.Contains(contentType.ToLower());
        }
    }
    
    public class Handler(IPhotoService photoService) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var result = await photoService.UploadUserProfilePicture(request.UpdateModel.ProfileImage, request
                .UserId, cancellationToken);

            return result;
        }
    }
}