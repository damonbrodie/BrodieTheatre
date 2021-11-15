using System;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Win32;


namespace BrodieTheatre
{
    public partial class FormSettings : Form
    {
        public RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
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
            Properties.Settings.Default.trayPlaybackLevel           = trackBarTrayPlayback.Value;
            Properties.Settings.Default.potsPlaybackLevel           = trackBarPotsPlayback.Value;
            Properties.Settings.Default.trayPausedLevel             = trackBarTrayPaused.Value;
            Properties.Settings.Default.potsPausedLevel             = trackBarPotsPaused.Value;
            Properties.Settings.Default.trayStoppedLevel            = trackBarTrayStopped.Value;
            Properties.Settings.Default.potsStoppedLevel            = trackBarPotsStopped.Value;
            Properties.Settings.Default.trayEnteringLevel           = trackBarTrayEntering.Value;
            Properties.Settings.Default.potsEnteringLevel           = trackBarPotsEntering.Value;
            Properties.Settings.Default.motionSensorAddress         = textBoxMotionSensorAddress.Text;
            Properties.Settings.Default.doorSensorAddress           = textBoxDoorSensorAddress.Text;
            Properties.Settings.Default.startMinimized              = checkBoxStartMinimized.Checked;
            Properties.Settings.Default.kodiJSONPort                = (int)numericUpDownKodiPort.Value;
            Properties.Settings.Default.connectKodiLocalhost        = checkBoxConnectKodiLocalhost.Checked;
            Properties.Settings.Default.insteonMotionLatch          = trackBarInsteonMotionMinimumTime.Value;
            Properties.Settings.Default.lightingDelayProjectorOn    = trackBarDelayLightingProjectorStart.Value;
            Properties.Settings.Default.shutdownLatch               = trackBarShutdown.Value;

            if (checkBoxStartWithWindows.Checked)
            {
                rkApp.SetValue("BrodieTheatre", Application.ExecutablePath);
            }
            else
            {
                rkApp.DeleteValue("BrodieTheatre", false);
            }

            Properties.Settings.Default.Save();

            this.Close();
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            checkBoxStartMinimized.Checked          = Properties.Settings.Default.startMinimized;
            textBoxHarmonyHubIP.Text                = Properties.Settings.Default.harmonyHubIP;
            numericUpDownKodiPort.Value             = (decimal)Properties.Settings.Default.kodiJSONPort;
            checkBoxConnectKodiLocalhost.Checked    = Properties.Settings.Default.connectKodiLocalhost;
            textBoxPotsAddress.Text                 = Properties.Settings.Default.potsAddress;
            textBoxTrayAddress.Text                 = Properties.Settings.Default.trayAddress;
            textBoxMotionSensorAddress.Text         = Properties.Settings.Default.motionSensorAddress;
            textBoxDoorSensorAddress.Text           = Properties.Settings.Default.doorSensorAddress;

            if (Properties.Settings.Default.connectKodiLocalhost == true)
            {
                numericUpDownKodiPort.Enabled = true;
            }
            else
            {
                numericUpDownKodiPort.Enabled = false;
            }

            if (rkApp.GetValue("BrodieTheatre") == null)
            {
                checkBoxStartWithWindows.Checked = false;
            }
            else
            {
                checkBoxStartWithWindows.Checked = true;
            }

            try
            {
                trackBarShutdown.Value = Properties.Settings.Default.shutdownLatch;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarShutdown.Value = trackBarShutdown.Minimum;
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

        private void trackBarShutdown_ValueChanged(object sender, EventArgs e)
        {
            labelShutdownMinutes.Text = trackBarShutdown.Value.ToString();
        }

        private void checkBoxConnectKodiLocalhost_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxConnectKodiLocalhost.Checked == true)
            {
                numericUpDownKodiPort.Enabled = true;
            }
            else
            {
                numericUpDownKodiPort.Enabled = false;
            }
        }
    }
}