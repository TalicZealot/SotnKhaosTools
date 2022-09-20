﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using SotnKhaosTools.Configuration.Interfaces;
using SotnKhaosTools.Constants;
using SotnKhaosTools.Khaos.Interfaces;
using SotnKhaosTools.Khaos.Models;
using SotnKhaosTools.Services;
using SotnKhaosTools.Services.Interfaces;
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
		private const int RefreshTokenInterval = 3 * (60 * (60 * 1000)) + (30 * (60 * 1000)); // 3.5 hours

		private readonly IToolConfig toolConfig;
		private readonly ITwitchListener twitchListener;
		private readonly IActionDispatcher actionDispatcher;
		private readonly INotificationService notificationService;
		private readonly IEnemyRenamer enemyRenamer;

		private System.Windows.Forms.Timer refreshTokenTimer = new();
		private List<string> scopes = new List<string> { "channel:read:subscriptions", "channel:read:redemptions", "channel:manage:redemptions" };
		private TwitchAPI api = new TwitchAPI();
		private TwitchPubSub client = new TwitchPubSub();
		private string broadcasterId;
		private string refreshToken;
		private List<string> customRewardIds = new();
		private List<Timer> actionsStartingOnCooldown = new();
		private BindingList<Redemption> redemptions = new();
		private System.Windows.Forms.Timer redemptionFulfilTimer = new();

		private bool manualDisconnect = false;

		public ChannelPointsController(IToolConfig toolConfig, ITwitchListener twitchListener, IActionDispatcher actionDispatcher, INotificationService notificationService, IEnemyRenamer enemyRenamer)
		{
			if (toolConfig is null) throw new ArgumentNullException(nameof(toolConfig));
			if (twitchListener is null) throw new ArgumentNullException(nameof(twitchListener));
			if (actionDispatcher is null) throw new ArgumentNullException(nameof(actionDispatcher));
			if (notificationService is null) throw new ArgumentNullException(nameof(notificationService));
			if (enemyRenamer is null) throw new ArgumentNullException(nameof(enemyRenamer));
			this.toolConfig = toolConfig;
			this.twitchListener = twitchListener;
			this.actionDispatcher = actionDispatcher;
			this.notificationService = notificationService;
			this.enemyRenamer = enemyRenamer;

			refreshTokenTimer.Tick += RefreshToken;
			refreshTokenTimer.Interval = RefreshTokenInterval;
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

		public async Task<bool> Connect()
		{
			Console.WriteLine($"Authenticating...");
			validateCreds();
			api.Settings.ClientId = TwitchConfiguration.TwitchClientId;
			Process.Start(getAuthorizationCodeUrl(api.Settings.ClientId, Paths.TwitchRedirectUri, scopes));
			Services.Models.Authorization? auth = await twitchListener.Listen();
			if (auth.Code == String.Empty)
			{
				return false;
			}
			TwitchLib.Api.Auth.AuthCodeResponse? resp = await api.Auth.GetAccessTokenFromCodeAsync(auth.Code, TwitchConfiguration.TwitchClientSecret, Paths.TwitchRedirectUri);
			api.Settings.AccessToken = resp.AccessToken;
			refreshToken = resp.RefreshToken;

			var user = (await api.Helix.Users.GetUsersAsync()).Users[0];
			broadcasterId = user.Id;
			Console.WriteLine($"Authorization success!\r\nUser: {user.DisplayName}\r\nId: {user.Id} \r\nToken expires in : {resp.ExpiresIn}");

			await GetSubscribers();
			await CreateRewards();

			client.OnPubSubServiceConnected += onPubSubServiceConnected;
			client.OnListenResponse += onListenResponse;
			client.OnChannelPointsRewardRedeemed += Client_OnChannelPointsRewardRedeemed;
			client.OnPubSubServiceClosed += Client_OnPubSubServiceClosed;
			client.ListenToChannelPoints(user.Id);
			client.Connect();
			notificationService.AddMessage("Connected to Twitch");
			refreshTokenTimer.Start();
			redemptionFulfilTimer.Start();
			return true;
		}

		public async Task Disconnect()
		{
			refreshTokenTimer.Stop();
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
			Console.WriteLine($"Fetching subscribers...");

			GetBroadcasterSubscriptionsResponse subs = await RetryRequest.Do(
				async () => await api.Helix.Subscriptions.GetBroadcasterSubscriptions(
					broadcasterId,
					null,
					MaxSubscribers,
					api.Settings.AccessToken
					),
				RetryBaseMs,
				RetryCount
				);
			List<string> totalSubs = subs.Data.Select(u => u.UserName).ToList();

			if (subs.Total > MaxSubscribers)
			{
				GetBroadcasterSubscriptionsResponse subsPageTwoData = await RetryRequest.Do(
				async () => await api.Helix.Subscriptions.GetBroadcasterSubscriptions(
					broadcasterId,
					subs.Pagination.Cursor,
					MaxSubscribers,
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
			Console.WriteLine($"Creating rewards...");
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

					Console.WriteLine($"Request parameters: Title: {request.Title} Cost: {request.Cost} Cooldown: {request.GlobalCooldownSeconds}");

					CreateCustomRewardsResponse response = await RetryRequest.Do(
						async () => await api.Helix.ChannelPoints.CreateCustomRewards(
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
						async () => await api.Helix.ChannelPoints.CreateCustomRewards(
							broadcasterId,
							request,
							api.Settings.AccessToken
						),
						RetryBaseMs,
						RetryCount
						);
			customRewardIds.Add(response.Data[0].Id);
			Console.WriteLine($"Added new delayed reward {request.Title}.");
			notificationService.AddMessage($"{request.Title} reward added.");
		}
		//TODO: Add a way to remove rewards on the fly
		private async Task DeleteRewards()
		{
			Console.WriteLine($"Deleting rewards...");
			for (int i = 0; i < customRewardIds.Count; i++)
			{
				Console.WriteLine($"Deleting reward with id: {customRewardIds[i]}");

				await RetryRequest.Do(
					async () => await api.Helix.ChannelPoints.DeleteCustomReward(
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

		private async void RefreshToken(object sender, EventArgs e)
		{
			try
			{
				var refresh = await api.Auth.RefreshAuthTokenAsync(refreshToken, TwitchConfiguration.TwitchClientSecret, api.Settings.ClientId);
				api.Settings.AccessToken = refresh.AccessToken;
				Console.WriteLine("Successfully refreshed authentication token!");
			}
			catch (Exception ex)
			{
				throw new Exception("Server error while refreshing connection.", ex);
			}
		}

		private async void Client_OnChannelPointsRewardRedeemed(object sender, OnChannelPointsRewardRedeemedArgs e)
		{
			Console.WriteLine($"Channel point reward redeemed: {e.RewardRedeemed.Redemption.Reward.Title}");

			var action = toolConfig.Khaos.Actions.Where(a => a.Name == e.RewardRedeemed.Redemption.Reward.Title).FirstOrDefault();

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
				async () => await api.Helix.ChannelPoints.UpdateCustomReward(
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

		public async Task FulfillRedemption(Redemption redemption)
		{
			for (int i = 0; i <= UpdateRetryCount; i++)
			{
				try
				{
					await api.Helix.ChannelPoints.UpdateCustomRewardRedemptionStatus(
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
							Console.WriteLine("Server responded with 404 while updating redemption status on attempt: " + i);
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
					await api.Helix.ChannelPoints.UpdateCustomRewardRedemptionStatus(
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
							Console.WriteLine("Server responded with 404 while updating redemption status.");
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
			Console.WriteLine(e.ChannelId + " " + e.Topic);
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

		private string getAuthorizationCodeUrl(string clientId, string redirectUri, List<string> scopes)
		{
			var scopesStr = String.Join("+", scopes);

			return "https://id.twitch.tv/oauth2/authorize?" +
				   $"client_id={clientId}&" +
				   $"redirect_uri={HttpUtility.UrlEncode(redirectUri)}&" +
				   "response_type=code&" +
				   $"scope={scopesStr}";
		}

		private void validateCreds()
		{
			if (String.IsNullOrEmpty(TwitchConfiguration.TwitchClientId))
				throw new Exception("client id cannot be null or empty");
			if (String.IsNullOrEmpty(TwitchConfiguration.TwitchClientSecret))
				throw new Exception("client secret cannot be null or empty");
			if (String.IsNullOrEmpty(Paths.TwitchRedirectUri))
				throw new Exception("redirect uri cannot be null or empty");
		}

	}
}
