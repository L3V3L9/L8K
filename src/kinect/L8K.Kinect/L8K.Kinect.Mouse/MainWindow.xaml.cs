using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using CCT.NUI.HandTracking;
using CCT.NUI.KinectSDK;
using Coding4Fun.Kinect.Wpf;
using Microsoft.Kinect;

namespace L8K.Kinect.Mouse
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private const float ClickThreshold = 0.33f;
		private const float SkeletonMaxX = 0.60f;
		private const float SkeletonMaxY = 0.40f;

		public bool HandIsOpened { get; set; }
		public bool Drag { get; set; }

		private readonly NotifyIcon _notifyIcon = new NotifyIcon();
		private HandDataSource _handDataSource;

		public MainWindow()
		{
			InitializeComponent();
			
			_notifyIcon.Visible = true;
			_notifyIcon.DoubleClick += delegate
			{
				Show();
				WindowState = WindowState.Normal;
				Focus();
			};
		}



		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			kinectSensorChooser.KinectSensorChanged += kinectSensorChooser_KinectSensorChanged;
		}

		public void HandDataSource_NewDataAvailable(HandCollection data)
		{
			if (!data.HandsDetected)
			{
				Fingers.Text = "No fingers";
				return;
			}

			var hand = data.Hands.First();
			Fingers.Text = hand.FingerCount.ToString(CultureInfo.InvariantCulture);
		}

		private static void StopKinect(KinectSensor sensor)
		{
			if (sensor == null || !sensor.IsRunning) return;
			
			sensor.Stop();
			sensor.AudioSource.Stop();
		}

		void kinectSensorChooser_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var old = (KinectSensor)e.OldValue;

			StopKinect(old);

			var sensor = (KinectSensor)e.NewValue;

			if (sensor == null)
				return;			

			var parameters = new TransformSmoothParameters
				{
					Smoothing = 0.7f,
					Correction = 0.3f,
					Prediction = 0.4f,
					JitterRadius = 1.0f,
					MaxDeviationRadius = 0.5f
				};

			sensor.SkeletonStream.Enable(parameters);
			sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
			sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);

			sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
			try
			{
				sensor.Start();
			}
			catch (System.IO.IOException)
			{				
				kinectSensorChooser.AppConflictOccurred();
			}
		}

		void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			Sensor_DepthFrameReady(e);
			Sensor_SkeletonFrameReady(e);
		}


		private void Window_Closed(object sender, EventArgs e)
		{			
			_notifyIcon.Visible = false;

			if (kinectSensorChooser.Kinect != null)
			{
				kinectSensorChooser.Kinect.Stop();
			}

		}

		private void Window_StateChanged(object sender, EventArgs e)
		{
			if (WindowState == WindowState.Minimized)
			{
				Hide();
			}
		}

		private void Sensor_SkeletonFrameReady(AllFramesReadyEventArgs e)
		{
			using (var skeletonFrameData = e.OpenSkeletonFrame())
			{
				if (skeletonFrameData == null)
					return;

				var allSkeletons = new Skeleton[skeletonFrameData.SkeletonArrayLength];

				skeletonFrameData.CopySkeletonDataTo(allSkeletons);

				foreach (Skeleton sd in allSkeletons)
				{
					// the first found/tracked skeleton moves the mouse cursor
					if (sd.TrackingState == SkeletonTrackingState.Tracked)
					{
						// make sure both hands are tracked
						if (sd.Joints[JointType.HandLeft].TrackingState == JointTrackingState.Tracked &&
							sd.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked)
						{

							int cursorX, cursorY;

							// get the left and right hand Joints
							var jointRight = sd.Joints[JointType.HandRight];
							var jointLeft = sd.Joints[JointType.HandLeft];

							// scale those Joints to the primary screen width and height
							var scaledRight = jointRight.ScaleTo((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, SkeletonMaxX, SkeletonMaxY);
							var scaledLeft = jointLeft.ScaleTo((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, SkeletonMaxX, SkeletonMaxY);

							// figure out the cursor position based on left/right handedness
							if (LeftHand.IsChecked.GetValueOrDefault())
							{
								cursorX = (int)scaledLeft.Position.X;
								cursorY = (int)scaledLeft.Position.Y;
							}
							else
							{
								cursorX = (int)scaledRight.Position.X;
								cursorY = (int)scaledRight.Position.Y;
							}

							bool leftClick;

							// figure out whether the mouse button is down based on where the opposite hand is
							if ((LeftHand.IsChecked.GetValueOrDefault() && jointRight.Position.Y > ClickThreshold) ||
									(!LeftHand.IsChecked.GetValueOrDefault() && jointLeft.Position.Y > ClickThreshold))
								leftClick = true;
							else
								leftClick = false;

							Status.Text = cursorX + ", " + cursorY + ", " + leftClick;
							NativeMethods.SendMouseInput(cursorX, cursorY, (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, leftClick);

							return;
						}
					}
				}
			}


		}

		private void Sensor_DepthFrameReady(AllFramesReadyEventArgs e)
		{
			// if the window is displayed, show the depth buffer image
			if (WindowState != WindowState.Normal) return;

			using (var depthFrame = e.OpenDepthImageFrame())
			{
				if (depthFrame == null)
				{
					return;
				}

				video.Source = depthFrame.ToBitmapSource();
			}
		}
	}
}
