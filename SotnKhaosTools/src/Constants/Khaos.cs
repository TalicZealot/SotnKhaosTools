using System.Collections.Generic;
using SotnApi.Constants.Values.Alucard.Enums;
using SotnApi.Constants.Values.Game;
using SotnApi.Models;
using SotnKhaosTools.Khaos.Models;
using MapLocation = SotnKhaosTools.RandoTracker.Models.MapLocation;

namespace SotnKhaosTools.Constants
{
	public static class Khaos
	{
		public static readonly Relic[] ProgressionRelics =
		{
			Relic.SoulOfBat,
			Relic.SoulOfWolf,
			Relic.FormOfMist,
			Relic.GravityBoots,
			Relic.LeapStone,
			Relic.JewelOfOpen,
			Relic.MermanStatue
		};
		public static readonly List<Relic[]> FlightRelics = new()
		{
			new Relic[] { Relic.SoulOfBat },
			new Relic[] { Relic.LeapStone, Relic.GravityBoots },
			new Relic[] { Relic.FormOfMist, Relic.PowerOfMist },
			new Relic[] { Relic.SoulOfWolf, Relic.GravityBoots },
		};

		public static readonly List<MapLocation> LoadingRooms = new List<MapLocation>
		{
			new MapLocation{X =17,Y = 36, SecondCastle = 0},
			new MapLocation{X =21,Y = 26, SecondCastle = 0},
			new MapLocation{X =20,Y = 36, SecondCastle = 0},
			new MapLocation{X =30,Y = 25, SecondCastle = 0},
			new MapLocation{X =26,Y = 22, SecondCastle = 0},
			new MapLocation{X =13,Y = 22, SecondCastle = 0},
			new MapLocation{X = 4,Y = 28, SecondCastle = 0},
			new MapLocation{X =36,Y = 21, SecondCastle = 0},
			new MapLocation{X =17,Y = 19, SecondCastle = 0},
			new MapLocation{X =29,Y = 12, SecondCastle = 0},
			new MapLocation{X =39,Y = 12, SecondCastle = 0},
			new MapLocation{X =39,Y = 10, SecondCastle = 0},
			new MapLocation{X =60,Y = 14, SecondCastle = 0},
			new MapLocation{X =60,Y = 17, SecondCastle = 0},
			new MapLocation{X =59,Y = 21, SecondCastle = 0},
			new MapLocation{X =60,Y = 25, SecondCastle = 0},
			new MapLocation{X =40,Y = 26, SecondCastle = 0},
			new MapLocation{X =15,Y = 41, SecondCastle = 0},
			new MapLocation{X =28,Y = 38, SecondCastle = 0},
			new MapLocation{X =34,Y = 44, SecondCastle = 0},
			new MapLocation{X =32,Y = 49, SecondCastle = 0},
			new MapLocation{X =16,Y = 38, SecondCastle = 0},

			new MapLocation{X =24,Y = 51, SecondCastle = 1},
			new MapLocation{X =24,Y = 53, SecondCastle = 1},
			new MapLocation{X = 3,Y = 49, SecondCastle = 1},
			new MapLocation{X = 3,Y = 46, SecondCastle = 1},
			new MapLocation{X = 4,Y = 42, SecondCastle = 1},
			new MapLocation{X = 3,Y = 38, SecondCastle = 1},
			new MapLocation{X =23,Y = 37, SecondCastle = 1},
			new MapLocation{X =35,Y = 25, SecondCastle = 1},
			new MapLocation{X =29,Y = 19, SecondCastle = 1},
			new MapLocation{X =31,Y = 14, SecondCastle = 1},
			new MapLocation{X =48,Y = 22, SecondCastle = 1},
			new MapLocation{X =47,Y = 25, SecondCastle = 1},
			new MapLocation{X =43,Y = 27, SecondCastle = 1},
			new MapLocation{X =46,Y = 27, SecondCastle = 1},
			new MapLocation{X =59,Y = 35, SecondCastle = 1},
			new MapLocation{X =42,Y = 37, SecondCastle = 1},
			new MapLocation{X =33,Y = 38, SecondCastle = 1},
			new MapLocation{X =37,Y = 41, SecondCastle = 1},
			new MapLocation{X =50,Y = 41, SecondCastle = 1},
			new MapLocation{X =46,Y = 44, SecondCastle = 1},
			new MapLocation{X =27,Y = 42, SecondCastle = 1},
			new MapLocation{X =34,Y = 51, SecondCastle = 1},
		};
		public static readonly List<MapLocation> SuccubusRoom = new List<MapLocation>
		{
			new MapLocation{X = 0, Y = 0, SecondCastle = 0}
		};
		public static readonly List<MapLocation> ShopRoom = new List<MapLocation>
		{
			new MapLocation{X = 49, Y = 20, SecondCastle = 0}
		};
		public static readonly List<MapLocation> RichterRooms = new List<MapLocation>
		{
			new MapLocation{X = 31, Y = 8, SecondCastle = 0},
			new MapLocation{X = 32, Y = 8, SecondCastle = 0},
			new MapLocation{X = 33, Y = 8, SecondCastle = 0},
			new MapLocation{X = 34, Y = 8, SecondCastle = 0},
		};
		public static readonly List<MapLocation> EntranceCutsceneRooms = new List<MapLocation>
		{
			new MapLocation{X = 0, Y = 44, SecondCastle = 0},
			new MapLocation{X = 1, Y = 44, SecondCastle = 0},
			new MapLocation{X = 2, Y = 44, SecondCastle = 0},
			new MapLocation{X = 3, Y = 44, SecondCastle = 0},
			new MapLocation{X = 4, Y = 44, SecondCastle = 0},
			new MapLocation{X = 5, Y = 44, SecondCastle = 0},
			new MapLocation{X = 6, Y = 44, SecondCastle = 0},
			new MapLocation{X = 7, Y = 44, SecondCastle = 0},
			new MapLocation{X = 8, Y = 44, SecondCastle = 0},
			new MapLocation{X = 9, Y = 44, SecondCastle = 0},
			new MapLocation{X = 10, Y = 44, SecondCastle = 0},
			new MapLocation{X = 11, Y = 44, SecondCastle = 0},
			new MapLocation{X = 12, Y = 44, SecondCastle = 0},
			new MapLocation{X = 13, Y = 44, SecondCastle = 0},
			new MapLocation{X = 14, Y = 44, SecondCastle = 0},
			new MapLocation{X = 15, Y = 44, SecondCastle = 0},
			new MapLocation{X = 16, Y = 44, SecondCastle = 0},
			new MapLocation{X = 17, Y = 44, SecondCastle = 0},
			new MapLocation{X = 18, Y = 44, SecondCastle = 0},
		};
		public static readonly List<MapLocation> SwitchRoom = new List<MapLocation>
		{
			new MapLocation{X = 46, Y = 24, SecondCastle = 0}
		};
		public static readonly List<MapLocation> GalamothRooms = new List<MapLocation>
		{
			new MapLocation{X = 44, Y = 12, SecondCastle = 1},
			new MapLocation{X = 45, Y = 12, SecondCastle = 1},
			new MapLocation{X = 44, Y = 13, SecondCastle = 1},
			new MapLocation{X = 45, Y = 13, SecondCastle = 1},
		};
		public static readonly List<MapLocation> LesserDemonZone = new List<MapLocation>
		{
			new MapLocation{X = 45, Y = 20, SecondCastle = 0},
			new MapLocation{X = 46, Y = 20, SecondCastle = 0},
			new MapLocation{X = 47, Y = 20, SecondCastle = 0},
			new MapLocation{X = 48, Y = 20, SecondCastle = 0},
			new MapLocation{X = 48, Y = 19, SecondCastle = 0},
			new MapLocation{X = 47, Y = 19, SecondCastle = 0}
		};
		public static readonly List<MapLocation> LibraryRoom = new List<MapLocation>
		{
			new MapLocation{X = 49, Y = 20, SecondCastle = 0}
		};
		public static readonly List<MapLocation> DraculaRoom = new List<MapLocation>
		{
			new MapLocation{X = 31, Y = 30, SecondCastle = 1}
		};
		public static readonly List<MapLocation> ShaftRooms = new List<MapLocation>
		{
			new MapLocation{X = 30, Y = 31, SecondCastle = 1},
			new MapLocation{X = 31, Y = 31, SecondCastle = 1},
			new MapLocation{X = 32, Y = 31, SecondCastle = 1},
			new MapLocation{X = 30, Y = 32, SecondCastle = 1},
			new MapLocation{X = 31, Y = 32, SecondCastle = 1},
			new MapLocation{X = 32, Y = 32, SecondCastle = 1},
			new MapLocation{X = 30, Y = 33, SecondCastle = 1},
			new MapLocation{X = 31, Y = 33, SecondCastle = 1},
			new MapLocation{X = 32, Y = 33, SecondCastle = 1}
		};

