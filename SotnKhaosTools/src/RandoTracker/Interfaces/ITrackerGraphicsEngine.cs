using System.Collections.Generic;
using SotnKhaosTools.RandoTracker.Interfaces;
using SotnKhaosTools.RandoTracker.Models;

namespace SotnKhaosTools.RandoTracker
{
	internal interface ITrackerGraphicsEngine
	{
		bool Refreshed { get; set; }
		void SetProgression();
		void CalculateGrid(int width, int height);
		void DrawSeedInfo(string seedInfo);
		void Render();
		void ChangeGraphics(IGraphics formGraphics);
		void InitializeItems(List<Models.Relic> relics, List<Item> progressionItems, List<Item> thrustSwords);
	}
}