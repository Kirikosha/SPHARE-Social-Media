namespace Application.Services.PhotoService;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;
    public PhotoService(IOptions<CloudinarySettings> config)
    {
        var acc = new Account(config.Value.CloudName, config.Value.ApiKey, config.Value.ApiSecret);
        _cloudinary = new Cloudinary(acc);
    }

    public async Task<ImageUploadResult> AddPhotoAsync(IFormFile image)
    {
        var uploadResult = new ImageUploadResult();

        if (image.Length > 0)
        {
            using var stream = image.OpenReadStream();
            var uploadParmas = new ImageUploadParams
            {
                File = new FileDescription(image.FileName, stream)
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParmas);
        }
        return uploadResult;
    }

    public async Task<ImageUploadResult> AddProfilePhotoAsync(IFormFile image)
    {
        var uploadResult = new ImageUploadResult();

        if (image.Length > 0)
        {
            using var stream = image.OpenReadStream();
            var uploadParmas = new ImageUploadParams
            {
                File = new FileDescription(image.FileName, stream),
                Transformation = new Transformation().Height(500).Width(500)
                .Crop("fill").Gravity("face")
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParmas);
        }
        return uploadResult;
    }

    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        return await _cloudinary.DestroyAsync(deleteParams);
    }
}
