using System.Collections.Generic;
using SotnKhaosTools.RandoTracker.Models;

namespace SotnKhaosTools.Khaos.Interfaces
{
	internal interface IKhaosController
	{
		bool AutoKhaosOn { get; set; }
		bool SpeedLocked { get; set; }
		bool ManaLocked { get; set; }
		bool InvincibilityLocked { get; set; }
		bool SpawnActive { get; set; }
		bool PandoraUsed { get; set; }
		public bool ActionViable();
		public bool FastActionViable();
		public void GainKhaosMeter(short meter);
		void Bankrupt(string user = "Khaos");
		void BattleOrders(string user = "Khaos");
		void BloodMana(string user = "Khaos");
		void Endurance(string user = "Khaos");
		void FourBeasts(string user = "Khaos");
		void Gamble(string user = "Khaos");
		void Banish(string user = "Khaos");
		void Haste(string user = "Khaos");
		void HeavytHelp(string user = "Khaos");
		void HnK(string user = "Khaos");
		void BulletHell(string user = "Khaos");
		void Horde(string user = "Khaos");
		void KhaosEquipment(string user = "Khaos");
		void KhaosRelics(string user = "Khaos");
		void KhaosStats(string user = "Khaos");
		void KhaosStatus(string user = "Khaos");
		void KhaosTrack(string track, string user = "Khaos");
		void LightHelp(string user = "Khaos");
		void Magician(string user = "Khaos");
		void MediumHelp(string user = "Khaos");
		void MeltyBlood(string user = "Khaos");
		void PandorasBox(string user = "Khaos");
		void RespawnBosses(string user = "Khaos");
		void Slow(string user = "Khaos");
		void StartKhaos();
		void StopKhaos();
		void Thirst(string user = "Khaos");
		void Update();
		void Quad(string user = "Khaos");
		void Weaken(string user = "Khaos");
		void ZaWarudo(string user = "Khaos");
	}
}
