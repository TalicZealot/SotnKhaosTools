using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SotnKhaosTools.Khaos.Models
{
	public struct Emitter
	{
		public double Xpos { get; set; }
		public double Ypos { get; set; }
		public double AccelerationX { get; set; }
		public double AccelerationY { get; set; }
		public double Angle { get; set; }
		public double Rotation { get; set; }
		public int Shots { get; set; }
		public int BulletCooldown { get; set; }
		public int BulletTimer { get; set; }
		public double BulletSpeed { get; set; }
		public int BurstLimit { get; set; }
		public int BurstCounter { get; set; }
		public int BurstCooldown { get; set; }
		public bool Enabled { get; set; }
	}
}
