using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAlarm
{
    class PermanenceConfig
    {
		private static readonly string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "alarmcfg.txt");
		

		public void Load()
		{

		}
    }
}
