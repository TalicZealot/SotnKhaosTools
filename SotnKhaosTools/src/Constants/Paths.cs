using System.Collections.Generic;

namespace SotnKhaosTools.Constants
{
	public static class Paths
	{
		public const string ItemPickupSound = "./ExternalTools/SotnKhaosTools/Sounds/Item.mp3";
		public const string AlucardWhatSound = "./ExternalTools/SotnKhaosTools/Sounds/AlucardWhat.mp3";
		public const string LibrarianThankYouSound = "./ExternalTools/SotnKhaosTools/Sounds/LibrarianThankYou.mp3";
		public const string DeathLaughSound = "./ExternalTools/SotnKhaosTools/Sounds/DeathLaugh.mp3";
		public const string RichterLaughSound = "./ExternalTools/SotnKhaosTools/Sounds/RichterLaugh.mp3";
		public const string FairyPotionSound = "./ExternalTools/SotnKhaosTools/Sounds/FairyPotion.mp3";
		public const string MeltySound = "./ExternalTools/SotnKhaosTools/Sounds/Melty.mp3";
		public const string DragonInstallSound = "./ExternalTools/SotnKhaosTools/Sounds/DragonInstall.mp3";
		public const string ZaWarudoSound = "./ExternalTools/SotnKhaosTools/Sounds/ZaWarudo.mp3";
		public const string DeathLaughAlternateSound = "./ExternalTools/SotnKhaosTools/Sounds/DeathLaughAlternate.mp3";
		public const string DieSound = "./ExternalTools/SotnKhaosTools/Sounds/Die.mp3";
		public const string DracLaughSound = "./ExternalTools/SotnKhaosTools/Sounds/DracLaugh.mp3";
		public const string HohoSound = "./ExternalTools/SotnKhaosTools/Sounds/Hoho.mp3";
		public const string SlowWhatSound = "./ExternalTools/SotnKhaosTools/Sounds/SlowWhat.mp3";
		public const string SwordBroSound = "./ExternalTools/SotnKhaosTools/Sounds/SwordBro.mp3";
		public const string AlreadyDeadSound = "./ExternalTools/SotnKhaosTools/Sounds/AlreadyDead.mp3";
		public const string BattleOrdersSound = "./ExternalTools/SotnKhaosTools/Sounds/BattleOrders.mp3";
		public const string QuadSound = "./ExternalTools/SotnKhaosTools/Sounds/Quad.mp3";
		public const string ExcellentSound = "./ExternalTools/SotnKhaosTools/Sounds/Excellent.mp3";

		public const string NamesFilePath = "./ExternalTools/SotnKhaosTools/Khaos/names.txt";

		public const string SourceLink = "https://github.com/TalicZealot/SotnKhaosTools/";
		public const string ReadmeLink = "https://github.com/TalicZealot/SotnKhaosTools/blob/main/README.md";
		public const string ApiLink = "https://github.com/TalicZealot/SotnApi";
		public const string UpdaterLink = "https://github.com/TalicZealot/SimpleLatestReleaseUpdater";
		public const string RandoSourceLink = "https://github.com/3snowp7im/SotN-Randomizer";
		public const string DonateLink = "https://www.paypal.com/donate?hosted_button_id=5F8565K23F2F8";

		public const string LatestReleaseApi = "https://api.github.com/repos/taliczealot/SotnKhaosTools/releases";
		public const string LatestReleaseUrl = "https://github.com/TalicZealot/SotnKhaosTools/releases/latest";
		public const string UpdaterPath = @"\ExternalTools\SotnKhaosTools\Updater\SimpleLatestReleaseUpdater.exe";
		public const string UpdaterFolderPath = @"\ExternalTools\SotnKhaosTools\Updater\";

		public const string RelicWatchesPath = "./ExternalTools/SotnKhaosTools/Watches/Relics.wch";
		public const string SafeLocationWatchesPath = "./ExternalTools/SotnKhaosTools/Watches/SafeLocations.wch";
		public const string EquipmentLocationWatchesPath = "./ExternalTools/SotnKhaosTools/Watches/EquipmentLocations.wch";
		public const string ProgressionItemWatchesPath = "./ExternalTools/SotnKhaosTools/Watches/ProgressionItems.wch";
		public const string ThrustSwordWatchesPath = "./ExternalTools/SotnKhaosTools/Watches/ThrustSwords.wch";
		public const string WarpsAndShortcutsWatchPath = "./ExternalTools/SotnKhaosTools/Watches/WarpsAndShortcuts.wch";

