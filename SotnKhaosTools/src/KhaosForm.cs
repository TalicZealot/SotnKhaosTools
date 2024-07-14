using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using BizHawk.Client.Common;
using BizHawk.Emulation.Common;
using SotnApi.Interfaces;
using SotnKhaosTools.Configuration.Interfaces;
using SotnKhaosTools.Khaos;
using SotnKhaosTools.Khaos.Interfaces;
using SotnKhaosTools.Khaos.Models;
using SotnKhaosTools.Services;
using SotnKhaosTools.Services.Adapters;

namespace SotnKhaosTools
{
	internal sealed partial class KhaosForm : Form, IKhaosActionsInfoDisplay, INotifyPropertyChanged, IRelicLocationDisplay
	{
		private ICheatCollectionAdapter adaptedCheats;
		private readonly IToolConfig toolConfig;
		private readonly ChannelPointsController? channelPointsController;
		private ActionDispatcher? actionDispatcher;
		private KhaosController? khaosController;
		private CheatsController? cheatsController;

		private int[] actionTimers;
		private System.Windows.Forms.Timer countdownTimer;

		private string heartOfVladLocation = String.Empty;
		private string toothOfVladLocation = String.Empty;
		private string ribOfVladLocation = String.Empty;
		private string ringOfVladLocation = String.Empty;
		private string eyeOfVladLocation = String.Empty;

		private string batLocation = String.Empty;
		private string mistLocation = String.Empty;
		private string jewelLocation = String.Empty;
		private string gravityBootsLocation = String.Empty;
		private string leapstoneLocation = String.Empty;
		private string mermanLocation = String.Empty;

		private bool started = false;
		private bool connected = false;

