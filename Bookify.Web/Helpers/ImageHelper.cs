using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace Bookify.Web.Helpers
{
	public static class ImageHelper
	{
		/// <summary>
		/// Resizes the image located at the specified input file path and saves it to the specified output file path.
		/// </summary>
		/// <param name="inputFilePath">The path of the input image file.</param>
		/// <param name="outputFilePath">The path where the resized image will be saved.</param>
		/// <param name="width">The desired width of the resized image.</param>
		/// <param name="format">The image format to save as (JPEG by default).</param>
		public static void ResizeImage(string inputFilePath, string outputFilePath, int width, IImageEncoder format = null)
		{
			using (Image image = Image.Load(inputFilePath))
			{
				double aspectRatio = (double)image.Width / image.Height;
				var height = image.Height / aspectRatio;
				// Resize the image while maintaining the aspect ratio
				image.Mutate(x => x.Resize(new ResizeOptions
				{
					Size = new Size(width, (int)height),
					Mode = ResizeMode.Max // Maintains aspect ratio
				}));

				// Save the resized image
				image.Save(outputFilePath, format ?? new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder());
			}
		}
	}
}
