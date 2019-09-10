using System;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using GoogleCast;
using System.Collections.Generic;


namespace BrodieTheatre
{
    public partial class FormSettings : Form
    {
        private IEnumerable<IReceiver> receivers;

        public FormSettings()
        {
            InitializeComponent();
        }

        public class ComboboxSmartSpeakerItem
        {
            public string Text { get; set; }
            public string Id { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.harmonyHubIP                = textBoxHarmonyHubIP.Text;
            Properties.Settings.Default.plmPort                     = comboBoxInsteonPort.Text;
            Properties.Settings.Default.projectorPort               = comboBoxProjectorPort.Text;
            Properties.Settings.Default.potsAddress                 = textBoxPotsAddress.Text;
            Properties.Settings.Default.trayAddress                 = textBoxTrayAddress.Text;
            Properties.Settings.Default.fanAddress                  = textBoxExhaustFanAddress.Text;
            Properties.Settings.Default.trayPlaybackLevel           = trackBarTrayPlayback.Value;
            Properties.Settings.Default.potsPlaybackLevel           = trackBarPotsPlayback.Value;
            Properties.Settings.Default.trayPausedLevel             = trackBarTrayPaused.Value;
            Properties.Settings.Default.potsPausedLevel             = trackBarPotsPaused.Value;
            Properties.Settings.Default.trayStoppedLevel            = trackBarTrayStopped.Value;
            Properties.Settings.Default.potsStoppedLevel            = trackBarPotsStopped.Value;
            Properties.Settings.Default.trayEnteringLevel           = trackBarTrayEntering.Value;
            Properties.Settings.Default.potsEnteringLevel           = trackBarPotsEntering.Value;
            Properties.Settings.Default.globalShutdown              = trackBarGlobalShutdown.Value;
            Properties.Settings.Default.motionSensorAddress         = textBoxMotionSensorAddress.Text;
            Properties.Settings.Default.doorSensorAddress           = textBoxDoorSensorAddress.Text;
            Properties.Settings.Default.startMinimized              = checkBoxStartMinimized.Checked;
            Properties.Settings.Default.kodiJSONPort                = (int)numericUpDownKodiPort.Value;
            Properties.Settings.Default.kodiIP                      = textBoxKodiIP.Text;
            Properties.Settings.Default.insteonMotionLatch          = trackBarInsteonMotionMinimumTime.Value;
            Properties.Settings.Default.lightingDelayProjectorOn    = trackBarDelayLightingProjectorStart.Value;
            Properties.Settings.Default.fanDelayOff                 = trackBarExhaustFanDelayOff.Value;
            Properties.Settings.Default.googleCloudCredentialsJSON  = textBoxGoogleCredentialsFile.Text;
            Properties.Settings.Default.webServerAuthToken          = textBoxAuthToken.Text;

            ComboboxSmartSpeakerItem item = comboBoxSmartSpeakers.SelectedItem as ComboboxSmartSpeakerItem;
            if (item != null)
            {
                Properties.Settings.Default.SmartSpeaker = item.Id.ToString();
            }
            else
            {
                Properties.Settings.Default.SmartSpeaker = "";
            }
            

            Properties.Settings.Default.Save();
            this.Close();
        }

