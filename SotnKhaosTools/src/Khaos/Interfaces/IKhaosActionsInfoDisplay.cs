using System.Collections.Generic;
using SotnKhaosTools.Khaos.Models;
using SotnKhaosTools.Services.Models;

namespace SotnKhaosTools.Khaos.Interfaces
{
	public interface IKhaosActionsInfoDisplay
	{
		List<QueuedAction> ActionQueue { get; set; }
		void AddTimer(ActionTimer timer);
		bool ContainsTimer(string name);
		string BatLocation { get; set; }
		string WolfLocation { get; set; }
		string MistLocation { get; set; }
		string PowerOfMistLocation { get; set; }
		string JewelOfOpenLocation { get; set; }
		string GravityBootsLocation { get; set; }
		string LepastoneLocation { get; set; }
		string MermanLocation { get; set; }
	}
}
