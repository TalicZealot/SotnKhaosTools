using System.Drawing;

namespace SotnKhaosTools.Configuration
{
	public class TrackerConfig
	{
		public TrackerConfig()
		{
			Default();
		}
		public bool ProgressionRelicsOnly { get; set; }
		public bool GridLayout { get; set; }
		public bool AlwaysOnTop { get; set; }
		public bool Locations { get; set; }
		public bool Stereo { get; set; }
		public bool CustomLocationsGuarded { get; set; }
		public bool CustomLocationsEquipment { get; set; }
		public bool CustomLocationsClassic { get; set; }
		public bool CustomLocationsSpread { get; set; }
		public string CustomExtension { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public Point Location { get; set; }

		public void Default()
		{
			ProgressionRelicsOnly = false;
			GridLayout = true;
			AlwaysOnTop = false;
			Locations = true;
			Stereo = true;
			CustomLocationsGuarded = true;
			CustomLocationsEquipment = false;
			CustomLocationsClassic = false;
			CustomLocationsSpread = false;
			Width = 260;
			Height = 490;
			CustomExtension = "";
		}
	}
}
