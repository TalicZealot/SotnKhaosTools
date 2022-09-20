using SotnKhaosTools.Khaos.Interfaces;

namespace SotnKhaosTools.RandoTracker
{
	internal interface ITracker
	{
		IRelicLocationDisplay RelicLocationDisplay { get; set; }
		void DrawRelicsAndItems();
		void Update();
		void SaveReplay();
	}
}
