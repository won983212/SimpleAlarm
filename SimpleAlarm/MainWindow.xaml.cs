using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SimpleAlarm
{
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window
	{
		private System.Windows.Forms.NotifyIcon notifyIcon;

		public MainWindow()
		{
			InitializeComponent();

			notifyIcon = new System.Windows.Forms.NotifyIcon();
			using (Stream s = Application.GetResourceStream(new Uri("alarm.ico", UriKind.Relative)).Stream)
				notifyIcon.Icon = new System.Drawing.Icon(s);
			notifyIcon.Text = "알람";
			notifyIcon.MouseDown += NotifyIcon_MouseDown;
			notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
			notifyIcon.BalloonTipClicked += NotifyIcon_DoubleClick;
			notifyIcon.Visible = true;
		}

		private string GetDayOfWeek(DayOfWeek now)
		{
			switch (now)
			{
				case DayOfWeek.Monday:
					return "MONDAY";
				case DayOfWeek.Tuesday:
					return "TUESDAY";
				case DayOfWeek.Wednesday:
					return "WEDNESDAY";
				case DayOfWeek.Thursday:
					return "THURSDAY";
				case DayOfWeek.Friday:
					return "FRIDAY";
				case DayOfWeek.Saturday:
					return "SATURDAY";
				case DayOfWeek.Sunday:
					return "SUNDAY";
			}

			return "";
		}

		private void NotifyIcon_DoubleClick(object sender, EventArgs e)
		{
			Show();
			WindowState = WindowState.Normal;
		}

		private void NotifyIcon_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(e.Button == System.Windows.Forms.MouseButtons.Right)
			{
				ContextMenu menu = (ContextMenu)FindResource("notifyIconContextMenu");
				menu.IsOpen = true;
			}
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e)
		{
			DragMove();
		}
		
		private void MenuItem_Close_Click(object sender, RoutedEventArgs e)
		{
			if (notifyIcon != null)
				notifyIcon.Dispose();
			Application.Current.Shutdown();
		}

		// Close
		private void Close_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (notifyIcon != null)
				notifyIcon.Dispose();
			Application.Current.Shutdown();
		}

		// Tray
		private void Tray_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Hide();
		}
	}
}
