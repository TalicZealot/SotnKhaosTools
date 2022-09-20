﻿using System.Collections.Generic;
using System.Drawing;
using SotnKhaosTools.Configuration.Models;
using SotnKhaosTools.Constants;

namespace SotnKhaosTools.Configuration
{
	public class KhaosConfig
	{
		//TODO: add profiles
		public KhaosConfig()
		{
			Default();
		}
		public Point Location { get; set; }
		public bool Alerts { get; set; }
		public bool ControlPannelQueueActions { get; set; }
		public int Volume { get; set; }
		public List<Action> Actions { get; set; }
		public float WeakenFactor { get; set; }
		public float HasteFactor { get; set; }
		public uint ThirstDrainPerSecond { get; set; }
		public int PandoraTrigger { get; set; }
		public int PandoraMinItems { get; set; }
		public int PandoraMaxItems { get; set; }
		public int MeterOnReset { get; set; }
		public string AutoKhaosDifficulty { get; set; }
		public System.TimeSpan QueueInterval { get; set; }
		public bool DynamicInterval { get; set; }
		public bool KeepVladRelics { get; set; }
		public bool CostDecay { get; set; }
		public string[] LightHelpItemRewards { get; set; }
		public string[] MediumHelpItemRewards { get; set; }
		public string[] HeavyHelpItemRewards { get; set; }
		public bool RomhackMode { get; set; }

