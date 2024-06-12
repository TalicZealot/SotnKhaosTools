using System;
using System.Drawing;
using System.Windows.Forms;
using SotnKhaosTools.Configuration.Interfaces;
using SotnKhaosTools.Services;

namespace SotnKhaosTools
{
	internal sealed partial class KhaosSettingsPanel : UserControl
	{
		private readonly IToolConfig? toolConfig;
		private BindingSource actionSettingsSource = new();

		public KhaosSettingsPanel(IToolConfig toolConfig)
		{
			if (toolConfig is null) throw new ArgumentNullException(nameof(toolConfig));
			this.toolConfig = toolConfig;

			InitializeComponent();
		}
		public INotificationService NotificationService { get; set; }

		private void KhaosSettingsPanel_Load(object sender, EventArgs e)
		{
			SetPalenValues();

			foreach (var action in toolConfig.Khaos.Actions)
			{
				actionSettingsSource.Add(action);
			}
			alertsGridView.AutoGenerateColumns = false;
			alertsGridView.DataSource = actionSettingsSource;
			alertsGridView.CellClick += AlertsGridView_BrowseClick;
			actionsGridView.AutoGenerateColumns = false;
			actionsGridView.DataSource = actionSettingsSource;
			actionCooldownsGridView.AutoGenerateColumns = false;
			actionCooldownsGridView.DataSource = actionSettingsSource;
			actionPricingGridView.AutoGenerateColumns = false;
			actionPricingGridView.DataSource = actionSettingsSource;
			actionPricingGridView.CellValidating += ActionPricingGridView_CellValidating;
		}

		private void SetPalenValues()
		{
			alertsCheckbox.Checked = toolConfig.Khaos.Alerts;
			volumeTrackBar.Value = toolConfig.Khaos.Volume;
			queueTextBox.Text = toolConfig.Khaos.QueueInterval.ToString();
			dynamicIntervalCheckBox.Checked = toolConfig.Khaos.DynamicInterval;
			keepVladRelicsCheckbox.Checked = toolConfig.Khaos.KeepVladRelics;
			costDecayCheckBox.Checked = toolConfig.Khaos.CostDecay;
			autoDifficultyComboBox.Text = toolConfig.Khaos.AutoKhaosDifficulty;
			minimumBitsTextBox.Text = toolConfig.Khaos.MinimumBits.ToString();
			choiceBitsTextBox.Text = toolConfig.Khaos.BitsChoice.ToString();
		}

		private void ActionPricingGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			string property = actionPricingGridView.Columns[e.ColumnIndex].DataPropertyName;

			if (property.Equals("Scaling"))
			{
				if (string.IsNullOrEmpty(e.FormattedValue.ToString()))
				{
					actionPricingGridView.Rows[e.RowIndex].ErrorText =
						"Scaling must not be empty.";
					e.Cancel = true;
				}
				else if (Convert.ToDouble(e.FormattedValue) < 1)
				{
					actionPricingGridView.Rows[e.RowIndex].ErrorText =
						"Scaling cannot be lower than 1.";
					e.Cancel = true;
				}
				else if (Convert.ToDouble(e.FormattedValue) > 10)
				{
					actionPricingGridView.Rows[e.RowIndex].ErrorText =
						"Scaling cannot be higher than 10.";
					e.Cancel = true;
				}
				else
				{
					actionPricingGridView.Rows[e.RowIndex].ErrorText = "";
					e.Cancel = false;
				}
			}
			else if (property.Equals("MaximumChannelPoints"))
			{
				if (string.IsNullOrEmpty(e.FormattedValue.ToString()))
				{
					actionPricingGridView.Rows[e.RowIndex].ErrorText =
						"Maximum Channel Points must not be empty.";
					e.Cancel = true;
				}
				else if (Convert.ToUInt32(e.FormattedValue) <= toolConfig.Khaos.Actions[e.RowIndex].ChannelPoints && Convert.ToUInt32(e.FormattedValue) != 0)
				{
					actionPricingGridView.Rows[e.RowIndex].ErrorText =
						"Maximum Channel Points must be higher than Channel Points or 0(uncapped).";
					e.Cancel = true;
				}
			}
		}

