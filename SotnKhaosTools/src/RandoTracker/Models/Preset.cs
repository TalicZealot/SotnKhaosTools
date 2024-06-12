﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace SotnKhaosTools.RandoTracker.Models
{
	internal sealed class PresetMetadata
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Author { get; set; }
		public int Weight { get; set; }
	}
	internal sealed class LockLocation
	{
		[JsonProperty("location")]
		public string Name { get; set; }
		[JsonProperty("locks")]
		public List<string> Locks { get; set; } = new List<string>();
	}

	internal sealed class Preset
	{
		[JsonProperty("metadata")]
		public PresetMetadata Metadata { get; set; }
		[JsonProperty("inherits")]
		public string Inherits { get; set; }
		[JsonProperty("relicLocationsExtension")]
		public string RelicLocationsExtension { get; set; }
		[JsonProperty("lockLocation")]
		public List<LockLocation> LockLocations { get; set; } = new List<LockLocation>();
		[JsonProperty("lockLocationAllowed")]
		public List<LockLocation> LockLocationsAllowed { get; set; } = new List<LockLocation>();
	}
}