        private async void FormSettings_Load(object sender, EventArgs e)
        {
            string ip = Network.GetLocalIPAddress();

            checkBoxStartMinimized.Checked  = Properties.Settings.Default.startMinimized;
            textBoxHarmonyHubIP.Text        = Properties.Settings.Default.harmonyHubIP;
            numericUpDownKodiPort.Value     = (decimal)Properties.Settings.Default.kodiJSONPort;
            textBoxKodiIP.Text              = Properties.Settings.Default.kodiIP;
            textBoxPotsAddress.Text         = Properties.Settings.Default.potsAddress;
            textBoxTrayAddress.Text         = Properties.Settings.Default.trayAddress;
            textBoxExhaustFanAddress.Text   = Properties.Settings.Default.fanAddress;
            textBoxMotionSensorAddress.Text = Properties.Settings.Default.motionSensorAddress;
            textBoxDoorSensorAddress.Text   = Properties.Settings.Default.doorSensorAddress;
            textBoxAuthToken.Text           = Properties.Settings.Default.webServerAuthToken;
            labelWebServerPort.Text         = Properties.Settings.Default.webServerPort.ToString();
            labelWebServerIP.Text           = ip;

            if (Network.IsPortListening(ip, Properties.Settings.Default.webServerPort))
            {
                labelwebServerStatus.Text = "Listening";
                labelwebServerStatus.ForeColor = System.Drawing.Color.ForestGreen;
            }

            else
            {
                labelwebServerStatus.Text = "Not Listening";
                labelwebServerStatus.ForeColor = System.Drawing.Color.Maroon;
            }

            if (File.Exists(Properties.Settings.Default.googleCloudCredentialsJSON))
            {
                textBoxGoogleCredentialsFile.Text = Properties.Settings.Default.googleCloudCredentialsJSON;
            }

            receivers = await new DeviceLocator().FindReceiversAsync();
            foreach (var receiver in receivers)
            {
                ComboboxSmartSpeakerItem item = new ComboboxSmartSpeakerItem
                {
                    Text = receiver.FriendlyName,
                    Id = receiver.Id
                };
                int index = comboBoxSmartSpeakers.Items.Add(item);
                if (receiver.Id == Properties.Settings.Default.SmartSpeaker)
                {
                    comboBoxSmartSpeakers.SelectedIndex = index;
                }
            }

            try
            {
                trackBarExhaustFanDelayOff.Value = Properties.Settings.Default.fanDelayOff;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarExhaustFanDelayOff.Value = trackBarExhaustFanDelayOff.Maximum;
                }
            }

            try
            {
                trackBarDelayLightingProjectorStart.Value = Properties.Settings.Default.lightingDelayProjectorOn;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarDelayLightingProjectorStart.Value = trackBarDelayLightingProjectorStart.Minimum;
                }
            }

            try
            {
                trackBarTrayPlayback.Value = Properties.Settings.Default.trayPlaybackLevel;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarTrayPlayback.Value = trackBarTrayPlayback.Minimum;
                }
            }

