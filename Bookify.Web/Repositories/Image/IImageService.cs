public interface IImageService
{
	Task<IActionResult> SaveImageAsync(IFormFile file, string folderPath, bool hasThumbnail);
	void DeleteOldImages(string imageUrl, string thumbnailUrl);
}
