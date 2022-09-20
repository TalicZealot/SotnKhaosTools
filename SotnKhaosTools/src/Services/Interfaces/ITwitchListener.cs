using System.Threading.Tasks;

namespace SotnKhaosTools.Services.Interfaces
{
	internal interface ITwitchListener
	{
		Task<Models.Authorization> Listen();
		void Stop();
	}
}
