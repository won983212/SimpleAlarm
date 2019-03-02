using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAlarm
{
	public class Alarm : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private bool isEnabled = true;
		
		public AlarmManager Manager { get; set; }
		public DateTime TargetTime { get; set; }
		public string Label { get; set; }
		public bool IsEnabled
		{
			get => isEnabled;
			set
			{
				bool prev = IsEnabled;
				isEnabled = value;
				if (prev != value)
				{
					UpdateRemainingString();
					Manager.OnChangeEnabled(this);
				}
			}
		}
		public string TimeString
		{
			get
			{
				return TargetTime.ToString("hh:mm");
			}
		}

		public string TimeAmPm
		{
			get
			{
				return TargetTime.Hour > 12 ? "PM" : "AM";
			}
		}

		public string RemainingTimeString
		{
			get
			{
				return IsEnabled ? GetTimeRemaining().ToString() : "-";
			}
		}

		public int GetTotalSeconds()
		{
			return TotalSeconds(TargetTime);
		}

		public TimeSpan GetTimeRemaining()
		{
			DateTime now = DateTime.Now;
			int diff = TotalSeconds(TargetTime) - TotalSeconds(now);
			
			if (diff < 0)
				return TimeSpan.FromHours(24) - new TimeSpan(0, 0, -diff);
			else
				return new TimeSpan(0, 0, diff);
		}

		public void UpdateRemainingString()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RemainingTimeString"));
		}

		public override string ToString()
		{
			return Label;
		}

		public static int TotalSeconds(DateTime date)
		{
			return date.Hour * 3600 + date.Minute * 60 + date.Second;
		}

		public static string GetDayOfWeek(DayOfWeek now)
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
	}
}
