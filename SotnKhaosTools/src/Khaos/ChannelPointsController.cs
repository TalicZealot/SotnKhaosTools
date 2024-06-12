using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SotnKhaosTools.Configuration;
using SotnKhaosTools.Configuration.Interfaces;
using SotnKhaosTools.Khaos.Interfaces;
using SotnKhaosTools.Khaos.Models;
using SotnKhaosTools.Services;
using SotnKhaosTools.Services.TwitchImplicitOAuth;
using SotnKhaosTools.Utils;
using TwitchLib.Api;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Helix.Models.ChannelPoints.CreateCustomReward;
using TwitchLib.Api.Helix.Models.ChannelPoints.UpdateCustomReward;
using TwitchLib.Api.Helix.Models.ChannelPoints.UpdateCustomRewardRedemptionStatus;
using TwitchLib.Api.Helix.Models.Subscriptions;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

namespace SotnKhaosTools.Khaos
{
	internal sealed class ChannelPointsController
	{
		private const int RetryBaseMs = 12;
		private const int RetryCount = 3;
		private const int UpdateRetryCount = 4;
		private const int MaxSubscribers = 100;
		private const int FulfilRewardDelay = 60;
		private const int FulfilTimerInterval = 3 * 30 * 1000;

		private readonly IToolConfig toolConfig;
		private readonly IActionDispatcher actionDispatcher;
		private readonly INotificationService notificationService;
		private readonly IEnemyRenamer enemyRenamer;

		private TwitchAPI api = new TwitchAPI();
		private TwitchPubSub client = new TwitchPubSub();
		private string broadcasterId;
		private List<string> customRewardIds = new();
		private List<Timer> actionsStartingOnCooldown = new();
		private BindingList<Redemption> redemptions = new();
		private System.Windows.Forms.Timer redemptionFulfilTimer = new();
		private string? currentState = null;
		private readonly Random rng = new();

		private bool manualDisconnect = false;

		public ChannelPointsController(IToolConfig toolConfig, IActionDispatcher actionDispatcher, INotificationService notificationService, IEnemyRenamer enemyRenamer)
		{
			if (toolConfig is null) throw new ArgumentNullException(nameof(toolConfig));
			if (actionDispatcher is null) throw new ArgumentNullException(nameof(actionDispatcher));
			if (notificationService is null) throw new ArgumentNullException(nameof(notificationService));
			if (enemyRenamer is null) throw new ArgumentNullException(nameof(enemyRenamer));
			this.toolConfig = toolConfig;
			this.actionDispatcher = actionDispatcher;
			this.notificationService = notificationService;
			this.enemyRenamer = enemyRenamer;
			redemptionFulfilTimer.Tick += FulfilOldRedemptions;
			redemptionFulfilTimer.Interval = FulfilTimerInterval;
			this.enemyRenamer = enemyRenamer;
		}

		public BindingList<Redemption> Redemptions
		{
			get
			{
				return this.redemptions;
			}
		}

		public void Connect()
		{
			api.Settings.ClientId = ApplicationDetails.twitchClientId;
			ImplicitOAuth ioa = new ImplicitOAuth();
			ioa.OnRevcievedValues += RequestConnection;
			currentState = ioa.RequestClientAuthorization();
		}

		private async void RequestConnection(string state, string token)
		{
			if (state != currentState)
			{
				return;
			}

			api.Settings.AccessToken = token;

			var user = (await api.Helix.Users.GetUsersAsync()).Users[0];
			broadcasterId = user.Id;

			await GetSubscribers();
			await CreateRewards();

			client.OnPubSubServiceConnected += onPubSubServiceConnected;
			client.OnListenResponse += onListenResponse;
			client.OnChannelSubscription += Client_OnSubscription;
			client.OnBitsReceivedV2 += Client_OnBitsReceivedV2;
			client.OnChannelPointsRewardRedeemed += Client_OnChannelPointsRewardRedeemed;
			client.OnPubSubServiceClosed += Client_OnPubSubServiceClosed;
			client.ListenToChannelPoints(user.Id);
			client.Connect();
			notificationService.AddMessage("Connected to Twitch");
			redemptionFulfilTimer.Start();
		}

