using System;
using System.Windows.Forms;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        /* My Insteon addresses
           42.22.B8 Pot
           42.20.F8 Tray
           41.66.88 Motion Sensor
           41.58.FC Door Sensor

          Mapped keypresses
           F12 - Lights to Entering level
           F11 - Lights Off
           F9  - Lights to Stopped level
           F7  - Lights to Playback level
           F6  - Projector Lens Kodi Menu (Not captured by App)
           F5  - Projector Lens to Narrow aspect ratio
           F4  - Projector Lens to Wide aspect ratio
           F3  - Kodi next audio language (Not captured by App)
        */

        static FormMain formMain;

        private int statusTickCounter = 0;

        public bool debugInsteon = true;
        public bool debugHarmony = false;

        public FormMain()
        {
            hookID = SetHook(proc);
            formMain = this;
            InitializeComponent();
            Logging.writeLog("------ Brodie Theatre Starting Up ------");
            if (Properties.Settings.Default.startMinimized)
            {
                this.WindowState = FormWindowState.Minimized;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
            }

        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private async void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form formSettings = new FormSettings();
            formSettings.ShowDialog();

            // Reset things after the settings have been saved.
            if (currentPLMport != Properties.Settings.Default.plmPort)
            {
                currentPLMport = Properties.Settings.Default.plmPort;
                insteonConnectPLM();
            }

            if (currentHarmonyIP != Properties.Settings.Default.harmonyHubIP)
            {
                currentHarmonyIP = Properties.Settings.Default.harmonyHubIP;
                await HarmonyConnectAsync(true);
            }

            if (currentKodiIP != Properties.Settings.Default.kodiIP || currentKodiPort != Properties.Settings.Default.kodiJSONPort)
            {
                currentKodiIP = Properties.Settings.Default.kodiIP;
                currentKodiPort = Properties.Settings.Default.kodiJSONPort;
                kodiStatusDisconnect();
            }

            if (Properties.Settings.Default.lightingDelayProjectorOn > 0)
            {
                timerStartLights.Interval = Properties.Settings.Default.lightingDelayProjectorOn * 1000;
            }
        }

        private async void FormMain_Load(object sender, EventArgs e)
        {
            formMain.BeginInvoke(new Action(() =>
            {
                formMain.timerSetLights.Enabled = true;
            }));

            currentPLMport = Properties.Settings.Default.plmPort;
            insteonConnectPLM();
            projectorConnect();

            if (labelProjectorStatus.Text == "Connected")
            {
                projectorCheckPower();
            }

            if (Properties.Settings.Default.potsAddress != string.Empty)
            {
                lights[Properties.Settings.Default.potsAddress] = -1;
            }

            if (Properties.Settings.Default.trayAddress != string.Empty)
            {
                lights[Properties.Settings.Default.trayAddress] = -1;
            }

            currentHarmonyIP = Properties.Settings.Default.harmonyHubIP;
            await HarmonyConnectAsync(true);
            if (Program.Client != null)
            {
                Program.Client.OnActivityChanged += HarmonyClient_OnActivityChanged;
            }
            currentHarmonyIP = Properties.Settings.Default.harmonyHubIP;

            formMain.BeginInvoke(new Action(() =>
            {
                if (Properties.Settings.Default.lightingDelayProjectorOn > 0)
                {
                    formMain.timerStartLights.Interval = Properties.Settings.Default.lightingDelayProjectorOn * 1000;
                }

            }));
        }

        private void TimerClearStatus_Tick(object sender, EventArgs e)
        {
            if (statusTickCounter > 0)
            {
                statusTickCounter -= 1;
            }
            else
            {
                toolStripStatus.Text = "";
            }
        }

        private void ToolStripStatus_TextChanged(object sender, EventArgs e)
        {
            if (toolStripStatus.Text != string.Empty)
            {
                statusTickCounter = 2;
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (Program.Client != null)
                {
                    Program.Client.Dispose();
                }
            }
            catch { }
            if (powerlineModem != null)
            {
                powerlineModem.Dispose();
            }

            UnhookWindowsHookEx(hookID);
            Logging.writeLog("------ Brodie Theatre Shutting Down ------");
        }

        

        private void ListBoxActivities_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Activities activity = (Activities)listBoxActivities.SelectedItem;
            HarmonyStartActivity(activity.Text, activity.Id, true);
        }

        private void LabelRoomOccupancy_TextChanged(object sender, EventArgs e)
        {
            if (labelRoomOccupancy.Text == "Occupied")
            {
                Logging.writeLog("Occupancy:  Room Occupied");

                if (!HarmonyIsActivityStarted() && labelKodiPlaybackStatus.Text == "Stopped")
                {
                    lightsToEnteringLevel();
                }

                toolStripStatus.Text = "Room is now occupied";
            }
            else if (labelRoomOccupancy.Text == "Vacant")
            {
                Logging.writeLog("Occupancy:  Room vacant");
                toolStripStatus.Text = "Room is now vacant";
            }
        }

        private void LabelProjectorPower_TextChanged(object sender, EventArgs e)
        {
            if (labelProjectorPower.Text == "On")
            {
                buttonProjectorChangeAspect.Enabled = true;
            }
            else
            {
                buttonProjectorChangeAspect.Enabled = false;
            }
        }

        private void LabelRoomOccupancy_Click(object sender, EventArgs e)
        {
            if (labelRoomOccupancy.Text == "Occupied")
            {
                labelRoomOccupancy.Text = "Vacant";
                Logging.writeLog("Occupancy:  Overriding Room to Vacant");
                insteonMotionLatchActive = false;
            }
            else
            {
                labelRoomOccupancy.Text = "Occupied";
                insteonDoMotion(false);
                Logging.writeLog("Occupancy:  Overriding Room to Occupied");
            }
        }
    }
}
