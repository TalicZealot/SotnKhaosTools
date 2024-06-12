using System.Collections.Generic;
using SotnKhaosTools.Khaos.Models;

namespace SotnKhaosTools.Services.Interfaces
{
	internal interface IOverlaySocketServer
	{
		void StartServer();
		void StopServer();
		void UpdateMeter(int meter);
		void AddTimer(int index, int duration);
		void UpdateQueue(List<QueuedAction> actionQueue);
	}
}
