using System.Collections.Generic;
using SotnKhaosTools.Khaos.Models;

namespace SotnKhaosTools.Services.Interfaces
{
	internal interface IOverlaySocketServer
	{
		void StartServer();
		void StopServer();
		void UpdateMeter(int meter);
		void AddTimer(string name, int duration);
		void UpdateQueue(List<QueuedAction> actionQueue);
		void UpdateTracker(int relics, int items);
	}
}