		public static readonly List<ushort> AcceptedHordeEnemies = new List<ushort>
		{
			0x6e,//Zombie
			0x40,//Bat
			0x69,//Bone Scimitawr
			0xd,//Bloody Zombie
			0xb4,//Corner Guard
			//0x46,//Blood Skeleton
			0x4b,//Skeleton
			0x51,//Spittle Bone
			0xf6,//Axe Knight
			0x6,//Axe Knight Marble Gallery
			0xb2,//Slinger
			0x31,//Marionette
			0xb,//Skelerang
			0x9c,//Ghost
			0x28,//Fleaman
			0x12f,//Medusa Head
			0x130,//Golden Medusa Head
			0x5d,//Spear Guard
			0x92,//Dhuron
			0x9d,//Thornweed
			0x29,//Flea Armor
			0x43,//Phantom Skull
			0xef,//Harpy
			0x30,//Flea Rider
			0x73,//Black Crow
			0x80,//Winged Guard
			0x76,//Bone Halberd
			0x7c,//Blade Soldier
			0x34,//Olrox Skull
			0x6a,//Toad
			0x6b,//Frog
			0xe1,//Gremlin
			0x83,//Lossoth
			0x129,//Granfaloon Zombie
			0xa5,//Bomb Knight
			0x71,//Tombstone
			0x11d,//Balloon Pod
			0x25,//Black Panther
			0x11c,//Imp
			0xd8,//Ghost Dancer
			0x182,//Minotaur
			0x185,//Werewolf
			0x145,//Beeezelbub Fly
			0xb6,//Bitterfly
			0x74,//Jack O' Bones
			0x18b,//Blue Venus Weed Rose
			//0xfe,//Gaibon
			0x165,//Death Sickle
			0x143//Schmoo
		};
		public static readonly List<ushort> EnduranceBosses = new List<ushort>
		{
			0xfe,//Gaibon
			0xfd,//Dopple10
			0x118,//Karasuman ---2nd castle
			0x32,//Olrox
			0x156,//Succubus
			0x16b,//Cerberus
			0X133,//Richter
			0X111,//DarkwingBat
			0x174,//Dopple40
			0x16e,//Medusa
			0x10b,//Akmodan
			0x151,//Sypha
			0x15f//Shaft
		};
		public static readonly List<ushort> EnduranceAlternateBosses = new List<ushort>
		{
			0xce,//Werewolf
			0x17,//LesserDemon
			0x12c,//Hippo
			0X11F,//Scylla
			0x127,//Gran
			0x172,//Creature
			0x164,//Death
			0x144,//Beez
			0x17b,//Drac
		};
		public static readonly Dictionary<ushort, string> EnduranceBossNames = new Dictionary<ushort, string>
		{
			{ 0xfe, "Gaibon" },
			{ 0xfd, "Doppleganger 10" },
			{ 0xce, "Werewolf" },
			{ 0xcb, "Minotaur" },
			{ 0x17, "LesserDemon" },
			{ 0x118, "Karasuman" },
			{ 0x32, "Olrox" },
			{ 0x156, "Succubus" },
			{ 0x16b, "Cerberus" },
			{ 0X133, "Richter" },
			{ 0X111, "Darkwing Bat" },
			{ 0x174, "Doppleganger 40" },
			{ 0x16e, "Medusa" },
			{ 0x10b, "Akmodan" },
			{ 0x151, "Sypha" },
			{ 0x15f, "Shaft" },
			{ 0x12c, "Hippogryph" },
			{ 0X11F, "Scylla" },
			{ 0x127, "Granfaloon" },
			{ 0x172, "Creature" },
			{ 0x164, "Death" },
			{ 0x144, "Beezelbub" },
			{ 0x17b, "Dracula" },
		};
		public static readonly ushort GalamothTorsoId = 0xc6;
		public static readonly ushort GalamothHeadId = 0xc7;
		public static readonly ushort GalamothPartsId = 0xc6;
		public static readonly ushort ShaftOrbId = 0x0147;
		public static readonly ushort DoorId = 0x0003;

