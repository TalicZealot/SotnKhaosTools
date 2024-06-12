using System;

namespace SotnKhaosTools.Khaos.Models
{
	public struct ScheduledAction
	{
		public int Timer { get; set; }
		public int Delay { get; set; }
		public bool On { get; set; }
		public bool Repeat { get; set; }
		public Action Action { get; set; }

		public void Start()
		{
			Timer = Delay;
			On = true;
		}
		public void Stop()
		{
			On = false;
		}
	}
}
