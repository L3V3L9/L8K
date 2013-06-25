using System;
using System.IO;
using System.Security;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace L8K.Kinect.Mouse.Extensions
{
	public enum ImageFormat
	{
		Png,
		Jpeg,
		Bmp,
	}

	public static class BitmapSourceExtensions
	{
		[SecurityCritical]
		public static void Save(this BitmapSource image, string filePath, ImageFormat format)
		{
			BitmapEncoder bitmapEncoder = (BitmapEncoder)null;
			switch (format)
			{
				case ImageFormat.Png:
					bitmapEncoder = (BitmapEncoder)new PngBitmapEncoder();
					break;
				case ImageFormat.Jpeg:
					bitmapEncoder = (BitmapEncoder)new JpegBitmapEncoder();
					break;
				case ImageFormat.Bmp:
					bitmapEncoder = (BitmapEncoder)new BmpBitmapEncoder();
					break;
			}
			if (bitmapEncoder == null)
				return;
			bitmapEncoder.Frames.Add(BitmapFrame.Create((BitmapSource)BitmapFrame.Create(image)));
			using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
				bitmapEncoder.Save((Stream)fileStream);
		}

		public static BitmapSource ToBitmapSource(this byte[] pixels, int width, int height)
		{
			return BitmapSourceExtensions.ToBitmapSource(pixels, width, height, PixelFormats.Bgr32);
		}

		private static BitmapSource ToBitmapSource(this byte[] pixels, int width, int height, PixelFormat format)
		{
			return BitmapSource.Create(width, height, 96.0, 96.0, format, (BitmapPalette)null, (Array)pixels, width * format.BitsPerPixel / 8);
		}
	}
}
