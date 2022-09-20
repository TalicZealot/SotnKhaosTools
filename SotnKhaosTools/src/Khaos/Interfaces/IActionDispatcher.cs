using SotnKhaosTools.Khaos.Models;

namespace SotnKhaosTools.Khaos.Interfaces
{
	internal interface IActionDispatcher
	{
		bool AutoKhaosOn { get; set; }

		void EnqueueAction(EventAddAction eventData);
		void StartActions();
		void StopActions();
	}
}