            try
            {
                trackBarPotsPlayback.Value = Properties.Settings.Default.potsPlaybackLevel;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarPotsPlayback.Value = trackBarPotsPlayback.Minimum;
                }
            }

            try
            {
                trackBarTrayPaused.Value = Properties.Settings.Default.trayPausedLevel;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarTrayPaused.Value = trackBarTrayPaused.Minimum;
                }
            }

            try
            {
                trackBarPotsPaused.Value = Properties.Settings.Default.potsPausedLevel;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarPotsPaused.Value = trackBarPotsPaused.Minimum;
                }
            }

            try
            {
                trackBarTrayStopped.Value = Properties.Settings.Default.trayStoppedLevel;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarTrayStopped.Value = trackBarTrayStopped.Minimum;
                }
            }

            try
            {
                trackBarPotsStopped.Value = Properties.Settings.Default.potsStoppedLevel;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarPotsStopped.Value = trackBarPotsStopped.Minimum;
                }
            }

            try
            {
                trackBarPotsEntering.Value = Properties.Settings.Default.potsEnteringLevel;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarPotsEntering.Value = trackBarPotsEntering.Minimum;
                }
            }

            try
            {
                trackBarTrayEntering.Value = Properties.Settings.Default.trayEnteringLevel;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarTrayEntering.Value = trackBarTrayEntering.Minimum;
                }
            }

            try
            {
                trackBarGlobalShutdown.Value = Properties.Settings.Default.globalShutdown;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarGlobalShutdown.Value = trackBarGlobalShutdown.Minimum;
                }
            }

            try
            {
                trackBarInsteonMotionMinimumTime.Value = Properties.Settings.Default.insteonMotionLatch;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarInsteonMotionMinimumTime.Value = trackBarInsteonMotionMinimumTime.Minimum; 
                }
            }

            //show list of valid com ports
            foreach (string s in SerialPort.GetPortNames())
            {
                comboBoxInsteonPort.Items.Add(s);
                comboBoxProjectorPort.Items.Add(s);
                if (s == Properties.Settings.Default.plmPort)
                {
                    comboBoxInsteonPort.SelectedItem = s;
                }
                if (s == Properties.Settings.Default.projectorPort)
                {
                    comboBoxProjectorPort.SelectedItem = s;
                }
            }
        }

        private void trackBarTrayPlayback_ValueChanged(object sender, EventArgs e)
        {           
            labelTrayPlayback.Text = (trackBarTrayPlayback.Value * 10).ToString() + "%";
        }

        private void trackBarPotsPlayback_ValueChanged(object sender, EventArgs e)
        {
            labelPotsPlayback.Text = (trackBarPotsPlayback.Value * 10).ToString() + "%";
        }

        private void trackBarTrayPaused_ValueChanged(object sender, EventArgs e)
        {
            labelTrayPaused.Text = (trackBarTrayPaused.Value * 10).ToString() + "%";
        }

        private void trackBarPotsPaused_ValueChanged(object sender, EventArgs e)
        {
            labelPotsPaused.Text = (trackBarPotsPaused.Value * 10).ToString() + "%";
        }

        private void trackBarTrayStopped_ValueChanged(object sender, EventArgs e)
        {
            labelTrayStopped.Text = (trackBarTrayStopped.Value * 10).ToString() + "%";
        }

        private void trackBarPotsStopped_ValueChanged(object sender, EventArgs e)
        {
            labelPotsStopped.Text = (trackBarPotsStopped.Value * 10).ToString() + "%";
        }

        private void trackBarTrayEntering_ValueChanged(object sender, EventArgs e)
        {
            labelTrayEntering.Text = (trackBarTrayEntering.Value * 10).ToString() + "%";
        }

        private void trackBarPotsEntering_ValueChanged(object sender, EventArgs e)
        {
            labelPotsEntering.Text = (trackBarPotsEntering.Value * 10).ToString() + "%";
        }

        private void trackBarGlobalShutdown_ValueChanged(object sender, EventArgs e)
        {
            if (trackBarGlobalShutdown.Value == 1)
            {
                labelGlobalShutdownHours.Text = "minute";
            }
            else
            {
                labelGlobalShutdownHours.Text = "minutes";
            }
            labelGlobalShutdown.Text = trackBarGlobalShutdown.Value.ToString();
        }

        private void trackBarInsteonMotionMinimumTime_ValueChanged(object sender, EventArgs e)
        {
            labelInsteonMotionLatch.Text = trackBarInsteonMotionMinimumTime.Value.ToString();
        }

        private void trackBarDelayLightingProjectorStart_ValueChanged(object sender, EventArgs e)
        {
            if (trackBarDelayLightingProjectorStart.Value == trackBarDelayLightingProjectorStart.Minimum)
            {
                labelDelayLightingProjectorStart.Text = "Off";
            }
            else
            {
                labelDelayLightingProjectorStart.Text = trackBarDelayLightingProjectorStart.Value.ToString();
            }
        }

        private void trackBarExhaustFanDelayOff_ValueChanged(object sender, EventArgs e)
        {
            if (trackBarExhaustFanDelayOff.Value == trackBarExhaustFanDelayOff.Minimum)
            {
                labelExhaustFanDelayOffMinutes.Text = "";
                labelExhaustFanDelayOff.Text = "Off";
            }
            else
            {
                labelExhaustFanDelayOffMinutes.Text = "minutes";
                labelExhaustFanDelayOff.Text = trackBarExhaustFanDelayOff.Value.ToString();
            }
        }

        private void buttonSelectCredentials_Click(object sender, EventArgs e)
        {
            if(openFileDialogGoogleCredentials.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxGoogleCredentialsFile.Text = openFileDialogGoogleCredentials.FileName;
            }
        }
    }
}