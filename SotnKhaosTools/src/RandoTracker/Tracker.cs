using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BizHawk.Client.Common;
using BizHawk.Emulation.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SotnApi.Constants.Values.Game;
using SotnApi.Interfaces;
using SotnKhaosTools.Configuration.Interfaces;
using SotnKhaosTools.Constants;
using SotnKhaosTools.Khaos.Interfaces;
using SotnKhaosTools.RandoTracker.Models;
using SotnKhaosTools.Services;

namespace SotnKhaosTools.RandoTracker
{
	internal sealed class Tracker : ITracker
	{
		private const string DefaultSeedInfo = "seed(preset)";
		private readonly ITrackerRenderer trackerRenderer;
		private readonly IToolConfig toolConfig;
		private readonly IWatchlistService watchlistService;
		private readonly ISotnApi sotnApi;
		private readonly INotificationService notificationService;

		private readonly List<Location> locations = new List<Location>
		{
			new Location { Name = "SoulOfBat", MapRow = 66, MapCol = 96, ClassicExtension = true, Y = 66, X = 192, WatchIndecies = new List<int>{0}, Rooms = new List<Room>{
					new Room { WatchIndex = 0, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "FireOfBat", MapRow = 26, MapCol = 118, ClassicExtension = true, Y = 26, X = 236, WatchIndecies = new List<int>{1}, Rooms = new List<Room>{
					new Room { WatchIndex = 1, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "EchoOfBat", MapRow = 46, MapCol = 32, ClassicExtension = true, Y = 46, X = 64, WatchIndecies = new List<int>{2}, Rooms = new List<Room>{
					new Room { WatchIndex = 2, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "SoulOfWolf", MapRow = 54, MapCol = 122, ClassicExtension = true, Y = 54, X = 244, WatchIndecies = new List<int>{3, 4}, Rooms = new List<Room>{
					new Room { WatchIndex = 3, Values = new int[] { 0x10 }},
					new Room { WatchIndex = 4, Values = new int[] { 0x10 }},
			}},
			new Location { Name = "PowerOfWolf", MapRow = 134, MapCol = 4, ClassicExtension = true, Y = 134, X = 8, WatchIndecies = new List<int>{5}, Rooms = new List<Room>{
					new Room { WatchIndex = 5, Values = new int[] { 0x01, 0x04 }},
			}},
			new Location { Name = "SkillOfWolf", MapRow = 114, MapCol = 30, ClassicExtension = true, Y = 114, X = 60, WatchIndecies = new List<int>{6}, Rooms = new List<Room>{
					new Room { WatchIndex = 6, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "FormOfMist", MapRow = 70, MapCol = 42, ClassicExtension = true, Y = 70, X = 84, WatchIndecies = new List<int>{7}, Rooms = new List<Room>{
					new Room { WatchIndex = 7, Values = new int[] { 0x04, 0x10 }},
			}},
			new Location { Name = "PowerOfMist", MapRow = 18, MapCol = 62, ClassicExtension = true, Y = 18, X = 124, WatchIndecies = new List<int>{8, 9, 10}, Rooms = new List<Room>{
					new Room { WatchIndex = 8, Values = new int[] { 0x01 }},
					new Room { WatchIndex = 9, Values = new int[] { 0x01 }},
					new Room { WatchIndex = 10, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "CubeOfZoe", MapRow = 126, MapCol = 38, ClassicExtension = true, Y = 126, X = 76, WatchIndecies = new List<int>{11}, Rooms = new List<Room>{
					new Room { WatchIndex = 11, Values = new int[] { 0x01, 0x04 }},
			}},
			new Location { Name = "SpiritOrb", MapRow = 107, MapCol = 50, ClassicExtension = true, Y = 107, X = 100, WatchIndecies = new List<int>{12, 13}, Rooms = new List<Room>{
					new Room { WatchIndex = 12, Values = new int[] { 0x10 }},
					new Room { WatchIndex = 13, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "GravityBoots", MapRow = 74, MapCol = 68, ClassicExtension = true, Y = 74, X = 136, WatchIndecies = new List<int>{14}, Rooms = new List<Room>{
					new Room { WatchIndex = 14, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "LeapStone", MapRow = 26, MapCol = 62, ClassicExtension = true, Y = 26, X = 124, WatchIndecies = new List<int>{15, 16}, Rooms = new List<Room>{
					new Room { WatchIndex = 15, Values = new int[] { 0x01 }},
					new Room { WatchIndex = 16, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "HolySymbol", MapRow = 146, MapCol = 110, ClassicExtension = true, Y = 146, X = 220, WatchIndecies = new List<int>{17}, Rooms = new List<Room>{
					new Room { WatchIndex = 17, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "FaerieScroll", MapRow = 54, MapCol = 118, ClassicExtension = true, Y = 54, X = 236, WatchIndecies = new List<int>{18}, Rooms = new List<Room>{
					new Room { WatchIndex = 18, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "JewelOfOpen", MapRow = 62, MapCol = 98, ClassicExtension = true, Y = 62, X = 196, WatchIndecies = new List<int>{19}, Rooms = new List<Room>{
					new Room { WatchIndex = 19, Values = new int[] { 0x10 }},
			}},
			new Location { Name = "MermanStatue", MapRow = 150, MapCol = 16, ClassicExtension = true, Y = 150, X = 32, WatchIndecies = new List<int>{20}, Rooms = new List<Room>{
					new Room { WatchIndex = 20, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "BatCard", MapRow = 90, MapCol = 26, ClassicExtension = true, Y = 90, X = 52, WatchIndecies = new List<int>{21}, Rooms = new List<Room>{
					new Room { WatchIndex = 21, Values = new int[] { 0x10 }},
			}},
			new Location { Name = "GhostCard", MapRow = 10, MapCol = 78, ClassicExtension = true, Y = 10, X = 156, WatchIndecies = new List<int>{22}, Rooms = new List<Room>{
					new Room { WatchIndex = 22, Values = new int[] { 0x01, 0x04 }},
			}},
			new Location { Name = "FaerieCard", MapRow = 54, MapCol = 104, ClassicExtension = true, Y = 54, X = 208, WatchIndecies = new List<int>{23}, Rooms = new List<Room>{
					new Room { WatchIndex = 23, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "DemonCard", MapRow = 158, MapCol = 58, ClassicExtension = true, Y = 158, X = 117, WatchIndecies = new List<int>{24}, Rooms = new List<Room>{
					new Room { WatchIndex = 24, Values = new int[] { 0x10 }},
			}},
			new Location { Name = "SwordCard", MapRow = 54, MapCol = 40, ClassicExtension = true, Y = 54, X = 80, WatchIndecies = new List<int>{25}, Rooms = new List<Room>{
					new Room { WatchIndex = 25, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "CrystalCloak", MapRow = 134, MapCol = 80,  GuardedExtension = true,  SpreadExtension = true, Y = 134, X = 160, WatchIndecies = new List<int>{26}, Rooms = new List<Room>{
					new Room { WatchIndex = 26, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "Mormegil", MapRow = 182, MapCol = 34,  GuardedExtension = true, Y = 182, X = 68, WatchIndecies = new List<int>{27}, Rooms = new List<Room>{
					new Room { WatchIndex = 27, Values = new int[] { 0x10 }},
			}},
			new Location { Name = "GoldRing", MapRow = 114, MapCol = 90, ClassicExtension = true, Y = 114, X = 180, WatchIndecies = new List<int>{28}, Rooms = new List<Room>{
					new Room { WatchIndex = 28, Values = new int[] { 0x10 }},
			}},
			new Location { Name = "Spikebreaker", MapRow = 186, MapCol = 82, ClassicExtension = true, Y = 186, X = 164, WatchIndecies = new List<int>{29}, Rooms = new List<Room>{
					new Room { WatchIndex = 29, Values = new int[] { 0x10 }},
			}},
			new Location { Name = "SilverRing", MapRow = 42, MapCol = 17, ClassicExtension = true, Y = 42, X = 32, WatchIndecies = new List<int>{30}, Rooms = new List<Room>{
					new Room { WatchIndex = 30, Values = new int[] { 0x10 }},
			}},
			new Location { Name = "HolyGlasses", MapRow = 106, MapCol = 64, ClassicExtension = true, Y = 106, X = 128, WatchIndecies = new List<int>{31}, Rooms = new List<Room>{
					new Room { WatchIndex = 31, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "HeartOfVlad", MapRow = 165, MapCol = 80, ClassicExtension = true,  SecondCastle = true, Y = 165, X = 160, WatchIndecies = new List<int>{32, 33}, Rooms = new List<Room>{
					new Room { WatchIndex = 32, Values = new int[] { 0x01 }},
					new Room { WatchIndex = 33, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "ToothOfVlad", MapRow = 125, MapCol = 12, ClassicExtension = true,  SecondCastle = true, Y = 125, X = 24, WatchIndecies = new List<int>{34}, Rooms = new List<Room>{
					new Room { WatchIndex = 34, Values = new int[] { 0x10, 0x04 }},
			}},
			new Location { Name = "RibOfVlad", MapRow = 153, MapCol = 88, ClassicExtension = true,  SecondCastle = true, Y = 153, X = 176, WatchIndecies = new List<int>{35}, Rooms = new List<Room>{
					new Room { WatchIndex = 35, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "RingOfVlad", MapRow = 177, MapCol = 46, ClassicExtension = true,  SecondCastle = true, Y = 177, X = 92, WatchIndecies = new List<int>{36}, Rooms = new List<Room>{
					new Room { WatchIndex = 36, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "EyeOfVlad", MapRow = 57, MapCol = 66, ClassicExtension = true,  SecondCastle = true, Y = 57, X = 132, WatchIndecies = new List<int>{37}, Rooms = new List<Room>{
					new Room { WatchIndex = 37, Values = new int[] { 0x10, 0x40 }},
			}},
			new Location { Name = "ForceOfEcho", MapRow = 53, MapCol = 16, ClassicExtension = true,  SecondCastle = true, Y = 53, X = 32, WatchIndecies = new List<int>{38}, Rooms = new List<Room>{
					new Room { WatchIndex = 38, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "GasCloud", MapRow = 17, MapCol = 92, ClassicExtension = true,  SecondCastle = true, Y = 17, X = 184, WatchIndecies = new List<int>{39}, Rooms = new List<Room>{
					new Room { WatchIndex = 39, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "RingOfArcana", MapRow = 109, MapCol = 100, SecondCastle = true,  GuardedExtension = true,  SpreadExtension = true, Y = 109, X = 200, WatchIndecies = new List<int>{40}, Rooms = new List<Room>{
					new Room { WatchIndex = 40, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "DarkBlade", MapRow = 65, MapCol = 46, SecondCastle = true,  GuardedExtension = true,  SpreadExtension = true, Y = 65, X = 92, WatchIndecies = new List<int>{41}, Rooms = new List<Room>{
					new Room { WatchIndex = 41, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Trio", MapRow = 129, MapCol = 86, SecondCastle = true,  GuardedExtension = true,  SpreadExtension = true, Y = 129, X = 172, WatchIndecies = new List<int>{42, 43}, Rooms = new List<Room>{
					new Room { WatchIndex = 42, Values = new int[] { 0x40 }},
					new Room { WatchIndex = 43, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Walk armor", MapRow = 182, MapCol = 46,  EquipmentExtension = true, Y = 183, X = 93, WatchIndecies = new List<int>{0}, Rooms = new List<Room>{
					new Room { WatchIndex = 0, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Icebrand", MapRow = 182, MapCol = 48,  EquipmentExtension = true, Y = 183, X = 97, WatchIndecies = new List<int>{1}, Rooms = new List<Room>{
					new Room { WatchIndex = 1, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "Bloodstone", MapRow = 182, MapCol = 56, EquipmentExtension = true, Y = 183, X = 113, WatchIndecies = new List<int>{3}, Rooms = new List<Room>{
					new Room { WatchIndex = 3, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "Combat knife", MapRow = 174, MapCol = 62, EquipmentExtension = true, Y = 175, X = 125, WatchIndecies = new List<int>{4}, Rooms = new List<Room>{
					new Room { WatchIndex = 4, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Ring of Ares", MapRow = 146, MapCol = 74, EquipmentExtension = true, Y = 147, X = 149, WatchIndecies = new List<int>{5}, Rooms = new List<Room>{
					new Room { WatchIndex = 5, Values = new int[] { 0x10 }},
			}},
			new Location { Name = "Knuckle duster", MapRow = 150, MapCol = 80, EquipmentExtension = true, Y = 151, X = 161, WatchIndecies = new List<int>{6}, Rooms = new List<Room>{
					new Room { WatchIndex = 6, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "Caverns Onyx", MapRow = 146, MapCol = 92, EquipmentExtension = true, Y = 147, X = 181, WatchIndecies = new List<int>{7}, Rooms = new List<Room>{
					new Room { WatchIndex = 7, Values = new int[] { 0x10, 0x40 }},
			}},
			new Location { Name = "Bandanna", MapRow = 90, MapCol = 70, EquipmentExtension = true, Y = 91, X = 141, WatchIndecies = new List<int>{11}, Rooms = new List<Room>{
					new Room { WatchIndex = 11, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Nunchaku", MapRow = 134, MapCol = 76, EquipmentExtension = true, Y = 133, X = 153, WatchIndecies = new List<int>{12}, Rooms = new List<Room>{
					new Room { WatchIndex = 12, Values = new int[] { 0x04, 0x10 }},
			}},
			new Location { Name = "Secret Boots", MapRow = 138, MapCol = 48, EquipmentExtension = true, Y = 139, X = 97, WatchIndecies = new List<int>{13, 14}, Rooms = new List<Room>{
					new Room { WatchIndex = 13, Values = new int[] { 0x01 }},
					new Room { WatchIndex = 14, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Holy mail", MapRow = 134, MapCol = 10, EquipmentExtension = true, Y = 135, X = 21, WatchIndecies = new List<int>{16}, Rooms = new List<Room>{
					new Room { WatchIndex = 16, Values = new int[] { 0x10 }},
			}},
			new Location { Name = "Jewel sword", MapRow = 146, MapCol = 20, EquipmentExtension = true, Y = 147, X = 41, WatchIndecies = new List<int>{17}, Rooms = new List<Room>{
					new Room { WatchIndex = 17, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "Sunglasses", MapRow = 106, MapCol = 32, EquipmentExtension = true, Y = 107, X = 65, WatchIndecies = new List<int>{20}, Rooms = new List<Room>{
					new Room { WatchIndex = 20, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "Basilard", MapRow = 118, MapCol = 32, EquipmentExtension = true, Y = 119, X = 65, WatchIndecies = new List<int>{21}, Rooms = new List<Room>{
					new Room { WatchIndex = 21, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "Cloth cape", MapRow = 98, MapCol = 20, EquipmentExtension = true, Y = 99, X = 41, WatchIndecies = new List<int>{22}, Rooms = new List<Room>{
					new Room { WatchIndex = 22, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "Mystic pendant", MapRow = 86, MapCol = 8, EquipmentExtension = true, Y = 87, X = 15, WatchIndecies = new List<int>{23, 24}, Rooms = new List<Room>{
					new Room { WatchIndex = 23, Values = new int[] { 0x40 }},
					new Room { WatchIndex = 24, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "Ankh of Life", MapRow = 78, MapCol = 12, EquipmentExtension = true, Y = 79, X = 23, WatchIndecies = new List<int>{25}, Rooms = new List<Room>{
					new Room { WatchIndex = 25, Values = new int[] { 0x04, 0x01 }},
			}},
			new Location { Name = "Morningstar", MapRow = 66, MapCol = 16, EquipmentExtension = true, Y = 67, X = 33, WatchIndecies = new List<int>{26, 27}, Rooms = new List<Room>{
					new Room { WatchIndex = 26, Values = new int[] { 0x10 }},
					new Room { WatchIndex = 27, Values = new int[] { 0x10 }},
			}},
			new Location { Name = "Goggles", MapRow = 66, MapCol = 20, EquipmentExtension = true, Y = 67, X = 41, WatchIndecies = new List<int>{28}, Rooms = new List<Room>{
					new Room { WatchIndex = 28, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "Silver plate", MapRow = 30, MapCol = 28, EquipmentExtension = true, Y = 31, X = 59, WatchIndecies = new List<int>{29}, Rooms = new List<Room>{
					new Room { WatchIndex = 29, Values = new int[] { 0x04, 0x01 }},
			}},
			new Location { Name = "Cutlass", MapRow = 22, MapCol = 54, EquipmentExtension = true, Y = 23, X = 111, WatchIndecies = new List<int>{30, 31}, Rooms = new List<Room>{
					new Room { WatchIndex = 30, Values = new int[] { 0x40 }},
					new Room { WatchIndex = 31, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Platinum mail", MapRow = 6, MapCol = 70, EquipmentExtension = true, Y = 7, X = 141, WatchIndecies = new List<int>{32}, Rooms = new List<Room>{
					new Room { WatchIndex = 32, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Falchion", MapRow = 14, MapCol = 78, EquipmentExtension = true, Y = 15, X = 157, WatchIndecies = new List<int>{33}, Rooms = new List<Room>{
					new Room { WatchIndex = 33, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Gold plate", MapRow = 38, MapCol = 98, EquipmentExtension = true, Y = 39, X = 197, WatchIndecies = new List<int>{34}, Rooms = new List<Room>{
					new Room { WatchIndex = 34, Values = new int[] { 0x10 }},
			}},
			new Location { Name = "Bekatowa", MapRow = 38, MapCol = 111, EquipmentExtension = true, Y = 39, X = 223, WatchIndecies = new List<int>{35, 36}, Rooms = new List<Room>{
					new Room { WatchIndex = 35, Values = new int[] { 0x01, 0x04 }},
					new Room { WatchIndex = 36, Values = new int[] { 0x01, 0x04 }},
			}},
			new Location { Name = "Gladius", MapRow = 74, MapCol = 118, EquipmentExtension = true, Y = 75, X = 237, WatchIndecies = new List<int>{37}, Rooms = new List<Room>{
					new Room { WatchIndex = 37, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Jewel knuckles", MapRow = 90, MapCol = 118, EquipmentExtension = true, Y = 91, X = 237, WatchIndecies = new List<int>{38}, Rooms = new List<Room>{
					new Room { WatchIndex = 38, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Bronze cuirass", MapRow = 66, MapCol = 98, EquipmentExtension = true, Y = 67, X = 197, WatchIndecies = new List<int>{39}, Rooms = new List<Room>{
					new Room { WatchIndex = 39, Values = new int[] { 0x10 }},
			}},
			new Location { Name = "Holy rod", MapRow = 54, MapCol = 100, EquipmentExtension = true, Y = 55, X = 201, WatchIndecies = new List<int>{40}, Rooms = new List<Room>{
					new Room { WatchIndex = 40, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "Library Onyx", MapRow = 66, MapCol = 94, EquipmentExtension = true, Y = 67, X = 186, WatchIndecies = new List<int>{41}, Rooms = new List<Room>{
					new Room { WatchIndex = 41, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "Alucart Sword", MapRow = 82, MapCol = 68, EquipmentExtension = true, Y = 83, X = 137, WatchIndecies = new List<int>{42}, Rooms = new List<Room>{
					new Room { WatchIndex = 42, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "Broadsword", MapRow = 70, MapCol = 64, EquipmentExtension = true, Y = 71, X = 129, WatchIndecies = new List<int>{43}, Rooms = new List<Room>{
					new Room { WatchIndex = 43, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "Estoc", MapRow = 42, MapCol = 60, EquipmentExtension = true, Y = 43, X = 121, WatchIndecies = new List<int>{44}, Rooms = new List<Room>{
					new Room { WatchIndex = 44, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "Olrox Garnet", MapRow = 54, MapCol = 66, EquipmentExtension = true, Y = 55, X = 133, WatchIndecies = new List<int>{45}, Rooms = new List<Room>{
					new Room { WatchIndex = 45, Values = new int[] { 0x10 }},
			}},
			new Location { Name = "Holy sword", MapRow = 62, MapCol = 38, EquipmentExtension = true, Y = 63, X = 77, WatchIndecies = new List<int>{46}, Rooms = new List<Room>{
					new Room { WatchIndex = 46, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Knight shield", MapRow = 70, MapCol = 28, EquipmentExtension = true, Y = 71, X = 57, WatchIndecies = new List<int>{47}, Rooms = new List<Room>{
					new Room { WatchIndex = 47, Values = new int[] { 0x01, 0x04 }},
			}},
			new Location { Name = "Shield rod", MapRow = 78, MapCol = 26, EquipmentExtension = true, Y = 79, X = 53, WatchIndecies = new List<int>{48}, Rooms = new List<Room>{
					new Room { WatchIndex = 48, Values = new int[] { 0x10 }},
			}},
			new Location { Name = "Blood cloak", MapRow = 78, MapCol = 40, EquipmentExtension = true, Y = 79, X = 81, WatchIndecies = new List<int>{49}, Rooms = new List<Room>{
					new Room { WatchIndex = 49, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "Bastard sword", MapRow = 193, MapCol = 60, SecondCastle = true,  EquipmentExtension = true, Y = 194, X = 121, WatchIndecies = new List<int>{50}, Rooms = new List<Room>{
					new Room { WatchIndex = 50, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "Royal cloak", MapRow = 193, MapCol = 56, SecondCastle = true,  EquipmentExtension = true, Y = 194, X = 113, WatchIndecies = new List<int>{51}, Rooms = new List<Room>{
					new Room { WatchIndex = 51, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "Lightning mail", MapRow = 173, MapCol = 48, SecondCastle = true,  EquipmentExtension = true, Y = 174, X = 97, WatchIndecies = new List<int>{52}, Rooms = new List<Room>{
					new Room { WatchIndex = 52, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "Sword of Dawn", MapRow = 173, MapCol = 64, SecondCastle = true,  EquipmentExtension = true, Y = 174, X = 129, WatchIndecies = new List<int>{53, 54}, Rooms = new List<Room>{
					new Room { WatchIndex = 53, Values = new int[] { 0x40 }},
					new Room { WatchIndex = 54, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Moon rod", MapRow = 173, MapCol = 42, SecondCastle = true,  EquipmentExtension = true, Y = 174, X = 85, WatchIndecies = new List<int>{55}, Rooms = new List<Room>{
					new Room { WatchIndex = 55, Values = new int[] { 0x10 }},
			}},
			new Location { Name = "Sunstone", MapRow = 161, MapCol = 28, SecondCastle = true,  EquipmentExtension = true, Y = 162, X = 57, WatchIndecies = new List<int>{56}, Rooms = new List<Room>{
					new Room { WatchIndex = 56, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "Luminus", MapRow = 164, MapCol = 16, SecondCastle = true,  EquipmentExtension = true, Y = 165, X = 33, WatchIndecies = new List<int>{57}, Rooms = new List<Room>{
					new Room { WatchIndex = 57, Values = new int[] { 0x10, 0x40 }},
			}},
			new Location { Name = "Dragon helm", MapRow = 173, MapCol = 8, SecondCastle = true,  EquipmentExtension = true, Y = 174, X = 17, WatchIndecies = new List<int>{58}, Rooms = new List<Room>{
					new Room { WatchIndex = 58, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "Shotel", MapRow = 109, MapCol = 8, SecondCastle = true,  EquipmentExtension = true, Y = 110, X = 17, WatchIndecies = new List<int>{59}, Rooms = new List<Room>{
					new Room { WatchIndex = 59, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "Badelaire", MapRow = 145, MapCol = 26, SecondCastle = true,  EquipmentExtension = true, SpreadExtension = true, Y = 146, X = 53, WatchIndecies = new List<int>{60}, Rooms = new List<Room>{
					new Room { WatchIndex = 60, Values = new int[] { 0x10 }},
			}},
			new Location { Name = "Staurolite", MapRow = 132, MapCol = 30, SecondCastle = true,  EquipmentExtension = true, Y = 133, X = 61, WatchIndecies = new List<int>{62}, Rooms = new List<Room>{
					new Room { WatchIndex = 62, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "Forbidden Library Opal", MapRow = 137, MapCol = 27, SecondCastle = true,  EquipmentExtension = true, SpreadExtension = true, Y = 138, X = 57, WatchIndecies = new List<int>{61}, Rooms = new List<Room>{
					new Room { WatchIndex = 61, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "Reverse Caverns Diamond", MapRow = 109, MapCol = 56, SecondCastle = true,  EquipmentExtension = true, Y = 110, X = 113, WatchIndecies = new List<int>{63}, Rooms = new List<Room>{
					new Room { WatchIndex = 63, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "Reverse Caverns Opal", MapRow = 89, MapCol = 52, SecondCastle = true,  EquipmentExtension = true, Y = 90, X = 101, WatchIndecies = new List<int>{64}, Rooms = new List<Room>{
					new Room { WatchIndex = 64, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "Reverse Caverns Garnet", MapRow = 69, MapCol = 82, SecondCastle = true,  EquipmentExtension = true, Y = 70, X = 165, WatchIndecies = new List<int>{65}, Rooms = new List<Room>{
					new Room {  WatchIndex = 65, Values = new int[] { 0x10 }},
			}},
			new Location { Name = "Osafune katana", MapRow = 49, MapCol = 76, SecondCastle = true,  EquipmentExtension = true, Y = 50, X = 153, WatchIndecies = new List<int>{66}, Rooms = new List<Room>{
					new Room { WatchIndex = 66, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "Alucard shield", MapRow = 49, MapCol = 110, SecondCastle = true,  EquipmentExtension = true, Y = 50, X = 221, WatchIndecies = new List<int>{67}, Rooms = new List<Room>{
					new Room { WatchIndex = 67, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Alucard sword", MapRow = 41, MapCol = 68, SecondCastle = true,  EquipmentExtension = true, Y = 42, X = 137, WatchIndecies = new List<int>{68}, Rooms = new List<Room>{
					new Room { WatchIndex = 68, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "Necklace of J", MapRow = 17, MapCol = 78, SecondCastle = true,  EquipmentExtension = true, Y = 18, X = 157, WatchIndecies = new List<int>{69}, Rooms = new List<Room>{
					new Room { WatchIndex = 69, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Floating Catacombs Diamond", MapRow = 17, MapCol = 80, SecondCastle = true,  EquipmentExtension = true, Y = 18, X = 161, WatchIndecies = new List<int>{70}, Rooms = new List<Room>{
					new Room { WatchIndex = 70, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "Talwar",  SecondCastle = true, MapRow = 173, MapCol = 88, EquipmentExtension = true, Y = 174, X = 177, WatchIndecies = new List<int>{71, 72}, Rooms = new List<Room>{
					new Room { WatchIndex = 71, Values = new int[] { 0x01 }},
					new Room { WatchIndex = 72, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "Twilight cloak", MapRow = 157, MapCol = 110, SecondCastle = true,  EquipmentExtension = true, Y = 158, X = 221, WatchIndecies = new List<int>{73}, Rooms = new List<Room>{
					new Room { WatchIndex = 73, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "Alucard Mail", MapRow = 145, MapCol = 60, SecondCastle = true,  EquipmentExtension = true, Y = 146, X = 121, WatchIndecies = new List<int>{74}, Rooms = new List<Room>{
					new Room { WatchIndex = 74, Values = new int[] { 0x04 }},
			}},
			new Location { Name = "Sword of Hador", MapRow = 129, MapCol = 62, SecondCastle = true,  EquipmentExtension = true, Y = 130, X = 125, WatchIndecies = new List<int>{75}, Rooms = new List<Room>{
					new Room { WatchIndex = 75, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Fury Plate", MapRow = 137, MapCol = 88, SecondCastle = true,  EquipmentExtension = true, Y = 138, X = 177, WatchIndecies = new List<int>{76}, Rooms = new List<Room>{
					new Room { WatchIndex = 76, Values = new int[] { 0x40 }},
			}},
			new Location { Name = "Gram", MapRow = 121, MapCol = 86, SecondCastle = true,  EquipmentExtension = true, Y = 122, X = 173, WatchIndecies = new List<int>{77}, Rooms = new List<Room>{
					new Room { WatchIndex = 77, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Goddess shield", MapRow = 93, MapCol = 94, SecondCastle = true,  EquipmentExtension = true, Y = 94, X = 189, WatchIndecies = new List<int>{78}, Rooms = new List<Room>{
					new Room { WatchIndex = 78, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Katana", MapRow = 73, MapCol = 102, SecondCastle = true,  EquipmentExtension = true, Y = 74, X = 205, WatchIndecies = new List<int>{79}, Rooms = new List<Room>{
					new Room { WatchIndex = 79, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Talisman", MapRow = 69, MapCol = 86, SecondCastle = true,  EquipmentExtension = true, Y = 70, X = 173, WatchIndecies = new List<int>{80}, Rooms = new List<Room>{
					new Room { WatchIndex = 80, Values = new int[] { 0x01 }},
			}},
			new Location { Name = "Beryl circlet", MapRow = 53, MapCol = 106, SecondCastle = true,  EquipmentExtension = true, Y = 54, X = 213, WatchIndecies = new List<int>{81}, Rooms = new List<Room>{
					new Room { WatchIndex = 81, Values = new int[] { 0x10 }},
			}},
		};
		private readonly List<TrackerRelic> relics = new List<TrackerRelic>
		{
				new TrackerRelic { Name = "SoulOfBat", Progression = true},
				new TrackerRelic { Name = "FireOfBat", Progression = false},
				new TrackerRelic { Name = "EchoOfBat", Progression = true},
				new TrackerRelic { Name = "ForceOfEcho", Progression = false},
				new TrackerRelic { Name = "SoulOfWolf", Progression = true},
				new TrackerRelic { Name = "PowerOfWolf", Progression = true},
				new TrackerRelic { Name = "SkillOfWolf", Progression = true},
				new TrackerRelic { Name = "FormOfMist", Progression = true},
				new TrackerRelic { Name = "PowerOfMist", Progression = true},
				new TrackerRelic { Name = "GasCloud", Progression = false},
				new TrackerRelic { Name = "CubeOfZoe", Progression = false},
				new TrackerRelic { Name = "SpiritOrb", Progression = false},
				new TrackerRelic { Name = "GravityBoots", Progression = true},
				new TrackerRelic { Name = "LeapStone", Progression = true},
				new TrackerRelic { Name = "HolySymbol", Progression = false},
				new TrackerRelic { Name = "FaerieScroll", Progression = false},
				new TrackerRelic { Name = "JewelOfOpen", Progression = true},
				new TrackerRelic { Name = "MermanStatue", Progression = true},
				new TrackerRelic { Name = "BatCard", Progression = false},
				new TrackerRelic { Name = "GhostCard", Progression = false},
				new TrackerRelic { Name = "FaerieCard", Progression = false},
				new TrackerRelic { Name = "DemonCard", Progression = false},
				new TrackerRelic { Name = "SwordCard", Progression = false},
				new TrackerRelic { Name = "SpriteCard" , Progression = false},
				new TrackerRelic { Name = "NoseDevilCard", Progression = false},
				new TrackerRelic { Name = "HeartOfVlad", Progression = true},
				new TrackerRelic { Name = "ToothOfVlad", Progression = true},
				new TrackerRelic { Name = "RibOfVlad", Progression = true},
				new TrackerRelic { Name = "RingOfVlad", Progression = true},
				new TrackerRelic { Name = "EyeOfVlad", Progression = true}
		};
		private readonly List<Item> progressionItems = new List<Item>
		{
			new Item { Name = "GoldRing", Value = 72 },
			new Item { Name = "SilverRing", Value = 73 },
			new Item { Name = "SpikeBreaker", Value = 14 },
			new Item { Name = "HolyGlasses", Value = 34 }
		};
		private readonly List<Item> thrustSwords = new List<Item>
		{
			new Item { Name = "Estoc", Value = 95 },
			new Item { Name = "Claymore", Value = 98 },
			new Item { Name = "Flamberge", Value = 101 },
			new Item { Name = "Zweihander", Value = 103 },
			new Item { Name = "ObsidianSword", Value = 107 }
		};
		private readonly Dictionary<string, int> relicToIndex = new Dictionary<string, int>
		{
			{ "soulofbat", 0 },
			{ "fireofbat", 1 },
			{ "echoofbat", 2 },
			{ "forceofecho", 3 },
			{ "soulofwolf", 4 },
			{ "powerofwolf", 5 },
			{ "skillofwolf", 6 },
			{ "formofmist", 7 },
			{ "powerofmist", 8 },
			{ "gascloud", 9 },
			{ "cubeofzoe", 10 },
			{ "spiritorb", 11 },
			{ "gravityboots", 12 },
			{ "leapstone", 13 },
			{ "holysymbol", 14 },
			{ "faeriescroll", 15 },
			{ "jewelofopen", 16 },
			{ "mermanstatue", 17 },
			{ "batcard", 18 },
			{ "ghostcard", 19 },
			{ "faeriecard", 20 },
			{ "demoncard", 21 },
			{ "swordcard", 22 },
			{ "spritecard", 23 },
			{ "nosedevilcard", 24 },
			{ "heartofvlad", 25 },
			{ "toothofvlad", 26 },
			{ "ribofvlad", 27 },
			{ "ringofvlad", 28 },
			{ "eyeofvlad", 29 }
		};
		private readonly Dictionary<string, int> itemToIndex = new Dictionary<string, int>
		{
			{ "goldring", 0 },
			{ "silverring", 1 },
			{ "spikebreaker", 2 },
			{ "holyglasses", 3 },
		};
		private readonly Dictionary<string, int> swordToIndex = new Dictionary<string, int>
		{
			{ "estoc", 0 },
			{ "claymore", 1 },
			{ "flamberge", 2 },
			{ "zweihander", 3 },
			{ "obsidiansword", 4 },
		};
		private readonly HashSet<string> VanillaPresets = new HashSet<string>
		{
			"adventure",
			"bat-master",
			"casual",
			"empty-hand",
			"expedition",
			//"gem-farmer",
			//"glitch",
			"guarded-og",
			"lycanthrope",
			"nimble",
			"og",
			//"rat-race",
			"safe",
			"scavenger",
			"speedrun",
			"third-castle",
			"warlock"
		};
		private readonly int[] watchToLocation = new int[300];
		private readonly int[] watchToLocationEquipment = new int[300];
		private readonly int[] watchToRoom = new int[300];
		private readonly int[] watchToRoomEquipment = new int[300];

		private string preset = "";
		private string seedName = "";
		private uint roomCount = 2;
		private bool inGame = false;
		private bool classicExtension = true;
		private bool guardedExtension = true;
		private bool equipmentExtension = false;
		private bool spreadExtension = false;
		private bool customExtension = false;
		private bool gameReset = true;
		private bool secondCastle = false;
		private bool restarted = false;
		private int replayLenght = 0;
		private ushort prologueTime = 0;
		private bool draculaSpawned = false;
		private byte currentMapX = (byte) 0;
		private byte currentMapY = (byte) 0;
		private bool started = false;
		private bool finished = false;

		public Tracker(ITrackerRenderer trackerRenderer, IToolConfig toolConfig, IWatchlistService watchlistService, ISotnApi sotnApi, INotificationService notificationService)
		{
			if (trackerRenderer is null) throw new ArgumentNullException(nameof(trackerRenderer));
			if (toolConfig is null) throw new ArgumentNullException(nameof(toolConfig));
			if (watchlistService is null) throw new ArgumentNullException(nameof(watchlistService));
			if (sotnApi is null) throw new ArgumentNullException(nameof(sotnApi));
			if (notificationService is null) throw new ArgumentNullException(nameof(notificationService));
			this.trackerRenderer = trackerRenderer;
			this.toolConfig = toolConfig;
			this.watchlistService = watchlistService;
			this.sotnApi = sotnApi;
			this.notificationService = notificationService;

			if (toolConfig.Tracker.Locations)
			{
				InitializeWatchMaps();
				InitializeAllLocks();
				CheckReachability();
			}

			trackerRenderer.InitializeItems(relics, progressionItems, thrustSwords);
			trackerRenderer.CalculateGrid(toolConfig.Tracker.Width, toolConfig.Tracker.Height);
			this.SeedInfo = DefaultSeedInfo;
			trackerRenderer.SeedInfo = SeedInfo;
			trackerRenderer.Render();
		}

		public string SeedInfo { get; set; }

		public IRelicLocationDisplay RelicLocationDisplay { get; set; }

		public void Update()
		{
			UpdateSeedLabel();

			if (toolConfig.Tracker.Stereo && sotnApi.GameApi.Status == Status.MainMenu)
			{
				sotnApi.GameApi.EnableStartWithStereo();
			}

			inGame = sotnApi.GameApi.Status == Status.InGame;
			bool updatedSecondCastle = sotnApi.GameApi.SecondCastle;
			currentMapX = (byte) sotnApi.AlucardApi.MapX;
			currentMapY = (byte) sotnApi.AlucardApi.MapY;

			if (sotnApi.GameApi.InAlucardMode() && sotnApi.AlucardApi.HasHitbox())
			{
				restarted = false;
				bool objectChanges = false;

				if (updatedSecondCastle != secondCastle && toolConfig.Tracker.Locations)
				{
					secondCastle = updatedSecondCastle;
					CheckReachability();
					SetMapLocations();
				}
				else if (updatedSecondCastle != secondCastle)
				{
					secondCastle = updatedSecondCastle;
				}

				objectChanges = objectChanges || UpdateRelics();
				objectChanges = objectChanges || UpdateProgressionItems();
				objectChanges = objectChanges || UpdateThrustSwords();
				if (objectChanges)
				{
					trackerRenderer.Render();
					if (toolConfig.Tracker.Locations)
					{
						CheckReachability();
					}
				}

				if (toolConfig.Tracker.Locations)
				{
					UpdateLocations();
				}
				if (gameReset && toolConfig.Tracker.Locations)
				{
					SetMapLocations();
					gameReset = false;
				}
				if (!LocationsDrawn() && toolConfig.Tracker.Locations)
				{
					SetMapLocations();
				}
			}
			else if (!inGame)
			{
				gameReset = true;
			}
			else if (sotnApi.GameApi.InPrologue())
			{
				prologueTime++;
				if (!restarted)
				{
					ResetToDefaults();
					trackerRenderer.Render();
					restarted = true;
				}
			}
		}

		private void InitializeWatchMaps()
		{
			for (int i = 0; i < 37; i++)
			{
				for (int j = 0; j < locations[i].WatchIndecies.Count; j++)
				{
					watchToLocation[locations[i].WatchIndecies[j]] = i;
				}
				for (int j = 0; j < locations[i].Rooms.Count; j++)
				{
					watchToRoom[locations[i].Rooms[j].WatchIndex] = j;
				}
			}
			for (int i = 37; i < locations.Count; i++)
			{
				for (int j = 0; j < locations[i].WatchIndecies.Count; j++)
				{
					watchToLocationEquipment[locations[i].WatchIndecies[j]] = i;
				}
				for (int j = 0; j < locations[i].Rooms.Count; j++)
				{
					watchToRoomEquipment[locations[i].Rooms[j].WatchIndex] = j;
				}
			}
		}

		private void InitializeAllLocks()
		{
			LoadLocks(Paths.CasualPresetPath);
			LoadLocks(Paths.SpeedrunPresetPath, true);
		}

		private bool LoadExtension(string extensionFilePath)
		{
			if (!File.Exists(extensionFilePath))
			{
				return false;
			}

			Extension? extension = JsonConvert.DeserializeObject<Extension>(File.ReadAllText(extensionFilePath));
			MemoryDomain domain = watchlistService.SafeLocationWatches[0].Domain;
			customExtension = true;

			switch (extension.Extends)
			{
				case "equipment":
					equipmentExtension = true;
					guardedExtension = true;
					classicExtension = true;
					SetEquipmentProgression();
					break;
				case "guarded":
					classicExtension = true;
					guardedExtension = true;
					break;
				case "spread":
					spreadExtension = true;
					classicExtension = true;
					break;
				case "classic":
					classicExtension = true;
					break;
				default:
					break;
			}

			foreach (ExtensionLocation location in extension.Locations)
			{
				Location? customLocation = locations.Where(x => x.Name == location.Name).FirstOrDefault();

				if (customLocation is null)
				{
					customLocation = new Location
					{
						Name = location.Name,
						X = location.X,
						Y = location.Y,
						SecondCastle = location.SecondCastle,
						CustomExtension = true
					};
				}
				else
				{
					customLocation.X = location.X;
					customLocation.Y = location.Y;
					customLocation.SecondCastle = location.SecondCastle;
					customLocation.CustomExtension = true;
				}

				int roomCounter = 0;
				foreach (var room in location.Rooms)
				{
					watchlistService.SafeLocationWatches.Add(Watch.GenerateWatch(domain, Convert.ToInt64(room.Address, 16), WatchSize.Byte, WatchDisplayType.Hex, false, location.Name + roomCounter));
					roomCounter++;
					List<int> values = new List<int>();
					foreach (var value in room.Values)
					{
						values.Add(Convert.ToInt32(value.ToString(), 16));
					}

					Room customRoom = new Room { WatchIndex = watchlistService.SafeLocationWatches.Count - 1, Values = values.ToArray() };
					customLocation.WatchIndecies.Add(watchlistService.SafeLocationWatches.Count - 1);
					customLocation.Rooms.Add(customRoom);
					watchToRoom[watchlistService.SafeLocationWatches.Count - 1] = roomCounter - 1;
					watchToLocation[watchlistService.SafeLocationWatches.Count - 1] = locations.Count;
				}
				locations.Add(customLocation);
			}
			return true;
		}

		private void LoadLocks(string presetFilePath, bool outOfLogic = false, bool overwriteLocks = false)
		{
			if (!File.Exists(presetFilePath))
			{
				return;
			}

			Preset? preset = JsonConvert.DeserializeObject<Preset>(File.ReadAllText(presetFilePath));
			bool isVanilla = VanillaPresets.Contains(preset.Metadata.Id);

			if (preset.Metadata.Id == "glitch")
			{
				preset.RelicLocationsExtension = "equipment";
			}

			switch (preset.RelicLocationsExtension)
			{
				case "equipment":
					equipmentExtension = true;
					guardedExtension = true;
					classicExtension = true;
					SetEquipmentProgression();
					break;
				case "spread":
					spreadExtension = true;
					classicExtension = true;
					break;
				case "false":
					classicExtension = true;
					guardedExtension = false;
					break;
				default:
					if (!LoadExtension(Paths.PresetPath + preset.RelicLocationsExtension + ".json"))
					{
						guardedExtension = true;
						classicExtension = true;
					}
					break;
			}

			foreach (LockLocation location in preset.LockLocations)
			{
				var trackerLocation = locations.Where(x => x.Name.Replace(" ", String.Empty).ToLower() == location.Name.Replace(" ", String.Empty).ToLower()).FirstOrDefault();

				if (trackerLocation == null)
				{
					trackerLocation = new Location
					{
						Name = location.Name
					};
					locations.Add(trackerLocation);
				}

				if (overwriteLocks)
				{
					if (outOfLogic || !isVanilla)
					{
						trackerLocation.OutOfLogicLocks.Clear();
					}
					if (!outOfLogic)
					{
						trackerLocation.Locks.Clear();
					}
				}

				foreach (string lockSet in location.Locks)
				{
					if (outOfLogic)
					{
						trackerLocation.OutOfLogicLocks.Add(lockSet.Replace(" ", String.Empty).ToLower().Split('+'));
					}
					else
					{
						trackerLocation.Locks.Add(lockSet.Replace(" ", String.Empty).ToLower().Split('+'));
					}
				}
			}

			foreach (LockLocation location in preset.LockLocationsAllowed)
			{
				var trackerLocation = locations.Where(x => x.Name.Replace(" ", String.Empty).ToLower() == location.Name).FirstOrDefault();

				if (trackerLocation == null)
				{
					continue;
				}

				trackerLocation.OutOfLogicLocks.Clear();

				foreach (string lockSet in location.Locks)
				{
					trackerLocation.OutOfLogicLocks.Add(lockSet.Replace(" ", String.Empty).ToLower().Split('+'));
				}
			}
		}

		private void ResetToDefaults()
		{
			roomCount = 2;
			foreach (var relic in relics)
			{
				relic.Collected = false;
			}
			foreach (var item in progressionItems)
			{
				item.Status = false;
			}
			foreach (var sword in thrustSwords)
			{
				sword.Status = false;
			}
			foreach (var location in locations)
			{
				location.Visited = false;
				location.AvailabilityColor = MapColor.Unavailable;
			}
			PrepareMapLocations();
			if (toolConfig.Tracker.Locations)
			{
				CheckReachability();
			}
		}

		private void UpdateSeedLabel()
		{
			if (SeedInfo == DefaultSeedInfo && sotnApi.GameApi.Status == Status.MainMenu)
			{
				GetSeedData();
				trackerRenderer.Render();
			}
		}

		private void UpdateLocations()
		{
			uint currentRooms = sotnApi.AlucardApi.Rooms;
			if (currentRooms <= roomCount)
			{
				roomCount = currentRooms;
				return;
			}
			watchlistService.UpdateWatchlist(watchlistService.SafeLocationWatches);
			CheckRooms(watchlistService.SafeLocationWatches);
			watchlistService.SafeLocationWatches.ClearChangeCounts();
			if (equipmentExtension || spreadExtension)
			{
				watchlistService.UpdateWatchlist(watchlistService.EquipmentLocationWatches);
				CheckRooms(watchlistService.EquipmentLocationWatches, true);
				watchlistService.EquipmentLocationWatches.ClearChangeCounts();
			}

			roomCount = currentRooms;
		}

		private bool UpdateRelics()
		{
			int changes = 0;

			watchlistService.UpdateWatchlist(watchlistService.RelicWatches);
			for (int i = 0; i < watchlistService.RelicWatches.Count; i++)
			{
				if (watchlistService.RelicWatches[i].ChangeCount == 0)
				{
					continue;
				}

				if (watchlistService.RelicWatches[i].Value > 0)
				{
					if (!relics[i].Collected)
					{
						changes++;
						relics[i].Collected = true;
						if (RelicLocationDisplay is not null)
						{
							SetNearestLocation(relics[i].Name);
						}
					}
				}
				else
				{
					if (relics[i].Collected)
					{
						changes++;
						relics[i].Collected = false;
					}
				}
			}
			watchlistService.RelicWatches.ClearChangeCounts();

			return changes > 0;
		}

		private void SetNearestLocation(string relicName)
		{

			float adjustedX = sotnApi.AlucardApi.MapX * 2;
			float adjustedY = (secondCastle ? (sotnApi.AlucardApi.MapY - 8.75f) : (sotnApi.AlucardApi.MapY - 4.5f)) * 4;
			Location location = locations.Where(l => (l.MapRow >= adjustedY - 3 && l.MapRow <= adjustedY + 3) && (l.MapCol >= adjustedX - 3 && l.MapCol <= adjustedX + 3)).FirstOrDefault();
			if (location is not null)
			{
				SetLocationDisplay(location, relicName);
				return;
			}

			uint mapX = sotnApi.AlucardApi.MapX;
			uint mapY = sotnApi.AlucardApi.MapY;
			for (int i = 0; i < locations.Count; i++)
			{
				if (locations[i].SecondCastle != secondCastle)
				{
					continue;
				}
				int scaledX = (int)locations[i].X / 4;
				int scaledY = (int)(locations[i].Y / 3.3);

				long diffX = Math.Abs((int)mapX - (int)scaledX);
				long diffY = Math.Abs(mapY - scaledY);

				if (diffX < 2 && diffY < 2)
				{
					SetLocationDisplay(locations[i], relicName);
					return;
				}
			}
		}

		private void SetLocationDisplay(Location? location, string relicName)
		{
			string locationName = String.Empty;

			if (location is not null)
			{
				locationName = location.Name;
			}

			switch (relicName)
			{
				case "HeartOfVlad":
					if (RelicLocationDisplay.HeartOfVladLocation == String.Empty)
					{
						RelicLocationDisplay.HeartOfVladLocation = locationName;
					}
					else if (RelicLocationDisplay.HeartOfVladLocation == Constants.Khaos.KhaosName)
					{
						RelicLocationDisplay.HeartOfVladLocation = String.Empty;
					}
					break;
				case "ToothOfVlad":
					if (RelicLocationDisplay.ToothOfVladLocation == String.Empty)
					{
						RelicLocationDisplay.ToothOfVladLocation = locationName;
					}
					else if (RelicLocationDisplay.ToothOfVladLocation == Constants.Khaos.KhaosName)
					{
						RelicLocationDisplay.ToothOfVladLocation = String.Empty;
					}
					break;
				case "RibOfVlad":
					if (RelicLocationDisplay.RibOfVladLocation == String.Empty)
					{
						RelicLocationDisplay.RibOfVladLocation = locationName;
					}
					else if (RelicLocationDisplay.RibOfVladLocation == Constants.Khaos.KhaosName)
					{
						RelicLocationDisplay.RibOfVladLocation = String.Empty;
					}
					break;
				case "RingOfVlad":
					if (RelicLocationDisplay.RingOfVladLocation == String.Empty)
					{
						RelicLocationDisplay.RingOfVladLocation = locationName;
					}
					else if (RelicLocationDisplay.RingOfVladLocation == Constants.Khaos.KhaosName)
					{
						RelicLocationDisplay.RingOfVladLocation = String.Empty;
					}
					break;
				case "EyeOfVlad":
					if (RelicLocationDisplay.EyeOfVladLocation == String.Empty)
					{
						RelicLocationDisplay.EyeOfVladLocation = locationName;
					}
					else if (RelicLocationDisplay.EyeOfVladLocation == Constants.Khaos.KhaosName)
					{
						RelicLocationDisplay.EyeOfVladLocation = String.Empty;
					}
					break;
				case "SoulOfBat":
					if (RelicLocationDisplay.BatLocation == String.Empty)
					{
						RelicLocationDisplay.BatLocation = locationName;
					}
					else if (RelicLocationDisplay.BatLocation == Constants.Khaos.KhaosName)
					{
						RelicLocationDisplay.BatLocation = String.Empty;
					}
					break;
				case "SoulOfWolf":
					if (RelicLocationDisplay.WolfLocation == String.Empty)
					{
						RelicLocationDisplay.WolfLocation = locationName;
					}
					else if (RelicLocationDisplay.WolfLocation == Constants.Khaos.KhaosName)
					{
						RelicLocationDisplay.WolfLocation = String.Empty;
					}
					break;
				case "FormOfMist":
					if (RelicLocationDisplay.MistLocation == String.Empty)
					{
						RelicLocationDisplay.MistLocation = locationName;
					}
					else if (RelicLocationDisplay.MistLocation == Constants.Khaos.KhaosName)
					{
						RelicLocationDisplay.MistLocation = String.Empty;
					}
					break;
				case "PowerOfMist":
					if (RelicLocationDisplay.PowerOfMistLocation == String.Empty)
					{
						RelicLocationDisplay.PowerOfMistLocation = locationName;
					}
					else if (RelicLocationDisplay.PowerOfMistLocation == Constants.Khaos.KhaosName)
					{
						RelicLocationDisplay.PowerOfMistLocation = String.Empty;
					}
					break;
				case "JewelOfOpen":
					if (RelicLocationDisplay.JewelOfOpenLocation == String.Empty)
					{
						RelicLocationDisplay.JewelOfOpenLocation = locationName;
					}
					else if (RelicLocationDisplay.JewelOfOpenLocation == Constants.Khaos.KhaosName)
					{
						RelicLocationDisplay.JewelOfOpenLocation = String.Empty;
					}
					break;
				case "GravityBoots":
					if (RelicLocationDisplay.GravityBootsLocation == String.Empty)
					{
						RelicLocationDisplay.GravityBootsLocation = locationName;
					}
					else if (RelicLocationDisplay.GravityBootsLocation == Constants.Khaos.KhaosName)
					{
						RelicLocationDisplay.GravityBootsLocation = String.Empty;
					}
					break;
				case "LeapStone":
					if (RelicLocationDisplay.LepastoneLocation == String.Empty)
					{
						RelicLocationDisplay.LepastoneLocation = locationName;
					}
					else if (RelicLocationDisplay.LepastoneLocation == Constants.Khaos.KhaosName)
					{
						RelicLocationDisplay.LepastoneLocation = String.Empty;
					}
					break;
				case "MermanStatue":
					if (RelicLocationDisplay.MermanLocation == String.Empty)
					{
						RelicLocationDisplay.MermanLocation = locationName;
					}
					else if (RelicLocationDisplay.MermanLocation == Constants.Khaos.KhaosName)
					{
						RelicLocationDisplay.MermanLocation = String.Empty;
					}
					break;
				default:
					return;
			}

			if (location is null)
			{
				return;
			}

			if (location.SecondCastle)
			{
				notificationService.SetInvertedRelicCoordinates(relicName, location.X, location.Y);
			}
			else
			{
				notificationService.SetRelicCoordinates(relicName, location.X, location.Y);
			}
		}

		private bool UpdateProgressionItems()
		{
			int changes = 0;
			watchlistService.UpdateWatchlist(watchlistService.ProgressionItemWatches);
			for (int i = 0; i < watchlistService.ProgressionItemWatches.Count; i++)
			{
				if (watchlistService.ProgressionItemWatches[i].ChangeCount == 0)
				{
					continue;
				}
				if (watchlistService.ProgressionItemWatches[i].Value > 0)
				{
					progressionItems[i].Collected = true;
				}
				else
				{
					progressionItems[i].Collected = false;
				}
			}
			watchlistService.ProgressionItemWatches.ClearChangeCounts();

			for (int i = 0; i < progressionItems.Count; i++)
			{
				switch (i)
				{
					case 0:
					case 1:
						progressionItems[i].Equipped = (sotnApi.AlucardApi.Accessory1 == progressionItems[i].Value) || (sotnApi.AlucardApi.Accessory2 == progressionItems[i].Value);
						break;
					case 2:
						progressionItems[i].Equipped = (sotnApi.AlucardApi.Armor == progressionItems[i].Value);
						break;
					case 3:
						progressionItems[i].Equipped = (sotnApi.AlucardApi.Helm == progressionItems[i].Value);
						break;
					default:
						progressionItems[i].Equipped = false;
						break;
				}

				if (!progressionItems[i].Status && (progressionItems[i].Collected || progressionItems[i].Equipped))
				{
					progressionItems[i].Status = true;
					changes++;
				}
				else if (progressionItems[i].Status && !(progressionItems[i].Collected || progressionItems[i].Equipped))
				{
					progressionItems[i].Status = false;
					changes++;
				}
			}

			return changes > 0;
		}

		private bool UpdateThrustSwords()
		{
			int changes = 0;

			watchlistService.UpdateWatchlist(watchlistService.ThrustSwordWatches);
			for (int i = 0; i < watchlistService.ThrustSwordWatches.Count; i++)
			{
				if (watchlistService.ThrustSwordWatches[i].ChangeCount == 0)
				{
					continue;
				}
				if (watchlistService.ThrustSwordWatches[i].Value > 0)
				{
					thrustSwords[i].Collected = true;

				}
				else
				{
					thrustSwords[i].Collected = false;
				}
			}
			watchlistService.ThrustSwordWatches.ClearChangeCounts();

			for (int i = 0; i < thrustSwords.Count; i++)
			{
				thrustSwords[i].Equipped = (sotnApi.AlucardApi.RightHand == thrustSwords[i].Value);

				if (!thrustSwords[i].Status && (thrustSwords[i].Collected || thrustSwords[i].Equipped))
				{
					thrustSwords[i].Status = true;
					changes++;
				}
				else if (thrustSwords[i].Status && !(thrustSwords[i].Collected || thrustSwords[i].Equipped))
				{
					thrustSwords[i].Status = false;
					changes++;
				}
			}

			return changes > 0;
		}

		private void GetSeedData()
		{
			seedName = sotnApi.GameApi.ReadSeedName();
			preset = sotnApi.GameApi.ReadPresetName();
			if (preset == "tournament" || preset == "")
			{
				preset = "custom";
			}
			SeedInfo = seedName + "(" + preset + ")";
			trackerRenderer.SeedInfo = SeedInfo;
			if (preset == "custom")
			{
				switch (toolConfig.Tracker.CustomExtension)
				{
					case "guarded":
						guardedExtension = true;
						break;
					case "equipment":
						equipmentExtension = true;
						guardedExtension = true;
						break;
					case "spread":
						spreadExtension = true;
						break;
					case "classic":
						break;
					default:
						LoadExtension(Paths.PresetPath + toolConfig.Tracker.CustomExtension + ".json");
						break;
				}
			}
			else
			{
				LoadLocks(Paths.PresetPath + preset + ".json", false, true);
			}
			PrepareMapLocations();
		}

		private void SetEquipmentProgression()
		{
			//Set Cube of Zoe, Demon card and Nose Devil to progression
			relics[10].Progression = true;
			relics[21].Progression = true;
			relics[24].Progression = true;
			trackerRenderer.SetProgression();
			trackerRenderer.CalculateGrid(toolConfig.Tracker.Width, toolConfig.Tracker.Height);
			trackerRenderer.Render();
		}

		private void SetMapLocations()
		{
			for (int i = 0; i < locations.Count; i++)
			{
				if (!locations[i].Visited && locations[i].SecondCastle == secondCastle)
				{
					ColorMapRoom(i, (uint) locations[i].AvailabilityColor, locations[i].SecondCastle);
				}
			}
		}

		private void PrepareMapLocations()
		{
			for (int i = 0; i < locations.Count; i++)
			{
				locations[i].Visited = true;
				if ((locations[i].ClassicExtension && classicExtension) ||
					(locations[i].GuardedExtension && guardedExtension) ||
					(locations[i].EquipmentExtension && equipmentExtension) ||
					(locations[i].SpreadExtension && spreadExtension) ||
					(locations[i].CustomExtension && customExtension))
				{
					locations[i].Visited = false;
				}
			}
		}

		private void ClearMapLocation(int locationIndex)
		{
			ColorMapRoom(locationIndex, (uint) MapColor.Clear, locations[locationIndex].SecondCastle);
			foreach (var room in locations[locationIndex].Rooms)
			{
				Watch roomWatch;
				if (locations[locationIndex].EquipmentExtension)
				{
					roomWatch = watchlistService.EquipmentLocationWatches[room.WatchIndex];
				}
				else
				{
					roomWatch = watchlistService.SafeLocationWatches[room.WatchIndex];
				}

				sotnApi.GameApi.SetRoomToUnvisited(roomWatch.Address);
			}
		}

		private void ColorMapRoom(int locationIndex, uint color, bool secondCastle)
		{
			uint x = (uint) locations[locationIndex].X;
			uint y = (uint) locations[locationIndex].Y;
			if (secondCastle)
			{
				x = 252 - x;
				y = 199 - y;
			}
			uint borderColor = color > 0 ? (uint) MapColor.Border : 0;

			if (locations[locationIndex].EquipmentExtension && spreadExtension == false)
			{
				if (secondCastle)
				{
					x += 2;
					y += 1;
				}
				sotnApi.MapApi.ColorMapLocation(x, y, color);
			}
			else
			{
				sotnApi.MapApi.ColorMapRoom(x, y, color, borderColor);
			}
		}

		private bool LocationsDrawn()
		{
			Location uncheckedLocation = locations[0];

			for (int i = 0; i < locations.Count; i++)
			{
				if (!locations[i].Visited && locations[i].SecondCastle == secondCastle)
				{
					uncheckedLocation = locations[i];
				}
			}

			if (uncheckedLocation.Visited)
			{
				return true;
			}

			uint x = (uint) uncheckedLocation.X;
			uint y = (uint) uncheckedLocation.Y;
			if (secondCastle)
			{
				x = 252 - x;
				y = 199 - y;
				if (uncheckedLocation.EquipmentExtension)
				{
					x += 2;
					y += 1;
				}
			}
			if (!sotnApi.MapApi.RoomIsRendered(x, y))
			{
				return false;
			}


			return true;
		}

		private void CheckRooms(WatchList watchlist, bool equipment = false)
		{
			for (int j = 0; j < watchlist.Count; j++)
			{
				Watch? watch = watchlist[j];
				Location location = equipment ? locations[watchToLocationEquipment[j]] : locations[watchToLocation[j]];
				Room room = equipment ? location.Rooms[watchToRoomEquipment[j]] : location.Rooms[watchToRoom[j]];

				if (watch.ChangeCount == 0 || location.Visited || location.EquipmentExtension != equipment || watch.Value == 0)
				{
					continue;
				}

				foreach (int value in room.Values)
				{
					if ((watch.Value & value) == value)
					{
						location.Visited = true;
						Watch? coopWatch = null;
						int watchIndex = 0;
						for (int i = 0; i < watchlistService.CoopLocationWatches.Count; i++)
						{
							if (watchlistService.CoopLocationWatches[i].Notes == watch.Notes)
							{
								coopWatch = watchlistService.CoopLocationWatches[i];
								watchIndex = i;
								break;
							}
						}

						if (coopWatch is not null && watchlistService.CoopLocationValues[watchIndex] == 0)
						{
							coopWatch.Update(PreviousType.LastFrame);
							watchlistService.CoopLocationValues[watchIndex] = value;
						}
						ClearMapLocation(locations.IndexOf(location));
					}
				}
			}
		}

		private void CheckReachability()
		{
			int changes = 0;

			for (int j = 0; j < locations.Count; j++)
			{
				locations[j].AvailabilityColor = MapColor.Unavailable;

				if (locations[j].Visited)
				{
					continue;
				}

				if (locations[j].Locks.Count == 0)
				{
					locations[j].AvailabilityColor = MapColor.Available;
					continue;
				}
				foreach (var lockSet in locations[j].Locks)
				{
					bool unlock = true;
					for (int i = 0; i < lockSet.Length; i++)
					{
						unlock = unlock && TrackedObjectStatus(lockSet[i]);
					}
					if (unlock)
					{
						changes++;
						locations[j].AvailabilityColor = MapColor.Available;
						break;
					}
				}
				if (locations[j].AvailabilityColor == MapColor.Available)
				{
					continue;
				}
				if (locations[j].OutOfLogicLocks.Count == 0)
				{
					locations[j].AvailabilityColor = MapColor.Allowed;
					continue;
				}
				foreach (var lockSet in locations[j].OutOfLogicLocks)
				{
					bool unlock = true;
					for (int i = 0; i < lockSet.Length; i++)
					{
						unlock = unlock && TrackedObjectStatus(lockSet[i]);
					}
					if (unlock)
					{
						changes++;
						if (preset == "speedrun" || preset == "glitch")
						{
							locations[j].AvailabilityColor = MapColor.Available;
							break;
						}
						locations[j].AvailabilityColor = MapColor.Allowed;
						break;
					}
				}
			}

			if (changes > 0)
			{
				SetMapLocations();
			}
		}

		private bool TrackedObjectStatus(string name)
		{
			if (relicToIndex.ContainsKey(name))
			{
				TrackerRelic relic = relics[relicToIndex[name]];
				return relic.Collected;
			}
			if (name == "holyglasses" && secondCastle)
			{
				return true;
			}
			if (itemToIndex.ContainsKey(name))
			{
				Item progressionItem = progressionItems[itemToIndex[name]];
				return progressionItem.Status;
			}
			if (name == "thrustsword" && swordToIndex.ContainsKey(name))
			{
				return true;
			}

			return false;
		}

		private int EncodeItems()
		{
			int itemsNumber = 0;
			for (int i = 0; i < progressionItems.Count + 1; i++)
			{
				if (i < progressionItems.Count && progressionItems[i].Status)
				{
					itemsNumber |= (int) Math.Pow(2, i);
				}
				else if (i == progressionItems.Count)
				{
					foreach (var sword in thrustSwords)
					{
						if (sword.Status)
						{
							itemsNumber |= (int) Math.Pow(2, i);
							break;
						}
					}
				}
			}

			return itemsNumber;
		}

		private int EncodeRelics()
		{
			int relicsNumber = 0;
			for (int i = 0; i < relics.Count; i++)
			{
				if (relics[i].Collected)
				{
					relicsNumber |= (int) Math.Pow(2, i);
				}
			}
			return relicsNumber;
		}
	}
}
