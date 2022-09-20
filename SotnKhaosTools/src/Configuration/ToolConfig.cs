using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using SotnKhaosTools.Configuration.Interfaces;
using SotnKhaosTools.Constants;

namespace SotnKhaosTools.Configuration
{
	public class ToolConfig : IToolConfig
	{
		public ToolConfig()
		{
			Tracker = new TrackerConfig();
			Khaos = new KhaosConfig();
			Coop = new CoopConfig();
		}
		public TrackerConfig? Tracker { get; set; }
		public KhaosConfig? Khaos { get; set; }
		public CoopConfig? Coop { get; set; }
		public Point Location { get; set; }
		public string Version { get; set; }
		public void SaveConfig()
		{
			string output = JsonConvert.SerializeObject(this, Formatting.Indented);
			if (File.Exists(Paths.ConfigPath))
			{
				File.WriteAllText(Paths.ConfigPath, output);
			}
			else
			{
				using (StreamWriter sw = File.CreateText(Paths.ConfigPath))
				{
					sw.Write(output);
				}
			}
		}
	}
}