		public const string ImagesPath = "./ExternalTools/SotnKhaosTools/Images/";
		public const string TextboxImage = "./ExternalTools/SotnKhaosTools/Images/SotnTextBox.png";
		public const string IconVermillionBird = "./ExternalTools/SotnKhaosTools/Images/VermillionBird.png";
		public const string IconWhiteTiger = "./ExternalTools/SotnKhaosTools/Images/WhiteTiger.png";
		public const string IconAzureDragon = "./ExternalTools/SotnKhaosTools/Images/AzureDragon.png";
		public const string IconBlackTortoise = "./ExternalTools/SotnKhaosTools/Images/BlackTortoise.png";
		public static readonly Dictionary<string, string> RelicImages = new Dictionary<string, string>
		{
			{"SoulOfBat", "./ExternalTools/SotnKhaosTools/Images/SoulOfBat.png"},
			{"FireOfBat", "./ExternalTools/SotnKhaosTools/Images/FireOfBat.png"},
			{"EchoOfBat", "./ExternalTools/SotnKhaosTools/Images/EchoOfBat.png"},
			{"ForceOfEcho", "./ExternalTools/SotnKhaosTools/Images/ForceOfEcho.png"},
			{"SoulOfWolf", "./ExternalTools/SotnKhaosTools/Images/SoulOfWolf.png"},
			{"PowerOfWolf", "./ExternalTools/SotnKhaosTools/Images/PowerOfWolf.png"},
			{"SkillOfWolf", "./ExternalTools/SotnKhaosTools/Images/SkillOfWolf.png"},
			{"FormOfMist", "./ExternalTools/SotnKhaosTools/Images/FormOfMist.png"},
			{"PowerOfMist", "./ExternalTools/SotnKhaosTools/Images/PowerOfMist.png"},
			{"GasCloud", "./ExternalTools/SotnKhaosTools/Images/GasCloud.png"},
			{"CubeOfZoe", "./ExternalTools/SotnKhaosTools/Images/CubeOfZoe.png"},
			{"SpiritOrb", "./ExternalTools/SotnKhaosTools/Images/SpiritOrb.png"},
			{"GravityBoots", "./ExternalTools/SotnKhaosTools/Images/GravityBoots.png"},
			{"LeapStone", "./ExternalTools/SotnKhaosTools/Images/LeapStone.png"},
			{"HolySymbol", "./ExternalTools/SotnKhaosTools/Images/HolySymbol.png"},
			{"FaerieScroll", "./ExternalTools/SotnKhaosTools/Images/FaerieScroll.png"},
			{"JewelOfOpen", "./ExternalTools/SotnKhaosTools/Images/JewelOfOpen.png"},
			{"MermanStatue", "./ExternalTools/SotnKhaosTools/Images/MermanStatue.png"},
			{"BatCard", "./ExternalTools/SotnKhaosTools/Images/BatCard.png"},
			{"GhostCard", "./ExternalTools/SotnKhaosTools/Images/GhostCard.png"},
			{"FaerieCard", "./ExternalTools/SotnKhaosTools/Images/FaerieCard.png"},
			{"DemonCard", "./ExternalTools/SotnKhaosTools/Images/DemonCard.png"},
			{"SwordCard", "./ExternalTools/SotnKhaosTools/Images/SwordCard.png"},
			{"SpriteCard" , "./ExternalTools/SotnKhaosTools/Images/SpriteCard.png"},
			{"NoseDevilCard", "./ExternalTools/SotnKhaosTools/Images/NoseDevilCard.png"},
			{"HeartOfVlad", "./ExternalTools/SotnKhaosTools/Images/HeartOfVlad.png"},
			{"ToothOfVlad", "./ExternalTools/SotnKhaosTools/Images/ToothOfVlad.png"},
			{"RibOfVlad", "./ExternalTools/SotnKhaosTools/Images/RibOfVlad.png"},
			{"RingOfVlad", "./ExternalTools/SotnKhaosTools/Images/RingOfVlad.png"},
			{"EyeOfVlad", "./ExternalTools/SotnKhaosTools/Images/EyeOfVlad.png"}
		};

		public const string LogsPath = "./ExternalTools/SotnKhaosTools/Logs/";
		public const string ReplaysPath = "./ExternalTools/SotnKhaosTools/Replays/";
		public const string ChangeLogPath = @"\ExternalTools\SotnKhaosTools\ChangeLog.txt";

		public const string CasualPresetPath = "./ExternalTools/SotnKhaosTools/Presets/casual.json";
		public const string SafePresetPath = "./ExternalTools/SotnKhaosTools/Presets/safe.json";
		public const string SpeedrunPresetPath = "./ExternalTools/SotnKhaosTools/Presets/speedrun.json";
		public const string BatMasterPresetPath = "./ExternalTools/SotnKhaosTools/Presets/bat-master.json";

		public const string ConfigPath = "./ExternalTools/SotnKhaosTools/ToolConfig.ini";
		public const string SeedInfoPath = "./ExternalTools/SotnKhaosTools/TrackerOverlay/SeedInfo.txt";
		public const string CheatsPath = "./ExternalTools/SotnKhaosTools/Cheats/Cheats.cht";
		public const string CheatsBackupPath = "./ExternalTools/SotnKhaosTools/Cheats/Cheats.cht.bkp";

		public const string KhaosDatabase = "./ExternalTools/SotnKhaosTools/Khaos/Khaos.db";
		public const string TwitchRedirectUri = "http://localhost:8080/redirect/";
	}
}
