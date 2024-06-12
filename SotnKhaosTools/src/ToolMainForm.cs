using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using BizHawk.Emulation.Common;
using Newtonsoft.Json;
using SotnKhaosTools.Configuration;
using SotnKhaosTools.Constants;
using SotnKhaosTools.Services;
using SotnKhaosTools.Services.Adapters;

namespace SotnKhaosTools
{
	[ExternalTool("Symphony of the Night Khaos Tools",
		Description = "Tools for Twitch interaction for the SotN randomizer.",
		LoadAssemblyFiles = new[]
		{
			"SotnKhaosTools/dll/SotnApi.dll",
			"SotnKhaosTools/dll/System.Threading.Tasks.Extensions.dll",
			"SotnKhaosTools/dll/Microsoft.Extensions.Logging.Abstractions.dll",
			"SotnKhaosTools/dll/System.Net.Http.dll",
			"SotnKhaosTools/dll/TwitchLib.Api.dll",
			"SotnKhaosTools/dll/TwitchLib.Api.Helix.dll",
			"SotnKhaosTools/dll/TwitchLib.Api.Helix.Models.dll",
			"SotnKhaosTools/dll/TwitchLib.Api.Core.dll",
			"SotnKhaosTools/dll/TwitchLib.Api.Core.Enums.dll",
			"SotnKhaosTools/dll/TwitchLib.Api.Core.Interfaces.dll",
			"SotnKhaosTools/dll/TwitchLib.Api.Core.Models.dll",
			"SotnKhaosTools/dll/TwitchLib.Api.V5.dll",
			"SotnKhaosTools/dll/TwitchLib.Api.V5.Models.dll",
			"SotnKhaosTools/dll/TwitchLib.Communication.dll",
			"SotnKhaosTools/dll/TwitchLib.PubSub.dll"
		})]
	[ExternalToolEmbeddedIcon("SotnKhaosTools.Resources.BizAlucard.png")]
	[ExternalToolApplicability.SingleRom(VSystemID.Raw.PSX, "0DDCBC3D")]
	public partial class ToolMainForm : ToolFormBase, IExternalToolForm
	{
		[RequiredService]
		private IEmulator? _emu { get; set; }

		[RequiredService]
		private IMemoryDomains? _memoryDomains { get; set; }

		[RequiredApi]
		private ICommApi? _maybeCommAPI { get; set; }

		[RequiredApi]
		private IEmuClientApi? _maybeClientAPI { get; set; }

		[RequiredApi]
		private IJoypadApi? _maybeJoypadApi { get; set; }

		[RequiredApi]
		private IEmulationApi? _maybeEmuAPI { get; set; }

		[RequiredApi]
		private IGuiApi? _maybeGuiAPI { get; set; }

		[RequiredApi]
		private IMemoryApi? _maybeMemAPI { get; set; }

		[RequiredApi]
		private ISQLiteApi? _maybesQLiteApi { get; set; }

		private ApiContainer? _apis;

		private ApiContainer APIs => _apis ??= new ApiContainer(new Dictionary<Type, IExternalApi>
		{
			[typeof(ICommApi)] = _maybeCommAPI ?? throw new NullReferenceException(),
			[typeof(IEmuClientApi)] = _maybeClientAPI ?? throw new NullReferenceException(),
			[typeof(IJoypadApi)] = _maybeJoypadApi ?? throw new NullReferenceException(),
			[typeof(IEmulationApi)] = _maybeEmuAPI ?? throw new NullReferenceException(),
			[typeof(IGuiApi)] = _maybeGuiAPI ?? throw new NullReferenceException(),
			[typeof(IMemoryApi)] = _maybeMemAPI ?? throw new NullReferenceException(),
			[typeof(ISQLiteApi)] = _maybesQLiteApi ?? throw new NullReferenceException(),
		});
		private Config GlobalConfig => (_maybeEmuAPI as EmulationApi ?? throw new Exception("required API wasn't fulfilled")).ForbiddenConfigReference;

		private SotnApi.Main.SotnApi? sotnApi;
		private ToolConfig toolConfig;
		private WatchlistService? watchlistService;
		private NotificationService? notificationService;
		private TrackerForm? trackerForm;
		private KhaosForm? khaosForm;
		private AutotrackerSettingsPanel? autotrackerSettingsPanel;
		private KhaosSettingsPanel? khaosSettingsPanel;
		private string _windowTitle = "Khaos";
		private const int PanelOffset = 130;
		private int cooldown = 0;

		public ToolMainForm()
		{
			InitializeComponent();
			SuspendLayout();
			ResumeLayout();
			InitializeConfig();
		}

		private void InitializeConfig()
		{
			string currentVersion = typeof(ToolConfig).Assembly.GetName().Version.ToString().Substring(0, 5);
			if (File.Exists(Paths.ConfigPath))
			{
				string configJson = File.ReadAllText(Paths.ConfigPath);

				toolConfig = JsonConvert.DeserializeObject<ToolConfig>(configJson,
					new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace, MissingMemberHandling = MissingMemberHandling.Ignore }) ?? new ToolConfig();
			}
			else
			{
				toolConfig = new ToolConfig();
			}

			if (toolConfig.Version != currentVersion)
			{
				toolConfig.Version = currentVersion;
				toolConfig.Khaos.Default();
			}
		}

		protected override string WindowTitle => _windowTitle;

		protected override string WindowTitleStatic => "Khaos";

