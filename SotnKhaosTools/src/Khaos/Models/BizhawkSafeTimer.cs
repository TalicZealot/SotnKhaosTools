using System;
using System.Windows.Forms;

namespace SotnKhaosTools.Khaos.Models
{
	public class BizhawkSafeTimer
	{
		public DateTime Elapses { get; set; }
		public long Interval { get; set; }
		public MethodInvoker Invoker { get; set; }
	}
}