		private void AlertsGridView_BrowseClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex !=
			alertsGridView.Columns["Browse"].Index) return;
			alertFileDialog.Tag = e.RowIndex;
			alertFileDialog.ShowDialog();
		}
		private void alertFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			alertsGridView.Rows[(int) alertFileDialog.Tag].Cells[1].Value = alertFileDialog.FileName;
		}

		private void saveButton_Click(object sender, EventArgs e)
		{
			toolConfig.SaveConfig();
		}

		private void khaosDerfaultsButton_Click(object sender, EventArgs e)
		{
			toolConfig.Khaos.Default();
			SetPalenValues();
			actionPricingGridView.Refresh();

			actionSettingsSource.Clear();
			foreach (var action in toolConfig.Khaos.Actions)
			{
				actionSettingsSource.Add(action);
			}
		}

		private void volumeTrackBar_Scroll(object sender, EventArgs e)
		{
			toolConfig.Khaos.Volume = volumeTrackBar.Value;
			if (NotificationService is not null)
			{
				NotificationService.Volume = (double) volumeTrackBar.Value / 10F;
			}
		}

		private void alertsCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			toolConfig.Khaos.Alerts = alertsCheckbox.Checked;
		}

		private void queueTextBox_Validated(object sender, EventArgs e)
		{
			TimeSpan queueInterval;
			bool result = TimeSpan.TryParse(queueTextBox.Text, out queueInterval);
			if (result)
			{
				toolConfig.Khaos.QueueInterval = queueInterval;
			}
			queueTextBox.BackColor = Color.White;
			this.valueToolTip.Active = false;
		}

		private void queueTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			TimeSpan queueInterval;
			TimeSpan minSpan = new TimeSpan(0, 0, 10);
			TimeSpan maxSpan = new TimeSpan(0, 10, 0);
			bool result = TimeSpan.TryParse(queueTextBox.Text, out queueInterval);
			if (!result)
			{
				this.queueTextBox.Text = "";
				this.queueTextBox.BackColor = Color.Red;
				this.valueToolTip.SetToolTip(queueTextBox, "Invalid value! Format: (hh:mm:ss)");
				this.valueToolTip.ToolTipIcon = ToolTipIcon.Warning;
				this.valueToolTip.Active = true;
				e.Cancel = true;
			}
			if (queueInterval < minSpan || queueInterval > maxSpan)
			{
				this.queueTextBox.Text = "";
				this.queueTextBox.BackColor = Color.Red;
				this.valueToolTip.SetToolTip(queueTextBox, "Value must be greater than 10 seconds and lower than 10 minutes!");
				this.valueToolTip.ToolTipIcon = ToolTipIcon.Warning;
				this.valueToolTip.Active = true;
				e.Cancel = true;
			}
		}

		private void dynamicIntervalCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			toolConfig.Khaos.DynamicInterval = dynamicIntervalCheckBox.Checked;
		}

		private void keepVladRelicsCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			toolConfig.Khaos.KeepVladRelics = keepVladRelicsCheckbox.Checked;
		}

		private void costDecayCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			toolConfig.Khaos.CostDecay = costDecayCheckBox.Checked;
		}

		private void autoDifficultyComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			toolConfig.Khaos.AutoKhaosDifficulty = autoDifficultyComboBox.SelectedItem.ToString();
		}

		private void halfChannelPointPricesButton_Click(object sender, EventArgs e)
		{
			toolConfig.Khaos.ReduceAllChannelPointPrices();
			actionPricingGridView.Refresh();
		}

		private void doubleChannelPointPricesButton_Click(object sender, EventArgs e)
		{
			toolConfig.Khaos.IncreaseAllChannelPointPrices();
			actionPricingGridView.Refresh();
		}

		private void minimumBitsTextBox_TextChanged(object sender, EventArgs e)
		{
			toolConfig.Khaos.MinimumBits = Int32.Parse(minimumBitsTextBox.Text);
		}

		private void choiceBitsTextBox_TextChanged(object sender, EventArgs e)
		{
			toolConfig.Khaos.BitsChoice = Int32.Parse(choiceBitsTextBox.Text);
		}
	}
}
