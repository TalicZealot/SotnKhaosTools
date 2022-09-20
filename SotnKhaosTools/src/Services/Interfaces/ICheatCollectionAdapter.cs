﻿using BizHawk.Client.Common;

namespace SotnKhaosTools.Services.Adapters
{
	public interface ICheatCollectionAdapter
	{
		Cheat this[int index] { get; }

		void DisableAll();
		Cheat GetCheatByName(string name);
		bool Load(string path, bool append);
		Cheat AddCheat(long address, int value, string name, WatchSize size);
		void RemoveCheat(Cheat cheat);
	}
}