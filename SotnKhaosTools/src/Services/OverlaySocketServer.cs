using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;
using Newtonsoft.Json.Linq;
using SotnKhaosTools.Constants;
using SotnKhaosTools.Khaos.Models;
using SotnKhaosTools.Services.Interfaces;

namespace SotnKhaosTools.Services
{
	internal sealed class WSClient
	{
		public int SocketId { get; set; }
		public WebSocket Socket { get; set; }
	}
	internal sealed class OverlaySocketServer : IOverlaySocketServer
	{
		private HttpListener server;
		private bool started = false;
		private List<WSClient> clients = new List<WSClient>();
		private CancellationTokenSource socketLoopTokenSource;
		private CancellationTokenSource listenerLoopTokenSource;
		private int socketCounter = 0;
		private byte[] meterData = new byte[5];
		private byte[] timerData = new byte[9];
		private byte[] actionData = new byte[33];

		public OverlaySocketServer()
		{
		}

		public void StartServer()
		{
			if (started)
			{
				return;
			}
			socketLoopTokenSource = new CancellationTokenSource();
			listenerLoopTokenSource = new CancellationTokenSource();
			server = new HttpListener();
			server.Prefixes.Add(Globals.WebSocketUri);
			started = true;
			server.Start();
			Task.Run(() => AcceptClients().ConfigureAwait(false));
		}

		public async void StopServer()
		{
			if (!started)
			{
				return;
			}
			started = false;
			await CloseAllSocketsAsync();
			server.Stop();
			server.Close();
		}

		private async Task CloseAllSocketsAsync()
		{
			var disposeQueue = new List<WebSocket>(clients.Count);

			while (clients.Count > 0)
			{
				var client = clients[clients.Count - 1];

				if (client.Socket.State == WebSocketState.Open)
				{
					var timeout = new CancellationTokenSource(400);
					try
					{
						await client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token);
					}
					catch (OperationCanceledException ex)
					{
						Console.Write(ex.ToString());
					}
				}

				lock (clients)
				{
					clients.Remove(client);
				}
				disposeQueue.Add(client.Socket);
			}

			socketLoopTokenSource.Cancel();

			for (int i = 0; i < disposeQueue.Count; i++)
			{
				disposeQueue[i].Dispose();
			}
		}

		private async Task AcceptClients()
		{
			CancellationToken token = listenerLoopTokenSource.Token;
			while (started && !token.IsCancellationRequested)
			{
				HttpListenerContext ctx;
				try
				{
					ctx = await server.GetContextAsync().ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
					continue;
				}

				var endpoint = ctx.Request.RemoteEndPoint.Address.ToString();
				if ((endpoint != "127.0.0.1" && endpoint != "::1") || !ctx.Request.IsWebSocketRequest)
				{
					ctx.Response.StatusCode = 400;
					ctx.Response.Close();
					continue;
				}

				try
				{
					WebSocketContext wsContext = await ctx.AcceptWebSocketAsync(subProtocol: null);
					int socketId = Interlocked.Increment(ref socketCounter);
					WSClient client = new WSClient { SocketId = socketId, Socket = wsContext.WebSocket };
					lock (clients)
					{
						clients.Add(client);
					}
					_ = Task.Run(() => SocketProcessingLoopAsync(client).ConfigureAwait(false));

					// Start data receiver for the client in a new task
					//_ = Task.Run(() => DataReceiver(client, cancelToken), cancelToken);
					//await Task.Run(() => DataReceiver(client), token);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
					ctx.Response.StatusCode = 500;
					ctx.Response.Close();
				}
			}
		}

		private async Task SocketProcessingLoopAsync(WSClient client)
		{
			var socket = client.Socket;
			var loopToken = socketLoopTokenSource.Token;
			try
			{
				var buffer = WebSocket.CreateServerBuffer(4096);
				while (socket.State != WebSocketState.Closed && socket.State != WebSocketState.Aborted && !loopToken.IsCancellationRequested)
				{
					var receiveResult = await client.Socket.ReceiveAsync(buffer, loopToken);
					if (loopToken.IsCancellationRequested)
					{
						break;
					}
					if (client.Socket.State == WebSocketState.CloseReceived && receiveResult.MessageType == WebSocketMessageType.Close)
					{
						await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledge Close frame", CancellationToken.None);
					}

					if (client.Socket.State == WebSocketState.Open)
					{
						await socket.SendAsync(new ArraySegment<byte>(buffer.Array, 0, receiveResult.Count), receiveResult.MessageType, receiveResult.EndOfMessage, CancellationToken.None);
					}
				}
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Socket {client.SocketId}: {ex.ToString()}");
			}
			finally
			{
				if (client.Socket.State != WebSocketState.Closed)
				{
					client.Socket.Abort();
				}

				lock (clients)
				{
					clients.Remove(client);
				}
				socket.Dispose();
			}
		}

		private Task<bool> SendData(WSClient client, byte[] data)
		{
			Task<bool> task = MessageWriteAsync(client, new ArraySegment<byte>(data));
			return task;
		}

		private async Task<bool> MessageWriteAsync(WSClient client, ArraySegment<byte> data)
		{
			CancellationToken token = socketLoopTokenSource.Token;
			try
			{
				await client.Socket.SendAsync(data, WebSocketMessageType.Binary, true, token).ConfigureAwait(false);
				return true;
			}
			catch (Exception e)
			{
				lock (clients)
				{
					clients.Remove(client);
				}
			}
			finally
			{
				client = null;
			}

			return false;
		}

		public void UpdateMeter(int meter)
		{
			meterData[0] = 0;
			byte[] meterBytes = BitConverter.GetBytes(meter);
			int index = 1;
			for (int i = 0; i < 3; i++)
			{
				meterData[index] = meterBytes[i];
				index++;
			}

			for (int i = 0; i < clients.Count; i++)
			{
				SendData(clients[i], meterData);
			}
		}
		public void AddTimer(int index, int duration)
		{
			timerData[0] = 1;
			byte[] indexBytes = BitConverter.GetBytes(index);
			int dataIndex = 1;
			for (int i = 0; i < 3; i++)
			{
				timerData[dataIndex] = indexBytes[i];
				dataIndex++;
			}
			dataIndex++;
			byte[] durationBytes = BitConverter.GetBytes(duration);
			for (int i = 0; i < 3; i++)
			{
				timerData[dataIndex] = durationBytes[i];
				dataIndex++;
			}

			for (int i = 0; i < clients.Count; i++)
			{
				SendData(clients[i], timerData);
			}
		}
		public void UpdateQueue(List<QueuedAction> actionQueue)
		{
			actionData[0] = 2;
			int index = 1;
			for (int i = 0; i < actionQueue.Count; i++)
			{
				actionData[index] = (byte)actionQueue[i].Index;
				index++;
				if (i > 31)
				{
					break;
				}
			}
			actionData[index] = 0xFF;

			for (int i = 0; i < clients.Count; i++)
			{
				SendData(clients[i], actionData);
			}
		}
	}
}
