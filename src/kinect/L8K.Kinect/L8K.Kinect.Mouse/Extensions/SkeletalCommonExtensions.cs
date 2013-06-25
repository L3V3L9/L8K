using Microsoft.Kinect;

namespace L8K.Kinect.Mouse.Extensions
{
	internal static class SkeletalCommonExtensions
	{
		public static Joint ScaleTo(this Joint joint, int width, int height, float skeletonMaxX, float skeletonMaxY)
		{
			var skeletonPoint = new SkeletonPoint
			{
				X = Scale(width, skeletonMaxX, joint.Position.X),
				Y = Scale(height, skeletonMaxY, -joint.Position.Y),
				Z = joint.Position.Z
			};
			joint.Position = skeletonPoint;
			return joint;
		}

		public static Joint ScaleTo(this Joint joint, int width, int height)
		{
			return ScaleTo(joint, width, height, 1f, 1f);
		}

		private static float Scale(int maxPixel, float maxSkeleton, float position)
		{
			float num = (float)((double)maxPixel / (double)maxSkeleton / 2.0) * position + (float)(maxPixel / 2);
			if ((double)num > (double)maxPixel)
				return (float)maxPixel;
			if ((double)num < 0.0)
				return 0.0f;
			else
				return num;
		}
	}
}
