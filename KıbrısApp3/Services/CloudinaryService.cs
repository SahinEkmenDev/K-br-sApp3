using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var account = new Account(
     configuration["Cloudinary:CloudName"],
     configuration["Cloudinary:ApiKey"],
     configuration["Cloudinary:ApiSecret"]);


        _cloudinary = new Cloudinary(account);
    }

    // ✅ IFormFile ile doğrudan yüklemek için
    public async Task<string> UploadImageAsync(IFormFile file)
    {
        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "kibrisapp-images" // klasör ismi sana özel
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        return uploadResult.SecureUrl.ToString(); // Cloudinary URL
    }

    // ✅ Controller'da kullanabilmen için ekstra metod
    public async Task<ImageUploadResult> UploadAsync(ImageUploadParams uploadParams)
    {
        return await _cloudinary.UploadAsync(uploadParams);
    }
}