		public async Task Disconnect()
		{
			redemptionFulfilTimer.Stop();
			manualDisconnect = true;
			await DeleteRewards();
			client.Disconnect();
			for (int i = 0; i < actionsStartingOnCooldown.Count; i++)
			{
				actionsStartingOnCooldown[i].Dispose();
			}
			redemptions.Clear();
			notificationService.AddMessage("Disconnected");
		}

		private async Task GetSubscribers()
		{
			GetBroadcasterSubscriptionsResponse subs = await RetryRequest.Do(
				async () => await api.Helix.Subscriptions.GetBroadcasterSubscriptionsAsync(
					broadcasterId,
					MaxSubscribers,
					null,
					api.Settings.AccessToken
					),
				RetryBaseMs,
				RetryCount
				);
			List<string> totalSubs = subs.Data.Select(u => u.UserName).ToList();

			if (subs.Total > MaxSubscribers)
			{
				GetBroadcasterSubscriptionsResponse subsPageTwoData = await RetryRequest.Do(
				async () => await api.Helix.Subscriptions.GetBroadcasterSubscriptionsAsync(
					broadcasterId,
					MaxSubscribers,
					subs.Pagination.Cursor,
					api.Settings.AccessToken
				),
				RetryBaseMs,
				RetryCount
				);
				totalSubs = totalSubs.Concat(subsPageTwoData.Data.Select(u => u.UserName).ToList()).ToList();
			}

			enemyRenamer.OverwriteNames(totalSubs.ToArray());
		}

		private async Task CreateRewards()
		{
			for (int i = 0; i < toolConfig.Khaos.Actions.Count; i++)
			{
				Configuration.Models.Action? action = toolConfig.Khaos.Actions[i];
				if (action.IsUsable && action.ChannelPoints > 0)
				{

					CreateCustomRewardsRequest request = new CreateCustomRewardsRequest
					{
						Title = action.Name,
						Prompt = action.Description,
						Cost = (int) action.ChannelPoints,
						IsEnabled = true,
					};

					if (action.RequiresUserInput)
					{
						request.IsUserInputRequired = true;
					}


					if (action.Cooldown.TotalSeconds > 0)
					{
						request.IsGlobalCooldownEnabled = true;
						request.GlobalCooldownSeconds = (int) action.Cooldown.TotalSeconds;
					}

					if (action.StartsOnCooldown)
					{
						actionsStartingOnCooldown.Add(
							new Timer(CreateDelayedReward,
							new DelayedActionCallback { Index = toolConfig.Khaos.Actions.IndexOf(action) }, (int) action.Cooldown.TotalMilliseconds, -1)
							);
						continue;
					}

					CreateCustomRewardsResponse response = await RetryRequest.Do(
						async () => await api.Helix.ChannelPoints.CreateCustomRewardsAsync(
							broadcasterId,
							request,
							api.Settings.AccessToken
							),
						RetryBaseMs,
						RetryCount
						);
					customRewardIds.Add(response.Data[0].Id);
				}
			}
			notificationService.AddMessage("Channel Point rewards created");
		}

		private async void CreateDelayedReward(object state)
		{
			DelayedActionCallback action = (DelayedActionCallback) state;

			CreateCustomRewardsRequest request = new CreateCustomRewardsRequest
			{
				Title = toolConfig.Khaos.Actions[action.Index].Name,
				Prompt = toolConfig.Khaos.Actions[action.Index].Description,
				Cost = (int) toolConfig.Khaos.Actions[action.Index].ChannelPoints,
				IsEnabled = true,
				IsGlobalCooldownEnabled = true,
				GlobalCooldownSeconds = (int) toolConfig.Khaos.Actions[action.Index].Cooldown.TotalSeconds
			};

			if (toolConfig.Khaos.Actions[action.Index].RequiresUserInput)
			{
				request.IsUserInputRequired = true;
			}

			CreateCustomRewardsResponse response = await RetryRequest.Do(
						async () => await api.Helix.ChannelPoints.CreateCustomRewardsAsync(
							broadcasterId,
							request,
							api.Settings.AccessToken
						),
						RetryBaseMs,
						RetryCount
						);
			customRewardIds.Add(response.Data[0].Id);
			notificationService.AddMessage($"{request.Title} reward added.");
		}