		private void ToolMainForm_Load(object sender, EventArgs e)
		{
			this.Location = toolConfig.Location;

			autotrackerSettingsPanel = new AutotrackerSettingsPanel(toolConfig);
			autotrackerSettingsPanel.Location = new Point(0, PanelOffset);
			this.Controls.Add(autotrackerSettingsPanel);

			khaosSettingsPanel = new KhaosSettingsPanel(toolConfig);
			khaosSettingsPanel.Location = new Point(0, PanelOffset);
			if (notificationService is not null)
			{
				khaosSettingsPanel.NotificationService = notificationService;
			}
			this.Controls.Add(khaosSettingsPanel);
		}

		private void LoadCheats()
		{
			this.MainForm.CheatList.DisableAll();

			if (khaosForm is not null && !khaosForm.IsDisposed)
			{
				khaosForm.AdaptedCheats = new CheatCollectionAdapter(this.MainForm.CheatList, _memoryDomains);
			}
		}

		public override bool AskSaveChanges() => true;

		public override void Restart()
		{
			if (trackerForm is not null && !trackerForm.IsDisposed)
			{
				trackerForm.Close();
				trackerForm.Dispose();
			}
			if (khaosForm is not null && !khaosForm.IsDisposed)
			{
				khaosForm.Close();
				khaosForm.Dispose();
			}

			if (notificationService is not null)
			{
				notificationService.StopOverlayServer();
			}

			if (_memoryDomains is null)
			{
				string message = "Castlevania: Symphony of the Night must be open to use this tool";
				string caption = "Error Rom Not Loaded";
				MessageBoxButtons buttons = MessageBoxButtons.OK;
				DialogResult result;

				result = MessageBox.Show(message, caption, buttons);
				if (result == System.Windows.Forms.DialogResult.OK)
				{
					this.Close();
					return;
				}
			}

			LoadCheats();

			sotnApi = new SotnApi.Main.SotnApi(_maybeMemAPI);
			watchlistService = new WatchlistService(_memoryDomains, _emu?.SystemId, GlobalConfig);
			notificationService = new NotificationService(toolConfig, _maybeGuiAPI, _maybeClientAPI);
			if (khaosSettingsPanel is not null)
			{
				khaosSettingsPanel.NotificationService = notificationService;
			}
		}

		public override void UpdateValues(ToolFormUpdateType type)
		{
			if (type != ToolFormUpdateType.PostFrame)
			{
				return;
			}
			if (khaosForm is not null)
			{
				khaosForm.FrameAdvance();
			}
			if (notificationService is not null)
			{
				notificationService.Refresh();
			}
			cooldown++;
			if (cooldown == Globals.UpdateCooldownFrames)
			{
				cooldown = 0;
				if (trackerForm is not null)
				{
					trackerForm.UpdateTracker();
				}
				if (khaosForm is not null)
				{
					khaosForm.UpdateKhaosValues();
				}
				if (this.MainForm.CheatList.Count == 0)
				{
					LoadCheats();
				}
			}
		}

		private void ToolMainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			toolConfig.SaveConfig();
			if (trackerForm != null)
			{
				trackerForm.Close();
				trackerForm.Dispose();
			}

			if (khaosForm != null)
			{
				khaosForm.Close();
				khaosForm.Dispose();
			}

			if (notificationService != null)
			{
				notificationService.StopOverlayServer();
				notificationService = null;
			}

			sotnApi = null;
			watchlistService = null;

			this.MainForm.CheatList.DisableAll();
		}

		private void autotrackerLaunch_Click(object sender, EventArgs e)
		{
			if (sotnApi is not null && watchlistService is not null)
			{
				if (trackerForm is not null)
				{
					trackerForm.Close();
					trackerForm.Dispose();
				}
				trackerForm = new TrackerForm(toolConfig, watchlistService, sotnApi, notificationService);
				trackerForm.Show();
				if (khaosForm is not null && !khaosForm.IsDisposed)
				{
					trackerForm.SetTrackerVladRelicLocationDisplay(khaosForm);
				}
			}
		}

		private void khaosChatLaunch_Click(object sender, EventArgs e)
		{
			if (sotnApi is not null)
			{
				if (khaosForm is not null)
				{
					khaosForm.Close();
					khaosForm.Dispose();
				}
				khaosForm = new KhaosForm(toolConfig, this.MainForm.CheatList, sotnApi, notificationService, _memoryDomains);
				khaosForm.Show();
				if (trackerForm is not null && !trackerForm.IsDisposed)
				{
					trackerForm.SetTrackerVladRelicLocationDisplay(khaosForm);
				}
			}
		}

		private void autotrackerSelect_Click(object sender, EventArgs e)
		{
			autotrackerSettingsPanel.Visible = true;
			autotrackerSettingsPanel.Enabled = true;
			autotrackerLaunch.Visible = true;
			autotrackerLaunch.Enabled = true;

			khaosSettingsPanel.Visible = false;
			khaosSettingsPanel.Enabled = false;
			khaosChatLaunch.Visible = false;
			khaosChatLaunch.Enabled = false;
		}

		private void khaosChatSelect_Click(object sender, EventArgs e)
		{
			khaosSettingsPanel.Visible = true;
			khaosSettingsPanel.Enabled = true;
			khaosChatLaunch.Visible = true;
			khaosChatLaunch.Enabled = true;

			autotrackerSettingsPanel.Visible = false;
			autotrackerSettingsPanel.Enabled = false;
			autotrackerLaunch.Visible = false;
			autotrackerLaunch.Enabled = false;
		}

		private void ToolMainForm_Move(object sender, EventArgs e)
		{
			if (this.Location.X > 0)
			{
				toolConfig.Location = this.Location;
			}
		}

	}
}
