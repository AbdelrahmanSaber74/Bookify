public interface IImageService
{
	Task<IActionResult> SaveImageAsync(IFormFile file, string folderPath);
	void DeleteOldImages(string imageUrl, string thumbnailUrl);
}
