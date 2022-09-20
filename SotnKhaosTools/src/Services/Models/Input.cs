﻿using System.Collections.Generic;

namespace SotnKhaosTools.Services.Models
{
	public class Input
	{
		public bool Enabled { get; set; }
		public List<Dictionary<string, object>> MotionSequence { get; set; }
		public Dictionary<string, object>? Activator { get; set; }
	}
}