		public static readonly List<Entity> SpiritProperties = new List<Entity> { new Entity { EnemyId = 0x3, ObjectId = 0x3D } };
		public const byte SpiritPalette = 0x79;
		public const uint SpiritLockOn = 2;
		public const string SpiritLockOnName = "SpiritLockOn";
		public const uint WhiteTigerBallSpeedLeft = 0xFFFF;
		public const uint WhiteTigerBallSpeedRight = 0x0000;
		public const string WhiteTigerBallSpeedName = "WhiteTigerBallSpeed";
		public const int BloodthirstColorPalette = 33126;
		public const int OverdriveColorPalette = 33127;
		public const int QuadColorPalette = 33122;
		public const double SmallBulletSpeed = 0.7;

		public static readonly Entity KillEntity = new Entity { HitboxWidth = 0xFF, HitboxHeight = 0xFF, DamageType = Entities.Slam, Damage = 999, EnemyId = 0x8F };
		public static readonly Entity PoisonEntity = new Entity { HitboxWidth = 0xFF, HitboxHeight = 0xFF, DamageType = Entities.Poison, Damage = 1, HitboxState = 1, EnemyId = 0x8F };
		public static readonly Entity StoneEntity = new Entity { HitboxWidth = 0xFF, HitboxHeight = 0xFF, DamageType = Entities.Stone, Damage = 1, HitboxState = 1, EnemyId = 0x8F };
		public static readonly Entity CurseEntity = new Entity { HitboxWidth = 0xFF, HitboxHeight = 0xFF, DamageType = Entities.Curse, Damage = 1, HitboxState = 1, EnemyId = 0x8F };
		public static readonly Entity SlamEntity = new Entity { HitboxWidth = 0xFF, HitboxHeight = 0xFF, DamageType = Entities.Slam, Damage = 1, HitboxState = 1, EnemyId = 0x8F };
		public static readonly Entity StandEntity = new Entity { Palette = 0x005C, ZPriority = 0x0094, Flags = 0x08020000, AnimationSet = 0x01, Unk5A = 0x0B};
		public static readonly Entity DarkFireballEntity = new Entity
		{
			Xpos = 125d,
			Ypos = 125d,
			AccelerationX = 0,
			Palette = 0x0003,
			DrawFlags = 0x04,
			RotZ = 0x0800,
			ZPriority = 0x0096,
			ObjectId = 0x001B,
			UpdateFunctionAddress = 0x80127840,
			Step = 0x0001,
			Flags = 0x08100000,
			EnemyId = 0x0003,
			HitboxState = 0x0002,
			Damage = 0x0021,
			DamageType = 0x80,
			HitboxWidth = 0x08,
			HitboxHeight = 0x08,
			InvincibilityFrames = 0x04,
			AnimationFunctionAddress = 0x800B07C8,
			AnimationFrameIndex = 0x0017,
			AnimationFrameDuration = 0x0001,
			AnimationSet = 0x0009,
			AnimationCurrentFrame = 0x0009,
			StunFrames = 0x0008
		};
		public static readonly Entity FireballEntity = new Entity
		{
			Xpos = 125d,
			Ypos = 125d,
			AccelerationX = 1d,
			Palette = 0x0000,
			DrawFlags = 0x00,
			ZPriority = 0x0096,
			ObjectId = 0x001A,
			UpdateFunctionAddress = 0x000000DC,
			Step = 0x0000,
			Flags = 0x08010000,
			EnemyId = 0x0003,
			HitboxState = 0x0002,
			Damage = 0x0021,
			DamageType = 0x80,
			HitboxWidth = 0x04,
			HitboxHeight = 0x04,
			InvincibilityFrames = 0x14,
			AnimationFunctionAddress = 0x800B0798,
			AnimationFrameIndex = 0x0004,
			AnimationFrameDuration = 0x0001,
			AnimationSet = 0x0009,
			AnimationCurrentFrame = 0x0005,
			StunFrames = 0x0008
		};
		public static readonly Entity FlowerBullet = new Entity
		{
			Palette = 0x0024,
			HitboxWidth = 1,
			HitboxHeight = 1,
			Damage = 20,
			DamageType = 2,
			InvincibilityFrames = 1,
			ZPriority = 0x00FF,
			AnimationFrameIndex = 0x0001,
			AnimationSet = 0x0004,
			AnimationCurrentFrame = 0x0001,
			//AnimationFrameDuration = 0x0004,
			HitboxState = 0x1,
			EnemyId = 0x8F
		};
		public static readonly Entity ZigZagBullet = new Entity
		{
			Palette = 0x0025,
			HitboxWidth = 1,
			HitboxHeight = 1,
			Damage = 20,
			DamageType = 2,
			InvincibilityFrames = 1,
			ZPriority = 0x00FF,
			AnimationFrameIndex = 0x0001,
			AnimationSet = 0x0004,
			AnimationCurrentFrame = 0x0003,
			HitboxState = 0x1,
			EnemyId = 0x8F
		};
		public static readonly Entity JailBullet = new Entity
		{
			Palette = 0x006,
			HitboxWidth = 1,
			HitboxHeight = 1,
			Damage = 20,
			DamageType = 2,
			InvincibilityFrames = 1,
			ZPriority = 0x00FF,
			AnimationFrameIndex = 0x0001,
			AnimationSet = 0x0004,
			AnimationCurrentFrame = 0x0004,
			HitboxState = 0x1,
			EnemyId = 0x8F
		};
		public static readonly Entity DiamondBullet = new Entity
		{
			Palette = 0x002,
			HitboxWidth = 1,
			HitboxHeight = 1,
			Damage = 20,
			DamageType = 2,
			InvincibilityFrames = 1,
			ZPriority = 0x00FF,
			AnimationFrameIndex = 0x0001,
			AnimationSet = 0x0004,
			AnimationCurrentFrame = 0x0002,
			HitboxState = 0x1,
			EnemyId = 0x8F
		};

