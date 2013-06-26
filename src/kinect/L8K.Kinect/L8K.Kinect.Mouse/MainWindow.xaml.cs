using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using L8K.Kinect.Mouse.Extensions;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Interaction;

namespace L8K.Kinect.Mouse
{
	public partial class MainWindow
	{
		private const float SkeletonMaxX = 0.60f;
		private const float SkeletonMaxY = 0.40f;

		private KinectSensor _sensor;  //The Kinect Sensor the application will use
		private InteractionStream _interactionStream;

		private Skeleton[] _skeletons; //the skeletons 
		private UserInfo[] _userInfos; //the information about the interactive users

		public MainWindow()
		{
			InitializeComponent();
			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			// this is just a test, so it only works with one Kinect, and quits if that is not available.
			_sensor = KinectSensor.KinectSensors.FirstOrDefault();
			if (_sensor == null)
			{
				MessageBox.Show("No Kinect Sensor detected!");
				Close();
				return;
			}

			_skeletons = new Skeleton[_sensor.SkeletonStream.FrameSkeletonArrayLength];
			_userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];

			_sensor.DepthStream.Range = DepthRange.Near;
			_sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

			_sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
			_sensor.SkeletonStream.EnableTrackingInNearRange = true;
			var parameters = new TransformSmoothParameters
			{
				Smoothing = 0.7f,
				Correction = 0.3f,
				Prediction = 0.4f,
				JitterRadius = 1.0f,
				MaxDeviationRadius = 0.5f
			};
			_sensor.SkeletonStream.Enable(parameters);

			_interactionStream = new InteractionStream(_sensor, new MouseInteractionClient());
			_interactionStream.InteractionFrameReady += InteractionStreamOnInteractionFrameReady;

			_sensor.DepthFrameReady += SensorOnDepthFrameReady;
			_sensor.SkeletonFrameReady += SensorOnSkeletonFrameReady;

			_sensor.Start();
		}



