﻿using System;

namespace SotnKhaosTools.Services.Models
{
	public class ActionTimer
	{
		public string Name { get; set; }
		public TimeSpan Duration { get; set; }
		public int TotalDuration { get; set; }
	}
}