		public static readonly string[] AcceptedMusicTrackTitles =
	   {
			"lost painting",
			"cursed sanctuary",
			"requiem for the gods",
			"rainbow cemetary",
			"wood carving partita",
			"crystal teardrops",
			"marble gallery",
			"dracula castle",
			"the tragic prince",
			"tower of evil mist",
			"doorway of spirits",
			"dance of pearls",
			"abandoned pit",
			"heavenly doorway",
			"festival of servants",
			"dance of illusions",
			"prologue",
			"wandering ghosts",
			"doorway to the abyss",
			"metamorphosis",
			"metamorphosis 2",
			"dance of gold",
			"enchanted banquet",
			"prayer",
			"death's ballad",
			"blood relations",
			"finale toccata",
			"black banquet",
			"silence",
			"nocturne",
			"moonlight nocturne"
		};
		public static readonly Dictionary<string, string> AlternateTrackTitles = new Dictionary<string, string>
		{
			{ "deaths ballad", "death's ballad" },
			{ "death ballad", "death's ballad" },
			{ "poetic death", "death's ballad" },
			{ "illusionary dance", "dance of illusions" },
			{ "dracula", "dance of illusions" },
			{ "nocturne in the moonlight", "moonlight nocturne" },
			{ "dracula's castle", "dracula castle" },
			{ "draculas castle", "dracula castle" },
			{ "castle entrance", "dracula castle" },
			{ "entrance", "dracula castle" },
			{ "tower of mist", "tower of evil mist" },
			{ "outer wall", "tower of evil mist" },
			{ "library", "wood carving partita" },
			{ "alchemy lab", "dance of gold" },
			{ "alchemy laboratory", "dance of gold" },
			{ "chapel", "requiem for the gods" },
			{ "royal chapel", "requiem for the gods" },
			{ "crystal teardrop", "crystal teardrops" },
			{ "caverns", "crystal teardrops" },
			{ "underground caverns", "crystal teardrops" },
			{ "departer way", "abandoned pit" },
			{ "pit", "abandoned pit" },
			{ "mines", "abandoned pit" },
			{ "mine", "abandoned pit" },
			{ "catacombs", "rainbow cemetary" },
			{ "rainbow cemetery", "rainbow cemetary" },
			{ "lost paintings", "lost painting" },
			{ "antichapel", "lost painting" },
			{ "reverse caverns", "lost painting" },
			{ "forbidden library", "lost painting" },
			{ "waltz of pearls", "dance of pearls" },
			{ "olrox's quarters", "dance of pearls" },
			{ "olroxs quarters", "dance of pearls" },
			{ "olrox quarters", "dance of pearls" },
			{ "cursed zone", "cursed sanctuary" },
			{ "floating catacombs", "cursed sanctuary" },
			{ "reverse catacombs", "cursed sanctuary" },
			{ "demonic banquet", "enchanted banquet" },
			{ "medusa", "enchanted banquet" },
			{ "succubus", "enchanted banquet" },
			{ "colosseum", "wandering ghosts" },
			{ "wandering ghost", "wandering ghosts" },
			{ "pitiful scion", "the tragic prince" },
			{ "clock tower", "the tragic prince" },
			{ "tragic prince", "the tragic prince" },
			{ "alucard", "the tragic prince" },
			{ "door to the abyss", "doorway to the abyss" },
			{ "doorway to heaven", "heavenly doorway" },
			{ "keep", "heavenly doorway" },
			{ "castle keep", "heavenly doorway" },
			{ "divine bloodlines", "blood relations" },
			{ "strange bloodlines", "blood relations" },
			{ "richter belmont", "blood relations" },
			{ "richter", "blood relations" },
		};

