﻿using System;
using System.Windows.Forms;
using SotnKhaosTools.Configuration.Interfaces;

namespace SotnKhaosTools
{
	public partial class AutotrackerSettingsPanel : UserControl
	{
		private readonly IToolConfig? toolConfig;
		private BindingSource configBindingSource = new BindingSource();

		public AutotrackerSettingsPanel(IToolConfig toolConfig)
		{
			if (toolConfig is null) throw new ArgumentNullException(nameof(toolConfig));
			this.toolConfig = toolConfig;

			InitializeComponent();
		}

		private void AutotrackerSettingsPanel_Load(object sender, EventArgs e)
		{
			this.configBindingSource.DataSource = toolConfig;

			if (toolConfig.Tracker.ProgressionRelicsOnly)
			{
				radioProgression.Checked = true;
			}
			else
			{
				radioAllRelics.Checked = true;
			}

			if (toolConfig.Tracker.GridLayout)
			{
				radioGrid.Checked = true;
			}
			else
			{
				radioCollected.Checked = true;
			}

			alwaysOnTopCheckBox.Checked = toolConfig.Tracker.AlwaysOnTop;
			locationsCheckbox.Checked = toolConfig.Tracker.Locations;
			replaysCheckBox.Checked = toolConfig.Tracker.SaveReplays;
			overlayCheckBox.Checked = toolConfig.Tracker.UseOverlay;
			autosplitterCheckBox.Checked = toolConfig.Tracker.EnableAutosplitter;

			customLocationsGuardedRadio.Checked = toolConfig.Tracker.CustomLocationsGuarded;
			customLocationsEquipmentRadio.Checked = toolConfig.Tracker.CustomLocationsEquipment;
			customLocationsClassicRadio.Checked = toolConfig.Tracker.CustomLocationsClassic;
			customLocationsSpreadRadio.Checked = toolConfig.Tracker.CustomLocationsSpread;

			username.Text = toolConfig.Tracker.Username;
		}

		private void radioProgression_CheckedChanged(object sender, EventArgs e)
		{
			if (radioProgression.Checked)
			{
				toolConfig.Tracker.ProgressionRelicsOnly = true;
			}
			else
			{
				toolConfig.Tracker.ProgressionRelicsOnly = false;
			}
		}

		private void radioCollected_CheckedChanged(object sender, EventArgs e)
		{
			if (radioCollected.Checked)
			{
				toolConfig.Tracker.GridLayout = false;
			}
			else
			{
				toolConfig.Tracker.GridLayout = true;
			}
		}

		private void alwaysOnTopCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			toolConfig.Tracker.AlwaysOnTop = alwaysOnTopCheckBox.Checked;
		}

		private void locationsCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			toolConfig.Tracker.Locations = locationsCheckbox.Checked;
		}

		private void saveButton_Click(object sender, EventArgs e)
		{
			toolConfig.SaveConfig();
		}

		private void trackerDerfaultsButton_Click(object sender, EventArgs e)
		{
			toolConfig.Tracker.Default();
		}

		private void replaysCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			toolConfig.Tracker.SaveReplays = replaysCheckBox.Checked;
		}

		private void customLocationsGuardedRadio_CheckedChanged(object sender, EventArgs e)
		{
			toolConfig.Tracker.CustomLocationsGuarded = customLocationsGuardedRadio.Checked;
		}

		private void customLocationsEquipmentRadio_CheckedChanged(object sender, EventArgs e)
		{
			toolConfig.Tracker.CustomLocationsEquipment = customLocationsEquipmentRadio.Checked;
		}

		private void customLocationsClassicRadio_CheckedChanged(object sender, EventArgs e)
		{
			toolConfig.Tracker.CustomLocationsClassic = customLocationsClassicRadio.Checked;
		}

		private void customLocationsSpreadRadio_CheckedChanged(object sender, EventArgs e)
		{
			toolConfig.Tracker.CustomLocationsSpread = customLocationsSpreadRadio.Checked;
		}

		private void username_TextChanged(object sender, EventArgs e)
		{
			toolConfig.Tracker.Username = username.Text;
		}

		private void overlayCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			toolConfig.Tracker.UseOverlay = overlayCheckBox.Checked;
		}

		private void autosplitterCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			toolConfig.Tracker.EnableAutosplitter = autosplitterCheckBox.Checked;
		}
	}
}
