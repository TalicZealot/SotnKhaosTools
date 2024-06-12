using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Media;
using BizHawk.Client.Common;
using BizHawk.Common;
using SotnApi.Constants.Addresses;
using SotnApi.Constants.Values.Alucard;
using SotnApi.Constants.Values.Alucard.Enums;
using SotnApi.Constants.Values.Game;
using SotnApi.Interfaces;
using SotnApi.Models;
using SotnKhaosTools.Configuration.Interfaces;
using SotnKhaosTools.Constants;
using SotnKhaosTools.Khaos.Interfaces;
using SotnKhaosTools.Khaos.Models;
using SotnKhaosTools.Services;
using static BizHawk.Client.EmuHawk.PaletteViewer;
using MapLocation = SotnKhaosTools.RandoTracker.Models.MapLocation;

namespace SotnKhaosTools.Khaos
{
	internal sealed class KhaosController : IKhaosController
	{
		private readonly IToolConfig toolConfig;
		private readonly ISotnApi sotnApi;
		private readonly ICheatsController cheatsController;
		private readonly INotificationService notificationService;
		private readonly IKhaosActionsInfoDisplay statusInfoDisplay;

		private readonly Random rng = new();
		private int khaosMeter = 0;
		private int pandoraProgress = 0;
		private bool alucardSecondCastle = false;
		private bool inMainMenu = false;
		private long renameAddress = 0xe09a0;

		private uint goalQuadKills = 0;
		private uint currentQuadKills = 0;
		private uint currentKills = 0;
		private uint quadLevel = 0;

		private uint dracMusicCounter = 0;

		private float thirstLevel = 0;

		private uint hordeZone = 0;
		private uint hordeTriggerRoomX = 0;
		private uint hordeTriggerRoomY = 0;
		private bool hordeWaiting = false;
		private List<Entity> hordeEnemies = new(10);

		private int enduranceCount = 0;
		private int superEnduranceCount = 0;
		private uint enduranceRoomX = 0;
		private uint enduranceRoomY = 0;

		private uint storedHearts = 0;
		private uint storedMana = 0;
		private uint storedMaxMana = 0;
		private int spentMana = 0;

		private int fireballCooldown = 0;
		private List<Entity> fireballs = new(3);
		private Entity? whiteTigerFireball;

		private bool bulletHellActive = false;
		private bool superBulletHell = false;
		private Emitter FlowerEmitter = new Emitter { BulletCooldown = 12, Rotation = 0.0174533 * 2, BulletTimer = 10, BurstCooldown = 100, BurstLimit = 7, Shots = 10, BulletSpeed = Constants.Khaos.SmallBulletSpeed };
		private Emitter ZigZagEmitter = new Emitter { BulletCooldown = 16, Rotation = 0.61803, BulletTimer = 8, BurstCooldown = 0, BurstLimit = 10, Shots = 9, BulletSpeed = Constants.Khaos.SmallBulletSpeed * 2 };
		private Emitter DiamondEmitter = new Emitter { BulletCooldown = 120, Rotation = 0.61803, BulletTimer = 8, BulletSpeed = Constants.Khaos.SmallBulletSpeed * 2 };
		private Emitter JailFirstEmitter = new Emitter { BulletCooldown = 10, Rotation = 0.61803, BulletTimer = 8, BurstCooldown = 60, BurstLimit = 10, Shots = 12, BulletSpeed = Constants.Khaos.SmallBulletSpeed * 2 };
		private Emitter JailSecondEmitter = new Emitter { BulletCooldown = 10, Rotation = 0.61803, BulletTimer = 8, BurstCooldown = 60, BurstLimit = 10, Shots = 12, BulletSpeed = Constants.Khaos.SmallBulletSpeed * 2 };
		private List<Entity> bullets = new(100);

		private Entity stand;
		private Entity alucardEntity;

		private ScheduledAction[] scheduledActions;

		private bool bloodManaActive = false;
		private bool banishActive = false;
		private string banishUser = "";
		private bool quadActive = false;
		private bool hasteActive = false;
		private bool hasteSpeedOn = false;
		private bool overdriveOn = false;
		private bool gasCloudTaken = false;
		private bool thirstActive = false;
		private bool slowActive = false;
		private bool slowPaused = false;
		private bool dizzyActive = false;

		private bool thirstGlow = false;
		private bool overdriveGlow = false;

		private bool vermilionBirdPollong = false;

		private bool azureSpiritActive = false;
		private bool whiteTigerBallActive = false;

		private bool azureDragonUsed = false;
		private bool vermilionBirdUsed = false;
		private bool whiteTigerUsed = false;
		private bool blackTortoiseUsed = false;
		private bool darkMetamorphosisCasted = true;
		private bool hellfireCasted = false;

		private bool battleOrdersActive = false;
		private uint battleOrdersBonusHp = 0;
		private uint battleOrdersBonusMp = 0;

		private bool superThirst = false;
		private bool superHorde = false;
		private bool superMelty = false;
		private bool superHaste = false;

		private bool hnkOn = false;
		private int hnkToggled = 0;

		private int bankruptLevel = 1;
		private short mainMenuCounter = 0;

		private bool inAlucardMode = false;
		private bool hasHitbox = false;
		private uint currentHp = 0;
		private uint currentMp = 0;
		private bool inTransition = false;
		private bool isLoading = false;
		private bool hasControl = false;
		private bool isInvincible = false;
		private bool canMenu = false;
		private bool canSave = false;
		private uint roomX = 0;
		private uint roomY = 0;
		private uint area = 0;
		private float cameraAdjustmentX = 0;
		private float cameraAdjustmentY = 0;
		public KhaosController(IToolConfig toolConfig, ISotnApi sotnApi, ICheatsController cheatsController, INotificationService notificationService, IKhaosActionsInfoDisplay statusInfoDisplay)
		{
			if (toolConfig is null) throw new ArgumentNullException(nameof(toolConfig));
			if (sotnApi is null) throw new ArgumentNullException(nameof(sotnApi));
			if (cheatsController is null) throw new ArgumentNullException(nameof(cheatsController));
			if (notificationService is null) throw new ArgumentNullException(nameof(notificationService));
			if (statusInfoDisplay is null) throw new ArgumentNullException(nameof(statusInfoDisplay));
			this.toolConfig = toolConfig;
			this.sotnApi = sotnApi;
			this.cheatsController = cheatsController;
			this.notificationService = notificationService;
			this.statusInfoDisplay = statusInfoDisplay;
			InitializeTimers();
		}

		public bool AutoKhaosOn { get; set; }
		public bool SpeedLocked { get; set; }
		public bool ManaLocked { get; set; }
		public bool InvincibilityLocked { get; set; }
		public bool SpawnActive { get; set; }
		private bool ShaftHpSet { get; set; }
		private bool GalamothStatsSet { get; set; }
		public bool PandoraUsed { get; set; }
		private int TotalMeterGained { get; set; }
		private uint AlucardMapX { get; set; }
		private uint AlucardMapY { get; set; }

		public void StartKhaos()
		{
			cheatsController.StartCheats();
			DateTime startedAt = DateTime.Now;
			for (int i = 0; i < toolConfig.Khaos.Actions.Count; i++)
			{
				if (toolConfig.Khaos.Actions[i].StartsOnCooldown)
				{
					toolConfig.Khaos.Actions[i].LastUsedAt = startedAt;
				}
				else
				{
					toolConfig.Khaos.Actions[i].LastUsedAt = null;
				}
			}

			notificationService.StartOverlayServer();
			notificationService.AddMessage($"Khaos started");
			sotnApi.GameApi.OverwriteString(SotnApi.Constants.Addresses.Strings.ItemNameAddresses["Library card"], "Khaotic card", true);
			sotnApi.GameApi.OverwriteString(renameAddress, "Khaos", true);
			alucardEntity = sotnApi.EntityApi.GetLiveEntity(SotnApi.Constants.Addresses.Alucard.Entity.Address);
		}
		public void StopKhaos()
		{
			StopTimers();
			cheatsController.FaerieScroll.Disable();
			notificationService.AddMessage($"Khaos stopped");
		}
		public void GainKhaosMeter(short meter)
		{
			khaosMeter += meter;
			TotalMeterGained += meter;

			notificationService.UpdateOverlayMeter(khaosMeter);

			if (!PandoraUsed && pandoraProgress < 6 && TotalMeterGained >= (toolConfig.Khaos.PandoraTrigger / 7) * (pandoraProgress + 1))
			{
				string label = "PANDORA";
				notificationService.AddMessage(label.Substring(0, pandoraProgress + 1));
				pandoraProgress++;
			}
		}
		private bool IsInRoomList(List<MapLocation> rooms)
		{
			for (int i = 0; i < rooms.Count; i++)
			{
				if (AlucardMapX == rooms[i].X && AlucardMapY == rooms[i].Y && alucardSecondCastle == Convert.ToBoolean(rooms[i].SecondCastle))
				{
					return true;
				}
			}

			return false;
		}
		private void SetGalamothtStats()
		{
			long galamothHeadAddress = sotnApi.EntityApi.FindEntity(Constants.Khaos.GalamothHeadId);
			if (galamothHeadAddress > 0)
			{
				Entity galamothHead = sotnApi.EntityApi.GetLiveEntity(galamothHeadAddress);
				galamothHead.Xpos -= Constants.Khaos.GalamothKhaosPositionOffset;

				List<long> galamothParts = sotnApi.EntityApi.GetAllEntities(new List<ushort> { Constants.Khaos.GalamothPartsId });
				for (int i = 0; i < galamothParts.Count; i++)
				{
					Entity galamothEntity = sotnApi.EntityApi.GetLiveEntity(galamothParts[i]);
					if (i == 0 && enduranceCount > 0)
					{
						enduranceCount--;
						enduranceRoomX = roomX;
						enduranceRoomY = roomY;
						if (enduranceCount == 0)
						{
							scheduledActions[(int) Enums.Event.EnduranceSpawn].Stop();
						}
						if (superEnduranceCount > 0)
						{
							galamothEntity.Hp = (short) Math.Round(3.5 * Constants.Khaos.GalamothKhaosHp);
							notificationService.AddMessage($"Super Endurance Galamoth");
						}
						else
						{
							galamothEntity.Hp = (short) Math.Round(2.3 * Constants.Khaos.GalamothKhaosHp);
							notificationService.AddMessage($"Endurance Galamoth");
						}
					}
					else if (i == 0)
					{
						galamothEntity.Hp = (short)Constants.Khaos.GalamothKhaosHp;
					}
					galamothEntity.Xpos -= Constants.Khaos.GalamothKhaosPositionOffset;
				}

				GalamothStatsSet = true;
			}
		}
		private void CheckMainMenu()
		{
			uint status = sotnApi.GameApi.Status;
			if (inMainMenu != (status == SotnApi.Constants.Values.Game.Status.MainMenu))
			{
				if (inMainMenu && (status != SotnApi.Constants.Values.Game.Status.InGame))
				{
					return;
				}
				inMainMenu = status == SotnApi.Constants.Values.Game.Status.MainMenu;
				if (inMainMenu)
				{
					GainKhaosMeter((short) (toolConfig.Khaos.MeterOnReset * mainMenuCounter));
					mainMenuCounter++;
					battleOrdersBonusHp = 0;
					battleOrdersBonusMp = 0;

					thirstLevel = 0;
					cheatsController.VisualEffectPalette.Disable();
					cheatsController.VisualEffectTimer.Disable();
				}
			}
		}
		private void CheckCastleChanged()
		{
			if (alucardSecondCastle != sotnApi.GameApi.SecondCastle)
			{
				alucardSecondCastle = sotnApi.GameApi.SecondCastle;
				SetSaveColorPalette();
			}
		}
		private void CheckRichterFight()
		{
			bool keepRichterRoom = IsInRoomList(Constants.Khaos.RichterRooms);
			if (FastActionViable() && keepRichterRoom && !ShaftHpSet)
			{
				long shaftAddress = sotnApi.EntityApi.FindEntity(Constants.Khaos.ShaftOrbId);
				if (shaftAddress > 0)
				{
					Entity shaft = sotnApi.EntityApi.GetLiveEntity(shaftAddress);
					shaft.Hp = (int) Constants.Khaos.ShaftKhaosHp;
					ShaftHpSet = true;
				}
			}
		}
		private void CheckGalamoth()
		{
			bool galamothRoom = IsInRoomList(Constants.Khaos.GalamothRooms);
			if (FastActionViable() && galamothRoom && !GalamothStatsSet)
			{
				SetGalamothtStats();
			}
			if (!galamothRoom)
			{
				GalamothStatsSet = false;
			}
		}

