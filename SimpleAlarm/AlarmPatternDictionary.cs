using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAlarm
{
	public class AlarmPattern
	{
		public string Name { get; set; }
		public AlarmManager Manager;

		public AlarmPattern(AlarmPatternDictionary settings)
		{
			Manager = new AlarmManager(settings);
		}
	}

	public class AlarmPatternDictionary
	{
		public ObservableCollection<AlarmPattern> Patterns { get; private set; }

		public void LoadFromSettings()
		{
			string data = App.Settings.Get("Patterns", "");
			Patterns = new ObservableCollection<AlarmPattern>();
			if (data.Length > 0)
			{
				byte[] bytes = Encoding.UTF8.GetBytes(data);
				using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes)))
				{
					int len = reader.ReadInt32();
					for (int i = 0; i < len; i++)
					{
						AlarmPattern pattern = new AlarmPattern(this);
						pattern.Name = reader.ReadString();
						int alarmLen = reader.ReadInt32();
						for (int j = 0; j < alarmLen; j++)
						{
							Alarm a = new Alarm
							{
								Manager = pattern.Manager,
								Label = reader.ReadString(),
								TargetTime = new DateTime(1, 1, 1, reader.ReadInt16(), reader.ReadInt16(), 0),
								IsEnabled = reader.ReadBoolean()
							};
							pattern.Manager.InsertAlarm(a);
						}
						Patterns.Add(pattern);
					}
				}
			}

			if (Patterns.Count == 0)
			{
				Patterns.Add(new AlarmPattern(this) { Name = "알람 패턴" });
				SaveToSettings();
			}
		}

		public void SaveToSettings()
		{
			MemoryStream stream = new MemoryStream();
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				writer.Write(Patterns.Count);
				foreach (AlarmPattern pattern in Patterns)
				{
					writer.Write(pattern.Name);
					writer.Write(pattern.Manager.Collection.Count);
					foreach (Alarm alarm in pattern.Manager.Collection)
					{
						writer.Write(alarm.Label);
						writer.Write((short)alarm.TargetTime.Hour);
						writer.Write((short)alarm.TargetTime.Minute);
						writer.Write(alarm.IsEnabled);
					}
				}
			}

			App.Settings.Set("Patterns", Encoding.UTF8.GetString(stream.ToArray()));
			App.Settings.Save();
		}
	}
}
