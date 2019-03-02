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
using System.Windows.Media.Effects;
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
		private AlarmPatternDictionary patterns;
		private BlurEffect blur = new BlurEffect();
		
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
			timer.Interval = TimeSpan.FromMilliseconds(1000);
			timer.Start();

			patterns = new AlarmPatternDictionary();
			patterns.LoadFromSettings();

			cbxAlarmPattern.ItemsSource = patterns.Patterns;
			cbxAlarmPattern.SelectedIndex = App.Settings.Get("SelectedPatternIndex", 0);
			itemsAlarm.ItemsSource = Alarms.Collection;

			for (int i = 1; i <= 12; i++)
				cbxAlarmHour.Items.Add(i);

			for (int i = 1; i < 60; i++)
				cbxAlarmMinute.Items.Add(i < 10 ? ("0" + i) : i.ToString());

			// Initial setting to show current time at first.
			Timer_Tick(null, null);
		}

		private AlarmManager Alarms
		{
			get
			{
				return patterns.Patterns[cbxAlarmPattern.SelectedIndex].Manager;
			}
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			DateTime now = DateTime.Now;

			clock.Time = now;
			counterCurrentTime.Time = new TimeSpan(now.Hour % 12, now.Minute, 0);
			tblCurrentAmPm.Text = now.Hour > 12 ? "PM" : "AM";
			tblCurrentDate.Text = now.ToString("yyyy.MM.dd ") + Alarm.GetDayOfWeek(now.DayOfWeek);

			Alarm next = Alarms.NextAlarm();
			Alarm prev = Alarms.PreviousAlarm();
			if (next != null && prev != null)
			{
				TimeSpan remain = next.GetTimeRemaining();
				counterCurrentAlarm.Visibility = Visibility.Visible;
				counterCurrentAlarm.Time = remain;
				tblAlarmLabel.Text = prev.Label;
				Alarms.UpdateAlarms();

				if (remain.TotalSeconds < 1)
				{
					AlarmCall wnd = new AlarmCall(next.Label, (next.TargetTime.Hour > 12 ? "PM " : "AM ") + next.TargetTime.ToString("h:mm"));
					wnd.Left = SystemParameters.PrimaryScreenWidth - 430;
					wnd.Top = SystemParameters.PrimaryScreenHeight - 150;
					wnd.Show();

					System.Media.SystemSounds.Hand.Play();
				}
			}
			else
			{
				counterCurrentAlarm.Visibility = Visibility.Collapsed;
				tblAlarmLabel.Text = "";
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
			if (e.LeftButton == MouseButtonState.Pressed)
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
			// 0: All open, 1: menu closed, 2: All closed
			int menuMode = App.Settings.Get("MenuOpened", 0);
			if (menuMode == 0)
			{
				alarmMenuAnimation.Close();
				App.Settings.Set("MenuOpened", 1);
			}
			else if (menuMode == 1)
			{
				bkgStatusBar.Visibility = Visibility.Hidden;
				cbxAlarmPattern.Visibility = Visibility.Hidden;
				tblCurrentDate.Visibility = Visibility.Hidden;
				pnlTopBar.Visibility = Visibility.Hidden;
				App.Settings.Set("MenuOpened", 2);
			}
			else
			{
				alarmMenuAnimation.Open();
				bkgStatusBar.Visibility = Visibility.Visible;
				cbxAlarmPattern.Visibility = Visibility.Visible;
				tblCurrentDate.Visibility = Visibility.Visible;
				pnlTopBar.Visibility = Visibility.Visible;
				App.Settings.Set("MenuOpened", 0);
			}
			App.Settings.Save();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Topmost = App.Settings.Get("TopMost", false);
			chxTopmost.IsChecked = Topmost;
			alarmMenuAnimation = new ExpandAnimation(pnlAlarmMenu);
			
			int menuMode = App.Settings.Get("MenuOpened", 0);
			if (menuMode == 0)
				alarmMenuAnimation.Open();
			else if (menuMode == 1)
				alarmMenuAnimation.Close();
			else
			{
				alarmMenuAnimation.Close();
				bkgStatusBar.Visibility = Visibility.Hidden;
				cbxAlarmPattern.Visibility = Visibility.Hidden;
				tblCurrentDate.Visibility = Visibility.Hidden;
				pnlTopBar.Visibility = Visibility.Hidden;
			}
		}

		private void AddAlarm_Click(object sender, RoutedEventArgs e)
		{
			tbxAlarmName.Text = "";
			pnlAddAlarm.Visibility = Visibility.Visible;
			wndGrid.Effect = blur;
		}

		private void AddAlarmPattern_Click(object sender, RoutedEventArgs e)
		{
			tbxPatternName.Text = "";
			pnlAddPattern.Visibility = Visibility.Visible;
			wndGrid.Effect = blur;
		}

		private void RemoveAlarm_Click(object sender, RoutedEventArgs e)
		{
			if(patterns.Patterns.Count == 1)
			{
				MessageBox.Show("패턴을 최소 1개 이상은 남겨두어야 합니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			patterns.Patterns.RemoveAt(cbxAlarmPattern.SelectedIndex);
			patterns.SaveToSettings();
			cbxAlarmPattern.SelectedIndex = patterns.Patterns.Count - 1;
		}

		private void AddPattern_Click(object sender, RoutedEventArgs e)
		{
			if (tbxPatternName.Text.Count() == 0)
			{
				MessageBox.Show("패턴 이름란이 비어있습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			
			patterns.Patterns.Add(new AlarmPattern(patterns) { Name = tbxPatternName.Text });
			patterns.SaveToSettings();

			cbxAlarmPattern.SelectedIndex = patterns.Patterns.Count - 1;
			pnlAddPattern.Visibility = Visibility.Hidden;
			wndGrid.Effect = null;
		}

		private void CancelPattern_Click(object sender, RoutedEventArgs e)
		{
			pnlAddPattern.Visibility = Visibility.Hidden;
			wndGrid.Effect = null;
		}

		private void AlarmPattern_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cbxAlarmPattern.SelectedIndex != -1)
			{
				itemsAlarm.ItemsSource = Alarms.Collection;
				App.Settings.Set("SelectedPatternIndex", cbxAlarmPattern.SelectedIndex);
				App.Settings.Save();
			}
		}

		private void AlarmAdd_Click(object sender, RoutedEventArgs e)
		{
			int hour = 1;
			int min = 1;

			if (!(int.TryParse(cbxAlarmHour.Text, out hour) && int.TryParse(cbxAlarmMinute.Text, out min)))
			{
				MessageBox.Show("시각 표기란에는 정수를 입력해야합니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			if (hour <= 0 || hour > 12)
			{
				MessageBox.Show("시는 1과 12사이의 정수이어야 합니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			if (min < 0 || min > 59)
			{
				MessageBox.Show("분은 0과 59사이의 정수이어야 합니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			if (cbxAlarmAmPm.SelectedIndex == 1)
				hour += 12;

			Alarms.InsertAlarm(new Alarm() { Manager = Alarms, Label = tbxAlarmName.Text, TargetTime = new DateTime(1, 1, 1, hour, min, 0) });
			patterns.SaveToSettings();
			pnlAddAlarm.Visibility = Visibility.Hidden;
			wndGrid.Effect = null;
		}

		private void AlarmCancel_Click(object sender, RoutedEventArgs e)
		{
			pnlAddAlarm.Visibility = Visibility.Hidden;
			wndGrid.Effect = null;
		}

		private void TopEnabled_Changed(object sender, RoutedEventArgs e)
		{
			Topmost = chxTopmost.IsChecked == true;
			App.Settings.Set("TopMost", Topmost);
			App.Settings.Save();
		}

		private void RemoveAlarmEntry_Click(object sender, RoutedEventArgs e)
		{
			Alarm alarm = (Alarm)((Button)sender).Tag;
			if(MessageBox.Show("'" + alarm.Label + "'을(를) 삭제합니다.", "삭제", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
			{
				Alarms.RemoveAlarm(alarm);
				patterns.SaveToSettings();
			}
		}
	}
}
