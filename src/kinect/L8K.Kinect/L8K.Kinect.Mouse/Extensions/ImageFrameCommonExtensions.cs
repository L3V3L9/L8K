using System;
using Microsoft.Kinect;

namespace L8K.Kinect.Mouse.Extensions
{
	internal static class ImageFrameCommonExtensions
	{
		public const int RedIndex = 2;
		public const int GreenIndex = 1;
		public const int BlueIndex = 0;
		private const float MaxDepthDistance = 4000f;
		private const float MinDepthDistance = 800f;
		private const float MaxDepthDistanceOffset = 3200f;

		public static int GetDistance(this DepthImageFrame depthFrame, int x, int y)
		{
			int width = depthFrame.Width;
			if (x > width)
				throw new ArgumentOutOfRangeException("x", "x is larger than the width");
			if (y > depthFrame.Height)
				throw new ArgumentOutOfRangeException("y", "y is larger than the height");
			if (x < 0)
				throw new ArgumentOutOfRangeException("x", "x is smaller than zero");
			if (y < 0)
				throw new ArgumentOutOfRangeException("y", "y is smaller than zero");
			int index = width * y + x;
			short[] pixelData = new short[depthFrame.PixelDataLength];
			depthFrame.CopyPixelDataTo(pixelData);
			return ImageFrameCommonExtensions.GetDepth(pixelData[index]);
		}

		public static void GetMidpoint(this short[] depthData, int width, int height, int startX, int startY, int endX, int endY, int minimumDistance, out double xLocation, out double yLocation)
		{
			if (depthData == null)
				throw new ArgumentNullException("depthData");
			if (width * height != depthData.Length)
				throw new ArgumentOutOfRangeException("depthData", "Depth Data length does not match target height and width");
			if (endX > width)
				throw new ArgumentOutOfRangeException("endX", "endX is larger than the width");
			if (endY > height)
				throw new ArgumentOutOfRangeException("endY", "endY is larger than the height");
			if (startX < 0)
				throw new ArgumentOutOfRangeException("startX", "startX is smaller than zero");
			if (startY < 0)
				throw new ArgumentOutOfRangeException("startY", "startY is smaller than zero");
			xLocation = 0.0;
			yLocation = 0.0;
			int num1 = 0;
			for (int index1 = startX; index1 < endX; ++index1)
			{
				for (int index2 = startY; index2 < endY; ++index2)
				{
					short num2 = depthData[index1 + width * index2];
					if ((int)num2 <= minimumDistance && (int)num2 > 0)
					{
						xLocation += (double)index1;
						yLocation += (double)index2;
						++num1;
					}
				}
			}
			if (num1 <= 0)
				return;
			xLocation /= (double)num1;
			yLocation /= (double)num1;
		}

		public static short[] ToDepthArray(this DepthImageFrame image)
		{
			if (image == null)
				throw new ArgumentNullException("image");
			int width = image.Width;
			int height = image.Height;
			short[] pixelData = new short[image.PixelDataLength];
			image.CopyPixelDataTo(pixelData);
			short[] numArray = new short[image.PixelDataLength];
			for (int index = 0; index < pixelData.Length; ++index)
				numArray[index] = (short)ImageFrameCommonExtensions.GetDepth(pixelData[index]);
			return numArray;
		}

		public static byte CalculateIntensityFromDepth(int distance)
		{
			return (byte)((double)byte.MaxValue - (double)byte.MaxValue * (double)Math.Max((float)distance - 800f, 0.0f) / 3200.0);
		}

		public static void SkeletonOverlay(ref byte redFrame, ref byte greenFrame, ref byte blueFrame, int player)
		{
			switch (player)
			{
				case 1:
					greenFrame = (byte)0;
					blueFrame = (byte)0;
					break;
				case 2:
					redFrame = (byte)0;
					greenFrame = (byte)0;
					break;
				case 3:
					redFrame = (byte)0;
					blueFrame = (byte)0;
					break;
				case 4:
					greenFrame = (byte)0;
					break;
				case 5:
					blueFrame = (byte)0;
					break;
				case 6:
					redFrame = (byte)0;
					break;
				case 7:
					redFrame /= (byte)2;
					blueFrame = (byte)0;
					break;
			}
		}

		public static byte[] ConvertDepthFrameToBitmap(DepthImageFrame depthFrame)
		{
			if (depthFrame == null)
				return (byte[])null;
			short[] pixelData = new short[depthFrame.PixelDataLength];
			depthFrame.CopyPixelDataTo(pixelData);
			byte[] numArray = new byte[pixelData.Length * 4];
			int index1 = 0;
			int index2 = 0;
			while (index1 < numArray.Length)
			{
				int depth = ImageFrameCommonExtensions.GetDepth(pixelData[index2]);
				if (depth == -1)
				{
					numArray[index1 + 2] = (byte)66;
					numArray[index1 + 1] = (byte)66;
					numArray[index1] = (byte)33;
				}
				else
				{
					byte num = ImageFrameCommonExtensions.CalculateIntensityFromDepth(depth);
					numArray[index1 + 2] = num;
					numArray[index1 + 1] = num;
					numArray[index1] = num;
				}
				int playerIndex = ImageFrameCommonExtensions.GetPlayerIndex(pixelData[index2]);
				ImageFrameCommonExtensions.SkeletonOverlay(ref numArray[index1 + 2], ref numArray[index1 + 1], ref numArray[index1], playerIndex);
				index1 += 4;
				++index2;
			}
			return numArray;
		}

		public static int GetPlayerIndex(short depthPoint)
		{
			return (int)depthPoint & 7;
		}

		public static int GetDepth(short depthPoint)
		{
			return (int)depthPoint >> 3;
		}
	}
}
