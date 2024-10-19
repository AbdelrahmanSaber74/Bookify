public class ImageService : IImageService
{
	private readonly IWebHostEnvironment _webHostEnvironment;

	public ImageService(IWebHostEnvironment webHostEnvironment)
	{
		_webHostEnvironment = webHostEnvironment;
	}

	public async Task<IActionResult> SaveImageAsync(IFormFile file, string folderPath, bool hasThumbnail)
	{
		if (file == null || file.Length == 0)
		{
			return new BadRequestObjectResult(Errors.NoFileProvided);
		}

		const int maxFileSize = 5 * 1024 * 1024; // 5 MB
		if (file.Length > maxFileSize)
		{
			return new BadRequestObjectResult(string.Format(Errors.FileSizeExceeded, maxFileSize / (1024 * 1024)));
		}

		var allowedFileTypes = new[] { "image/jpeg", "image/png", "image/gif" };
		if (!allowedFileTypes.Contains(file.ContentType))
		{
			return new BadRequestObjectResult(string.Format(Errors.InvalidFileType, string.Join(", ", allowedFileTypes)));
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

			string thumbnailRelativePath = null;

			// Create thumbnail if specified
			if (hasThumbnail)
			{
				var thumbnailPath = Path.Combine(fullFolderPath, "thumbs", imageName);
				var thumbnailDirectory = Path.GetDirectoryName(thumbnailPath);

				if (thumbnailDirectory != null && !Directory.Exists(thumbnailDirectory))
				{
					Directory.CreateDirectory(thumbnailDirectory);
				}

				// Resize the original image and save it as a thumbnail
				ImageHelper.ResizeImage(filePath, thumbnailPath, width: 150);

				thumbnailRelativePath = "/" + Path.Combine(folderPath, "thumbs", imageName).Replace("\\", "/");
			}

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
			return new BadRequestObjectResult(Errors.SaveError);
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

		// Delete the old thumbnail if specified
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
