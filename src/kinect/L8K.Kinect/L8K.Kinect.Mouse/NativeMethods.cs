using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace L8K.Kinect.Mouse
{
	internal struct MouseInput
	{
		public int X;
		public int Y;
		public uint MouseData;
		public uint Flags;
		public uint Time;
		public IntPtr ExtraInfo;
	}

	internal struct Input
	{
		public int Type;
		public MouseInput MouseInput;
	}

	public static class NativeMethods
	{
		public const int InputMouse = 0;

		public const int MouseEventMove = 0x01;
		public const int MouseEventLeftDown = 0x02;
		public const int MouseEventLeftUp = 0x04;
		public const int MouseEventRightDown = 0x08;
		public const int MouseEventRightUp = 0x10;
		public const int MouseEventMiddleDown = 0x0020;
		public const int MouseEventMiddleUp = 0x0040;
		public const int MouseEventAbsolute = 0x8000;

		private static bool _lastLeftDown;
		private static bool _lastMiddleDown;

		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint SendInput(uint numInputs, Input[] inputs, int size);

		public static void SendMouseInput(int positionX, int positionY, int maxX, int maxY, bool leftDown, bool middleDown)
		{
			if (positionX > int.MaxValue)
				throw new ArgumentOutOfRangeException("positionX");
			if (positionY > int.MaxValue)
				throw new ArgumentOutOfRangeException("positionY");

			var i = new Input[3];

			// move the mouse to the position specified
			i[0] = new Input
				{
					Type = InputMouse,
					MouseInput = { X = (positionX * 65535) / maxX, Y = (positionY * 65535) / maxY, Flags = MouseEventAbsolute | MouseEventMove }
				};

			uint middleClick = 0;

			// determine if we need to send a mouse down or mouse up event
			if (!_lastMiddleDown && middleDown)
			{
				middleClick = MouseEventMiddleDown;
			}
			else if (_lastMiddleDown && !middleDown)
			{
				middleClick = MouseEventMiddleUp;
			}

			// determine if we need to send a mouse down or mouse up event
			if (!_lastLeftDown && leftDown)
			{
				i[1] = new Input { Type = InputMouse, MouseInput = { Flags = MouseEventLeftDown | middleClick } };
			}
			else if (_lastLeftDown && !leftDown)
			{
				i[1] = new Input { Type = InputMouse, MouseInput = { Flags = MouseEventLeftUp | middleClick } };
			}
			else if (middleClick > 0)
			{
				i[1] = new Input { Type = InputMouse, MouseInput = { Flags = MouseEventLeftUp | middleClick } };
			}

			_lastLeftDown = leftDown;
			_lastMiddleDown = middleDown;

			// send it off
			uint result = SendInput(2, i, Marshal.SizeOf(i[0]));
			if (result == 0)
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}
}