		private void SensorOnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs skeletonFrameReadyEventArgs)
		{
			using (SkeletonFrame skeletonFrame = skeletonFrameReadyEventArgs.OpenSkeletonFrame())
			{
				if (skeletonFrame == null)
					return;

				skeletonFrame.CopySkeletonDataTo(_skeletons);
				ProcessSkeletonInteraction(skeletonFrame);
			}

			MoveMouse(_skeletons);
		}

		private void MoveMouse(IEnumerable<Skeleton> skeletons)
		{
			foreach (var sd in skeletons.Where(sd => sd.TrackingState == SkeletonTrackingState.Tracked).Where(sd => sd.Joints[JointType.HandLeft].TrackingState == JointTrackingState.Tracked && sd.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked))
			{
				var jointRight = sd.Joints[JointType.HandRight];

				var scaledRight = jointRight.ScaleTo((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, SkeletonMaxX, SkeletonMaxY);				

				var cursorX = (int)scaledRight.Position.X;
				var cursorY = (int)scaledRight.Position.Y;


				var leftClick = false;
				var rightClick = false;
				InteractionHandEventType eventType;
				if (_lastRightHandEvents.TryGetValue(sd.TrackingId, out eventType))
				{
					leftClick = eventType == InteractionHandEventType.Grip;					
					ClickStatus.Text = eventType.ToString();
				}
				else
				{
					ClickStatus.Text = "No event type for skeleton #" + sd.TrackingId;
				}

				if (_lastRightHandPress.TryGetValue(sd.TrackingId, out rightClick))
					RightClickStatus.Text = rightClick.ToString();
				else
					RightClickStatus.Text = "No right click";

				Status.Text = cursorX + ", " + cursorY + ", " + leftClick;
				NativeMethods.SendMouseInput(cursorX, cursorY, (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, leftClick, rightClick);
			}
		}

		private void ProcessSkeletonInteraction(SkeletonFrame skeletonFrame)
		{
			try
			{
				var accelerometerReading = _sensor.AccelerometerGetCurrentReading();
				_interactionStream.ProcessSkeleton(_skeletons, accelerometerReading, skeletonFrame.Timestamp);
			}
			catch (InvalidOperationException)
			{
				// SkeletonFrame functions may throw when the sensor gets
				// into a bad state.  Ignore the frame in that case.
			}
		}

		private void SensorOnDepthFrameReady(object sender, DepthImageFrameReadyEventArgs depthImageFrameReadyEventArgs)
		{
			using (var depthFrame = depthImageFrameReadyEventArgs.OpenDepthImageFrame())
			{
				if (depthFrame == null)
					return;

				ShowVideo(depthFrame);

				ProcessSkeletonInteraction(depthFrame);
			}
		}

		private void ProcessSkeletonInteraction(DepthImageFrame depthFrame)
		{
			try
			{
				_interactionStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
			}
			catch (InvalidOperationException)
			{
				// DepthFrame functions may throw when the sensor gets
				// into a bad state.  Ignore the frame in that case.
			}
		}

		private void ShowVideo(DepthImageFrame depthFrame)
		{
			Video.Source = depthFrame.ToBitmapSource();
		}

		private readonly Dictionary<int, InteractionHandEventType> _lastLeftHandEvents = new Dictionary<int, InteractionHandEventType>();
		private readonly Dictionary<int, InteractionHandEventType> _lastRightHandEvents = new Dictionary<int, InteractionHandEventType>();
		private readonly Dictionary<int, bool> _lastRightHandPress = new Dictionary<int, bool>();

		private void InteractionStreamOnInteractionFrameReady(object sender, InteractionFrameReadyEventArgs args)
		{
			using (var iaf = args.OpenInteractionFrame()) //dispose as soon as possible
			{
				if (iaf == null)
					return;

				iaf.CopyInteractionDataTo(_userInfos);
			}

			var dump = new StringBuilder();

			var hasUser = false;
			foreach (var userInfo in _userInfos)
			{
				var userId = userInfo.SkeletonTrackingId;
				if (userId == 0)
					continue;

				hasUser = true;
				dump.AppendLine("User ID = " + userId);
				dump.AppendLine("  Hands: ");
				var hands = userInfo.HandPointers;
				if (hands.Count == 0)
					dump.AppendLine("    No hands");
				else
				{
					foreach (var hand in hands)
					{
						if (hand.HandType == InteractionHandType.Right)
							_lastRightHandPress[userId] = hand.IsPressed;							

						var lastHandEvents = hand.HandType == InteractionHandType.Left
												 ? _lastLeftHandEvents
												 : _lastRightHandEvents;

						if (hand.HandEventType != InteractionHandEventType.None)
							lastHandEvents[userId] = hand.HandEventType;

						
						var lastHandEvent = lastHandEvents.ContainsKey(userId)
												? lastHandEvents[userId]
												: InteractionHandEventType.None;

						dump.AppendLine();
						dump.AppendLine("    HandType: " + hand.HandType);
						dump.AppendLine("    HandEventType: " + hand.HandEventType);
						dump.AppendLine("    LastHandEventType: " + lastHandEvent);
						dump.AppendLine("    IsActive: " + hand.IsActive);
						dump.AppendLine("    IsPrimaryForUser: " + hand.IsPrimaryForUser);
						dump.AppendLine("    IsInteractive: " + hand.IsInteractive);
						dump.AppendLine("    PressExtent: " + hand.PressExtent.ToString("N3"));
						dump.AppendLine("    IsPressed: " + hand.IsPressed);
						dump.AppendLine("    IsTracked: " + hand.IsTracked);
						dump.AppendLine("    X: " + hand.X.ToString("N3"));
						dump.AppendLine("    Y: " + hand.Y.ToString("N3"));
						dump.AppendLine("    RawX: " + hand.RawX.ToString("N3"));
						dump.AppendLine("    RawY: " + hand.RawY.ToString("N3"));
						dump.AppendLine("    RawZ: " + hand.RawZ.ToString("N3"));
					}
				}

				tb.Text = dump.ToString();
			}

			if (!hasUser)
				tb.Text = "No user detected.";
		}
	}
}