		public const string KhaosName = "Khaos";

		public const float SuperWeakenFactor = 0.7F;
		public const float SuperCrippleFactor = 0.5F;
		public const int SlowQueueIntervalEnd = 3;
		public const int FastQueueIntervalStart = 8;
		public const uint SuperThirstExtraDrain = 1u;
		public const float ThirstLevelIncreaseRate = 0.005F;
		public const int HelpItemRetryCount = 15;
		public const float BattleOrdersHpMultiplier = 2F;
		public const uint GuiltyGearInvincibility = 3;
		public const uint GuiltyGearAttack = 50;
		public const uint GuiltyGearDefence = 50;
		public const uint GuiltyGearDarkMetamorphosis = 50;
		public const uint ShaftKhaosHp = 25;
		public const uint GalamothKhaosHp = 2000;
		public const uint GalamothKhaosPositionOffset = 100;
		public const float HasteDashFactor = 1.8F;
		public const int SaveIcosahedronFirstCastle = 0xBC9E;
		public const int SaveIcosahedronSecondCastle = 0x1144;
		public const int KhaosActionsCount = 30;

		public const int AutoKhaosDifficultyEasy = 70;
		public const int AutoKhaosDifficultyNormal = 50;
		public const int AutoKhaosDifficultyHard = 20;

		public const uint MinimumHp = 70;
		public const uint MinimumMp = 30;
		public const uint MinimumHearts = 60;
		public const uint MinimumStat = 6;
	}
}
