using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SimpleAlarm
{
    public class PermanenceConfig
    {
		private static readonly string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "alarmcfg.txt");
		private Dictionary<string, object> settingsDatas = new Dictionary<string, object>();
		private JavaScriptSerializer serializer = new JavaScriptSerializer();

		public void Load()
		{
			if(!File.Exists(ConfigPath))
			{
				Save();
				return;
			}
			settingsDatas = serializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(ConfigPath));
		}

		public void Save()
		{
			File.WriteAllText(ConfigPath, serializer.Serialize(settingsDatas));
		}

		public T Get<T>(string key, T def)
		{
			if (!settingsDatas.ContainsKey(key))
			{
				settingsDatas.Add(key, def);
				return def;
			}
			return (T) settingsDatas[key];
		}

		public void Set(string key, object value)
		{
			if (!settingsDatas.ContainsKey(key))
			{
				settingsDatas.Add(key, value);
				return;
			}
			settingsDatas[key] = value;
		}
    }
}
