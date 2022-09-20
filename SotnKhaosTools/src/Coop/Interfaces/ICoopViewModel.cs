using System.ComponentModel;

namespace SotnKhaosTools.Coop.Models
{
	public interface ICoopViewModel
	{
		bool ClientConnected { get; set; }
		bool ServerStarted { get; set; }
		string Message { get; set; }

		event PropertyChangedEventHandler PropertyChanged;
	}
}