		public void DefaultActions()
		{
			Actions = new List<Action>
			{
				new Action{
					Name = "Khaos Status",
					Description = "Inflicts a random status effect.",
					Meter = 2,
					AlertPath = Paths.AlucardWhatSound,
					Cooldown = new System.TimeSpan(0, 0, 20),
					Bits = 10,
					ChannelPoints = 100,
					MaximumChannelPoints = 300,
					Scaling = 1.02
				},
				new Action{
					Name = "Khaos Equipment",
					Description = "Randomizes Alucard's equipped gear. If the player has a progression item it gets given back to them and put in the inventory.",
					Meter = 7,
					AlertPath = Paths.AlucardWhatSound,
					Cooldown = new System.TimeSpan(0, 35, 0),
					StartsOnCooldown = true,
					Bits = 20,
					ChannelPoints = 200,
					MaximumChannelPoints = 2000,
					Scaling = 1.5
				},
				new Action{
					Name = "Khaos Stats",
					Description = "Randomizes Alucard's stats, HP and Mana. Sums up current stats for stat pool, hp and mana for points pool, then randomly distributes. Can also give or take points.",
					Meter = 8,
					AlertPath = Paths.AlucardWhatSound,
					Cooldown = new System.TimeSpan(0, 30, 0),
					StartsOnCooldown = true,
					Bits = 20,
					ChannelPoints = 200,
					MaximumChannelPoints = 4000,
					Scaling = 1.5
				},
				new Action{
					Name = "Khaos Relics",
					Description = "Randomizes Alucard's relics. It cannot give or take away Vlad relics.",
					Meter = 12,
					AlertPath = Paths.AlucardWhatSound,
					Cooldown = new System.TimeSpan(0, 35, 0),
					StartsOnCooldown = true,
					Bits = 50,
					ChannelPoints = 500,
					Scaling = 2.0
				},
				new Action{
					Name = "Pandora's Box",
					Description = "",
					IsUsable = false,
					Meter = 15,
					AlertPath = Paths.AlucardWhatSound
				},
				new Action{
					Name = "Gamble",
					Description = "Gambles away a percentage of Alucard's gold for a completely random item.",
					Meter = 2,
					AlertPath = Paths.LibrarianThankYouSound,
					Cooldown = new System.TimeSpan(0, 5, 0),
					StartsOnCooldown = true,
					Bits = 10,
					ChannelPoints = 100,
					Scaling = 2.0
				},
				new Action{
					Name = "Banish",
					Description = "Banishes the player to a random zone.",
					Meter = 7,
					AlertPath = Paths.AlucardWhatSound,
					Cooldown = new System.TimeSpan(0, 35, 0),
					StartsOnCooldown = true,
					Bits = 20,
					ChannelPoints = 200,
					MaximumChannelPoints = 2000,
					Scaling = 1.5
				},
				new Action{
					Name = "Khaotic Burst",
					Description = "Increases the Khaos Meter by 100 points, guaranteeing an enhanced action soon.",
					Meter = 10,
					AlertPath = Paths.AlucardWhatSound,
					Cooldown = new System.TimeSpan(0, 10, 0),
					Bits = 20,
					ChannelPoints = 200,
					Scaling = 1.5
				},
				new Action{
					Name = "Khaos Track",
					Description = "Queues up an in-game music track.",
					Meter = 10,
					AlertPath = Paths.AlucardWhatSound,
					Cooldown = new System.TimeSpan(0, 5, 0),
					Duration = new System.TimeSpan(0, 3, 0),
					RequiresUserInput = true,
					Bits = 15,
					ChannelPoints = 200,
					Scaling = 1
				},
				new Action{
					Name = "Bankrupt",
					Description = "Alucard loses all inventory. Loses some gold and equipment depending on Bankrupt level. Player can't lose progression items. Respawns all items on the map.",
					Meter = 12,
					AlertPath = Paths.DeathLaughSound,
					Cooldown = new System.TimeSpan(0, 35, 0),
					StartsOnCooldown = true,
					Bits = 50,
					ChannelPoints = 500,
					Scaling = 2.0
				},
				new Action{
					Name = "Weaken",
					Description = "Alucard's level, stats, hp and mana get halved. Experience is adjusted so that the player can regain their stats and levels.",
					Meter = 8,
					AlertPath = Paths.DieSound,
					Cooldown = new System.TimeSpan(0, 35, 0),
					StartsOnCooldown = true,
					Bits = 50,
					ChannelPoints = 500,
					Scaling = 2.0
				},
				new Action{
					Name = "Respawn Bosses",
					Description = "All the defeated bosses in the game rise back to life. Boss zone needs to be reloaded for boss to appear, not just the room.",
					Meter = 3,
					AlertPath = Paths.HohoSound,
					Cooldown = new System.TimeSpan(0, 10, 0),
					StartsOnCooldown = true,
					Bits = 20,
					ChannelPoints = 200,
					MaximumChannelPoints = 2000,
					Scaling = 1.5
				},
				new Action{
					Name = "Subweapons Only",
					Description = "Alucard can only use subweapons for combat for a period. Alucard is cursed and mana is locked to 5. Gives Cube of Zoe, a subweapon and 200 hearts. Takes away Gas Cloud temporarily.",
					Meter = 4,
					AlertPath = Paths.RichterLaughSound,
					Cooldown = new System.TimeSpan(0, 10, 0),
					Duration = new System.TimeSpan(0, 1, 0),
					Bits = 30,
					ChannelPoints = 300,
					MaximumChannelPoints = 3000,
					Scaling = 1.5
				},
				new Action{
					Name = "Slow",
					Description = "Not so fast! Alucard is slowed for a period. Underwater gravity is turned on.",
					Meter = 8,
					AlertPath = Paths.SlowWhatSound,
					Duration = new System.TimeSpan(0, 0, 40),
					Cooldown = new System.TimeSpan(0, 6, 0),
					StartsOnCooldown = true,
					Bits = 30,
					ChannelPoints = 300,
					MaximumChannelPoints = 3000,
					Scaling = 1.5
				},
				new Action{
					Name = "Blood Mana",
					Description = "Spells cost HP instead of mana.",
					Meter = 4,
					AlertPath = Paths.DeathLaughSound,
					Duration = new System.TimeSpan(0, 1, 0),
					Cooldown = new System.TimeSpan(0, 6, 0),
					Bits = 20,
					ChannelPoints = 200,
					MaximumChannelPoints = 2000,
					Scaling = 1.5
				},
				new Action{
					Name = "Thirst",
					Description = "Alucard's vampire nature makes him thirst for blood! Alucard gets Dark Metamorphasis effect, but loses hp over time for a period.",
					Meter = 6,
					AlertPath = Paths.DeathLaughSound,
					Duration = new System.TimeSpan(0, 2, 0),
					Cooldown = new System.TimeSpan(0, 5, 0),
					StartsOnCooldown = true,
					Bits = 20,
					ChannelPoints = 200,
					MaximumChannelPoints = 2000,
					Scaling = 1.5
				},
				new Action{
					Name = "Khaos Horde",
					Description = "Khaos has awakened! The castle begins to continuously spawn enemies. Triggers after a viable enemy is encountered in the area.",
					Meter = 8,
					AlertPath = Paths.DracLaughSound,
					Duration = new System.TimeSpan(0, 1, 30),
					Interval = new System.TimeSpan(0, 0, 1),
					Cooldown = new System.TimeSpan(0, 8, 0),
					Bits = 40,
					ChannelPoints = 300,
					MaximumChannelPoints = 4000,
					Scaling = 1.5
				},
				new Action{
					Name = "Endurance",
					Description = "The next boss spawns a clone or gets buffed. Both get increased HP. Waits for the next boss, so it will not activate in the current room if it has a boss.",
					Meter = 7,
					AlertPath = Paths.DeathLaughAlternateSound,
					Cooldown = new System.TimeSpan(0, 3, 0),
					Bits = 30,
					ChannelPoints = 300,
					MaximumChannelPoints = 4000,
					Scaling = 1.5
				},
				new Action{
					Name = "HnK",
					Description = "Alucard loses all invincibility and can get comboed, but gains defense. The player is already dead.",
					Meter = 7,
					AlertPath = Paths.AlreadyDeadSound,
					Duration = new System.TimeSpan(0, 1, 30),
					Cooldown = new System.TimeSpan(0, 8, 0),
					StartsOnCooldown = true,
					Bits = 40,
					ChannelPoints = 400,
					MaximumChannelPoints = 4000,
					Scaling = 1.5
				},
				new Action{
					Name = "Quad",
					Description = "Massive damage boost for the duration. Str buff on kill streak.",
					Meter = 5,
					AlertPath = Paths.QuadSound,
					Duration = new System.TimeSpan(0, 0, 35),
					Cooldown = new System.TimeSpan(0, 15, 0),
					Bits = 30,
					ChannelPoints = 400,
					MaximumChannelPoints = 4000,
					Scaling = 1.5
				},
				new Action{
					Name = "Light Help",
					Description = "Activates a Potion, Shield Potion or gives Alucard a minor item.",
					Meter = 2,
					AlertPath = Paths.FairyPotionSound,
					Cooldown = new System.TimeSpan(0, 0, 0),
					Bits = 10,
					ChannelPoints = 100,
					MaximumChannelPoints = 1000,
					Scaling = 1.5
				},
				new Action{
					Name = "Medium Help",
					Description = "Activates an Elixir, Manna Prism or gives Alucard a decent item.",
					Meter = 5,
					AlertPath = Paths.FairyPotionSound,
					Cooldown = new System.TimeSpan(0, 3, 0),
					Bits = 20,
					ChannelPoints = 200,
					MaximumChannelPoints = 2000,
					Scaling = 1.5
				},
				new Action{
					Name = "Heavy Help",
					Description = "Gives Alucard a top-tier item or a non-Vlad progression relic.",
					Meter = 8,
					AlertPath = Paths.FairyPotionSound,
					StartsOnCooldown = true,
					Cooldown = new System.TimeSpan(0, 7, 0),
					Bits = 30,
					ChannelPoints = 300,
					MaximumChannelPoints = 4000,
					Scaling = 2.0
				},
				new Action{
					Name = "Battle Orders",
					Description = "A glorious warcry echoes, Alucard's max HP and MP are doubled for the duration.",
					Meter = 6,
					AlertPath = Paths.BattleOrdersSound,
					Duration = new System.TimeSpan(0, 1, 0),
					Cooldown = new System.TimeSpan(0, 8, 0),
					Bits = 20,
					ChannelPoints = 200,
					MaximumChannelPoints = 2000,
					Scaling = 1.5
				},
				new Action{
					Name = "Magician",
					Description = "Mana is locked to max for a period. Activates an Int Potion.",
					Meter = 6,
					Duration = new System.TimeSpan(0, 1, 0),
					Cooldown = new System.TimeSpan(0, 8, 0),
					Bits = 30,
					ChannelPoints = 300,
					MaximumChannelPoints = 3000,
					Scaling = 1.5
				},
				new Action{
					Name = "Melty Blood",
					Description = "The hitboxes of most of Alucard's non-special attacks become S+ tier for a period.",
					Meter = 5,
					AlertPath = Paths.MeltySound,
					Duration = new System.TimeSpan(0, 1, 0),
					Cooldown = new System.TimeSpan(0, 8, 0),
					Bits = 30,
					ChannelPoints = 300,
					MaximumChannelPoints = 3000,
					Scaling = 1.5
				},
				new Action{
					Name = "Guilty Gear",
					Description = "In addition to the standard effect the player gets some iframes, boosted attack, boosted speed and a very fast wingsmash.",
					IsUsable = false,
					Meter = 5,
					AlertPath = Paths.DragonInstallSound,
					Duration = new System.TimeSpan(0, 1, 0),
				},
				new Action{
					Name = "Four Beasts",
					Description = "For a period Alucard becomes fully invincible, damages enemies when they touch him and has an attack buff.",
					Meter = 10,
					Duration = new System.TimeSpan(0, 1, 30),
					Cooldown = new System.TimeSpan(0, 6, 0),
					Bits = 20,
					ChannelPoints = 200,
					MaximumChannelPoints = 3000,
					Scaling = 1.5
				},
				new Action{
					Name = "ZA WARUDO",
					Description = "Time has stopped! Long enough to throw a road roller at enemies.",
					Meter = 4,
					AlertPath = Paths.ZaWarudoSound,
					Duration = new System.TimeSpan(0, 0, 30),
					Cooldown = new System.TimeSpan(0, 6, 0),
					Bits = 20,
					ChannelPoints = 200,
					MaximumChannelPoints = 2000,
					Scaling = 1.0
				},
				new Action{
					Name = "Haste",
					Description = "Increase movement speed for a period. Player can dash by double tapping forward like Richter.",
					Meter = 6,
					Duration = new System.TimeSpan(0, 0, 30),
					Cooldown = new System.TimeSpan(0, 5, 0),
					Bits = 30,
					ChannelPoints = 300,
					MaximumChannelPoints = 3000,
					Scaling = 1.5
				},
				new Action{
					Description = "Alucard is also a Lord in the castle. Similar to Horde, but the spawned monsters are the player's allies!",
					Name = "Lord",
					Meter = 8,
					AlertPath = Paths.SwordBroSound,
					Duration = new System.TimeSpan(0, 1, 30),
					Interval = new System.TimeSpan(0, 0, 1),
					Cooldown = new System.TimeSpan(0, 8, 0),
					Bits = 30,
					ChannelPoints = 300,
					MaximumChannelPoints = 3000,
					Scaling = 2.0
				}
			};
		}

