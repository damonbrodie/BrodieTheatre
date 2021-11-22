using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        Dictionary<string, int> lights = new Dictionary<string, int>();

        private void trackBarTray_ValueChanged(object sender, EventArgs e)
        {
            labelTray.Text = (trackBarTray.Value * 10).ToString() + "%";
        }

        private void trackBarPots_ValueChanged(object sender, EventArgs e)
        {
            labelPots.Text = (trackBarPots.Value * 10).ToString() + "%";
        }

        private void timerPotTrack_Tick(object sender, EventArgs e)
        {
            timerPotTrack.Enabled = false;
            queueLightLevel(Properties.Settings.Default.potsAddress, trackBarPots.Value);
        }

        private void timerTrayTrack_Tick(object sender, EventArgs e)
        {
            timerTrayTrack.Enabled = false;
            queueLightLevel(Properties.Settings.Default.trayAddress, trackBarTray.Value);
        }

        private void trackBarTray_Scroll(object sender, EventArgs e)
        {
            timerTrayTrack.Enabled = false;
            timerTrayTrack.Enabled = true;
        }

        private void trackBarPots_Scroll(object sender, EventArgs e)
        {
            timerPotTrack.Enabled = false;
            timerPotTrack.Enabled = true;
        }

        private void setDelayedLightTimer()
        {
            if (Properties.Settings.Default.lightingDelayProjectorOn > 0 && timerStartLights.Enabled == false)
            {
                timerStartLights.Enabled = true;
            }
        }

        private void timerStartLights_Tick(object sender, EventArgs e)
        {
            timerStartLights.Enabled = false;
            lightsToStoppedLevel();
        }

        private bool areLightsOff()
        {
            if (formMain.trackBarPots.Value == 0 && formMain.trackBarTray.Value == 0)
            {
                return true;
            }
            return false;
        }

        private void lightsToStoppedLevel()
        {
            Logging.writeLog("Lighting:  Setting lights to Stopped Level");
            toolStripStatus.Text = "Setting lights to Stopped Level";
            queueLightLevel(Properties.Settings.Default.potsAddress, Properties.Settings.Default.potsStoppedLevel);
            trackBarPots.Value = Properties.Settings.Default.potsStoppedLevel;
            queueLightLevel(Properties.Settings.Default.trayAddress, Properties.Settings.Default.trayStoppedLevel);
            trackBarTray.Value = Properties.Settings.Default.trayStoppedLevel;
        }

        private void lightsOn()
        {
            Logging.writeLog("Lighting:  Setting lights to On");
            toolStripStatus.Text = "Turning lights on";
            queueLightLevel(Properties.Settings.Default.potsAddress, 100);
            trackBarPots.Value = trackBarPots.Maximum;
            queueLightLevel(Properties.Settings.Default.trayAddress, 100);
            trackBarTray.Value = trackBarTray.Maximum;
        }

        private void lightsToEnteringLevel()
        {
            Logging.writeLog("Lighting:  Setting lights to Occupancy Level");
            toolStripStatus.Text = "Turning on lights to Occupancy Level";
            queueLightLevel(Properties.Settings.Default.potsAddress, Properties.Settings.Default.potsEnteringLevel);
            trackBarPots.Value = Properties.Settings.Default.potsEnteringLevel;
            queueLightLevel(Properties.Settings.Default.trayAddress, Properties.Settings.Default.trayEnteringLevel);
            trackBarTray.Value = Properties.Settings.Default.trayEnteringLevel;
        }

        private void lightsOff()
        {
            Logging.writeLog("Lighting:  Setting lights to Off");
            toolStripStatus.Text = "Turning lights off";
            queueLightLevel(Properties.Settings.Default.potsAddress, 0);
            trackBarPots.Value = trackBarPots.Minimum;
            queueLightLevel(Properties.Settings.Default.trayAddress, 0);
            trackBarTray.Value = trackBarTray.Minimum;
        }

        private void lightsToPlaybackLevel()
        {
            Logging.writeLog("Lighting:  Setting lights to Playback Level");
            toolStripStatus.Text = "Dimming lights to Playback Level";
            queueLightLevel(Properties.Settings.Default.potsAddress, Properties.Settings.Default.potsPlaybackLevel);
            trackBarPots.Value = Properties.Settings.Default.potsPlaybackLevel;
            queueLightLevel(Properties.Settings.Default.trayAddress, Properties.Settings.Default.trayPlaybackLevel);
            trackBarTray.Value = Properties.Settings.Default.trayPlaybackLevel;
        }

        private void lightsToPausedLevel()
        {
            Logging.writeLog("Lighting:  Setting lights to Paused Level");
            toolStripStatus.Text = "Setting lights to Paused Level";
            queueLightLevel(Properties.Settings.Default.potsAddress, Properties.Settings.Default.potsPausedLevel);
            trackBarPots.Value = Properties.Settings.Default.potsPausedLevel;
            queueLightLevel(Properties.Settings.Default.trayAddress, Properties.Settings.Default.trayPausedLevel);
            trackBarTray.Value = Properties.Settings.Default.trayPausedLevel;
        }

        private void timerSetLights_Tick(object sender, EventArgs e)
        {

            if (Properties.Settings.Default.potsAddress != string.Empty && lights.ContainsKey(Properties.Settings.Default.potsAddress) && lights[Properties.Settings.Default.potsAddress] != -1)
            {
                insteonSetLightLevel(Properties.Settings.Default.potsAddress, lights[Properties.Settings.Default.potsAddress]);
            }
            else if (Properties.Settings.Default.trayAddress != string.Empty && lights.ContainsKey(Properties.Settings.Default.trayAddress) && lights[Properties.Settings.Default.trayAddress] != -1)
            {
                insteonSetLightLevel(Properties.Settings.Default.trayAddress, lights[Properties.Settings.Default.trayAddress]);
            }
        }

        public void queueLightLevel(string address, int level)
        {
            Logging.writeLog("Lighting:  Queuing light '" + address + "' to level '" + level.ToString() + "'");
            lights[address] = level;
        }
    }
}