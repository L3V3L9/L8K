using Microsoft.Kinect.Toolkit.Interaction;

namespace L8K.Kinect.Mouse
{
	public class MouseInteractionClient : IInteractionClient
	{
		public InteractionInfo GetInteractionInfoAtLocation(int skeletonTrackingId, InteractionHandType handType, double x, double y)
		{
			return new InteractionInfo
				{
					IsGripTarget = true,
					IsPressTarget = true,
					PressAttractionPointX = 0.5,
					PressAttractionPointY = 0.5,
					PressTargetControlId = 1
				};
		}
	}
}
