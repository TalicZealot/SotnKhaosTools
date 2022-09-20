﻿using SotnKhaosTools.Coop.Enums;

namespace SotnKhaosTools.Coop.Interfaces
{
	internal interface ICoopMessanger
	{
		void Connect(string hostIp, int port);
		void Disconnect();
		void StartServer(int port);
		void StopServer();
		void DisposeAll();
		void SendData(MessageType type, byte[] data);
		bool IsConnected();
	}
}