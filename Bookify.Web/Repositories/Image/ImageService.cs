using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ImageService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> SaveImageAsync(IFormFile file, string folderPath)
    {
        if (file == null || file.Length == 0)
        {
            return new BadRequestObjectResult("No file provided.");
        }

        const int maxFileSize = 5 * 1024 * 1024; // 5 MB
        if (file.Length > maxFileSize)
        {
            return new BadRequestObjectResult($"File size exceeds the limit of {maxFileSize / (1024 * 1024)} MB.");
        }

        var allowedFileTypes = new[] { "image/jpeg", "image/png", "image/gif" };
        if (!allowedFileTypes.Contains(file.ContentType))
        {
            return new BadRequestObjectResult($"Invalid file type. Allowed types are: {string.Join(", ", allowedFileTypes)}.");
        }

        try
        {
            // Generate a unique filename
            var extension = Path.GetExtension(file.FileName).ToLower();
            var imageName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, folderPath, imageName);

            // Ensure the directory exists
            var fullFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, folderPath);
            Directory.CreateDirectory(fullFolderPath); // Create the folder if it doesn't exist

            // Save the original file
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create thumbnail if specified
            string thumbnailRelativePath = null;
            var thumbnailPath = Path.Combine(fullFolderPath, "thumbs", imageName);
            var thumbnailDirectory = Path.GetDirectoryName(thumbnailPath);

            if (thumbnailDirectory != null && !Directory.Exists(thumbnailDirectory))
            {
                Directory.CreateDirectory(thumbnailDirectory);
            }

            // Resize the original image and save it as a thumbnail
            ImageHelper.ResizeImage(filePath, thumbnailPath, width: 150);

            thumbnailRelativePath = "/" + Path.Combine(folderPath, "thumbs", imageName).Replace("\\", "/");
            var relativePath = "/" + Path.Combine(folderPath, imageName).Replace("\\", "/");

            return new OkObjectResult(new
            {
                relativePath,
                thumbnailRelativePath
            });
        }
        catch (Exception ex)
        {
            // Log the exception if necessary (consider using a logging framework)
            Console.WriteLine(ex.Message); // For debugging purposes
            return new StatusCodeResult(500); // Internal Server Error
        }
    }

    public void DeleteOldImages(string imageUrl, string thumbnailUrl)
    {
        // Check if imageUrl and thumbnailUrl are both null or empty
        if (string.IsNullOrEmpty(imageUrl) && string.IsNullOrEmpty(thumbnailUrl))
        {
            return; // Nothing to delete
        }

        // Delete the old image
        if (!string.IsNullOrEmpty(imageUrl))
        {
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
        }

        // Delete the old thumbnail if hasThumbnail is true
        if (!string.IsNullOrEmpty(thumbnailUrl))
        {
            var oldThumbnailPath = Path.Combine(_webHostEnvironment.WebRootPath, thumbnailUrl.TrimStart('/'));
            if (System.IO.File.Exists(oldThumbnailPath))
            {
                System.IO.File.Delete(oldThumbnailPath);
            }
        }
    }
}
