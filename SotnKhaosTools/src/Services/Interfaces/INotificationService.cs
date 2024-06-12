using System.Collections.Generic;
using SotnKhaosTools.Khaos.Models;

namespace SotnKhaosTools.Services
{
	internal interface INotificationService
	{
		double Volume { set; }
		bool MapOpen { get; set; }
		bool InvertedMapOpen { get; set; }
		int VermillionBirds { get; set; }
		int AzureDragons { get; set; }
		int BlackTortoises { get; set; }
		int WhiteTigers { get; set; }
		void AddMessage(string message);
		void PlayAlert(string uri);
		void StartOverlayServer();
		void StopOverlayServer();
		void UpdateOverlayMeter(int meter);
		void AddOverlayTimer(int index, int duration);
		void UpdateOverlayQueue(List<QueuedAction> actionQueue);
		void SetRelicCoordinates(string relic, int mapCol, int mapRow);
		void SetInvertedRelicCoordinates(string relic, int mapCol, int mapRow);
		void Refresh();
	}
}