		private async Task DeleteRewards()
		{
			for (int i = 0; i < customRewardIds.Count; i++)
			{
				await RetryRequest.Do(
					async () => await api.Helix.ChannelPoints.DeleteCustomRewardAsync(
						broadcasterId,
						customRewardIds[i],
						api.Settings.AccessToken
						),
					RetryBaseMs,
					RetryCount
				);
			}
			notificationService.AddMessage("Channel Point rewards removed");
		}

		private async void Client_OnChannelPointsRewardRedeemed(object sender, OnChannelPointsRewardRedeemedArgs e)
		{
			Configuration.Models.Action? action = null;
			for (int i = 0; i < toolConfig.Khaos.Actions.Count; i++)
			{
				if (e.RewardRedeemed.Redemption.Reward.Title == toolConfig.Khaos.Actions[i].Name)
				{
					action = toolConfig.Khaos.Actions[i];
					break;
				}
			}

			if (action is null)
			{
				return;
			}

			redemptions.Add(
				new Redemption
				{
					RedemptionId = e.RewardRedeemed.Redemption.Id,
					RewardId = e.RewardRedeemed.Redemption.Reward.Id,
					Title = e.RewardRedeemed.Redemption.Reward.Title,
					Username = e.RewardRedeemed.Redemption.User.DisplayName,
					RedeemedAt = DateTime.Now
				});

			int NewCost = (int) Math.Round(e.RewardRedeemed.Redemption.Reward.Cost * action.Scaling);
			if (action.MaximumChannelPoints != 0 && NewCost > action.MaximumChannelPoints)
			{
				NewCost = (int) action.MaximumChannelPoints;
			}
			if (action.MaximumChannelPoints != 0 && toolConfig.Khaos.CostDecay && e.RewardRedeemed.Redemption.Reward.Cost == action.MaximumChannelPoints)
			{
				NewCost = (int) Math.Round(action.MaximumChannelPoints * 0.2);
				if (NewCost < action.ChannelPoints)
				{
					NewCost = (int) action.ChannelPoints;
				}
			}

			await RetryRequest.Do(
				async () => await api.Helix.ChannelPoints.UpdateCustomRewardAsync(
					broadcasterId,
					e.RewardRedeemed.Redemption.Reward.Id,
					new UpdateCustomRewardRequest { Cost = NewCost, },
					api.Settings.AccessToken),
				RetryBaseMs,
				RetryCount
			);

			var actionEvent = new EventAddAction { UserName = e.RewardRedeemed.Redemption.User.DisplayName, ActionIndex = toolConfig.Khaos.Actions.IndexOf(action), Data = e.RewardRedeemed.Redemption.UserInput };

			actionDispatcher.EnqueueAction(actionEvent);
		}

		private void Client_OnSubscription(object sender, OnChannelSubscriptionArgs e)
		{
			Configuration.Models.Action? action = null;
			string message = e.Subscription.SubMessage.Message != null ? e.Subscription.SubMessage.Message.ToLower() : String.Empty;
			for (int i = 0; i < toolConfig.Khaos.Actions.Count; i++)
			{
				string actionName = toolConfig.Khaos.Actions[i].Name.ToLower();
				if (message == actionName || message.StartsWith(actionName) || message.EndsWith(actionName))
				{
					action = toolConfig.Khaos.Actions[i];
					break;
				}
			}

			if (action is null)
			{
				int index = rng.Next(1, toolConfig.Khaos.Actions.Count);
				action = toolConfig.Khaos.Actions[index];
			}

			var actionEvent = new EventAddAction { UserName = e.Subscription.Username, ActionIndex = toolConfig.Khaos.Actions.IndexOf(action), Data = String.Empty };

			actionDispatcher.EnqueueAction(actionEvent);
		}

