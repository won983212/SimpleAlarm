using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		private ExpandAnimation alarmMenuAnimation;
		private AlarmManager alarms = new AlarmManager();

		// TODO 다이나믹한 배경 만들기
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

			// timer for calculating current time.
			DispatcherTimer timer = new DispatcherTimer();
			timer.Tick += Timer_Tick;
			timer.Interval = TimeSpan.FromMilliseconds(300);
			timer.Start();

			// Initial setting to show current time at first.
			Timer_Tick(null, null);

			itemsAlarm.ItemsSource = alarms.Collection;
			SystemEvents.TimeChanged += delegate
			{
				Console.WriteLine("TimeChanged");
			};
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			DateTime now = DateTime.Now;

			clock.Time = now;
			counterCurrentTime.Time = now;
			tblCurrentAmPm.Text = now.Hour > 12 ? "PM" : "AM";
			tblCurrentDate.Text = now.ToString("yyyy.MM.dd ") + Alarm.GetDayOfWeek(now.DayOfWeek);

			Alarm next = alarms.NextAlarm();
			Alarm prev = alarms.PreviousAlarm();
			if (next != null && prev != null)
			{
				counterCurrentAlarm.Visibility = Visibility.Visible;
				counterCurrentAlarm.Time = new DateTime() + next.GetTimeRemaining();
				tblAlarmLabel.Text = prev.Label;
				alarms.UpdateAlarms();
			}
			else
			{
				counterCurrentAlarm.Visibility = Visibility.Collapsed;
				tblAlarmLabel.Text = "알람 없음";
			}
		}

		private void NotifyIcon_DoubleClick(object sender, EventArgs e)
		{
			Show();
			WindowState = WindowState.Normal;
		}

		private void NotifyIcon_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Right)
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

		private void Tray_Click(object sender, RoutedEventArgs e)
		{
			Hide();
		}

		private void Close_Click(object sender, RoutedEventArgs e)
		{
			if (notifyIcon != null)
				notifyIcon.Dispose();
			Application.Current.Shutdown();
		}

		private void MenuToggle_Click(object sender, RoutedEventArgs e)
		{
			bool opened = App.Settings.Get("MenuOpened", true);
			if (opened)
				alarmMenuAnimation.Close();
			else
				alarmMenuAnimation.Open();
			App.Settings.Set("MenuOpened", !opened);
			App.Settings.Save();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			alarmMenuAnimation = new ExpandAnimation(pnlAlarmMenu);
			if (App.Settings.Get("MenuOpened", true))
				alarmMenuAnimation.Open();
		}

		private void AddAlarm_Click(object sender, RoutedEventArgs e)
		{

		}

		private void AddAlarmPattern_Click(object sender, RoutedEventArgs e)
		{

		}

		private void RemoveAlarm_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
