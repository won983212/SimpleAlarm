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
		public AlarmManager Manager;

		public AlarmPattern(AlarmPatternDictionary settings)
		{
			Manager = new AlarmManager(settings);
		}
	}

	public class AlarmPatternDictionary
	{
		public const int CountOfPatterns = 5;
		public AlarmPattern[] Patterns = new AlarmPattern[CountOfPatterns];

		public void LoadFromSettings()
		{
			string data = App.Settings.Get("Patterns", "");
			if (data.Length > 0)
			{
				byte[] bytes = Encoding.UTF8.GetBytes(data);
				using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes)))
				{
					for (int i = 0; i < CountOfPatterns; i++)
					{
						AlarmPattern pattern = new AlarmPattern(this);
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
						Patterns[i] = pattern;
					}
				}
			}
			else
			{
				for(int i = 0; i < CountOfPatterns; i++)
					Patterns[i] = new AlarmPattern(this);
			}
		}

		public void SaveToSettings()
		{
			MemoryStream stream = new MemoryStream();
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				foreach (AlarmPattern pattern in Patterns)
				{
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
