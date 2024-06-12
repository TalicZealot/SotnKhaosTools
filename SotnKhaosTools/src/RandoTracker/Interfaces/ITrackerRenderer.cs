using System.Collections.Generic;
using SotnKhaosTools.RandoTracker.Interfaces;
using SotnKhaosTools.RandoTracker.Models;

namespace SotnKhaosTools.RandoTracker
{
	internal interface ITrackerRenderer
	{
		bool Refreshed { get; set; }
		string SeedInfo { get; set; }
		void SetProgression();
		void CalculateGrid(int width, int height);
		void Render();
		void ChangeGraphics(IGraphics formGraphics);
		void InitializeItems(List<Models.TrackerRelic> relics, List<Item> progressionItems, List<Item> thrustSwords);
	}
}