		public KhaosForm(IToolConfig toolConfig, CheatCollection cheats, ISotnApi sotnApi, INotificationService notificationService, IMemoryDomains memoryDomains)
		{
			if (toolConfig is null) throw new ArgumentNullException(nameof(toolConfig));
			if (cheats is null) throw new ArgumentNullException(nameof(cheats));
			if (toolConfig is null) throw new ArgumentNullException(nameof(toolConfig));
			if (cheats == null) throw new ArgumentNullException(nameof(cheats));
			if (memoryDomains == null) throw new ArgumentNullException(nameof(memoryDomains));
			this.toolConfig = toolConfig;

			adaptedCheats = new CheatCollectionAdapter(cheats, memoryDomains);
			cheatsController = new CheatsController(adaptedCheats);
			khaosController = new KhaosController(toolConfig, sotnApi, cheatsController, notificationService, this);
			actionDispatcher = new ActionDispatcher(toolConfig, khaosController, notificationService, this);
			channelPointsController = new ChannelPointsController(toolConfig, actionDispatcher, notificationService, new EnemyRenamer(sotnApi));

			InitializeComponent();
			SuspendLayout();
			ResumeLayout();
			countdownTimer = new();
			countdownTimer.Interval = 1000;
			countdownTimer.Tick += DecrementTimers;
			countdownTimer.Start();
			this.PropertyChanged += KhaosForm_PropertyChanged;

			heartLocation.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, nameof(HeartOfVladLocation)));
			toothLocation.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, nameof(ToothOfVladLocation)));
			ribLocation.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, nameof(RibOfVladLocation)));
			ringLocation.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, nameof(RingOfVladLocation)));
			eyeLocation.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, nameof(EyeOfVladLocation)));

			batLocationLabel.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, nameof(BatLocation)));
			mistLocationLabel.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, nameof(MistLocation)));
			jewelLocationLabel.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, nameof(JewelOfOpenLocation)));
			gravLocationLabel.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, nameof(GravityBootsLocation)));
			leapLocationLabel.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, nameof(LepastoneLocation)));
			mermanLocationLabel.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, nameof(MermanLocation)));

			redemptionsGridView.AutoGenerateColumns = false;
			redemptionsGridView.DataSource = channelPointsController.Redemptions;
			redemptionsGridView.CellClick += RefundRedemption;

			actionTimers = new int[toolConfig.Khaos.Actions.Count];
		}

		public ICheatCollectionAdapter AdaptedCheats
		{
			get => adaptedCheats;

			set
			{
				adaptedCheats = value;
				cheatsController.GetCheats();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public string HeartOfVladLocation
		{
			get => heartOfVladLocation;
			set
			{
				heartOfVladLocation = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HeartOfVladLocation)));
			}
		}
		public string ToothOfVladLocation
		{
			get => toothOfVladLocation;
			set
			{
				toothOfVladLocation = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToothOfVladLocation)));
			}
		}
		public string RibOfVladLocation
		{
			get => ribOfVladLocation;
			set
			{
				ribOfVladLocation = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RibOfVladLocation)));
			}
		}
		public string RingOfVladLocation
		{
			get => ringOfVladLocation;
			set
			{
				ringOfVladLocation = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RingOfVladLocation)));
			}
		}
		public string EyeOfVladLocation
		{
			get => eyeOfVladLocation;
			set
			{
				eyeOfVladLocation = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EyeOfVladLocation)));
			}
		}
		public string BatLocation
		{
			get => batLocation;
			set
			{
				batLocation = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BatLocation)));
			}
		}
		public string WolfLocation
		{
			get => batLocation;
			set
			{
				batLocation = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WolfLocation)));
			}
		}
		public string MistLocation
		{
			get => mistLocation;
			set
			{
				mistLocation = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MistLocation)));
			}
		}
		public string PowerOfMistLocation
		{
			get => mistLocation;
			set
			{
				mistLocation = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PowerOfMistLocation)));
			}
		}
		public string JewelOfOpenLocation
		{
			get => jewelLocation;
			set
			{
				jewelLocation = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JewelOfOpenLocation)));
			}
		}
		public string GravityBootsLocation
		{
			get => gravityBootsLocation;
			set
			{
				gravityBootsLocation = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GravityBootsLocation)));
			}
		}
		public string LepastoneLocation
		{
			get => leapstoneLocation;
			set
			{
				leapstoneLocation = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LepastoneLocation)));
			}
		}
		public string MermanLocation
		{
			get => mermanLocation;
			set
			{
				mermanLocation = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MermanLocation)));
			}
		}
		public List<QueuedAction> ActionQueue { get; set; }

		public void AddTimer(int i)
		{
			actionTimers[i] = (int) toolConfig.Khaos.Actions[i].Duration.TotalSeconds;
		}
		public void UpdateKhaosValues()
		{
			khaosController?.Update();
		}
		public void FrameAdvance()
		{
			khaosController?.FrameAdvance();
			actionDispatcher?.FrameAdvance();
		}
		private void KhaosForm_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
		}

		private async void RefundRedemption(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex != redemptionsGridView.Columns["Refund"].Index) return;
			await channelPointsController.CancelRedemption(channelPointsController.Redemptions[e.RowIndex]);
		}
		private void DecrementTimers(Object sender, EventArgs e)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < actionTimers.Length; i++)
			{
				if (actionTimers[i] == 0)
				{
					continue;
				}
				actionTimers[i] -= 1;
				int minutes = actionTimers[i] / 60;
				int seconds = actionTimers[i] % 60;
				sb.Append(toolConfig.Khaos.Actions[i].Name.PadRight(16, ' '));
				sb.Append(minutes);
				sb.Append(':');
				sb.Append(seconds);
				sb.AppendLine();
			}
			timersTextBox.Text = sb.ToString();
			DrawQueue();
		}
		private void DrawQueue()
		{
			string queueLines = "";

			foreach (QueuedAction? action in ActionQueue)
			{
				queueLines += action.Name + "\r\n";
			}

			queueTextBox.Text = queueLines;
		}
		private void Khaos_Load(object sender, EventArgs e)
		{
			this.Location = toolConfig.Khaos.Location;
			queueRadio.Checked = toolConfig.Khaos.ControlPannelQueueActions;
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				queueRadio.Checked = true;
			}
			else
			{
				instantRadio.Checked = true;
			}
		}
		private void KhaosForm_Move(object sender, EventArgs e)
		{
			if (this.Location.X > 0)
			{
				toolConfig.Khaos.Location = this.Location;
			}
		}
		private void queueRadio_CheckedChanged(object sender, EventArgs e)
		{
			if (queueRadio.Checked)
			{
				toolConfig.Khaos.ControlPannelQueueActions = true;
			}
			else
			{
				toolConfig.Khaos.ControlPannelQueueActions = false;
			}
		}
		private async void startButton_Click(object sender, EventArgs e)
		{
			if (started)
			{
				if (connected)
				{
					await channelPointsController.Disconnect();
					connected = false;
				}
				started = false;
				khaosController.StopKhaos();
				actionDispatcher.StopActions();
				startButton.Text = "Start";
				startButton.BackColor = System.Drawing.Color.FromArgb(17, 0, 17);
				startButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(48, 20, 48);
				connectButton.Enabled = false;
				connectButton.Text = "Connect to Twitch";
				autoKhaosButton.Enabled = false;
				actionDispatcher.AutoKhaosOn = false;
				autoKhaosButton.Text = "Start Auto Khaos";
				autoKhaosButton.BackColor = System.Drawing.Color.FromArgb(17, 0, 17);
				autoKhaosButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(48, 20, 48);
			}
			else
			{
				started = true;
				khaosController.StartKhaos();
				actionDispatcher.StartActions();
				startButton.Text = "Stop";
				startButton.BackColor = System.Drawing.Color.FromArgb(114, 32, 25);
				startButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(169, 19, 7);
				connectButton.Enabled = true;
				autoKhaosButton.Enabled = true;
			}
		}
		private void autoKhaosButton_Click(object sender, EventArgs e)
		{
			if (actionDispatcher.AutoKhaosOn)
			{
				actionDispatcher.AutoKhaosOn = false;
				autoKhaosButton.Text = "Start Auto Khaos";
				autoKhaosButton.BackColor = System.Drawing.Color.FromArgb(17, 0, 17);
				autoKhaosButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(48, 20, 48);
			}
			else
			{
				actionDispatcher.AutoKhaosOn = true;
				autoKhaosButton.Text = "Stop Auto Khaos";
				autoKhaosButton.BackColor = System.Drawing.Color.FromArgb(62, 68, 91);
				autoKhaosButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(72, 81, 118);
			}
		}
		private async void connectButton_Click(object sender, EventArgs e)
		{
			if (connected)
			{
				await channelPointsController.Disconnect();
				connectButton.Text = "Connect to Twitch";
				connected = false;
				connectButton.BackColor = System.Drawing.Color.FromArgb(17, 0, 17);
				connectButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(48, 20, 48);
			}
			else
			{
				channelPointsController.Connect();

				connectButton.Text = "Disonnect";
				connected = true;
				connectButton.BackColor = System.Drawing.Color.FromArgb(93, 56, 147);
				connectButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(145, 70, 255);
			}
		}

		#region Khaotic effects
		private void randomStatusButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.KhaosStatus, UserName = "Khaos" });
			}
			else
			{
				khaosController.KhaosStatus();
			}
		}
		private void randomEquipmentButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.KhaosEquipment, UserName = "Khaos" });
			}
			else
			{

				khaosController.KhaosEquipment();
			}
		}
		private void randomizeStatsButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.KhaosStats, UserName = "Khaos" });
			}
			else
			{

				khaosController.KhaosStats();
			}
		}
		private void randomizeRelicsButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.KhaosRelics, UserName = "Khaos" });
			}
			else
			{

				khaosController.KhaosRelics();
			}
		}
		private void pandorasBoxButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.PandorasBox, UserName = "Khaos" });
			}
			else
			{

				khaosController.PandorasBox();
			}
		}
		private void gambleButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.Gamble, UserName = "Khaos" });
			}
			else
			{

				khaosController.Gamble();
			}
		}
		private void banishButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.Banish, UserName = "Khaos" });
			}
			else
			{

				khaosController.Banish();
			}
		}
		private void khaosTrackButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.KhaosTrack, UserName = "Khaos", Data = "random" });
			}
			else
			{
				khaosController.KhaosTrack("random");
			}
		}
		#endregion
		#region Debuffs
		private void bankruptButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.Bankrupt, UserName = "Khaos" });
			}
			else
			{
				khaosController.Bankrupt();
			}
		}
		private void weakenButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.Weaken, UserName = "Khaos" });
			}
			else
			{
				khaosController.Weaken();
			}
		}
		private void respawnBossesButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.RespawnBosses, UserName = "Khaos" });
			}
			else
			{
				khaosController.RespawnBosses();
			}
		}
		private void crippleButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.Slow, UserName = "Khaos" });
			}
			else
			{

				khaosController.Slow();
			}
		}
		private void bloodManaButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.BloodMana, UserName = "Khaos" });
			}
			else
			{
				khaosController.BloodMana();
			}
		}
		private void thurstButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.Thirst, UserName = "Khaos" });
			}
			else
			{
				khaosController.Thirst();
			}
		}
		private void hordeButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.KhaosHorde, UserName = "Khaos" });
			}
			else
			{
				khaosController.Horde();
			}
		}
		private void enduranceButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.Endurance, UserName = "Khaos" });
			}
			else
			{
				khaosController.Endurance();
			}
		}
		private void hnkButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.HnK, UserName = "Khaos" });
			}
			else
			{
				khaosController.HnK();
			}
		}
		private void bulletHellButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.BulletHell, UserName = "Khaos" });
			}
			else
			{
				khaosController.BulletHell();
			}
		}
		#endregion
		#region Buffs
		private void vampireButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.Quad, UserName = "Khaos" });
			}
			else
			{

				khaosController.Quad();
			}
		}
		private void lightHelpButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.LightHelp, UserName = "Khaos" });
			}
			else
			{
				khaosController.LightHelp();
			}
		}
		private void mediumHelpButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.MediumHelp, UserName = "Khaos" });
			}
			else
			{

				khaosController.MediumHelp();
			}
		}
		private void heavyHelpButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.HeavyHelp, UserName = "Khaos" });
			}
			else
			{

				khaosController.HeavytHelp();
			}
		}
		private void battleOrdersButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.BattleOrders, UserName = "Khaos" });
			}
			else
			{

				khaosController.BattleOrders();
			}
		}
		private void magicianButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.Magician, UserName = "Khaos" });
			}
			else
			{

				khaosController.Magician();
			}
		}
		private void meltyButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.MeltyBlood, UserName = "Khaos" });
			}
			else
			{

				khaosController.MeltyBlood();
			}
		}
		private void fourBeastsButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.FourBeasts, UserName = "Khaos" });
			}
			else
			{
				khaosController.FourBeasts();
			}
		}
		private void zawarudoButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.ZAWARUDO, UserName = "Khaos" });
			}
			else
			{
				khaosController.ZaWarudo();
			}
		}
		private void hasteButton_Click(object sender, EventArgs e)
		{
			if (toolConfig.Khaos.ControlPannelQueueActions)
			{
				actionDispatcher.EnqueueAction(new EventAddAction { ActionIndex = (int) Khaos.Enums.Action.Haste, UserName = "Khaos" });
			}
			else
			{
				khaosController.Haste();
			}
		}
		#endregion

		private async void KhaosForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (started)
			{
				started = false;
				khaosController.StopKhaos();
				actionDispatcher.StopActions();
			}
			startButton.Text = "Start";
			startButton.BackColor = System.Drawing.Color.FromArgb(17, 0, 17);
			connectButton.Enabled = false;
			connectButton.Text = "Connect to Twitch";
			if (connected)
			{
				await channelPointsController.Disconnect();
				connected = false;
			};
			autoKhaosButton.Enabled = false;
			actionDispatcher.AutoKhaosOn = false;
			autoKhaosButton.Text = "Start Auto Khaos";
			autoKhaosButton.BackColor = System.Drawing.Color.FromArgb(17, 0, 17);
			khaosController = null;
			actionDispatcher = null;
			cheatsController = null;
		}
	}
}
