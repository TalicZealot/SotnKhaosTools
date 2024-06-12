using System;
using System.Drawing;
using System.Windows.Forms;
using SotnApi.Interfaces;
using SotnKhaosTools.Configuration.Interfaces;
using SotnKhaosTools.Khaos.Interfaces;
using SotnKhaosTools.RandoTracker;
using SotnKhaosTools.RandoTracker.Adapters;
using SotnKhaosTools.Services;

namespace SotnKhaosTools
{
	internal sealed partial class TrackerForm : Form
	{
		private readonly IToolConfig toolConfig;
		private readonly IWatchlistService watchlistService;
		private readonly ISotnApi sotnApi;
		private readonly INotificationService notificationService;

		private GraphicsAdapter? formGraphics;
		private Tracker? tracker;
		private TrackerRendererGDI? trackerRendererGDI;
		private Bitmap drawingSurface;

		public TrackerForm(IToolConfig toolConfig, IWatchlistService watchlistService, ISotnApi sotnApi, INotificationService notificationService)
		{
			if (toolConfig is null) throw new ArgumentNullException(nameof(toolConfig));
			if (watchlistService is null) throw new ArgumentNullException(nameof(watchlistService));
			if (sotnApi is null) throw new ArgumentNullException(nameof(sotnApi));
			if (notificationService is null) throw new ArgumentNullException(nameof(notificationService));
			this.toolConfig = toolConfig;
			this.watchlistService = watchlistService;
			this.sotnApi = sotnApi;
			this.notificationService = notificationService;

			InitializeComponent();
			SuspendLayout();
			ResumeLayout();
		}

		public void UpdateTracker()
		{
			if (tracker is not null)
			{
				tracker.Update();
				if (trackerRendererGDI.Refreshed)
				{
					trackerRendererGDI.Refreshed = false;
					this.Invalidate();
				}
			}
		}

		public void SetTrackerVladRelicLocationDisplay(IRelicLocationDisplay vladRelicLocationDisplay)
		{
			if (vladRelicLocationDisplay is null) throw new ArgumentNullException(nameof(vladRelicLocationDisplay));
			tracker.RelicLocationDisplay = vladRelicLocationDisplay;
		}

		private void TrackerForm_Load(object sender, EventArgs e)
		{
			TopMost = toolConfig.Tracker.AlwaysOnTop;
			Size = new Size(toolConfig.Tracker.Width, toolConfig.Tracker.Height);

			if (SystemInformation.VirtualScreen.Width > toolConfig.Tracker.Location.X && SystemInformation.VirtualScreen.Height > toolConfig.Tracker.Location.Y)
			{
				Location = toolConfig.Tracker.Location;
			}

			drawingSurface = new Bitmap(this.Width, this.Height);
			Graphics internalGraphics = Graphics.FromImage(drawingSurface);
			this.formGraphics = new GraphicsAdapter(internalGraphics);
			trackerRendererGDI = new TrackerRendererGDI(formGraphics, toolConfig);
			tracker = new Tracker(trackerRendererGDI, toolConfig, watchlistService, sotnApi, notificationService);
		}

		private void TrackerForm_Paint(object sender, PaintEventArgs e)
		{
			if (tracker != null)
			{
				e.Graphics.DrawImage(drawingSurface, 0, 0);
			}
		}

		private void TrackerForm_Resize(object sender, EventArgs e)
		{
			if (this.Location.X < 0)
			{
				return;
			}
			if (this.Width > toolConfig.Tracker.Width || this.Height > toolConfig.Tracker.Height)
			{
				drawingSurface = new Bitmap(this.Width, this.Height);
				Graphics internalGraphics = Graphics.FromImage(drawingSurface);
				this.formGraphics = new GraphicsAdapter(internalGraphics);
			}

			if (this.Width <= this.MaximumSize.Width && this.Height <= this.MaximumSize.Height)
			{
				toolConfig.Tracker.Width = this.Width;
				toolConfig.Tracker.Height = this.Height;
			}

			if (tracker is not null && formGraphics is not null)
			{
				trackerRendererGDI.ChangeGraphics(formGraphics);
				trackerRendererGDI.CalculateGrid(this.Width, this.Height);
				this.Invalidate();
				trackerRendererGDI.Render();
			}
		}

		private void TrackerForm_Move(object sender, EventArgs e)
		{
			if (this.Location.X > 0)
			{
				toolConfig.Tracker.Location = this.Location;
			}
		}

		private void TrackerForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			notificationService.StopOverlayServer();
			//Dispose of tracker properly.
			tracker = null;
		}

	}
}