		private void Client_OnBitsReceivedV2(object sender, OnBitsReceivedV2Args e)
		{
			if (e.BitsUsed < toolConfig.Khaos.MinimumBits)
			{
				return;
			}
			string message = e.ChatMessage != null ? e.ChatMessage.ToLower() : String.Empty;
			Configuration.Models.Action? action = null;
			if (e.BitsUsed >= toolConfig.Khaos.BitsChoice)
			{
				for (int i = 0; i < toolConfig.Khaos.Actions.Count; i++)
				{
					string actionName = toolConfig.Khaos.Actions[i].Name.ToLower();
					if (message == actionName || message.StartsWith(actionName) || message.EndsWith(actionName))
					{
						action = toolConfig.Khaos.Actions[i];
						break;
					}
				}
			}

			if (action is null)
			{
				int index = rng.Next(1, toolConfig.Khaos.Actions.Count);
				action = toolConfig.Khaos.Actions[index];
			}

			var actionEvent = new EventAddAction { UserName = e.UserName, ActionIndex = toolConfig.Khaos.Actions.IndexOf(action), Data = String.Empty };

			actionDispatcher.EnqueueAction(actionEvent);
		}

		public async Task FulfillRedemption(Redemption redemption)
		{
			for (int i = 0; i <= UpdateRetryCount; i++)
			{
				try
				{
					await api.Helix.ChannelPoints.UpdateRedemptionStatusAsync(
						broadcasterId,
						redemption.RewardId,
						new List<string> { redemption.RedemptionId },
						new UpdateCustomRewardRedemptionStatusRequest { Status = TwitchLib.Api.Core.Enums.CustomRewardRedemptionStatus.FULFILLED },
						api.Settings.AccessToken
					);
				}
				catch (Exception e)
				{
					if (i == UpdateRetryCount)
					{
						if (e.GetType() == typeof(BadResourceException))
						{
							//Console.WriteLine("Server responded with 404 while updating redemption status on attempt: " + i);
						}
						else
						{
							throw new Exception("Server error while updating redemption status. Click Continue.", e);
						}
					}
					else
					{
						await Task.Delay((int) Math.Pow(RetryBaseMs, i));
					}
				}
			}
		}

		public async Task CancelRedemption(Redemption redemption)
		{
			for (int i = 0; i <= UpdateRetryCount; i++)
			{
				try
				{
					await api.Helix.ChannelPoints.UpdateRedemptionStatusAsync(
						broadcasterId,
						redemption.RewardId,
						new List<string> { redemption.RedemptionId },
						new UpdateCustomRewardRedemptionStatusRequest { Status = TwitchLib.Api.Core.Enums.CustomRewardRedemptionStatus.CANCELED },
						api.Settings.AccessToken
					);

					redemptions.Remove(redemption);
				}
				catch (Exception e)
				{
					if (i == UpdateRetryCount)
					{
						if (e.GetType() == typeof(BadResourceException))
						{
							//Console.WriteLine("Server responded with 404 while updating redemption status.");
						}
						else
						{
							throw new Exception("Server error while updating redemption status. Click Continue.", e);
						}
					}
					else
					{
						await Task.Delay((int) Math.Pow(RetryBaseMs, i));
					}
				}
			}
		}

		private async void FulfilOldRedemptions(object sender, EventArgs e)
		{
			DateTime currentTime = DateTime.Now;

			int requestDelay = 2000;

			while (redemptions.Count * requestDelay > FulfilTimerInterval)
			{
				requestDelay /= 2;
			}

			for (int i = redemptions.Count - 1; i >= 0; i--)
			{
				if ((currentTime - redemptions[i].RedeemedAt).TotalSeconds > FulfilRewardDelay)
				{
					await FulfillRedemption(redemptions[i]);
					redemptions.Remove(redemptions[i]);
					await Task.Delay(requestDelay);
				}
			}
		}

		private void onListenResponse(object sender, OnListenResponseArgs e)
		{
			if (!e.Successful)
				throw new Exception($"Failed to listen! Response: {e.Response}");
		}

		private void onPubSubServiceConnected(object sender, EventArgs e)
		{
			client.SendTopics(api.Settings.AccessToken);
		}

		private async void Client_OnPubSubServiceClosed(object sender, EventArgs e)
		{
			if (manualDisconnect)
			{
				manualDisconnect = false;
				return;
			}
			for (int i = 0; i <= RetryCount; i++)
			{
				try
				{
					client.Connect();
				}
				catch (Exception ex)
				{
					if (i == RetryCount)
					{
						throw new Exception("Failed to reconnect! Click continue and disconnect to delete rewards.", ex);
					}
					else
					{
						await Task.Delay((int) Math.Pow(RetryBaseMs, i));
					}
				}
			}
		}
	}
}
