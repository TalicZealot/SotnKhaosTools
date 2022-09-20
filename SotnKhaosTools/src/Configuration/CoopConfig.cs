﻿using System.Drawing;

namespace SotnKhaosTools.Configuration
{
	public class CoopConfig
	{
		public CoopConfig()
		{
			ShareRelics = true;
			ShareWarps = true;
			SendItems = true;
			SendAssists = true;
			ShareLocations = true;
			StoreLastServer = true;
			DefaultServer = "";
			DefaultPort = 46318;
		}

		public Point Location { get; set; }
		public bool ShareRelics { get; set; }
		public bool ShareWarps { get; set; }
		public bool SendItems { get; set; }
		public bool ShareLocations { get; set; }
		public bool StoreLastServer { get; set; }
		public bool SendAssists;
		public int DefaultPort { get; set; }
		public string DefaultServer { get; set; }
		public bool ConnectionShareRelics { get; set; }
		public bool ConnectionShareWarps { get; set; }
		public bool ConnectionSendItems { get; set; }
		public bool ConnectionSendAssists { get; set; }
		public bool ConnectionShareLocations { get; set; }

		public void InitiateServerSettings()
		{
			ConnectionShareRelics = ShareRelics;
			ConnectionShareWarps = ShareWarps;
			ConnectionSendItems = SendItems;
			ConnectionSendAssists = SendAssists;
			ConnectionShareLocations = ShareLocations;
		}
	}
}