		public void Default()
		{
			Alerts = true;
			ControlPannelQueueActions = true;
			Volume = 3;
			WeakenFactor = 0.7F;
			HasteFactor = 3.2F;
			ThirstDrainPerSecond = 1;
			PandoraTrigger = 1700;
			PandoraMinItems = 15;
			PandoraMaxItems = 35;
			MeterOnReset = 50;
			QueueInterval = new System.TimeSpan(0, 0, 21);
			DynamicInterval = true;
			KeepVladRelics = false;
			RomhackMode = false;
			CostDecay = true;
			LightHelpItemRewards = new string[]
			{
				"Leather shield",
				"Shaman shield",
				"Pot Roast",
				"Holbein dagger",
				"Heart Refresh",
				"Str. potion",
				"Attack potion",
				"Wizard hat",
				"Mirror cuirass",
				"Brilliant mail",
				"Aquamarine",
				"Lapis lazuli",
				"Mystic pendant",
				"Gauntlet"
			};
			MediumHelpItemRewards = new string[]
			{
				"Fire shield",
				"Iron shield",
				"Medusa shield",
				"Alucard shield",
				"Shield rod",
				"Buffalo star",
				"Flame star",
				"Obsidian sword",
				"Marsil",
				"Elixir",
				"Holy sword",
				"Mourneblade",
				"Fury plate",
				"Twilight cloak",
				"Library card",
				"Diamond",
				"Onyx",
				"Mystic pendant",
				"Ring of Feanor",
				"King's stone",
				"Manna prism",
				"Dark armor",
				"Ring of Ares"
			};
			HeavyHelpItemRewards = new string[]
			{
				"Mablung Sword",
				"Masamune",
				"Fist of Tulkas",
				"Gurthang",
				"Alucard sword",
				"Vorpal blade",
				"Crissaegrim",
				"Yasatsuna",
				"Dragon helm",
				"Holy glasses",
				"Spike Breaker",
				"Dracula tunic",
				"Ring of Varda",
				"Duplicator",
				"Covenant stone",
				"Gold Ring",
				"Silver Ring"
			};
			AutoKhaosDifficulty = "Normal";
			DefaultActions();
		}

		public void ReduceAllChannelPointPrices()
		{
			for (int i = 0; i < Actions.Count; i++)
			{
				if ((Actions[i].ChannelPoints * 0.5) > 50)
				{
					Actions[i].ChannelPoints = (uint) (Actions[i].ChannelPoints * 0.5);
				}
				else
				{
					Actions[i].ChannelPoints = 50;
				}

				if ((Actions[i].MaximumChannelPoints * 0.5) > 50)
				{
					Actions[i].MaximumChannelPoints = (uint) (Actions[i].MaximumChannelPoints * 0.5);
				}
				else
				{
					Actions[i].MaximumChannelPoints = 50;
				}
			}
		}

		public void IncreaseAllChannelPointPrices()
		{
			for (int i = 0; i < Actions.Count; i++)
			{
				Actions[i].ChannelPoints = (uint) (Actions[i].ChannelPoints * 2);
				Actions[i].MaximumChannelPoints = (uint) (Actions[i].MaximumChannelPoints * 2);
			}
		}
	}
}
