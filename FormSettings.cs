using System;
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

        public List<string> panasonic_pj_labels = new List<string>
        {
            "Lens Memory 1",
            "Lens Memory 2",
            "Lens Memory 3",
            "Lens Memory 4",
            "Lens Memory 5"
        };

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
            Properties.Settings.Default.shutdownLatch               = trackBarShutdownProjector.Value;
            Properties.Settings.Default.projectorARSetting1         = comboBoxUsePJ1.Text;
            Properties.Settings.Default.projectorARSetting2         = comboBoxUsePJ2.Text;
            Properties.Settings.Default.projectorARSetting3         = comboBoxUsePJ3.Text;
            Properties.Settings.Default.projectorARSetting4         = comboBoxUsePJ4.Text;
            Properties.Settings.Default.projectorARSetting5         = comboBoxUsePJ5.Text;
            Properties.Settings.Default.projectorARRangeHigh1       = numericUpDownAR1.Value;
            Properties.Settings.Default.projectorARRangeHigh2       = numericUpDownAR2.Value;
            Properties.Settings.Default.projectorARRangeHigh3       = numericUpDownAR3.Value;
            Properties.Settings.Default.projectorARRangeHigh4       = numericUpDownAR4.Value;
            Properties.Settings.Default.projectorARRangeHigh5       = numericUpDownAR5.Value;
            Properties.Settings.Default.projectorARRangeLow2        = labelAR2From.Text;
            Properties.Settings.Default.projectorARRangeLow3        = labelAR3From.Text;
            Properties.Settings.Default.projectorARRangeLow4        = labelAR4From.Text;
            Properties.Settings.Default.projectorARRangeLow5        = labelAR5From.Text;
            Properties.Settings.Default.lightingShutdownMinutes     = trackBarShutdownLights.Value;

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
            labelAR2From.Text                       = Properties.Settings.Default.projectorARRangeLow2;
            labelAR3From.Text                       = Properties.Settings.Default.projectorARRangeLow3;
            labelAR4From.Text                       = Properties.Settings.Default.projectorARRangeLow4;
            labelAR5From.Text                       = Properties.Settings.Default.projectorARRangeLow5;
            numericUpDownAR1.Value                  = Properties.Settings.Default.projectorARRangeHigh1;
            numericUpDownAR2.Value                  = Properties.Settings.Default.projectorARRangeHigh2;
            numericUpDownAR3.Value                  = Properties.Settings.Default.projectorARRangeHigh3;
            numericUpDownAR4.Value                  = Properties.Settings.Default.projectorARRangeHigh4;
            numericUpDownAR5.Value                  = Properties.Settings.Default.projectorARRangeHigh5;

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
                trackBarShutdownProjector.Value = Properties.Settings.Default.shutdownLatch;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarShutdownProjector.Value = trackBarShutdownProjector.Minimum;
                }
            }

            try
            {
                trackBarShutdownLights.Value = Properties.Settings.Default.lightingShutdownMinutes;
            }
            catch(Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarShutdownLights.Value = trackBarShutdownLights.Minimum;
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

            foreach (string s in panasonic_pj_labels)
            {
                comboBoxUsePJ1.Items.Add(s);
                if (s == Properties.Settings.Default.projectorARSetting1)
                {
                    comboBoxUsePJ1.SelectedItem = s;
                }
                comboBoxUsePJ2.Items.Add(s);
                if (s == Properties.Settings.Default.projectorARSetting2)
                {
                    comboBoxUsePJ2.SelectedItem = s;
                }
                comboBoxUsePJ3.Items.Add(s);
                if (s == Properties.Settings.Default.projectorARSetting3)
                {
                    comboBoxUsePJ3.SelectedItem = s;
                }
                comboBoxUsePJ4.Items.Add(s);
                if (s == Properties.Settings.Default.projectorARSetting4)
                {
                    comboBoxUsePJ4.SelectedItem = s;
                }
                comboBoxUsePJ5.Items.Add(s);
                if (s == Properties.Settings.Default.projectorARSetting5)
                {
                    comboBoxUsePJ5.SelectedItem = s;
                }
            }
        }

        private void trackBarShutdownLights_ValueChanged(object sender, EventArgs e)
        {
            labelShutdownLightsMinutes.Text = trackBarShutdownLights.Value.ToString();
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
            labelShutdownProjectorMinutes.Text = trackBarShutdownProjector.Value.ToString();
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

        private void numericUpDownAR1_ValueChanged(object sender, EventArgs e)
        {
            decimal new_val = decimal.Add(numericUpDownAR1.Value, 0.01m);
            labelAR2From.Text = new_val.ToString();
        }

        private void numericUpDownAR2_ValueChanged(object sender, EventArgs e)
        {
            decimal new_val = decimal.Add(numericUpDownAR2.Value, 0.01m);
            labelAR3From.Text = new_val.ToString();
            decimal below_val = Decimal.Parse(labelAR2From.Text);
            if (numericUpDownAR2.Value <= below_val)
            {
                labelAR2From.Text = decimal.Subtract(numericUpDownAR2.Value, 0.01m).ToString();
            }
        }

        private void numericUpDownAR3_ValueChanged(object sender, EventArgs e)
        {
            decimal new_val = decimal.Add(numericUpDownAR3.Value, 0.01m);
            labelAR4From.Text = new_val.ToString();
            decimal below_val = Decimal.Parse(labelAR3From.Text);
            if (numericUpDownAR3.Value <= below_val)
            {
                labelAR3From.Text = decimal.Subtract(numericUpDownAR3.Value, 0.01m).ToString();
            }
        }

        private void numericUpDownAR4_ValueChanged(object sender, EventArgs e)
        {
            decimal new_val = decimal.Add(numericUpDownAR4.Value, 0.01m);
            labelAR5From.Text = new_val.ToString();
            decimal below_val = Decimal.Parse(labelAR4From.Text);
            if (numericUpDownAR4.Value <= below_val)
            {
                labelAR4From.Text = decimal.Subtract(numericUpDownAR4.Value, 0.01m).ToString();
            }
        }

        private void numericUpDownAR5_ValueChanged(object sender, EventArgs e)
        {
            decimal below_val = Decimal.Parse(labelAR5From.Text);
            if (numericUpDownAR5.Value <= below_val)
            {
                labelAR5From.Text = decimal.Subtract(numericUpDownAR5.Value, 0.01m).ToString();
            }
        }

        private void labelAR2From_TextChanged(object sender, EventArgs e)
        {
            decimal new_val = Decimal.Parse(labelAR2From.Text);
            if (numericUpDownAR2.Value <= new_val)
            {
                numericUpDownAR2.Value = (Decimal.Add(new_val, 0.01m));
            }
            if (new_val <= numericUpDownAR1.Value)
            {
                numericUpDownAR1.Value = (Decimal.Subtract(new_val, 0.01m));
            }

        }

        private void labelAR3From_TextChanged(object sender, EventArgs e)
        {
            decimal new_val = Decimal.Parse(labelAR3From.Text);
            if (numericUpDownAR3.Value <= new_val)
            {
                numericUpDownAR3.Value = (Decimal.Add(new_val, 0.01m));
            }
            if (new_val <= numericUpDownAR2.Value)
            {
                numericUpDownAR2.Value = (Decimal.Subtract(new_val, 0.01m));
            }
        }

        private void labelAR4From_TextChanged(object sender, EventArgs e)
        {
            decimal new_val = Decimal.Parse(labelAR4From.Text);
            if (numericUpDownAR4.Value <= new_val)
            {
                numericUpDownAR4.Value = (Decimal.Add(new_val, 0.01m));
            }
            if (new_val <= numericUpDownAR3.Value)
            {
                numericUpDownAR3.Value = (Decimal.Subtract(new_val, 0.01m));
            }
        }

        private void labelAR5From_TextChanged(object sender, EventArgs e)
        {
            decimal new_val = Decimal.Parse(labelAR5From.Text);
            if (numericUpDownAR5.Value <= new_val)
            {
                numericUpDownAR5.Value = (Decimal.Add(new_val, 0.01m));
            }
            if (new_val <= numericUpDownAR4.Value)
            {
                numericUpDownAR4.Value = (Decimal.Subtract(new_val, 0.01m));
            }
        }
    }
}