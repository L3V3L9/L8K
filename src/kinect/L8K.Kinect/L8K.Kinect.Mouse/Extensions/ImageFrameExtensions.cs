using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace L8K.Kinect.Mouse.Extensions
{
	public static class ImageFrameExtensions
	{
		public static short[] ToDepthArray(this DepthImageFrame image)
		{
			return ImageFrameCommonExtensions.ToDepthArray(image);
		}

		public static int GetDistance(this DepthImageFrame image, int x, int y)
		{
			return ImageFrameCommonExtensions.GetDistance(image, x, y);
		}

		public static Point GetMidpoint(this short[] depthData, int width, int height, int startX, int startY, int endX, int endY, int minimumDistance)
		{
			double xLocation;
			double yLocation;
			ImageFrameCommonExtensions.GetMidpoint(depthData, width, height, startX, startY, endX, endY, minimumDistance, out xLocation, out yLocation);
			return new Point(xLocation, yLocation);
		}

		public static BitmapSource ToBitmapSource(this short[] depthData, int width, int height, int minimumDistance, Color highlightColor)
		{
			if (depthData == null)
				return (BitmapSource)null;
			byte[] pixels = new byte[depthData.Length * 4];
			int index1 = 0;
			int index2 = 0;
			while (index1 < pixels.Length)
			{
				if ((int)depthData[index2] == -1)
				{
					pixels[index1 + 2] = (byte)66;
					pixels[index1 + 1] = (byte)66;
					pixels[index1] = (byte)33;
				}
				else
				{
					byte num = ImageFrameCommonExtensions.CalculateIntensityFromDepth((int)depthData[index2]);
					pixels[index1 + 2] = num;
					pixels[index1 + 1] = num;
					pixels[index1] = num;
					if ((int)depthData[index2] <= minimumDistance && (int)depthData[index2] > 0)
					{
						Color color = Color.Multiply(highlightColor, (float)num / (float)byte.MaxValue);
						pixels[index1 + 2] = color.R;
						pixels[index1 + 1] = color.G;
						pixels[index1] = color.B;
					}
				}
				index1 += 4;
				++index2;
			}
			return BitmapSourceExtensions.ToBitmapSource(pixels, width, height);
		}

		public static BitmapSource ToBitmapSource(this DepthImageFrame image)
		{
			if (image == null)
				return (BitmapSource)null;
			else
				return BitmapSourceExtensions.ToBitmapSource(ImageFrameCommonExtensions.ConvertDepthFrameToBitmap(image), image.Width, image.Height);
		}

		public static BitmapSource ToBitmapSource(this ColorImageFrame image)
		{
			if (image == null)
				return (BitmapSource)null;
			byte[] numArray = new byte[image.PixelDataLength];
			image.CopyPixelDataTo(numArray);
			return BitmapSourceExtensions.ToBitmapSource(numArray, image.Width, image.Height);
		}
	}
}
