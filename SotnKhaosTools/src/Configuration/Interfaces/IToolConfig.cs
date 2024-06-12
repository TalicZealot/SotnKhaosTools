using System.Drawing;

namespace SotnKhaosTools.Configuration.Interfaces
{
	public interface IToolConfig
	{
		TrackerConfig? Tracker { get; set; }
		KhaosConfig? Khaos { get; set; }
		Point Location { get; set; }
		string Version { get; set; }
		void SaveConfig();
	}
}