		#region Khaotic Effects
		public void KhaosStatus(string user = Constants.Khaos.KhaosName)
		{
			bool entranceCutscene = IsInRoomList(Constants.Khaos.EntranceCutsceneRooms);
			bool succubusRoom = IsInRoomList(Constants.Khaos.SuccubusRoom);
			bool alucardIsImmuneToCurse = sotnApi.AlucardApi.HasRelic(Relic.HeartOfVlad)
				|| Equipment.Items[(int) (sotnApi.AlucardApi.Helm + Equipment.HandCount + 1)] == "Coral circlet";
			bool alucardIsImmuneToStone = Equipment.Items[(int) (sotnApi.AlucardApi.Armor + Equipment.HandCount + 1)] == "Mirror cuirass"
				|| Equipment.Items[(int) (sotnApi.AlucardApi.RightHand)] == "Medusa shield"
				|| Equipment.Items[(int) (sotnApi.AlucardApi.LeftHand)] == "Medusa shield";
			bool alucardIsImmuneToPoison = Equipment.Items[(int) (sotnApi.AlucardApi.Helm + Equipment.HandCount + 1)] == "Topaz circlet";
			bool highHp = currentHp > sotnApi.AlucardApi.MaxtHp - 15;
			sotnApi.GameApi.OverwriteString(renameAddress, user, true);

			int result = RollStatus(entranceCutscene, succubusRoom, alucardIsImmuneToCurse, alucardIsImmuneToStone, alucardIsImmuneToPoison, highHp);

			while (result == 0)
			{
				result = RollStatus(entranceCutscene, succubusRoom, alucardIsImmuneToCurse, alucardIsImmuneToStone, alucardIsImmuneToPoison, highHp);
			}

			RandomizeLibraryCardDestination();
			switch (result)
			{
				case 1:
					SpawnPoisonHitbox();
					break;
				case 2:
					SpawnCurseHitbox();
					break;
				case 3:
					SpawnStoneHitbox();
					break;
				case 4:
					SpawnSlamHitbox();
					break;
				case 5:
					ActivateDizzy();
					notificationService.AddMessage($"{user} confused you");
					break;
				case 6:
					sotnApi.AlucardApi.CurrentMp -= 15;
					notificationService.AddMessage($"{user} used Mana Burn");
					break;
				case 7:
					sotnApi.AlucardApi.ActivatePotion(Potion.Antivenom);
					notificationService.AddMessage($"{user} used an antivenom");
					break;
				case 8:
					sotnApi.AlucardApi.ActivatePotion(Potion.StrPotion);
					notificationService.AddMessage($"{user} gave you strength");
					break;
				case 9:
					sotnApi.AlucardApi.Heal(15);
					notificationService.AddMessage($"{user} used a minor heal");
					break;
				case 10:
					sotnApi.AlucardApi.ActivatePotion(Potion.ShieldPotion);
					notificationService.AddMessage($"{user} gave you defence");
					break;
				case 11:
					ActivateGuardianSpirits();
					notificationService.AddMessage($"{user} used Guardian Spirits");
					break;
				default:
					break;
			}

			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.KhaosStatus]);
		}
		public void KhaosEquipment(string user = Constants.Khaos.KhaosName)
		{
			RandomizeEquipmentSlots();
			notificationService.AddMessage($"{user} used Khaos Equipment");
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.KhaosEquipment]);
		}
		public void KhaosStats(string user = Constants.Khaos.KhaosName)
		{
			RandomizeStatsActivate();
			notificationService.AddMessage($"{user} used Khaos Stats");
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.KhaosStats]);
		}
		public void KhaosRelics(string user = Constants.Khaos.KhaosName)
		{
			RandomizeRelicsActivate(false);
			sotnApi.AlucardApi.GrantItemByName("Library card");
			sotnApi.AlucardApi.GrantItemByName("Library card");
			notificationService.AddMessage($"{user} used Khaos Relics");
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.KhaosRelics]);
		}
		public void PandorasBox(string user = Constants.Khaos.KhaosName)
		{
			RandomizeGold();
			RandomizeStatsActivate();
			RandomizeEquipmentSlots();
			RandomizeRelicsActivate(!toolConfig.Khaos.KeepVladRelics);
			RandomizeInventory();
			RandomizeSubweapon();
			sotnApi.GameApi.RespawnBosses();
			sotnApi.GameApi.RespawnItems();
			notificationService.AddMessage($"{user} opened Pandora's Box");
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.PandorasBox]);
		}
		public void Gamble(string user = Constants.Khaos.KhaosName)
		{
			double goldPercent = rng.NextDouble();
			uint newGold = (uint) ((double) sotnApi.AlucardApi.Gold * goldPercent);
			uint goldSpent = sotnApi.AlucardApi.Gold - newGold;
			sotnApi.AlucardApi.Gold = newGold;
			string item = Equipment.Items[rng.Next(1, Equipment.Items.Count)];
			while (item.Contains("empty hand") || item.Contains("-"))
			{
				item = Equipment.Items[rng.Next(1, Equipment.Items.Count)];
			}
			sotnApi.AlucardApi.GrantItemByName(item);
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.Gamble]);
		}
		public void Banish(string user = Constants.Khaos.KhaosName)
		{
			banishActive = true;
			banishUser = user;
		}
		public void KhaosTrack(string track, string user = Constants.Khaos.KhaosName)
		{
			int trackIndex = Array.IndexOf(Constants.Khaos.AcceptedMusicTrackTitles, track.ToLower().Trim());
			bool alternateTitle = Constants.Khaos.AlternateTrackTitles.ContainsKey(track.ToLower().Trim());

			if (trackIndex >= 0)
			{
				trackIndex = (int) Various.MusicTracks[Constants.Khaos.AcceptedMusicTrackTitles[trackIndex]];
			}
			else if (alternateTitle)
			{
				string foundTrack = Constants.Khaos.AlternateTrackTitles[track.ToLower().Trim()];
				trackIndex = (int) Various.MusicTracks[foundTrack];
			}
			else
			{
				int roll = rng.Next(0, Constants.Khaos.AcceptedMusicTrackTitles.Length - 1);
				trackIndex = (int) Various.MusicTracks[Constants.Khaos.AcceptedMusicTrackTitles[roll]];
			}
			cheatsController.Music.PokeValue(trackIndex);
			cheatsController.Music.Enable();
			scheduledActions[(int) Enums.Event.KhaosTrackOff].Start();
			notificationService.AddMessage($"{user} queued {track}");
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.KhaosTrack]);
		}
		#endregion
		#region Debuffs
		public void Bankrupt(string user = Constants.Khaos.KhaosName)
		{
			notificationService.AddMessage($"{user} used Bankrupt level {bankruptLevel}");
			BankruptActivate();
			sotnApi.AlucardApi.GrantItemByName("Library card");
			sotnApi.AlucardApi.GrantItemByName("Library card");
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.Bankrupt]);
		}
		public void Weaken(string user = Constants.Khaos.KhaosName)
		{
			bool meterFull = khaosMeter >= 100;
			float enhancedFactor = 1;
			if (meterFull)
			{
				enhancedFactor = Constants.Khaos.SuperWeakenFactor;
				SpendKhaosMeter();
			}

			if (battleOrdersActive)
			{
				sotnApi.AlucardApi.MaxtHp -= (uint) battleOrdersBonusHp;
				sotnApi.AlucardApi.MaxtMp -= (uint) battleOrdersBonusMp;
			}

			uint newCurrentHp = (uint) (currentHp * toolConfig.Khaos.WeakenFactor * enhancedFactor);
			uint newCurrentMp = (uint) (currentMp * toolConfig.Khaos.WeakenFactor * enhancedFactor);
			uint newCurrentHearts = (uint) (sotnApi.AlucardApi.CurrentHearts * toolConfig.Khaos.WeakenFactor * enhancedFactor);
			uint newMaxtHp = (uint) (sotnApi.AlucardApi.MaxtHp * toolConfig.Khaos.WeakenFactor * enhancedFactor);
			uint newMaxtMp = (uint) (sotnApi.AlucardApi.MaxtHp * toolConfig.Khaos.WeakenFactor * enhancedFactor);
			uint newMaxtHearts = (uint) (sotnApi.AlucardApi.MaxtHearts * toolConfig.Khaos.WeakenFactor * enhancedFactor);
			uint newStr = (uint) (sotnApi.AlucardApi.Str * toolConfig.Khaos.WeakenFactor * enhancedFactor);
			uint newCon = (uint) (sotnApi.AlucardApi.Con * toolConfig.Khaos.WeakenFactor * enhancedFactor);
			uint newInt = (uint) (sotnApi.AlucardApi.Int * toolConfig.Khaos.WeakenFactor * enhancedFactor);
			uint newLck = (uint) (sotnApi.AlucardApi.Lck * toolConfig.Khaos.WeakenFactor * enhancedFactor);

			sotnApi.AlucardApi.CurrentHp = newCurrentHp >= Constants.Khaos.MinimumHp ? newCurrentHp : Constants.Khaos.MinimumHp;
			sotnApi.AlucardApi.CurrentMp = newCurrentMp >= Constants.Khaos.MinimumMp ? newCurrentHp : Constants.Khaos.MinimumMp;
			sotnApi.AlucardApi.CurrentHearts = newCurrentHearts >= Constants.Khaos.MinimumHearts ? newCurrentHearts : Constants.Khaos.MinimumHearts;
			sotnApi.AlucardApi.MaxtHp = newMaxtHp >= Constants.Khaos.MinimumHp ? newMaxtHp : Constants.Khaos.MinimumHp;
			sotnApi.AlucardApi.MaxtMp = newMaxtMp >= Constants.Khaos.MinimumMp ? newMaxtMp : Constants.Khaos.MinimumMp;
			sotnApi.AlucardApi.MaxtHearts = newMaxtHearts >= Constants.Khaos.MinimumHearts ? newMaxtHearts : Constants.Khaos.MinimumHearts;
			sotnApi.AlucardApi.Str = newStr >= Constants.Khaos.MinimumStat ? newStr : Constants.Khaos.MinimumStat;
			sotnApi.AlucardApi.Con = newCon >= Constants.Khaos.MinimumStat ? newCon : Constants.Khaos.MinimumStat;
			sotnApi.AlucardApi.Int = newInt >= Constants.Khaos.MinimumStat ? newInt : Constants.Khaos.MinimumStat;
			sotnApi.AlucardApi.Lck = newLck >= Constants.Khaos.MinimumStat ? newLck : Constants.Khaos.MinimumStat;

			if (battleOrdersActive)
			{
				battleOrdersBonusHp = sotnApi.AlucardApi.MaxtHp;
				battleOrdersBonusMp = sotnApi.AlucardApi.MaxtMp;
				sotnApi.AlucardApi.MaxtHp += battleOrdersBonusHp;
				sotnApi.AlucardApi.MaxtMp += battleOrdersBonusMp;
			}

			uint newLevel = (uint) (sotnApi.AlucardApi.Level * toolConfig.Khaos.WeakenFactor * enhancedFactor);
			sotnApi.AlucardApi.Level = newLevel;
			uint newExperience = 0;
			if (newLevel <= StatsValues.ExperienceValues.Length && newLevel > 1)
			{
				newExperience = (uint) StatsValues.ExperienceValues[(int) newLevel - 1];
			}
			else if (newLevel > 1)
			{
				newExperience = (uint) StatsValues.ExperienceValues[StatsValues.ExperienceValues.Length - 1];
			}
			if (newLevel > 1)
			{
				sotnApi.AlucardApi.Level = newLevel;
				sotnApi.AlucardApi.Experiecne = newExperience;
			}

			string message = meterFull ? $"{user} used Super Weaken" : $"{user} used Weaken";
			notificationService.AddMessage(message);
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.Weaken]);
		}
		public void RespawnBosses(string user = Constants.Khaos.KhaosName)
		{
			sotnApi.GameApi.RespawnBosses();
			notificationService.AddMessage($"{user} used Respawn Bosses");
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.RespawnBosses]);
		}
		public void Slow(string user = Constants.Khaos.KhaosName)
		{
			cheatsController.UnderwaterPhysics.Enable();
			scheduledActions[(int) Enums.Event.SlowOff].Start();
			slowActive = true;
			notificationService.AddOverlayTimer((int) Enums.Action.Slow, (int) toolConfig.Khaos.Actions[(int) Enums.Action.Slow].Duration.TotalMilliseconds);
			statusInfoDisplay.AddTimer((int) Enums.Action.Slow);

			string message = $"{user} used {toolConfig.Khaos.Actions[(int) Enums.Action.Slow].Name}";
			notificationService.AddMessage(message);
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.Slow]);
		}
		public void BloodMana(string user = Constants.Khaos.KhaosName)
		{
			storedMana = currentMp;
			storedMaxMana = sotnApi.AlucardApi.MaxtMp;
			bloodManaActive = true;
			scheduledActions[(int) Enums.Event.BloodManaOff].Start();
			ManaLocked = true;
			notificationService.AddMessage($"{user} used Blood Mana");
			notificationService.AddOverlayTimer((int) Enums.Action.BloodMana, (int) toolConfig.Khaos.Actions[(int) Enums.Action.BloodMana].Duration.TotalMilliseconds);
			statusInfoDisplay.AddTimer((int) Enums.Action.BloodMana);
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.BloodMana]);
		}
		public void Thirst(string user = Constants.Khaos.KhaosName)
		{
			bool meterFull = khaosMeter >= 100;
			if (meterFull)
			{
				superThirst = true;
				SpendKhaosMeter();
			}

			cheatsController.DarkMetamorphasis.PokeValue(1);
			cheatsController.DarkMetamorphasis.Enable();

			scheduledActions[(int) Enums.Event.ThirstDrain].Start();
			scheduledActions[(int) Enums.Event.ThirstOff].Start();
			notificationService.AddOverlayTimer((int) Enums.Action.Thirst, (int) toolConfig.Khaos.Actions[(int) Enums.Action.Thirst].Duration.TotalMilliseconds);
			statusInfoDisplay.AddTimer((int) Enums.Action.Thirst);

			string message = meterFull ? $"{user} used Super Thirst" : $"{user} used Thirst";
			notificationService.AddMessage(message);
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.Thirst]);
			thirstActive = true;
		}
		public void Horde(string user = Constants.Khaos.KhaosName)
		{
			hordeTriggerRoomX = roomX;
			hordeTriggerRoomY = roomY;
			SpawnActive = true;
			bool meterFull = khaosMeter >= 100;
			if (meterFull)
			{
				superHorde = true;
				SpendKhaosMeter();
			}
			hordeWaiting = true;
			scheduledActions[(int) Enums.Event.HordeSpawn].Start();
			hordeEnemies.Clear();
			string message = meterFull ? $"{user} summoned the Super Horde" : $"{user} summoned the Horde";
			notificationService.AddMessage(message);
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.KhaosHorde]);
		}
		public void Endurance(string user = Constants.Khaos.KhaosName)
		{
			enduranceRoomX = roomX;
			enduranceRoomY = roomY;
			bool meterFull = khaosMeter >= 100;
			if (meterFull)
			{
				superEnduranceCount++;
				SpendKhaosMeter();
			}

			enduranceCount++;
			scheduledActions[(int) Enums.Event.EnduranceSpawn].Start();
			string message = meterFull ? $"{user} used Super Endurance" : $"{user} used Endurance";
			notificationService.AddMessage(message);
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.Endurance]);
		}
		public void HnK(string user = Constants.Khaos.KhaosName)
		{
			hnkOn = true;
			cheatsController.DefencePotion.PokeValue(1);
			cheatsController.DefencePotion.Enable();
			InvincibilityLocked = true;
			scheduledActions[(int) Enums.Event.HnkOff].Start();
			notificationService.AddOverlayTimer((int) Enums.Action.HnK, (int) toolConfig.Khaos.Actions[(int) Enums.Action.HnK].Duration.TotalMilliseconds);
			statusInfoDisplay.AddTimer((int) Enums.Action.HnK);
			notificationService.AddMessage($"{user} used HnK");
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.HnK]);
		}
		public void BulletHell(string user = Constants.Khaos.KhaosName)
		{
			bool meterFull = khaosMeter >= 100;
			if (meterFull)
			{
				superBulletHell = true;
				SpendKhaosMeter();
			}
			Constants.Khaos.FlowerBullet.Damage = (ushort)(sotnApi.AlucardApi.Def + 15);
			Constants.Khaos.ZigZagBullet.Damage = (ushort) (sotnApi.AlucardApi.Def + 10);
			Constants.Khaos.JailBullet.Damage = (ushort) (sotnApi.AlucardApi.Def + 10);
			Constants.Khaos.DiamondBullet.Damage = (ushort) (sotnApi.AlucardApi.Def + 20);
			sotnApi.GameApi.OverwriteString(renameAddress, user, true);
			bulletHellActive = true;
			sotnApi.AlucardApi.TakeRelic(Relic.PowerOfMist);
			scheduledActions[(int) Enums.Event.BulletHellOff].Start();
			notificationService.AddOverlayTimer((int) Enums.Action.BulletHell, (int) toolConfig.Khaos.Actions[(int) Enums.Action.BulletHell].Duration.TotalMilliseconds);
			statusInfoDisplay.AddTimer((int) Enums.Action.BulletHell);
			notificationService.AddMessage($"{user} used Bullet Hell");
			string message = meterFull ? $"{user} used Super Bullet Hell" : $"{user} used Bullet Hell";
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.BulletHell]);
		}
		#endregion
		#region Buffs
		public void Quad(string user = Constants.Khaos.KhaosName)
		{
			cheatsController.AttackPotion.PokeValue(1);
			cheatsController.AttackPotion.Enable();
			cheatsController.StrengthPotion.PokeValue(1);
			cheatsController.StrengthPotion.Enable();
			cheatsController.VisualEffectPalette.PokeValue(Constants.Khaos.QuadColorPalette);
			cheatsController.VisualEffectPalette.Enable();
			cheatsController.VisualEffectTimer.PokeValue(30);
			cheatsController.VisualEffectTimer.Enable();

			scheduledActions[(int) Enums.Event.QuadOff].Start();
			notificationService.AddMessage(user + " used Quad");
			quadActive = true;

			if (goalQuadKills == 0)
			{
				goalQuadKills = 10 + (quadLevel * quadLevel);
			}
			notificationService.AddOverlayTimer((int) Enums.Action.Quad, (int) toolConfig.Khaos.Actions[(int) Enums.Action.Quad].Duration.TotalMilliseconds);
			statusInfoDisplay.AddTimer((int) Enums.Action.Quad);
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.Quad]);
		}
		public void LightHelp(string user = Constants.Khaos.KhaosName)
		{
			string item = toolConfig.Khaos.LightHelpItemRewards[rng.Next(0, toolConfig.Khaos.LightHelpItemRewards.Length)];
			int rolls = 0;
			while (sotnApi.AlucardApi.HasItemInInventory(item) && rolls < Constants.Khaos.HelpItemRetryCount)
			{
				item = toolConfig.Khaos.LightHelpItemRewards[rng.Next(0, toolConfig.Khaos.LightHelpItemRewards.Length)];
				rolls++;
			}

			bool highHp = currentHp > sotnApi.AlucardApi.MaxtHp * 0.6;
			bool highMp = currentMp > sotnApi.AlucardApi.MaxtMp * 0.6;

			int roll = rng.Next(1, 4);

			if (highHp && roll == 2)
			{
				roll = 3;
			}
			if ((highMp || ManaLocked) && roll == 3)
			{
				roll = 2;
			}
			if ((highHp && highMp))
			{
				roll = 1;
			}

			switch (roll)
			{
				case 1:
					sotnApi.AlucardApi.GrantItemByName(item);
					notificationService.AddMessage($"{user} gave you a {item}");
					break;
				case 2:
					sotnApi.AlucardApi.ActivatePotion(Potion.Potion);
					notificationService.AddMessage($"{user} healed you");
					break;
				case 3:
					sotnApi.AlucardApi.CurrentMp += 20;
					notificationService.AddMessage($"{user} gave you mana");
					break;
				default:
					break;
			}
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.LightHelp]);
		}
		public void MediumHelp(string user = Constants.Khaos.KhaosName)
		{
			string item = toolConfig.Khaos.MediumHelpItemRewards[rng.Next(0, toolConfig.Khaos.MediumHelpItemRewards.Length)];
			int rolls = 0;
			while (sotnApi.AlucardApi.HasItemInInventory(item) && rolls < Constants.Khaos.HelpItemRetryCount)
			{
				item = toolConfig.Khaos.MediumHelpItemRewards[rng.Next(0, toolConfig.Khaos.MediumHelpItemRewards.Length)];
				rolls++;
			}

			bool highHp = currentHp > sotnApi.AlucardApi.MaxtHp * 0.6;
			bool highMp = currentMp > sotnApi.AlucardApi.MaxtMp * 0.6;

			int roll = rng.Next(1, ManaLocked ? 3 : 4);

			if (highHp && roll == 2)
			{
				roll = 3;
			}
			if ((highMp && roll == 3) || ManaLocked)
			{
				roll = 2;
			}
			if ((highHp && highMp))
			{
				roll = 1;
			}

			switch (roll)
			{
				case 1:
					sotnApi.AlucardApi.GrantItemByName(item);
					notificationService.AddMessage($"{user} gave you a {item}");
					break;
				case 2:
					sotnApi.AlucardApi.ActivatePotion(Potion.Elixir);
					notificationService.AddMessage($"{user} healed you");
					break;
				case 3:
					sotnApi.AlucardApi.ActivatePotion(Potion.Mannaprism);
					notificationService.AddMessage($"{user} used a Mana Prism");
					break;
				default:
					break;
			}
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.MediumHelp]);
		}
		public void HeavytHelp(string user = Constants.Khaos.KhaosName)
		{
			bool meterFull = khaosMeter >= 100;
			if (meterFull)
			{
				khaosMeter -= 100;
				notificationService.UpdateOverlayMeter(khaosMeter);
			}

			string item;
			int relic;
			int roll;
			RollRewards(out item, out relic, out roll);
			GiveRewards(user, item, relic, roll);
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.HeavyHelp]);

			if (meterFull)
			{
				RollRewards(out item, out relic, out roll);
				GiveRewards(user, item, relic, roll);
			}

			void RollRewards(out string item, out int relic, out int roll)
			{
				item = toolConfig.Khaos.HeavyHelpItemRewards[rng.Next(0, toolConfig.Khaos.HeavyHelpItemRewards.Length)];
				int rolls = 0;
				while (sotnApi.AlucardApi.HasItemInInventory(item) && rolls < Constants.Khaos.HelpItemRetryCount)
				{
					item = toolConfig.Khaos.HeavyHelpItemRewards[rng.Next(0, toolConfig.Khaos.HeavyHelpItemRewards.Length)];
					rolls++;
				}

				relic = rng.Next(0, Constants.Khaos.ProgressionRelics.Length);

				roll = rng.Next(1, 3);
				for (int i = 0; i < 11; i++)
				{
					if (!sotnApi.AlucardApi.HasRelic(Constants.Khaos.ProgressionRelics[relic]))
					{
						break;
					}
					if (i == 10)
					{
						roll = 1;
						break;
					}
					relic = rng.Next(0, Constants.Khaos.ProgressionRelics.Length);
				}
			}

			void GiveRewards(string user, string item, int relic, int roll)
			{
				switch (roll)
				{
					case 1:
						sotnApi.AlucardApi.GrantItemByName(item);
						notificationService.AddMessage($"{user} gave you a {item}");
						break;
					case 2:
						SetRelicLocationDisplay(Constants.Khaos.ProgressionRelics[relic], false);
						sotnApi.AlucardApi.GrantRelic(Constants.Khaos.ProgressionRelics[relic], true);
						notificationService.AddMessage($"{user} gave you {Constants.Khaos.ProgressionRelics[relic]}");
						break;
					default:
						break;
				}
			}
		}
		public void BattleOrders(string user = Constants.Khaos.KhaosName)
		{
			float currentHpPercentage = (float) currentHp / (float) sotnApi.AlucardApi.MaxtHp;
			float currentMpPercentage = (float) currentMp / (float) sotnApi.AlucardApi.MaxtMp;

			battleOrdersActive = true;
			battleOrdersBonusHp = sotnApi.AlucardApi.MaxtHp;
			battleOrdersBonusMp = sotnApi.AlucardApi.MaxtMp;

			sotnApi.AlucardApi.MaxtHp += battleOrdersBonusHp;
			sotnApi.AlucardApi.MaxtMp += battleOrdersBonusMp;

			sotnApi.AlucardApi.CurrentHp = (uint) (sotnApi.AlucardApi.MaxtHp * currentHpPercentage);
			sotnApi.AlucardApi.CurrentMp = (uint) (sotnApi.AlucardApi.MaxtMp * currentMpPercentage);

			notificationService.AddMessage($"{user} used Battle Orders");
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.BattleOrders]);

			scheduledActions[(int) Enums.Event.BattleOrdersOff].Start();
			statusInfoDisplay.AddTimer((int) Enums.Action.BattleOrders);
			notificationService.AddOverlayTimer((int) Enums.Action.BattleOrders, (int) toolConfig.Khaos.Actions[(int) Enums.Action.BattleOrders].Duration.TotalMilliseconds);
		}
		public void Magician(string user = Constants.Khaos.KhaosName)
		{
			bool meterFull = khaosMeter >= 100;
			if (meterFull)
			{
				SpendKhaosMeter();
				SetRelicLocationDisplay(Relic.SoulOfBat, false);
				SetRelicLocationDisplay(Relic.FormOfMist, false);
				sotnApi.AlucardApi.GrantRelic(Relic.SoulOfBat, true);
				sotnApi.AlucardApi.GrantRelic(Relic.FireOfBat, true);
				sotnApi.AlucardApi.GrantRelic(Relic.EchoOfBat, true);
				sotnApi.AlucardApi.GrantRelic(Relic.ForceOfEcho, true);
				sotnApi.AlucardApi.GrantRelic(Relic.SoulOfWolf, true);
				sotnApi.AlucardApi.GrantRelic(Relic.PowerOfWolf, true);
				sotnApi.AlucardApi.GrantRelic(Relic.SkillOfWolf, true);
				sotnApi.AlucardApi.GrantRelic(Relic.FormOfMist, true);
				sotnApi.AlucardApi.GrantRelic(Relic.PowerOfMist, true);
				sotnApi.AlucardApi.GrantRelic(Relic.GasCloud, true);
			}

			sotnApi.AlucardApi.GrantItemByName("Wizard hat");
			sotnApi.AlucardApi.ActivatePotion(Potion.SmartPotion);
			cheatsController.Mana.PokeValue((int) sotnApi.AlucardApi.MaxtMp);
			cheatsController.Mana.Enable();
			ManaLocked = true;
			scheduledActions[(int) Enums.Event.MagicianOff].Start();
			notificationService.AddOverlayTimer((int) Enums.Action.Magician, (int) toolConfig.Khaos.Actions[(int) Enums.Action.Magician].Duration.TotalMilliseconds);
			statusInfoDisplay.AddTimer((int) Enums.Action.Magician);

			string message = meterFull ? $"{user} activated Shapeshifter" : $"{user} activated Magician";
			notificationService.AddMessage(message);

			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.Magician]);
		}
		public void MeltyBlood(string user = Constants.Khaos.KhaosName)
		{
			bool meterFull = khaosMeter >= 100;
			if (meterFull)
			{
				superMelty = true;
				SetHasteStaticSpeeds(true);
				sotnApi.AlucardApi.CurrentHp = sotnApi.AlucardApi.MaxtHp;
				sotnApi.AlucardApi.CurrentMp = sotnApi.AlucardApi.MaxtMp;
				sotnApi.AlucardApi.ActivatePotion(Potion.StrPotion);
				sotnApi.AlucardApi.AttackPotionTimer = Constants.Khaos.GuiltyGearAttack;
				sotnApi.AlucardApi.DarkMetamorphasisTimer = Constants.Khaos.GuiltyGearDarkMetamorphosis;
				sotnApi.AlucardApi.DefencePotionTimer = Constants.Khaos.GuiltyGearDefence;
				sotnApi.AlucardApi.InvincibilityTimer = Constants.Khaos.GuiltyGearInvincibility;
				SpendKhaosMeter();
			}
			cheatsController.HitboxWidth.Enable();
			cheatsController.HitboxHeight.Enable();
			cheatsController.Hitbox2Width.Enable();
			cheatsController.Hitbox2Height.Enable();
			stand = sotnApi.EntityApi.GetLiveEntity(sotnApi.EntityApi.SpawnEntity(Constants.Khaos.StandEntity));
			SetRelicLocationDisplay(Relic.LeapStone, false);
			sotnApi.AlucardApi.GrantRelic(Relic.LeapStone, true);
			scheduledActions[(int) Enums.Event.MeltyBloodOff].Start();
			string message = meterFull ? $"{user} activated Overpowered" : $"{user} activated S tier";
			notificationService.AddMessage(message);
			if (meterFull)
			{
				Alert(toolConfig.Khaos.Actions[(int) Enums.Action.GuiltyGear]);
				notificationService.AddOverlayTimer((int) Enums.Action.GuiltyGear, (int) toolConfig.Khaos.Actions[(int) Enums.Action.GuiltyGear].Duration.TotalMilliseconds);
				statusInfoDisplay.AddTimer((int) Enums.Action.GuiltyGear);
			}
			else
			{
				Alert(toolConfig.Khaos.Actions[(int) Enums.Action.MeltyBlood]);
				notificationService.AddOverlayTimer((int) Enums.Action.MeltyBlood, (int) toolConfig.Khaos.Actions[(int) Enums.Action.MeltyBlood].Duration.TotalMilliseconds);
				statusInfoDisplay.AddTimer((int) Enums.Action.MeltyBlood);
			}
		}
		public void FourBeasts(string user = Constants.Khaos.KhaosName)
		{
			int beast = 0;
			if (azureDragonUsed && whiteTigerUsed && vermilionBirdUsed && blackTortoiseUsed)
			{
				beast = 5;
			}

			while (beast == 0)
			{
				beast = RollBeast();
			}

			switch (beast)
			{
				case 1:
					AzureDragon(user);
					break;
				case 2:
					WhiteTiger(user);
					break;
				case 3:
					VermilionBird(user);
					break;
				case 4:
					BlackTortoise(user);
					break;
				case 5:
					FourHolyBeasts(user);
					break;
				default:
					break;
			}
		}
		public void ZaWarudo(string user = Constants.Khaos.KhaosName)
		{
			sotnApi.GameApi.SetStopwatchTimer(20);
			sotnApi.AlucardApi.Subweapon = Subweapon.Stopwatch;
			sotnApi.AlucardApi.CurrentHearts += 40;
			scheduledActions[(int) Enums.Event.ZawarudoOff].Start();
			notificationService.AddMessage(user + " used ZA WARUDO");
			notificationService.AddOverlayTimer((int) Enums.Action.ZAWARUDO, (int) toolConfig.Khaos.Actions[(int) Enums.Action.ZAWARUDO].Duration.TotalMilliseconds);
			statusInfoDisplay.AddTimer((int) Enums.Action.ZAWARUDO);
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.ZAWARUDO]);
		}
		public void Haste(string user = Constants.Khaos.KhaosName)
		{
			bool meterFull = khaosMeter >= 100;

			if (meterFull)
			{
				SpendKhaosMeter();
				superHaste = true;
			}
			hasteActive = true;
			SpeedLocked = true;
			SetHasteStaticSpeeds(meterFull);
			scheduledActions[(int) Enums.Event.HasteOff].Start();
			notificationService.AddOverlayTimer((int) Enums.Action.Haste, (int) toolConfig.Khaos.Actions[(int) Enums.Action.Haste].Duration.TotalMilliseconds);
			statusInfoDisplay.AddTimer((int) Enums.Action.Haste);
			string message = meterFull ? $"{user} activated Super Haste" : $"{user} activated Haste";
			notificationService.AddMessage(message);
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.Haste]);
		}
		#endregion

		public void Update()
		{
			if (!inAlucardMode || !hasHitbox || currentHp < 1
				|| inTransition || isLoading || inMainMenu)
			{
				return;
			}

			if (sotnApi.GameApi.MapOpen)
			{
				if (sotnApi.GameApi.SecondCastle && !notificationService.InvertedMapOpen)
				{
					notificationService.InvertedMapOpen = true;
				}
				else if (!notificationService.MapOpen)
				{
					notificationService.MapOpen = true;
				}
			}
			else
			{
				if (notificationService.MapOpen)
				{
					notificationService.MapOpen = false;
				}
				if (notificationService.InvertedMapOpen)
				{
					notificationService.InvertedMapOpen = false;
				}
			}

			CheckBanishViable();
			CheckDracularoomMusic();
			CheckThirstKill();
			CheckVampireKill();
			CheckVermillionBirdFireballs();
			CheckAzureDragon();
			CheckBlackTortoise();
			CheckCastleChanged();
			FixEntranceSlow();
			FixSlowWarps();
			HandleHnkInvincibility();
			CheckGalamoth();
			CheckRichterFight();

			if (!PandoraUsed && TotalMeterGained >= toolConfig.Khaos.PandoraTrigger)
			{
				PandoraUsed = true;
			}
		}
		public void FrameAdvance()
		{
			//cache common checks
			inAlucardMode = sotnApi.GameApi.InAlucardMode();
			hasHitbox = sotnApi.AlucardApi.HasHitbox();
			currentHp = sotnApi.AlucardApi.CurrentHp;
			currentMp = sotnApi.AlucardApi.CurrentMp;
			inTransition = sotnApi.GameApi.InTransition;
			isLoading = sotnApi.GameApi.IsLoading;
			hasControl = sotnApi.AlucardApi.HasControl();
			isInvincible = sotnApi.AlucardApi.IsInvincible();
			AlucardMapX = sotnApi.AlucardApi.MapX;
			AlucardMapY = sotnApi.AlucardApi.MapY;
			canMenu = sotnApi.GameApi.CanMenu();
			canSave = sotnApi.GameApi.CanSave();
			roomX = sotnApi.GameApi.RoomX;
			roomY = sotnApi.GameApi.RoomY;
			area = sotnApi.GameApi.Area;
			cameraAdjustmentX = sotnApi.GameApi.CameraAdjustmentX;
			cameraAdjustmentY = sotnApi.GameApi.CameraAdjustmentY;

			CheckMainMenu();

			if (!inAlucardMode || inTransition || isLoading || inMainMenu)
			{
				return;
			}

			CheckTimers();
			CheckDashInput();
			CheckManaUsage();
			CheckHeartsPickedUp();
			CheckWhiteTiger();
			MeltyFrameAdvance();
			CheckDizzyDoors();
			CheckHasteAttack();
			EmittersFrameAdvance();
			BulletsFrameAdvance();
		}
		public bool ActionViable()
		{
			if (inAlucardMode && canMenu
				&& currentHp > 0 && !canSave
				&& !IsInRoomList(Constants.Khaos.RichterRooms)
				&& !IsInRoomList(Constants.Khaos.LoadingRooms)
				&& !IsInRoomList(Constants.Khaos.LibraryRoom))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public bool FastActionViable()
		{
			if (inAlucardMode && canMenu
				&& currentHp > 0 && !canSave
				&& !inTransition && !isLoading
				&& hasControl && hasHitbox && !isInvincible
				&& !IsInRoomList(Constants.Khaos.LibraryRoom)
				&& AlucardMapX < 99)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public void CheckTimers()
		{
			if (!hasHitbox || currentHp < 1)
			{
				return;
			}
			for (int i = 0; i < scheduledActions.Length; i++)
			{
				if (!scheduledActions[i].On)
				{
					continue;
				}
				scheduledActions[i].Timer--;
				if (scheduledActions[i].Timer == 0)
				{
					scheduledActions[i].Action();
					if (scheduledActions[i].Repeat)
					{
						scheduledActions[i].Timer = scheduledActions[i].Delay;
					}
					else
					{
						scheduledActions[i].On = false;
					}
				}
			}
		}
		private void InitializeTimers()
		{
			scheduledActions = new ScheduledAction[]
			{
				new ScheduledAction { Delay = 12 * 60, On = false, Action = DizzyOff },
				new ScheduledAction { Delay = 60, On = false, Action = KillAlucard },
				new ScheduledAction { Delay = (int) toolConfig.Khaos.Actions[(int) Enums.Action.KhaosTrack].Duration.TotalSeconds * 60, On = false, Action = KhaosTrackOff },
				new ScheduledAction { Delay = (int) toolConfig.Khaos.Actions[(int) Enums.Action.Slow].Duration.TotalSeconds * 60, On = false, Action = SlowOff },
				new ScheduledAction { Delay = (int) toolConfig.Khaos.Actions[(int) Enums.Action.BloodMana].Duration.TotalSeconds * 60, On = false, Action = BloodManaOff },
				new ScheduledAction { Delay = (int) toolConfig.Khaos.Actions[(int) Enums.Action.Thirst].Duration.TotalSeconds * 60, On = false, Action = ThirstOff },
				new ScheduledAction { Delay = 60, On = false, Action = ThirstDrain, Repeat = true },
				new ScheduledAction { Delay = (int) toolConfig.Khaos.Actions[(int) Enums.Action.KhaosHorde].Duration.TotalSeconds * 60, On = false, Action = HordeOff },
				new ScheduledAction { Delay = 60, On = false, Action = HordeSpawn, Repeat = true },
				new ScheduledAction { Delay = 1, On = false, Action = EnduranceSpawn, Repeat = true },
				new ScheduledAction { Delay = (int) toolConfig.Khaos.Actions[(int) Enums.Action.HnK].Duration.TotalSeconds * 60, On = false, Action = HnkOff },
				new ScheduledAction { Delay = (int) toolConfig.Khaos.Actions[(int) Enums.Action.BulletHell].Duration.TotalSeconds * 60, On = false, Action = BulletHellOff },
				new ScheduledAction { Delay = (int) toolConfig.Khaos.Actions[(int) Enums.Action.Quad].Duration.TotalSeconds * 60, On = false, Action = QuadOff },
				new ScheduledAction { Delay = (int) toolConfig.Khaos.Actions[(int) Enums.Action.Magician].Duration.TotalSeconds * 60, On = false, Action = MagicianOff },
				new ScheduledAction { Delay = (int) toolConfig.Khaos.Actions[(int) Enums.Action.BattleOrders].Duration.TotalSeconds * 60, On = false, Action = BattleOrdersOff },
				new ScheduledAction { Delay = (int) toolConfig.Khaos.Actions[(int) Enums.Action.MeltyBlood].Duration.TotalSeconds * 60, On = false, Action = MeltyBloodOff },
				new ScheduledAction { Delay = (int) toolConfig.Khaos.Actions[(int) Enums.Action.FourBeasts].Duration.TotalSeconds * 60, On = false, Action = FourBeastsOff },
				new ScheduledAction { Delay = 10 * 60, On = false, Action = AzureDragonOff },
				new ScheduledAction { Delay = 2 * 60, On = false, Action = WhiteTigerOff },
				new ScheduledAction { Delay = (int) toolConfig.Khaos.Actions[(int) Enums.Action.ZAWARUDO].Duration.TotalSeconds * 60, On = false, Action = ZawarudoOff, },
				new ScheduledAction { Delay = (int) toolConfig.Khaos.Actions[(int) Enums.Action.Haste].Duration.TotalSeconds * 60, On = false, Action = HasteOff },
				new ScheduledAction { Delay = 8 * 60, On = false, Action = GuardianSpiritsOff }
			};
		}
		private void StopTimers()
		{
			for (int i = 0; i < scheduledActions.Length; i++)
			{
				scheduledActions[i].On = false;
			}
		}

		#region Khaotic events
		private int RollStatus(bool entranceCutscene, bool succubusRoom, bool alucardIsImmuneToCurse, bool alucardIsImmuneToStone, bool alucardIsImmuneToPoison, bool highHp)
		{
			int min = 1;
			int max = 12;
			int result = rng.Next(min, max);

			switch (result)
			{
				case 1:
					if (alucardIsImmuneToPoison)
					{
						return 0;
					}
					break;
				case 2:
					if (alucardIsImmuneToCurse)
					{
						return 0;
					}
					break;
				case 3:
					if (alucardIsImmuneToStone || succubusRoom)
					{
						return 0;
					}
					break;
				case 4:
					if (succubusRoom || entranceCutscene)
					{
						return 0;
					}
					break;
				case 6:
					if (currentMp < 15 || ManaLocked)
					{
						return 0;
					}
					break;
				case 7:
					break;
				case 8:
					break;
				case 9:
					if (highHp)
					{
						return 0;
					}
					break;
				case 10:
					break;
				case 11:
					if (sotnApi.AlucardApi.SubweaponTimer > 0)
					{
						return 0;
					}
					break;
				default:
					break;
			}

			return result;
		}
		private void KhaosTrackOff()
		{
			cheatsController.Music.Disable();
			scheduledActions[(int) Enums.Event.KhaosTrackOff].Stop();
		}
		private void RandomizeGold()
		{
			uint gold = (uint) rng.Next(0, 5000);
			uint roll = (uint) rng.Next(0, 21);
			if (roll > 16 && roll < 20)
			{
				gold *= (uint) rng.Next(1, 11);
			}
			else if (roll > 19)
			{
				gold *= (uint) rng.Next(10, 81);
			}
			sotnApi.AlucardApi.Gold = gold;
		}
		private void RandomizeStatsActivate()
		{
			if (battleOrdersActive)
			{
				sotnApi.AlucardApi.MaxtHp -= (uint) battleOrdersBonusHp;
				sotnApi.AlucardApi.MaxtMp -= (uint) battleOrdersBonusMp;
			}

			uint maxHp = sotnApi.AlucardApi.MaxtHp;
			uint currentHp = sotnApi.AlucardApi.CurrentHp;
			uint maxMana = sotnApi.AlucardApi.MaxtMp;
			uint str = sotnApi.AlucardApi.Str;
			uint con = sotnApi.AlucardApi.Con;
			uint intel = sotnApi.AlucardApi.Int;
			uint lck = sotnApi.AlucardApi.Lck;
			uint totalMinStats = Constants.Khaos.MinimumStat * 4;
			uint totalMinPool = currentMp + currentHp;

			uint statPool = str + con + intel + lck > totalMinStats ? str + con + intel + lck - totalMinStats : str + con + intel + lck;
			uint offset = (uint) ((rng.NextDouble() / 2) * statPool);

			int statPoolRoll = rng.Next(1, 4);
			if (statPoolRoll == 2)
			{
				statPool += +offset;
			}
			else if (statPoolRoll == 3)
			{
				statPool -= offset;
			}

			double a = rng.NextDouble();
			double b = rng.NextDouble();
			double c = rng.NextDouble();
			double d = rng.NextDouble();
			double sum = a + b + c + d;
			double percentageStr = (a / sum);
			double percentageCon = (b / sum);
			double percentageInt = (c / sum);
			double percentageLck = (d / sum);
			uint newStr = (uint) Math.Round(statPool * percentageStr);
			uint newCon = (uint) Math.Round(statPool * percentageCon);
			uint newInt = (uint) Math.Round(statPool * percentageInt);
			uint newLck = (uint) Math.Round(statPool * percentageLck);

			sotnApi.AlucardApi.Str = Constants.Khaos.MinimumStat + newStr;
			sotnApi.AlucardApi.Con = Constants.Khaos.MinimumStat + newCon;
			sotnApi.AlucardApi.Int = Constants.Khaos.MinimumStat + newInt;
			sotnApi.AlucardApi.Lck = Constants.Khaos.MinimumStat + newLck;

			uint pointsPool = maxHp + maxMana > totalMinPool ? maxHp + maxMana - totalMinPool : maxHp + maxMana;
			if (maxHp + maxMana < totalMinPool)
			{
				pointsPool = totalMinPool;
			}
			offset = (uint) ((rng.NextDouble() / 2) * pointsPool);

			int pointsRoll = rng.Next(1, 4);
			if (pointsRoll == 2)
			{
				pointsPool += offset;
			}
			else if (pointsRoll == 3)
			{
				pointsPool -= offset;
			}

			double hpPercent = rng.NextDouble();
			uint pointsHp = Constants.Khaos.MinimumHp + (uint) Math.Round(hpPercent * pointsPool);
			uint pointsMp = Constants.Khaos.MinimumMp + pointsPool - (uint) Math.Round(hpPercent * pointsPool);

			if (currentHp > pointsHp)
			{
				sotnApi.AlucardApi.CurrentHp = pointsHp;
			}
			if (currentMp > pointsMp)
			{
				sotnApi.AlucardApi.CurrentMp = pointsMp;
			}

			sotnApi.AlucardApi.MaxtHp = pointsHp;
			sotnApi.AlucardApi.MaxtMp = pointsMp;

			if (battleOrdersActive)
			{
				battleOrdersBonusHp = sotnApi.AlucardApi.MaxtHp;
				battleOrdersBonusMp = sotnApi.AlucardApi.MaxtMp;
				sotnApi.AlucardApi.MaxtHp += battleOrdersBonusHp;
				sotnApi.AlucardApi.MaxtMp += battleOrdersBonusMp;
			}
		}
		private void RandomizeInventory()
		{
			bool hasHolyGlasses = sotnApi.AlucardApi.HasItemInInventory("Holy glasses");
			bool hasSpikeBreaker = sotnApi.AlucardApi.HasItemInInventory("Spike Breaker");
			bool hasGoldRing = sotnApi.AlucardApi.HasItemInInventory("Gold Ring");
			bool hasSilverRing = sotnApi.AlucardApi.HasItemInInventory("Silver Ring");

			sotnApi.AlucardApi.ClearInventory();

			int itemCount = rng.Next(toolConfig.Khaos.PandoraMinItems, toolConfig.Khaos.PandoraMaxItems + 1);

			for (int i = 0; i < itemCount; i++)
			{
				int result = rng.Next(0, Equipment.Items.Count);
				sotnApi.AlucardApi.GrantItemByName(Equipment.Items[result]);
			}

			sotnApi.AlucardApi.HandCursor = 0;
			sotnApi.AlucardApi.HelmCursor = 0;
			sotnApi.AlucardApi.ArmorCursor = 0;
			sotnApi.AlucardApi.CloakCursor = 0;
			sotnApi.AlucardApi.AccessoryCursor = 0;

			sotnApi.AlucardApi.GrantItemByName("Library card");
			sotnApi.AlucardApi.GrantItemByName("Library card");
			if (hasHolyGlasses)
			{
				sotnApi.AlucardApi.GrantItemByName("Holy glasses");
			}
			if (hasSpikeBreaker)
			{
				sotnApi.AlucardApi.GrantItemByName("Spike Breaker");
			}
			if (hasGoldRing)
			{
				sotnApi.AlucardApi.GrantItemByName("Gold Ring");
			}
			if (hasSilverRing)
			{
				sotnApi.AlucardApi.GrantItemByName("Silver Ring");
			}
		}
		private void RandomizeSubweapon()
		{
			Array? subweapons = Enum.GetValues(typeof(Subweapon));
			sotnApi.AlucardApi.Subweapon = (Subweapon) subweapons.GetValue(rng.Next(subweapons.Length));
		}
		private void RandomizeRelicsActivate(bool randomizeVladRelics = true)
		{
			Array? relics = Enum.GetValues(typeof(Relic));
			foreach (object? relic in relics)
			{
				int roll = rng.Next(0, 2);
				if (roll > 0)
				{
					if ((int) relic < 25 && !sotnApi.AlucardApi.HasRelic((Relic) relic))
					{
						SetRelicLocationDisplay((Relic) relic, false);
						sotnApi.AlucardApi.GrantRelic((Relic) relic, true);
					}
				}
				else
				{
					if ((int) relic < 25)
					{
						SetRelicLocationDisplay((Relic) relic, true);
						sotnApi.AlucardApi.TakeRelic((Relic) relic);
					}
					else if (randomizeVladRelics)
					{
						sotnApi.AlucardApi.TakeRelic((Relic) relic);
					}
				}
			}

			if (alucardSecondCastle)
			{
				int roll = rng.Next(0, Constants.Khaos.FlightRelics.Count);
				foreach (Relic relic in Constants.Khaos.FlightRelics[roll])
				{
					SetRelicLocationDisplay((Relic) relic, false);
					sotnApi.AlucardApi.GrantRelic((Relic) relic, true);
				}
			}

			if (IsInRoomList(Constants.Khaos.SwitchRoom))
			{
				SetRelicLocationDisplay(Relic.JewelOfOpen, false);
				sotnApi.AlucardApi.GrantRelic(Relic.JewelOfOpen, true);
			}
		}
		private void RandomizeEquipmentSlots()
		{
			bool equippedHolyGlasses = Equipment.Items[(int) (sotnApi.AlucardApi.Helm + Equipment.HandCount + 1)] == "Holy glasses";
			bool equippedSpikeBreaker = Equipment.Items[(int) (sotnApi.AlucardApi.Armor + Equipment.HandCount + 1)] == "Spike Breaker";
			bool equippedGoldRing = Equipment.Items[(int) (sotnApi.AlucardApi.Accessory1 + Equipment.HandCount + 1)] == "Gold Ring" || Equipment.Items[(int) (sotnApi.AlucardApi.Accessory2 + Equipment.HandCount + 1)] == "Gold Ring";
			bool equippedSilverRing = Equipment.Items[(int) (sotnApi.AlucardApi.Accessory1 + Equipment.HandCount + 1)] == "Silver Ring" || Equipment.Items[(int) (sotnApi.AlucardApi.Accessory2 + Equipment.HandCount + 1)] == "Silver Ring";

			uint newRightHand = (uint) rng.Next(0, Equipment.HandCount + 1);
			uint newLeftHand = (uint) rng.Next(0, Equipment.HandCount + 1);
			uint newArmor = (uint) rng.Next(0, Equipment.ArmorCount + 1);
			uint newHelm = Equipment.HelmStart + (uint) rng.Next(0, Equipment.HelmCount + 1);
			uint newCloak = Equipment.CloakStart + (uint) rng.Next(0, Equipment.CloakCount + 1);
			uint newAccessory1 = Equipment.AccessoryStart + (uint) rng.Next(0, Equipment.AccessoryCount + 1);
			uint newAccessory2 = Equipment.AccessoryStart + (uint) rng.Next(0, Equipment.AccessoryCount + 1);

			sotnApi.AlucardApi.RightHand = 0;
			sotnApi.AlucardApi.LeftHand = 0;
			sotnApi.AlucardApi.GrantItemByName(Equipment.Items[(int) newRightHand]);
			sotnApi.AlucardApi.GrantItemByName(Equipment.Items[(int) newLeftHand]);
			sotnApi.AlucardApi.Armor = newArmor;
			sotnApi.AlucardApi.Helm = newHelm;
			sotnApi.AlucardApi.Cloak = newCloak;
			sotnApi.AlucardApi.Accessory1 = newAccessory1;
			sotnApi.AlucardApi.Accessory2 = newAccessory2;

			RandomizeSubweapon();

			sotnApi.AlucardApi.GrantItemByName("Library card");
			sotnApi.AlucardApi.GrantItemByName("Library card");
			if (equippedHolyGlasses)
			{
				sotnApi.AlucardApi.GrantItemByName("Holy glasses");
			}
			if (equippedSpikeBreaker)
			{
				sotnApi.AlucardApi.GrantItemByName("Spike Breaker");
			}
			if (equippedGoldRing)
			{
				sotnApi.AlucardApi.GrantItemByName("Gold Ring");
			}
			if (equippedSilverRing)
			{
				sotnApi.AlucardApi.GrantItemByName("Silver Ring");
			}
		}
		#endregion
		#region Debuff events
		private void BloodManaUpdate()
		{
			uint currentMaxMana = sotnApi.AlucardApi.MaxtMp;

			if (currentMaxMana < storedMaxMana)
			{
				storedMaxMana = currentMaxMana;
				return;
			}

			if (spentMana > 0)
			{
				if (currentHp > spentMana)
				{
					sotnApi.AlucardApi.CurrentHp -= (uint) spentMana;
					sotnApi.AlucardApi.CurrentMp += (uint) spentMana;
				}
				else
				{
					sotnApi.AlucardApi.CurrentMp = 0;
					sotnApi.AlucardApi.CurrentHp = 0;
					sotnApi.AlucardApi.RightHand = (uint) Equipment.Items.IndexOf("Orange");
					sotnApi.AlucardApi.LeftHand = (uint) Equipment.Items.IndexOf("Orange");
					scheduledActions[(int) Enums.Event.KillAlucard].Start();
				}
			}
		}
		private void KillAlucard()
		{
			sotnApi.EntityApi.SpawnEntity(Constants.Khaos.KillEntity);
			sotnApi.AlucardApi.InvincibilityTimer = 0;
			scheduledActions[(int) Enums.Event.KhaosTrackOff].Stop();
		}
		private void BloodManaOff()
		{
			ManaLocked = false;
			bloodManaActive = false;
		}
		private void CheckThirstKill()
		{
			if (!thirstActive)
			{
				return;
			}

			uint updatedCurrentKills = sotnApi.AlucardApi.Kills;

			if (updatedCurrentKills < currentKills)
			{
				currentKills = updatedCurrentKills;
			}

			if (updatedCurrentKills > currentKills || sotnApi.GameApi.CanSave())
			{
				currentKills = updatedCurrentKills;
				thirstLevel = 0;
				thirstGlow = false;
				SetGlow();
			}
			else if (thirstLevel < 2.8)
			{
				thirstLevel += Constants.Khaos.ThirstLevelIncreaseRate;
				if (thirstLevel > 2.3F)
				{
					thirstGlow = true;
					SetGlow();
				}
			}
		}
		private void ThirstDrain()
		{
			if (thirstLevel < 1)
			{
				return;
			}

			uint superDrain = superThirst ? Constants.Khaos.SuperThirstExtraDrain : 0u;

			uint drainAmount = (uint) Math.Round((toolConfig.Khaos.ThirstDrainPerSecond + superDrain) * thirstLevel);

			if (currentHp > drainAmount + 1)
			{
				sotnApi.AlucardApi.CurrentHp -= drainAmount;
			}
			else
			{
				sotnApi.AlucardApi.CurrentHp = 1;
			}
		}
		private void ThirstOff()
		{
			cheatsController.VisualEffectPalette.Disable();
			cheatsController.VisualEffectTimer.Disable();
			cheatsController.DarkMetamorphasis.Disable();
			scheduledActions[(int) Enums.Event.ThirstDrain].Stop();
			superThirst = false;
			thirstActive = false;
		}
		private void HordeOff()
		{
			superHorde = false;
			SpawnActive = false;
			hordeEnemies.Clear();
			scheduledActions[(int) Enums.Event.HordeSpawn].Stop();
		}
		private void HordeSpawn()
		{
			if (!inAlucardMode || !canMenu || currentHp < 5 || canSave || IsInRoomList(Constants.Khaos.RichterRooms) || IsInRoomList(Constants.Khaos.ShopRoom) || IsInRoomList(Constants.Khaos.LesserDemonZone))
			{
				return;
			}

			if (hordeZone != area)
			{
				hordeEnemies.Clear();
				hordeZone = area;
			}

			FindHordeEnemy();

			if (hordeEnemies.Count > 0)
			{
				int enemyIndex = rng.Next(0, hordeEnemies.Count);
				if (hordeWaiting)
				{
					hordeWaiting = false;
					notificationService.AddOverlayTimer((int) Enums.Action.KhaosHorde, (int) toolConfig.Khaos.Actions[(int) Enums.Action.KhaosHorde].Duration.TotalMilliseconds);
					statusInfoDisplay.AddTimer((int) Enums.Action.KhaosHorde);
					scheduledActions[(int) Enums.Event.HordeOff].Start();
				}
				hordeEnemies[enemyIndex].Xpos = (rng.NextDouble() * 244) + 10;
				hordeEnemies[enemyIndex].Ypos = (rng.NextDouble() * 120);

				if (superHorde)
				{
					int damageTypeRoll = rng.Next(0, 5);

					switch (damageTypeRoll)
					{
						case 1:
							hordeEnemies[enemyIndex].DamageType = Entities.Poison;
							break;
						case 2:
							hordeEnemies[enemyIndex].DamageType = Entities.Curse;
							break;
						case 3:
							hordeEnemies[enemyIndex].DamageType = Entities.Stone;
							hordeEnemies[enemyIndex].DamageType = Entities.Stone;
							break;
						case 4:
							hordeEnemies[enemyIndex].DamageType = Entities.Slam;
							break;
						default:
							break;
					}
				}

				sotnApi.EntityApi.SpawnEntity(hordeEnemies[enemyIndex]);
			}
		}
		private bool FindHordeEnemy()
		{
			if ((roomX == hordeTriggerRoomX && roomY == hordeTriggerRoomY) || !inAlucardMode || !canMenu)
			{
				return false;
			}

			long enemy = sotnApi.EntityApi.FindEntityFrom(Constants.Khaos.AcceptedHordeEnemies);

			if (enemy > 0)
			{
				Entity hordeEnemy = new Entity(sotnApi.EntityApi.GetEntity(enemy));

				for (int i = 0; i < hordeEnemies.Count; i++)
				{
					if (hordeEnemies[i].EnemyId == hordeEnemy.EnemyId)
					{
						return false;
					}
				}

				if (superHorde)
				{
					hordeEnemy.Hp *= 2;
					hordeEnemy.Damage *= 2;
				}
				hordeEnemy.Palette += (byte) rng.Next(1, 10);
				hordeEnemies.Add(hordeEnemy);
				return true;
			}

			return false;
		}
		private void SlowOff()
		{
			cheatsController.UnderwaterPhysics.Disable();
			sotnApi.GameApi.UnderwaterPhysicsEnabled = false;
			scheduledActions[(int) Enums.Event.SlowOff].Stop();
			slowActive = false;
			slowPaused = false;
		}
		private void EnduranceSpawn()
		{
			float healthMultiplier = 3.5F;

			if ((roomX == enduranceRoomX && roomY == enduranceRoomY) || !inAlucardMode || !canMenu)
			{
				return;
			}

			Entity? bossCopy = null;

			long enemy = sotnApi.EntityApi.FindEntityFrom(Constants.Khaos.EnduranceBosses);

			if (enemy > 0)
			{
				Entity boss = sotnApi.EntityApi.GetLiveEntity(enemy);
				bossCopy = new Entity(sotnApi.EntityApi.GetEntity(enemy));
				string name = Constants.Khaos.EnduranceBossNames[boss.EnemyId];
				if (alucardSecondCastle && (name == "Karasuman" || name == "Gaibon"))
				{
					return;
				}

				bool right = rng.Next(0, 2) > 0;
				bossCopy.Xpos = right ? (double) (bossCopy.Xpos + rng.Next(40, 80)) : (double) (bossCopy.Xpos + rng.Next(-80, -40));
				bossCopy.Palette += (byte) rng.Next(1, 10);
				short newhp = (short) Math.Round(healthMultiplier * bossCopy.Hp);
				if (newhp > Int16.MaxValue)
				{
					newhp = Int16.MaxValue - 200;
				}
				bossCopy.Hp = newhp;
				sotnApi.EntityApi.SpawnEntity(bossCopy);

				boss.Hp = newhp;

				if (superEnduranceCount > 0)
				{
					superEnduranceCount--;

					bossCopy.Xpos = rng.Next(0, 2) == 1 ? (double) (bossCopy.Xpos + rng.Next(-80, -20)) : (double) (bossCopy.Xpos + rng.Next(20, 80));
					bossCopy.Palette = (byte) (bossCopy.Palette + rng.Next(1, 10));
					sotnApi.EntityApi.SpawnEntity(bossCopy);
					notificationService.AddMessage($"Super Endurance {name}");
				}
				else
				{
					notificationService.AddMessage($"Endurance {name}");
				}

				enduranceCount--;
				enduranceRoomX = roomX;
				enduranceRoomY = roomY;
				if (enduranceCount == 0)
				{
					scheduledActions[(int) Enums.Event.EnduranceSpawn].Stop();
				}
				return;
			}

			enemy = sotnApi.EntityApi.FindEntityFrom(Constants.Khaos.EnduranceAlternateBosses);
			if (enemy > 0)
			{
				Entity boss = sotnApi.EntityApi.GetLiveEntity(enemy);
				string name = Constants.Khaos.EnduranceBossNames[boss.EnemyId];
				if (alucardSecondCastle && (name == "Karasuman" || name == "Gaibon"))
				{
					return;
				}

				boss.Palette += (byte) rng.Next(1, 10);

				if (superEnduranceCount > 0)
				{
					short newhp = (short) Math.Round((healthMultiplier * 2.3) * boss.Hp);
					if (newhp > Int16.MaxValue)
					{
						newhp = Int16.MaxValue - 200;
					}
					boss.Hp = newhp;
					superEnduranceCount--;
					notificationService.AddMessage($"Super Endurance {name}");
				}
				else
				{
					short newhp = (short) Math.Round((healthMultiplier * 1.3) * boss.Hp);
					if (newhp > Int16.MaxValue)
					{
						newhp = Int16.MaxValue - 200;
					}
					boss.Hp = newhp;
					notificationService.AddMessage($"Endurance {name}");
				}

				enduranceCount--;
				enduranceRoomX = roomX;
				enduranceRoomY = roomY;
				if (enduranceCount == 0)
				{
					scheduledActions[(int) Enums.Event.EnduranceSpawn].Stop();
				}
			}
			else
			{
				return;
			}
		}
		private void SpawnPoisonHitbox()
		{
			int roll = rng.Next(0, 2);
			Constants.Khaos.PoisonEntity.Xpos = roll == 1 ? (double) (sotnApi.AlucardApi.ScreenX + 1) : (double) 0;
			sotnApi.EntityApi.SpawnEntity(Constants.Khaos.PoisonEntity);
		}
		private void SpawnCurseHitbox()
		{
			int roll = rng.Next(0, 2);
			Constants.Khaos.CurseEntity.Xpos = roll == 1 ? (double) (sotnApi.AlucardApi.ScreenX + 1) : (double) 0;
			sotnApi.EntityApi.SpawnEntity(Constants.Khaos.CurseEntity);
		}
		private void SpawnStoneHitbox()
		{
			int roll = rng.Next(0, 2);
			Constants.Khaos.StoneEntity.Xpos = roll == 1 ? (double) (sotnApi.AlucardApi.ScreenX + 1) : (double) 0;
			sotnApi.EntityApi.SpawnEntity(Constants.Khaos.StoneEntity);
		}
		private void SpawnSlamHitbox()
		{
			int roll = rng.Next(0, 2);
			Constants.Khaos.SlamEntity.Xpos = roll == 1 ? (double) (sotnApi.AlucardApi.ScreenX + 1) : (double) 0;
			Constants.Khaos.SlamEntity.Damage = (ushort) (sotnApi.AlucardApi.Def + 2);
			sotnApi.EntityApi.SpawnEntity(Constants.Khaos.SlamEntity);
		}
		private void ActivateDizzy()
		{
			dizzyActive = true;
			sotnApi.GameApi.SetMovementSpeedDirection(true);
			scheduledActions[(int) Enums.Event.DizzyOff].Start();
		}
		private void DizzyOff()
		{
			dizzyActive = false;
			sotnApi.GameApi.SetMovementSpeedDirection(false);
		}
		private void BankruptActivate()
		{
			bool clearRightHand = false;
			bool clearLeftHand = false;
			bool clearHelm = false;
			bool clearArmor = false;
			bool clearCloak = false;
			bool clearAccessory1 = false;
			bool clearAccessory2 = false;

			float goldPercentage = 0;
			int clearedSlots = 0;

			switch (bankruptLevel)
			{
				case 1:
					goldPercentage = 0.5f;
					clearedSlots = 2;
					break;
				case 2:
					goldPercentage = 0.2f;
					clearedSlots = 4;
					break;
				case 3:
					goldPercentage = 0;
					clearedSlots = 7;
					break;

				default:
					break;
			}

			bool hasHolyGlasses = sotnApi.AlucardApi.HasItemInInventory("Holy glasses");
			bool hasSpikeBreaker = sotnApi.AlucardApi.HasItemInInventory("Spike Breaker");
			bool hasGoldRing = sotnApi.AlucardApi.HasItemInInventory("Gold Ring");
			bool hasSilverRing = sotnApi.AlucardApi.HasItemInInventory("Silver Ring");
			bool equippedHolyGlasses = Equipment.Items[(int) (sotnApi.AlucardApi.Helm + Equipment.HandCount + 1)] == "Holy glasses";
			bool equippedSpikeBreaker = Equipment.Items[(int) (sotnApi.AlucardApi.Armor + Equipment.HandCount + 1)] == "Spike Breaker";
			bool equippedGoldRing = Equipment.Items[(int) (sotnApi.AlucardApi.Accessory1 + Equipment.HandCount + 1)] == "Gold Ring" || Equipment.Items[(int) (sotnApi.AlucardApi.Accessory2 + Equipment.HandCount + 1)] == "Gold Ring";
			bool equippedSilverRing = Equipment.Items[(int) (sotnApi.AlucardApi.Accessory1 + Equipment.HandCount + 1)] == "Silver Ring" || Equipment.Items[(int) (sotnApi.AlucardApi.Accessory2 + Equipment.HandCount + 1)] == "Silver Ring";


			sotnApi.AlucardApi.Gold = goldPercentage == 0 ? 0 : (uint) Math.Round(sotnApi.AlucardApi.Gold * goldPercentage);
			sotnApi.AlucardApi.ClearInventory();

			if (clearedSlots == 7)
			{
				clearRightHand = true;
				clearLeftHand = true;
				clearHelm = true;
				clearArmor = true;
				clearCloak = true;
				clearAccessory1 = true;
				clearAccessory2 = true;
			}
			else
			{
				int[] slots = new int[clearedSlots + 1];
				List<int> range = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
				int slotsIndex = 0;
				for (int i = 0; i <= clearedSlots; i++)
				{
					int result = rng.Next(0, range.Count);
					slots[slotsIndex] = range[result];
					range.RemoveAt(result);
					slotsIndex++;
				}

				for (int i = 0; i < slots.Length; i++)
				{
					switch (slots[i])
					{
						case 1:
							clearRightHand = true;
							break;
						case 2:
							clearLeftHand = true;
							break;
						case 3:
							clearHelm = true;
							break;
						case 4:
							clearArmor = true;
							break;
						case 5:
							clearCloak = true;
							break;
						case 6:
							clearAccessory1 = true;
							break;
						case 7:
							clearAccessory1 = true;
							break;
						default:
							break;
					}
				}
			}

			if (clearRightHand)
			{
				sotnApi.AlucardApi.RightHand = 0;
			}
			if (clearLeftHand)
			{
				sotnApi.AlucardApi.LeftHand = 0;
			}
			if (!equippedHolyGlasses && clearHelm)
			{
				sotnApi.AlucardApi.Helm = Equipment.HelmStart;
			}
			if (!equippedSpikeBreaker && clearArmor)
			{
				sotnApi.AlucardApi.Armor = 0;
			}
			if (clearCloak)
			{
				sotnApi.AlucardApi.Cloak = Equipment.CloakStart;
			}
			if (clearAccessory1)
			{
				sotnApi.AlucardApi.Accessory1 = Equipment.AccessoryStart;
			}
			if (clearAccessory2)
			{
				sotnApi.AlucardApi.Accessory2 = Equipment.AccessoryStart;
			}

			sotnApi.GameApi.RespawnItems();

			sotnApi.AlucardApi.HandCursor = 0;
			sotnApi.AlucardApi.HelmCursor = 0;
			sotnApi.AlucardApi.ArmorCursor = 0;
			sotnApi.AlucardApi.CloakCursor = 0;
			sotnApi.AlucardApi.AccessoryCursor = 0;

			if (hasHolyGlasses)
			{
				sotnApi.AlucardApi.GrantItemByName("Holy glasses");
			}
			if (hasSpikeBreaker)
			{
				sotnApi.AlucardApi.GrantItemByName("Spike Breaker");
			}
			if (equippedGoldRing || hasGoldRing)
			{
				sotnApi.AlucardApi.GrantItemByName("Gold Ring");
			}
			if (equippedSilverRing || hasSilverRing)
			{
				sotnApi.AlucardApi.GrantItemByName("Silver Ring");
			}
			if (bankruptLevel < 3)
			{
				bankruptLevel++;
			}
		}
		private void HnkOff()
		{
			hnkOn = false;
			cheatsController.DefencePotion.Disable();
			InvincibilityLocked = false;
		}
		private void BulletHellOff()
		{
			bulletHellActive = false;
			superBulletHell = false;
			FlowerEmitter.Enabled = false;
			ZigZagEmitter.Enabled = false;
			JailFirstEmitter.Enabled = false;
			JailSecondEmitter.Enabled = false;

			for (int i = 0; i < bullets.Count; i++)
			{
				bullets[i].Palette = 0;
				bullets[i].Xpos = 0;
				bullets[i].Ypos = 0;
				bullets[i].AccelerationX = 0;
				bullets[i].AccelerationY = 0;
				bullets[i].HitboxWidth = 0;
				bullets[i].HitboxHeight = 0;
				bullets[i].Damage = 0;
				bullets[i].DamageType = 0;
				bullets[i].InvincibilityFrames = 0;
				bullets[i].ZPriority = 0;
				bullets[i].AnimationFrameIndex = 0;
				bullets[i].AnimationSet = 0;
				bullets[i].AnimationCurrentFrame = 0;
				bullets[i].AnimationFrameDuration = 0;
				bullets[i].HitboxState = 0;
				bullets[i].EnemyId = 0;
			}
			bullets.Clear();
		}
		private void EmittersFrameAdvance()
		{
			if (!bulletHellActive || sotnApi.GameApi.MapOpen)
			{
				return;
			}

			int emittersActive = 0;
			int minEmitters = superBulletHell ? 2 : 1;

			if (FlowerEmitter.Enabled)
			{
				emittersActive++;
			}
			if (ZigZagEmitter.Enabled)
			{
				emittersActive++;
			}
			if (JailFirstEmitter.Enabled)
			{
				emittersActive++;
			}
			if (DiamondEmitter.Enabled)
			{
				emittersActive++;
			}

			while (emittersActive < minEmitters)
			{
				double xpos = rng.Next(20, 230);
				double ypos = rng.Next(2, 15);
				double accelX = rng.NextDouble() / 3;
				double accelY = rng.NextDouble() / 3;

				emittersActive++;
				int roll = rng.Next(0, 4);
				switch (roll)
				{
					case 0:
						FlowerEmitter.Enabled = true;
						FlowerEmitter.Xpos = xpos;
						FlowerEmitter.Ypos = ypos;
						FlowerEmitter.AccelerationX = accelX;
						FlowerEmitter.AccelerationY = accelY;
						break;
					case 1:
						ZigZagEmitter.Enabled = true;
						ZigZagEmitter.Xpos = xpos;
						ZigZagEmitter.Ypos = ypos;
						ZigZagEmitter.AccelerationX = accelX;
						ZigZagEmitter.AccelerationY = accelY;
						break;
					case 2:
						DiamondEmitter.Enabled = true;
						DiamondEmitter.Xpos = xpos;
						DiamondEmitter.Ypos = ypos;
						DiamondEmitter.AccelerationX = accelX;
						DiamondEmitter.AccelerationY = accelY;
						break;
					case 3:
						JailFirstEmitter.Enabled = true;
						JailFirstEmitter.Xpos = rng.Next(2, 25);
						JailFirstEmitter.Ypos = ypos;
						JailFirstEmitter.AccelerationX = rng.NextDouble() / 4;
						JailFirstEmitter.AccelerationY = rng.NextDouble() / 4;
						JailSecondEmitter.Enabled = true;
						JailSecondEmitter.Xpos = rng.Next(200, 254);
						JailSecondEmitter.Ypos = rng.Next(2, 15);
						JailSecondEmitter.AccelerationX = rng.NextDouble() / 4;
						JailSecondEmitter.AccelerationY = rng.NextDouble() / 4;
						break;
					default:
						break;
				}
			}

			FlowerFrameAdvance();
			ZigZagFrameAdvance();
			DiamondFrameAdvance();
			JailFrameAdvance();
		}
		private void FlowerFrameAdvance()
		{
			if (!FlowerEmitter.Enabled)
			{
				return;
			}

			FlowerEmitter.Xpos += cameraAdjustmentX + FlowerEmitter.AccelerationX;
			FlowerEmitter.Ypos += cameraAdjustmentY + FlowerEmitter.AccelerationY;

			if (FlowerEmitter.Xpos < -100 || FlowerEmitter.Xpos > 355 || FlowerEmitter.Ypos < -100 || FlowerEmitter.Ypos > 355)
			{
				FlowerEmitter.Enabled = false;
				return;
			}

			if (FlowerEmitter.Xpos > 253 && FlowerEmitter.AccelerationX > 0)
			{
				FlowerEmitter.AccelerationX *= -1;
			}
			if (FlowerEmitter.Xpos < 2 && FlowerEmitter.AccelerationX < 0)
			{
				FlowerEmitter.AccelerationX *= -1;
			}

			FlowerEmitter.BulletTimer--;
			if (FlowerEmitter.BulletTimer > 0)
			{
				return;
			}
			FlowerEmitter.BulletTimer = FlowerEmitter.BulletCooldown;
			FlowerEmitter.BurstCounter++;
			if (FlowerEmitter.BurstCounter >= FlowerEmitter.BurstLimit)
			{
				FlowerEmitter.BulletTimer = FlowerEmitter.BurstCooldown;
				FlowerEmitter.BurstCounter = 0;
				FlowerEmitter.BulletSpeed = Constants.Khaos.SmallBulletSpeed;
				return;
			}
			FlowerEmitter.BulletSpeed += 0.3;
			FlowerEmitter.Angle += FlowerEmitter.Rotation;

			for (int j = 0; j <= FlowerEmitter.Shots; j++)
			{
				double angle = (2 * Math.PI * j / FlowerEmitter.Shots) + FlowerEmitter.Angle;
				Constants.Khaos.FlowerBullet.Xpos = FlowerEmitter.Xpos;
				Constants.Khaos.FlowerBullet.Ypos = FlowerEmitter.Ypos;
				long address = sotnApi.EntityApi.SpawnEntity(Constants.Khaos.FlowerBullet);
				Entity liveBullet = sotnApi.EntityApi.GetLiveEntity(address);
				liveBullet.Locals.AccelerationX = FlowerEmitter.BulletSpeed * Math.Cos(angle);
				liveBullet.Locals.AccelerationY = FlowerEmitter.BulletSpeed * Math.Sin(angle);
				bullets.Add(liveBullet);
			}
		}
		private void ZigZagFrameAdvance()
		{
			if (!ZigZagEmitter.Enabled)
			{
				return;
			}

			ZigZagEmitter.Xpos += cameraAdjustmentX + ZigZagEmitter.AccelerationX;
			ZigZagEmitter.Ypos += cameraAdjustmentY + ZigZagEmitter.AccelerationY;

			if (ZigZagEmitter.Xpos < -100 || ZigZagEmitter.Xpos > 355 || ZigZagEmitter.Ypos < -100 || ZigZagEmitter.Ypos > 355)
			{
				ZigZagEmitter.Enabled = false;
				return;
			}

			if (ZigZagEmitter.Xpos > 253 && ZigZagEmitter.AccelerationX > 0)
			{
				ZigZagEmitter.AccelerationX *= -1;
			}
			if (ZigZagEmitter.Xpos < 2 && ZigZagEmitter.AccelerationX < 0)
			{
				ZigZagEmitter.AccelerationX *= -1;
			}

			ZigZagEmitter.BulletTimer--;
			if (ZigZagEmitter.BulletTimer > 0)
			{
				return;
			}
			ZigZagEmitter.BulletTimer = ZigZagEmitter.BulletCooldown;
			ZigZagEmitter.BurstCounter++;
			if (ZigZagEmitter.BurstCounter >= ZigZagEmitter.BurstLimit)
			{
				ZigZagEmitter.BulletTimer = ZigZagEmitter.BurstCooldown;
				ZigZagEmitter.BurstCounter = 0;
				ZigZagEmitter.Rotation *= -1;
				return;
			}
			ZigZagEmitter.Angle += ZigZagEmitter.Rotation;

			for (int j = 0; j <= ZigZagEmitter.Shots; j++)
			{
				double angle = (2 * Math.PI * j / ZigZagEmitter.Shots) + ZigZagEmitter.Angle;
				//if (angle > 2 * Math.PI || angle < 0)
				//{
				//	continue;
				//}
				Constants.Khaos.ZigZagBullet.Xpos = ZigZagEmitter.Xpos;
				Constants.Khaos.ZigZagBullet.Ypos = ZigZagEmitter.Ypos;
				long address = sotnApi.EntityApi.SpawnEntity(Constants.Khaos.ZigZagBullet);
				Entity liveBullet = sotnApi.EntityApi.GetLiveEntity(address);
				liveBullet.Locals.AccelerationX = ZigZagEmitter.BulletSpeed * Math.Cos(angle);
				liveBullet.Locals.AccelerationY = ZigZagEmitter.BulletSpeed * Math.Sin(angle);
				bullets.Add(liveBullet);
			}
		}
		private void DiamondFrameAdvance()
		{
			if (!DiamondEmitter.Enabled)
			{
				return;
			}

			DiamondEmitter.Xpos += cameraAdjustmentX + DiamondEmitter.AccelerationX;
			DiamondEmitter.Ypos += cameraAdjustmentY + DiamondEmitter.AccelerationY;

			if (DiamondEmitter.Xpos < -100 || DiamondEmitter.Xpos > 355 || DiamondEmitter.Ypos < -100 || DiamondEmitter.Ypos > 355)
			{
				DiamondEmitter.Enabled = false;
				return;
			}

			if (DiamondEmitter.Xpos > 253 && DiamondEmitter.AccelerationX > 0)
			{
				DiamondEmitter.AccelerationX *= -1;
			}
			if (DiamondEmitter.Xpos < 2 && DiamondEmitter.AccelerationX < 0)
			{
				DiamondEmitter.AccelerationX *= -1;
			}

			DiamondEmitter.BulletTimer--;
			if (DiamondEmitter.BulletTimer > 0)
			{
				return;
			}
			DiamondEmitter.BulletTimer = DiamondEmitter.BulletCooldown;

			double angle = Math.Atan2(alucardEntity.Ypos - DiamondEmitter.Ypos, alucardEntity.Xpos - DiamondEmitter.Xpos);

			Constants.Khaos.DiamondBullet.Xpos = DiamondEmitter.Xpos;
			Constants.Khaos.DiamondBullet.Ypos = DiamondEmitter.Ypos;
			long address = sotnApi.EntityApi.SpawnEntity(Constants.Khaos.DiamondBullet);
			Entity liveBullet = sotnApi.EntityApi.GetLiveEntity(address);
			liveBullet.Locals.AccelerationX = DiamondEmitter.BulletSpeed * Math.Cos(angle);
			liveBullet.Locals.AccelerationY = DiamondEmitter.BulletSpeed * Math.Sin(angle);
			bullets.Add(liveBullet);

			Constants.Khaos.DiamondBullet.Xpos = DiamondEmitter.Xpos;
			Constants.Khaos.DiamondBullet.Ypos = DiamondEmitter.Ypos;
			address = sotnApi.EntityApi.SpawnEntity(Constants.Khaos.DiamondBullet);
			liveBullet = sotnApi.EntityApi.GetLiveEntity(address);
			liveBullet.Locals.AccelerationX = (DiamondEmitter.BulletSpeed * 1.4) * Math.Cos(angle);
			liveBullet.Locals.AccelerationY = (DiamondEmitter.BulletSpeed * 1.4) * Math.Sin(angle);
			bullets.Add(liveBullet);

			Constants.Khaos.DiamondBullet.Xpos = DiamondEmitter.Xpos;
			Constants.Khaos.DiamondBullet.Ypos = DiamondEmitter.Ypos;
			address = sotnApi.EntityApi.SpawnEntity(Constants.Khaos.DiamondBullet);
			liveBullet = sotnApi.EntityApi.GetLiveEntity(address);
			liveBullet.Locals.AccelerationX = (DiamondEmitter.BulletSpeed * 1.2) * Math.Cos(angle + 0.0174533 * 5);
			liveBullet.Locals.AccelerationY = (DiamondEmitter.BulletSpeed * 1.2) * Math.Sin(angle + 0.0174533 * 5);
			bullets.Add(liveBullet);

			Constants.Khaos.DiamondBullet.Xpos = DiamondEmitter.Xpos;
			Constants.Khaos.DiamondBullet.Ypos = DiamondEmitter.Ypos;
			address = sotnApi.EntityApi.SpawnEntity(Constants.Khaos.DiamondBullet);
			liveBullet = sotnApi.EntityApi.GetLiveEntity(address);
			liveBullet.Locals.AccelerationX = (DiamondEmitter.BulletSpeed * 1.2) * Math.Cos(angle - 0.0174533 * 5);
			liveBullet.Locals.AccelerationY = (DiamondEmitter.BulletSpeed * 1.2) * Math.Sin(angle - 0.0174533 * 5);
			bullets.Add(liveBullet);
		}
		private void JailFrameAdvance()
		{
			if (!JailFirstEmitter.Enabled)
			{
				return;
			}

			JailFirstEmitter.Xpos += cameraAdjustmentX + JailFirstEmitter.AccelerationX;
			JailFirstEmitter.Ypos += cameraAdjustmentY + JailFirstEmitter.AccelerationY;
			JailSecondEmitter.Xpos += cameraAdjustmentX + JailSecondEmitter.AccelerationX;
			JailSecondEmitter.Ypos += cameraAdjustmentY + JailSecondEmitter.AccelerationY;

			if (JailFirstEmitter.Xpos < -100 || JailFirstEmitter.Xpos > 355 || JailFirstEmitter.Ypos < -100 || JailFirstEmitter.Ypos > 355)
			{
				JailFirstEmitter.Enabled = false;
				return;
			}

			if (JailSecondEmitter.Xpos < -100 || JailSecondEmitter.Xpos > 355 || JailSecondEmitter.Ypos < -100 || JailSecondEmitter.Ypos > 355)
			{
				JailSecondEmitter.Enabled = false;
				return;
			}

			if (JailFirstEmitter.Xpos > 253 && JailFirstEmitter.AccelerationX > 0)
			{
				JailFirstEmitter.AccelerationX *= -1;
			}
			if (JailFirstEmitter.Xpos < 2 && JailFirstEmitter.AccelerationX < 0)
			{
				JailFirstEmitter.AccelerationX *= -1;
			}

			if (JailSecondEmitter.Xpos > 253 && JailSecondEmitter.AccelerationX > 0)
			{
				JailSecondEmitter.AccelerationX *= -1;
			}
			if (JailSecondEmitter.Xpos < 2 && JailSecondEmitter.AccelerationX < 0)
			{
				JailSecondEmitter.AccelerationX *= -1;
			}

			JailFirstEmitter.BulletTimer--;
			if (JailFirstEmitter.BulletTimer > 0)
			{
				return;
			}
			JailFirstEmitter.BulletTimer = JailFirstEmitter.BulletCooldown;
			JailFirstEmitter.BurstCounter++;
			if (JailFirstEmitter.BurstCounter >= JailFirstEmitter.BurstLimit)
			{
				JailFirstEmitter.BulletTimer = JailFirstEmitter.BurstCooldown;
				JailFirstEmitter.BurstCounter = 0;
				return;
			}

			for (int j = 0; j <= JailFirstEmitter.Shots; j++)
			{
				double angle = 2 * Math.PI * j / JailFirstEmitter.Shots;
				Constants.Khaos.JailBullet.Xpos = JailFirstEmitter.Xpos;
				Constants.Khaos.JailBullet.Ypos = JailFirstEmitter.Ypos;
				long address = sotnApi.EntityApi.SpawnEntity(Constants.Khaos.JailBullet);
				Entity liveBullet = sotnApi.EntityApi.GetLiveEntity(address);
				liveBullet.Locals.AccelerationX = JailFirstEmitter.BulletSpeed * Math.Cos(angle);
				liveBullet.Locals.AccelerationY = JailFirstEmitter.BulletSpeed * Math.Sin(angle);
				bullets.Add(liveBullet);

				Constants.Khaos.JailBullet.Xpos = JailSecondEmitter.Xpos;
				Constants.Khaos.JailBullet.Ypos = JailSecondEmitter.Ypos;
				address = sotnApi.EntityApi.SpawnEntity(Constants.Khaos.JailBullet);
				liveBullet = sotnApi.EntityApi.GetLiveEntity(address);
				liveBullet.Locals.AccelerationX = JailSecondEmitter.BulletSpeed * Math.Cos(angle);
				liveBullet.Locals.AccelerationY = JailSecondEmitter.BulletSpeed * Math.Sin(angle);
				bullets.Add(liveBullet);
			}
		}
		private void BulletsFrameAdvance()
		{
			if (!bulletHellActive || bullets.Count < 1 || sotnApi.GameApi.MapOpen)
			{
				return;
			}

			for (int i = 0; i < bullets.Count; i++)
			{
				if (bullets[i].Xpos > 300 || bullets[i].Ypos > 300 || bullets[i].Xpos < -50 || bullets[i].Ypos < -50)
				{
					bullets[i].Palette = 0;
					bullets[i].Xpos = 0;
					bullets[i].Ypos = 0;
					bullets[i].AccelerationX = 0;
					bullets[i].AccelerationY = 0;
					bullets[i].HitboxWidth = 0;
					bullets[i].HitboxHeight = 0;
					bullets[i].Damage = 0;
					bullets[i].DamageType = 0;
					bullets[i].InvincibilityFrames = 0;
					bullets[i].ZPriority = 0;
					bullets[i].AnimationFrameIndex = 0;
					bullets[i].AnimationSet = 0;
					bullets[i].AnimationCurrentFrame = 0;
					bullets[i].AnimationFrameDuration = 0;
					bullets[i].HitboxState = 0;
					bullets[i].EnemyId = 0;
					continue;
				}
				if (bullets[i].Damage == 0 || bullets[i].ZPriority != 0x00FF)
				{
					continue;
				}

				bullets[i].Xpos += bullets[i].Locals.AccelerationX + cameraAdjustmentX;
				bullets[i].Ypos += bullets[i].Locals.AccelerationY + cameraAdjustmentY;
			}

			bullets.RemoveAll(f => f.ZPriority != 0x00FF || f.Damage == 0);
		}
		#endregion
		#region Buff events
		private void QuadOff()
		{
			quadActive = false;
			cheatsController.AttackPotion.Disable();
			cheatsController.StrengthPotion.Disable();
			SetGlow();
		}
		private void MagicianOff()
		{
			cheatsController.Mana.Disable();
			ManaLocked = false;
		}
		private void BattleOrdersOff()
		{
			sotnApi.AlucardApi.MaxtHp -= (uint) battleOrdersBonusHp;
			sotnApi.AlucardApi.MaxtMp -= (uint) battleOrdersBonusMp;
			battleOrdersActive = false;
			battleOrdersBonusHp = 0;
			battleOrdersBonusMp = 0;
		}
		private void MeltyFrameAdvance()
		{
			if (!superMelty)
			{
				return;
			}
			if (stand.Unk5A != Constants.Khaos.StandEntity.Unk5A)
			{
				stand = sotnApi.EntityApi.GetLiveEntity(sotnApi.EntityApi.SpawnEntity(Constants.Khaos.StandEntity));
			}
			stand.Facing = alucardEntity.Facing;
			stand.Xpos = sotnApi.AlucardApi.ScreenX - 10 + alucardEntity.AccelerationX;
			if (stand.Facing > 0)
			{
				stand.Xpos = sotnApi.AlucardApi.ScreenX + 10 + alucardEntity.AccelerationX;
			}
			stand.Ypos = sotnApi.AlucardApi.ScreenY - 10 + alucardEntity.AccelerationY;
			stand.AnimationCurrentFrame = alucardEntity.AnimationCurrentFrame;
		}
		private void MeltyBloodOff()
		{
			cheatsController.HitboxWidth.Disable();
			cheatsController.HitboxHeight.Disable();
			cheatsController.Hitbox2Width.Disable();
			cheatsController.Hitbox2Height.Disable();
			superMelty = false;
			stand.Xpos = 0;
			stand.Ypos = 0;
			stand.AccelerationX = 0;
			stand.AccelerationY = 0;
			stand.Palette = 0;
			stand.ZPriority = 0;
			stand.Flags = 0;
			stand.AnimationSet = 0;
			stand.Unk5A = 0;
			stand.AnimationCurrentFrame = 0;
			SetHasteStaticSpeeds(false);
		}
		private void FourBeastsOff()
		{
			cheatsController.InvincibilityCheat.Disable();
			InvincibilityLocked = false;
			cheatsController.AttackPotion.Disable();
			cheatsController.ShineCheat.Disable();
			cheatsController.ContactDamage.Disable();
			sotnApi.AlucardApi.ContactDamage = 0;
		}
		private void ZawarudoOff()
		{
			sotnApi.GameApi.SetStopwatchTimer();
		}
		private void HasteOff()
		{
			SetSpeed();
			superHaste = false;
			hasteActive = false;
			SpeedLocked = false;
		}
		private void SetHasteStaticSpeeds(bool super = false)
		{
			float superFactor = super ? 2F : 1F;
			float superWingsmashFactor = super ? 1.5F : 1F;
			float factor = toolConfig.Khaos.HasteFactor;

			uint wolfDashTopLeft = DefaultSpeeds.WolfDashTopLeft;

			sotnApi.AlucardApi.WingsmashHorizontalSpeed = (uint) (DefaultSpeeds.WingsmashHorizontal * ((factor * superWingsmashFactor) / 2.5));
			sotnApi.AlucardApi.WolfDashTopRightSpeed = (sbyte) Math.Floor(DefaultSpeeds.WolfDashTopRight * ((factor * superFactor) / 2));
			sotnApi.AlucardApi.WolfDashTopLeftSpeed = (sbyte) Math.Ceiling((sbyte) wolfDashTopLeft * ((factor * superFactor) / 2));
			sotnApi.AlucardApi.BackdashWholeSpeed = (int) (DefaultSpeeds.BackdashWhole - 1);
		}
		private void ToggleHasteDynamicSpeeds(float factor = 1)
		{
			uint horizontalWhole = (uint) (DefaultSpeeds.WalkingWhole * factor);
			uint horizontalFract = (uint) (DefaultSpeeds.WalkingFract * factor);

			sotnApi.AlucardApi.WalkingWholeSpeed = horizontalWhole;
			sotnApi.AlucardApi.WalkingFractSpeed = horizontalFract;
			sotnApi.AlucardApi.JumpingHorizontalWholeSpeed = horizontalWhole;
			sotnApi.AlucardApi.JumpingHorizontalFractSpeed = horizontalFract;
			sotnApi.AlucardApi.JumpingAttackLeftHorizontalWholeSpeed = (uint) (0xFF - horizontalWhole);
			sotnApi.AlucardApi.JumpingAttackLeftHorizontalFractSpeed = horizontalFract;
			sotnApi.AlucardApi.JumpingAttackRightHorizontalWholeSpeed = horizontalWhole;
			sotnApi.AlucardApi.JumpingAttackRightHorizontalFractSpeed = horizontalFract;
			sotnApi.AlucardApi.FallingHorizontalWholeSpeed = horizontalWhole;
			sotnApi.AlucardApi.FallingHorizontalFractSpeed = horizontalFract;
		}
		private int RollBeast()
		{
			int result = rng.Next(1, 5);

			switch (result)
			{
				case 1:
					if (azureDragonUsed)
					{
						return 0;
					}
					break;
				case 2:
					if (whiteTigerUsed)
					{
						return 0;
					}
					break;
				case 3:
					if (vermilionBirdUsed)
					{
						return 0;
					}
					break;
				case 4:
					if (blackTortoiseUsed)
					{
						return 0;
					}
					break;
				default:
					return 0;
			}

			return result;
		}
		private void AzureDragon(string user)
		{
			azureDragonUsed = true;
			notificationService.AzureDragons += 1;

			notificationService.AddMessage(user + " gave you 1 Azure Dragon Spirit");
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.FourBeasts]);
		}
		private void VermilionBird(string user)
		{
			vermilionBirdUsed = true;
			notificationService.VermillionBirds += 5;

			notificationService.AddMessage(user + " gave you 5 Vermilion Bird Fireballs");
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.FourBeasts]);
		}
		private void WhiteTiger(string user)
		{
			whiteTigerUsed = true;
			notificationService.WhiteTigers += 2;

			notificationService.AddMessage(user + " gave you 2 White Tiger Hellfires");
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.FourBeasts]);
		}
		private void BlackTortoise(string user)
		{
			blackTortoiseUsed = true;
			notificationService.BlackTortoises += 2;

			notificationService.AddMessage(user + " gave you 2 Black Tortoise Dark Metamorphasis");
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.FourBeasts]);
		}
		private void FourHolyBeasts(string user)
		{
			cheatsController.InvincibilityCheat.PokeValue(1);
			cheatsController.InvincibilityCheat.Enable();
			InvincibilityLocked = true;
			cheatsController.AttackPotion.PokeValue(1);
			cheatsController.AttackPotion.Enable();
			cheatsController.ShineCheat.PokeValue(1);
			cheatsController.ShineCheat.Enable();
			scheduledActions[(int) Enums.Event.FourBeastsOff].Start();
			cheatsController.ContactDamage.PokeValue(4);
			cheatsController.ContactDamage.Enable();
			azureDragonUsed = false;
			whiteTigerUsed = false;
			vermilionBirdUsed = false;
			blackTortoiseUsed = false;

			notificationService.AzureDragons += 2;
			notificationService.VermillionBirds += 10;
			notificationService.WhiteTigers += 3;
			notificationService.BlackTortoises += 3;

			notificationService.AddMessage(user + " used Four Beasts");
			notificationService.AddOverlayTimer((int) Enums.Action.FourBeasts, (int) toolConfig.Khaos.Actions[(int) Enums.Action.FourBeasts].Duration.TotalMilliseconds);
			statusInfoDisplay.AddTimer((int) Enums.Action.FourBeasts);
			Alert(toolConfig.Khaos.Actions[(int) Enums.Action.FourBeasts]);
		}
		private void CheckVermillionBirdFireballs()
		{
			if (notificationService.VermillionBirds > 0 && fireballCooldown == 0 && sotnApi.GameApi.QcfInputCounter == 0x00ff && (sotnApi.GameApi.InputFlags & SotnApi.Constants.Values.Game.Controller.Square) == SotnApi.Constants.Values.Game.Controller.Square)
			{
				notificationService.VermillionBirds--;
				bool alucardFacing = sotnApi.AlucardApi.FacingLeft;
				double offsetX = alucardFacing ? -20.0d : 20.0d;
				Constants.Khaos.FireballEntity.Facing = alucardFacing ? (ushort) 1 : (ushort) 0;
				Constants.Khaos.FireballEntity.Xpos = sotnApi.AlucardApi.ScreenX + offsetX;
				Constants.Khaos.FireballEntity.Ypos = sotnApi.AlucardApi.ScreenY - 20.0d;

				long address = sotnApi.EntityApi.SpawnEntity(Constants.Khaos.FireballEntity, false);
				Entity liveFireball = sotnApi.EntityApi.GetLiveEntity(address);
				fireballs.Add(liveFireball);

				Constants.Khaos.FireballEntity.Ypos += 10;
				address = sotnApi.EntityApi.SpawnEntity(Constants.Khaos.FireballEntity, false);
				Entity liveFireball2 = sotnApi.EntityApi.GetLiveEntity(address);
				fireballs.Add(liveFireball2);

				Constants.Khaos.FireballEntity.Ypos += 10;
				address = sotnApi.EntityApi.SpawnEntity(Constants.Khaos.FireballEntity, false);
				Entity liveFireball3 = sotnApi.EntityApi.GetLiveEntity(address);
				fireballs.Add(liveFireball3);

				fireballCooldown = 20;
			}

			if (fireballs.Count == 0)
			{
				fireballCooldown = 0;
				return;
			}

			fireballs.RemoveAll(f => f.Damage == 0 || f.Damage == 80);

			for (int i = 0; i < fireballs.Count; i++)
			{
				fireballs[i].Damage = 40;
				if (fireballs[i].Facing > 0)
				{
					fireballs[i].AccelerationX = 3.0d;
				}
				else
				{
					fireballs[i].AccelerationX = -3.0d;
				}
				if ((sotnApi.GameApi.InputFlags & SotnApi.Constants.Values.Game.Controller.Down) == SotnApi.Constants.Values.Game.Controller.Down)
				{
					fireballs[i].AccelerationY = 3.0d;
				}
				else if ((sotnApi.GameApi.InputFlags & SotnApi.Constants.Values.Game.Controller.Up) == SotnApi.Constants.Values.Game.Controller.Up)
				{
					fireballs[i].AccelerationY = -3.0d;
				}
			}

			if (fireballCooldown > 0)
			{
				fireballCooldown--;
			}
		}
		private void CheckVampireKill()
		{
			if (!quadActive)
			{
				return;
			}

			uint updatedCurrentKills = sotnApi.AlucardApi.Kills;

			if (updatedCurrentKills < currentKills)
			{
				currentKills = updatedCurrentKills;
			}

			if (updatedCurrentKills > currentKills)
			{
				currentKills = updatedCurrentKills;
				currentQuadKills++;
			}

			if (currentQuadKills >= goalQuadKills)
			{
				currentQuadKills = 0;
				sotnApi.AlucardApi.Str += 2;
				quadLevel++;
				goalQuadKills = 10 + (quadLevel * quadLevel);
				notificationService.PlayAlert(Paths.ExcellentSound);
				return;
			}
		}
		private void AzureDragonOff()
		{
			var lockOnCheat = cheatsController.Cheats.GetCheatByName(Constants.Khaos.SpiritLockOnName);
			lockOnCheat.Disable();
			cheatsController.Cheats.RemoveCheat(lockOnCheat);
			azureSpiritActive = false;
		}
		private void WhiteTigerOff()
		{
			whiteTigerBallActive = false;
		}
		private void ActivateGuardianSpirits()
		{
			cheatsController.Activator.PokeValue((int) SotnApi.Constants.Values.Alucard.Effects.AutoSummonSpirit);
			cheatsController.Activator.Enable();
			scheduledActions[(int) Enums.Event.GuardianSpiritsOff].Start();
		}
		private void GuardianSpiritsOff()
		{
			cheatsController.Activator.Disable();
		}
		private void CheckAzureDragon()
		{
			if (notificationService.AzureDragons > 0 && !azureSpiritActive)
			{
				var spiritAddress = sotnApi.EntityApi.FindEntityFrom(Constants.Khaos.SpiritProperties, false);
				if (spiritAddress > 0)
				{
					Entity liveSpirit = sotnApi.EntityApi.GetLiveEntity(spiritAddress);
					if (liveSpirit.Step == Entities.LockedOn)
					{
						notificationService.AzureDragons--;
						liveSpirit.Palette = Constants.Khaos.SpiritPalette;
						liveSpirit.InvincibilityFrames = 4;
						azureSpiritActive = true;
						cheatsController.Cheats.AddCheat(spiritAddress + Entities.Step, Entities.LockedOn, Constants.Khaos.SpiritLockOnName, WatchSize.Byte);
						scheduledActions[(int) Enums.Event.AzureDragonOff].Start();
					}
				}
			}
		}
		private void CheckBlackTortoise()
		{
			if (notificationService.BlackTortoises > 0 && !darkMetamorphosisCasted && sotnApi.AlucardApi.State == SotnApi.Constants.Values.Alucard.States.DarkMetamorphosis)
			{
				notificationService.BlackTortoises--;
				sotnApi.AlucardApi.ActivatePotion(Potion.HighPotion);
				darkMetamorphosisCasted = true;
			}

			if (notificationService.BlackTortoises > 0 && darkMetamorphosisCasted && sotnApi.AlucardApi.State != SotnApi.Constants.Values.Alucard.States.DarkMetamorphosis)
			{
				darkMetamorphosisCasted = false;
			}
		}
		private void CheckWhiteTiger()
		{
			if (notificationService.WhiteTigers > 0 && !whiteTigerBallActive && !hellfireCasted && sotnApi.AlucardApi.State == SotnApi.Constants.Values.Alucard.States.Hellfire)
			{
				notificationService.WhiteTigers--;
				bool alucardFacing = alucardEntity.Facing > 0;
				double offsetX = alucardFacing ? -20d : 20d;
				Constants.Khaos.DarkFireballEntity.Xpos = alucardEntity.Xpos + offsetX;
				Constants.Khaos.DarkFireballEntity.Ypos = alucardEntity.Ypos - 10;
				Constants.Khaos.DarkFireballEntity.AccelerationX = alucardFacing ? (double) 0xFFFF : (double) 0;

				long address = sotnApi.EntityApi.SpawnEntity(Constants.Khaos.DarkFireballEntity, false);
				whiteTigerFireball = sotnApi.EntityApi.GetLiveEntity(address);
				hellfireCasted = true;
				whiteTigerBallActive = true;
				scheduledActions[(int) Enums.Event.WhiteTigerOff].Start();
			}

			if (whiteTigerBallActive)
			{
				if (whiteTigerFireball.EnemyId != 0x0003)
				{
					whiteTigerBallActive = false;
					return;
				}
					else
				{
					bool alucardFacing = alucardEntity.Facing > 0;
					whiteTigerFireball.AccelerationX = alucardFacing ? -0.2 : 0.2;
				}
			}

			if (notificationService.WhiteTigers > 0 && hellfireCasted && sotnApi.AlucardApi.State != SotnApi.Constants.Values.Alucard.States.Hellfire)
			{
				hellfireCasted = false;
			}
		}
		private void CheckHeartsPickedUp()
		{
			uint currentHearts = sotnApi.AlucardApi.CurrentHearts;

			uint gatheredhearts = 0;
			if (currentHearts > storedHearts)
			{
				gatheredhearts = currentHearts - storedHearts;
			}
			if (gatheredhearts == 0)
			{
				return;
			}

			storedHearts = currentHearts;
			uint mp = currentMp;
			mp += gatheredhearts * 5;
			if (mp > sotnApi.AlucardApi.MaxtMp)
			{
				sotnApi.AlucardApi.CurrentMp = sotnApi.AlucardApi.MaxtMp;
			}
			else
			{
				sotnApi.AlucardApi.CurrentMp = mp;
			}
		}
		#endregion

		private void RandomizeLibraryCardDestination()
		{
			int index = rng.Next(0, SotnApi.Constants.Values.Game.Various.SafeLibraryCardZones.Count);
			TeleportDestination zone = SotnApi.Constants.Values.Game.Various.SafeLibraryCardZones[index];
			sotnApi.GameApi.SetLibraryCardDestination(zone.Zone, zone.Xpos, zone.Ypos, zone.Room);
		}
		private void SetGlow()
		{
			if (thirstGlow)
			{
				cheatsController.VisualEffectPalette.PokeValue(Constants.Khaos.BloodthirstColorPalette);
				cheatsController.VisualEffectPalette.Enable();
				cheatsController.VisualEffectTimer.PokeValue(30);
				cheatsController.VisualEffectTimer.Enable();
			}
			else if (overdriveGlow)
			{
				cheatsController.VisualEffectPalette.PokeValue(Constants.Khaos.OverdriveColorPalette);
				cheatsController.VisualEffectPalette.Enable();
				cheatsController.VisualEffectTimer.PokeValue(30);
				cheatsController.VisualEffectTimer.Enable();
			}
			else if (quadActive)
			{
				cheatsController.VisualEffectPalette.PokeValue(Constants.Khaos.QuadColorPalette);
				cheatsController.VisualEffectPalette.Enable();
				cheatsController.VisualEffectTimer.PokeValue(30);
				cheatsController.VisualEffectTimer.Enable();
			}
			else
			{
				cheatsController.VisualEffectPalette.Disable();
				cheatsController.VisualEffectTimer.Disable();
			}
		}
		private void SetSaveColorPalette()
		{
			int offset = rng.Next(0, 15);
			if (alucardSecondCastle)
			{
				cheatsController.SavePalette.PokeValue(Constants.Khaos.SaveIcosahedronSecondCastle + offset);
			}
			else
			{
				cheatsController.SavePalette.PokeValue(Constants.Khaos.SaveIcosahedronFirstCastle + offset);
			}
		}
		private void SetRelicLocationDisplay(Relic relic, bool take)
		{
			switch (relic)
			{
				case Relic.SoulOfBat:
					if (take)
					{
						if (statusInfoDisplay.BatLocation == Constants.Khaos.KhaosName)
						{
							statusInfoDisplay.BatLocation = String.Empty;
						}
					}
					else
					{
						if (statusInfoDisplay.BatLocation == String.Empty)
						{
							statusInfoDisplay.BatLocation = Constants.Khaos.KhaosName;
						}
					}
					break;
				case Relic.SoulOfWolf:
					if (take)
					{
						if (statusInfoDisplay.WolfLocation == Constants.Khaos.KhaosName)
						{
							statusInfoDisplay.WolfLocation = String.Empty;
						}
					}
					else
					{
						if (statusInfoDisplay.WolfLocation == String.Empty)
						{
							statusInfoDisplay.WolfLocation = Constants.Khaos.KhaosName;
						}
					}
					break;
				case Relic.FormOfMist:
					if (take)
					{
						if (statusInfoDisplay.MistLocation == Constants.Khaos.KhaosName)
						{
							statusInfoDisplay.MistLocation = String.Empty;
						}
					}
					else
					{
						if (statusInfoDisplay.MistLocation == String.Empty)
						{
							statusInfoDisplay.MistLocation = Constants.Khaos.KhaosName;
						}
					}
					break;
				case Relic.PowerOfMist:
					if (take)
					{
						if (statusInfoDisplay.PowerOfMistLocation == Constants.Khaos.KhaosName)
						{
							statusInfoDisplay.PowerOfMistLocation = String.Empty;
						}
					}
					else
					{
						if (statusInfoDisplay.PowerOfMistLocation == String.Empty)
						{
							statusInfoDisplay.PowerOfMistLocation = Constants.Khaos.KhaosName;
						}
					}
					break;
				case Relic.GravityBoots:
					if (take)
					{
						if (statusInfoDisplay.GravityBootsLocation == Constants.Khaos.KhaosName)
						{
							statusInfoDisplay.GravityBootsLocation = String.Empty;
						}
					}
					else
					{
						if (statusInfoDisplay.GravityBootsLocation == String.Empty)
						{
							statusInfoDisplay.GravityBootsLocation = Constants.Khaos.KhaosName;
						}
					}
					break;
				case Relic.LeapStone:
					if (take)
					{
						if (statusInfoDisplay.LepastoneLocation == Constants.Khaos.KhaosName)
						{
							statusInfoDisplay.LepastoneLocation = String.Empty;
						}
					}
					else
					{
						if (statusInfoDisplay.LepastoneLocation == String.Empty)
						{
							statusInfoDisplay.LepastoneLocation = Constants.Khaos.KhaosName;
						}
					}
					break;
				case Relic.JewelOfOpen:
					if (take)
					{
						if (statusInfoDisplay.JewelOfOpenLocation == Constants.Khaos.KhaosName)
						{
							statusInfoDisplay.JewelOfOpenLocation = String.Empty;
						}
					}
					else
					{
						if (statusInfoDisplay.JewelOfOpenLocation == String.Empty)
						{
							statusInfoDisplay.JewelOfOpenLocation = Constants.Khaos.KhaosName;
						}
					}
					break;
				case Relic.MermanStatue:
					if (take)
					{
						if (statusInfoDisplay.MermanLocation == Constants.Khaos.KhaosName)
						{
							statusInfoDisplay.MermanLocation = String.Empty;
						}
					}
					else
					{
						if (statusInfoDisplay.MermanLocation == String.Empty)
						{
							statusInfoDisplay.MermanLocation = Constants.Khaos.KhaosName;
						}
					}
					break;
				default:
					break;
			}
		}
		private void SetSpeed(float factor = 1)
		{
			bool slow = factor < 1;
			bool fast = factor > 1;

			uint horizontalWhole = (uint) (DefaultSpeeds.WalkingWhole * factor);
			uint horizontalFract = (uint) (DefaultSpeeds.WalkingFract * factor);
			uint wolfDashTopLeft = DefaultSpeeds.WolfDashTopLeft;

			sotnApi.AlucardApi.WingsmashHorizontalSpeed = (uint) (DefaultSpeeds.WingsmashHorizontal * factor);
			sotnApi.AlucardApi.WalkingWholeSpeed = horizontalWhole;
			sotnApi.AlucardApi.WalkingFractSpeed = horizontalFract;
			sotnApi.AlucardApi.JumpingHorizontalWholeSpeed = horizontalWhole;
			sotnApi.AlucardApi.JumpingHorizontalFractSpeed = horizontalFract;
			sotnApi.AlucardApi.JumpingAttackLeftHorizontalWholeSpeed = (uint) (0xFF - horizontalWhole);
			sotnApi.AlucardApi.JumpingAttackLeftHorizontalFractSpeed = horizontalFract;
			sotnApi.AlucardApi.JumpingAttackRightHorizontalWholeSpeed = horizontalWhole;
			sotnApi.AlucardApi.JumpingAttackRightHorizontalFractSpeed = horizontalFract;
			sotnApi.AlucardApi.FallingHorizontalWholeSpeed = horizontalWhole;
			sotnApi.AlucardApi.FallingHorizontalFractSpeed = horizontalFract;
			sotnApi.AlucardApi.WolfDashTopRightSpeed = (sbyte) Math.Floor(DefaultSpeeds.WolfDashTopRight * factor);
			sotnApi.AlucardApi.WolfDashTopLeftSpeed = (sbyte) Math.Ceiling((sbyte) wolfDashTopLeft * factor);
			sotnApi.AlucardApi.BackdashWholeSpeed = (int) (DefaultSpeeds.BackdashWhole * factor);
			sotnApi.AlucardApi.BackdashDecel = slow == true ? DefaultSpeeds.BackdashDecelSlow : DefaultSpeeds.BackdashDecel;
		}

		private void FixEntranceSlow()
		{
			if (slowActive && !slowPaused && IsInRoomList(Constants.Khaos.EntranceCutsceneRooms))
			{
				slowPaused = true;
				cheatsController.UnderwaterPhysics.Disable();
			}

			if (slowActive && slowPaused && !IsInRoomList(Constants.Khaos.EntranceCutsceneRooms))
			{
				slowPaused = false;
				cheatsController.UnderwaterPhysics.Enable();
			}
		}
		private void FixSlowWarps()
		{
			if (slowActive && !slowPaused && sotnApi.GameApi.CanWarp())
			{
				slowPaused = true;
				cheatsController.UnderwaterPhysics.Disable();
			}

			if (slowActive && slowPaused && !sotnApi.GameApi.CanWarp())
			{
				slowPaused = false;
				cheatsController.UnderwaterPhysics.Enable();
			}
		}
		private void HandleHnkInvincibility()
		{
			if (hnkOn && hnkToggled == 1)
			{
				hnkToggled = 0;
				sotnApi.AlucardApi.InvincibilityTimer = 0;
				sotnApi.AlucardApi.PotionInvincibilityTimer = 0;
				sotnApi.AlucardApi.KnockbackInvincibilityTimer = 0;
				sotnApi.AlucardApi.FreezeInvincibilityTimer = 0;
			}
			else if (hnkOn && hnkToggled < 1)
			{
				hnkToggled++;
			}
		}
		private void CheckBanishViable()
		{
			if (!banishActive)
			{
				return;
			}
			if (sotnApi.AlucardApi.SubweaponTimer == 0 && !sotnApi.AlucardApi.EffectActive())
			{
				RandomizeLibraryCardDestination();
				sotnApi.AlucardApi.ForceLibraryCard();
				sotnApi.AlucardApi.HorizontalVelocityWhole = 0;
				sotnApi.AlucardApi.HorizontalVelocityFractional = 0;
				sotnApi.AlucardApi.HorizontalVelocityFractional = 0;
				sotnApi.AlucardApi.StunTimer = 0xFF;
				banishActive = false;
				notificationService.AddMessage($"{banishUser} used Banish");
				Alert(toolConfig.Khaos.Actions[(int) Enums.Action.Banish]);
			}
		}
		private void CheckManaUsage()
		{
			if (!bloodManaActive)
			{
				return;
			}

			spentMana = 0;
			if (currentMp < storedMana)
			{
				spentMana = (int) storedMana - (int) currentMp;
			}

			storedMana = currentMp;
			BloodManaUpdate();
		}
		private void CheckDracularoomMusic()
		{
			if (IsInRoomList(Constants.Khaos.DraculaRoom))
			{
				if (dracMusicCounter == 0)
				{
					cheatsController.Music.PokeValue(rng.Next(0, 256));
					cheatsController.Music.Enable();
					dracMusicCounter = 30;
				}
				else
				{
					dracMusicCounter--;
				}
			}
		}
		private void CheckDizzyDoors()
		{
			if (!dizzyActive)
			{
				return;
			}
			var doors = sotnApi.EntityApi.GetAllEntities(new List<ushort> { Constants.Khaos.DoorId });

			for (int i = 0; i < doors.Count; i++)
			{
				Entity door = sotnApi.EntityApi.GetLiveEntity(doors[i]);
				if (door.Step > 1)
				{
					DizzyOff();
				}
			}
		}
		private void CheckHasteAttack()
		{
			if (!hasteActive || !superHaste)
			{
				return;
			}
			if (alucardEntity.Step2 >= 0x0040 && alucardEntity.Step != 0x0004 && alucardEntity.AnimationFrameDuration > 2)
			{
				alucardEntity.AnimationFrameDuration = 1;
			}
		}
		private void CheckDashInput()
		{
			if (!hasHitbox || currentHp < 1)
			{
				return;
			}
			if (hasteActive && !hasteSpeedOn && sotnApi.GameApi.QcfInputCounter == 0xff)
			{
				ToggleHasteDynamicSpeeds(superHaste ? toolConfig.Khaos.HasteFactor * Constants.Khaos.HasteDashFactor : toolConfig.Khaos.HasteFactor);
				hasteSpeedOn = true;
			}
			else if (hasteSpeedOn &&
				((sotnApi.AlucardApi.FacingLeft && (sotnApi.GameApi.InputFlags & SotnApi.Constants.Values.Game.Controller.Left) != SotnApi.Constants.Values.Game.Controller.Left) ||
				(!sotnApi.AlucardApi.FacingLeft && (sotnApi.GameApi.InputFlags & SotnApi.Constants.Values.Game.Controller.Right) != SotnApi.Constants.Values.Game.Controller.Right))
				)
			{
				ToggleHasteDynamicSpeeds();
				hasteSpeedOn = false;
			}
		}
		private void SpendKhaosMeter()
		{
			khaosMeter -= 100;
			notificationService.UpdateOverlayMeter(khaosMeter);
		}
		private void Alert(Configuration.Models.Action action)
		{
			if (!toolConfig.Khaos.Alerts)
			{
				return;
			}

			if (action is not null && action.AlertPath is not null && action.AlertPath != String.Empty)
			{
				notificationService.PlayAlert(action.AlertPath);
			}
		}
	}
}
