using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAlarm
{
    public class AlarmManager
    {
		// Collection은 반드시 시각순으로 sort되어있어야 한다.
		public ObservableCollection<Alarm> Collection { get; }
		private SortedList<int, Alarm> enabledAlarms = new SortedList<int, Alarm>();

		public AlarmManager()
		{
			Collection = new ObservableCollection<Alarm>();

			// TODO Test
			InsertAlarm(new Alarm() { Manager = this, TargetTime = new DateTime(1, 1, 1, 9, 20, 00), IsEnabled = true, Label = "Hello, world?" });
			InsertAlarm(new Alarm() { Manager = this, TargetTime = new DateTime(1, 1, 1, 14, 40, 00), IsEnabled = true, Label = "앙 테스트 알람~" });
			InsertAlarm(new Alarm() { Manager = this, TargetTime = new DateTime(1, 1, 1, 19, 00, 00), IsEnabled = false, Label = "이건 끄자 ㅎㅎ" });
			InsertAlarm(new Alarm() { Manager = this, TargetTime = new DateTime(1, 1, 1, 15, 00, 00), IsEnabled = false, Label = "이건 끄자 ㅇㅋ??" });
		}

		public void OnChangeEnabled(Alarm alarm)
		{
			if (alarm.IsEnabled)
				enabledAlarms.Add(alarm.GetTotalSeconds(), alarm);
			else
				enabledAlarms.Remove(alarm.GetTotalSeconds());
		}

		public void InsertAlarm(Alarm alarm)
		{
			int idx;
			for (idx = 0; idx < Collection.Count; idx++)
			{
				if (Alarm.TotalSeconds(Collection[idx].TargetTime) > Alarm.TotalSeconds(alarm.TargetTime))
					break;
			}

			Collection.Insert(idx, alarm);
			if(alarm.IsEnabled)
				enabledAlarms.Add(alarm.GetTotalSeconds(), alarm);
		}

		public void UpdateAlarms()
		{
			foreach (Alarm ent in Collection)
			{
				if (ent.IsEnabled)
					ent.UpdateRemainingString();
			}
		}

		private int GetInsertAt(DateTime time)
		{
			int idx;
			for (idx = 0; idx < enabledAlarms.Count; idx++)
			{
				if (enabledAlarms.Keys[idx] > Alarm.TotalSeconds(time))
					break;
			}
			return idx;
		}

		public Alarm NextAlarm()
		{
			if (enabledAlarms.Count == 0)
				return null;

			int idx = GetInsertAt(DateTime.Now);
			if (idx == enabledAlarms.Count)
				return enabledAlarms.Values[0];
			return enabledAlarms.Values[idx];
		}

		public Alarm PreviousAlarm()
		{
			if (enabledAlarms.Count == 0)
				return null;

			int idx = GetInsertAt(DateTime.Now);
			if (idx - 1 < 0)
				return enabledAlarms.Values[enabledAlarms.Count - 1];
			return enabledAlarms.Values[idx - 1];
		}
	}
}
