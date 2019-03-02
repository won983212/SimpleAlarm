using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SimpleAlarm
{
	internal enum AccentState
	{
		ACCENT_DISABLED = 1,
		ACCENT_ENABLE_GRADIENT = 0,
		ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
		ACCENT_ENABLE_BLURBEHIND = 3,
		ACCENT_INVALID_STATE = 4
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct AccentPolicy
	{
		public AccentState AccentState;
		public int AccentFlags;
		public int GradientColor;
		public int AnimationId;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct WindowCompositionAttributeData
	{
		public WindowCompositionAttribute Attribute;
		public IntPtr Data;
		public int SizeOfData;
	}

	internal enum WindowCompositionAttribute
	{
		WCA_ACCENT_POLICY = 19
	}

	internal static class UnsafeNativeMethods
	{
		[DllImport("user32.dll")]
		internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
	}

	public partial class AlarmCall : Window
    {
		public AlarmCall(string title, string description)
        {
            InitializeComponent();

			tblTitle.Text = title;
			tblDescription.Text = description;
        }

		internal void EnableBlur()
		{
			var windowHelper = new WindowInteropHelper(this);

			var accent = new AccentPolicy();
			accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

			var accentStructSize = Marshal.SizeOf(accent);

			var accentPtr = Marshal.AllocHGlobal(accentStructSize);
			Marshal.StructureToPtr(accent, accentPtr, false);

			var data = new WindowCompositionAttributeData();
			data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
			data.SizeOfData = accentStructSize;
			data.Data = accentPtr;

			UnsafeNativeMethods.SetWindowCompositionAttribute(windowHelper.Handle, ref data);

			Marshal.FreeHGlobal(accentPtr);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			EnableBlur();
		}

		private void wndRoot_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Close();
		}
	}
}
