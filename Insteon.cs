using System;
using System.Windows.Forms;
using SoapBox.FluentDwelling;
using SoapBox.FluentDwelling.Devices;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        Plm powerlineModem;
        public string currentPLMport;
        public bool plmConnected;

        public DateTime insteonMotionLatchExpires;
        public bool insteonMotionLatchActive = false;

        public int insteonProcessDimmerMessage(string message, string address)
        {
            int level;
            switch (message)
            {
                case "Turn On":
                    level = 10;
                    break;
                case "Turn Off":
                    level = 0;
                    break;
                case "Begin Manual Brightening":
                    level = -1;
                    break;
                case "End Manual Brightening/Dimming":
                    level = insteonGetLightLevel(address);
                    break;
                default:
                    level = insteonGetLightLevel(address);
                    break;
            }
            return level;
        }

        public int insteonProcessSwitchMessage(string message, string address)
        {
            int level = -1;
            switch (message)
            {
                case "Turn On":
                    level = 10;
                    break;
                case "Turn Off":
                    level = 0;
                    break;
            }
            return level;
        }

        public int insteonProcessMotionSensorMessage(string message, string address)
        {
            if (debugInsteon)
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    Logging.writeLog("Insteon:  Process motion sensor from address '" + address + "' message '" + message + "'");
                }));
            }
            int state = -1;
            if (message == string.Empty)
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    Logging.writeLog("Process Motion - empty message");
                }));
                return state;
            }
            switch (message)
            {
                case string sub when sub.StartsWith("Turn On"):
                    formMain.BeginInvoke(new Action(() =>
                    {
                        Logging.writeLog("Process Motion - turn on");
                    }));
                    state = 1;
                    break;
                case string sub when sub.StartsWith ("Turn Off"):
                    formMain.BeginInvoke(new Action(() =>
                    {
                        Logging.writeLog("Process Motion - turn off");
                    }));
                    state = 0;
                    break;
            }
            return state;
        }

        private void timerPLMreceive_Tick(object sender, EventArgs e)
        {
            powerlineModem.Receive();
        }

        private void insteonConnectPLM()
        {
            if (Properties.Settings.Default.plmPort != string.Empty)
            {
                plmConnected = false;
                labelPLMstatus.Text = "Disconnected";
                Logging.writeLog("Insteon:  Connecting Serial Port " + Properties.Settings.Default.plmPort + " to PLM");
                labelPLMstatus.ForeColor = System.Drawing.Color.Maroon;

                powerlineModem = new Plm(Properties.Settings.Default.plmPort);
                powerlineModem.Network.StandardMessageReceived += Network_StandardMessageReceived;

                timerPLMreceive.Enabled = true;
                queueLightLevel(Properties.Settings.Default.potsAddress, 0);
                queueLightLevel(Properties.Settings.Default.trayAddress, 0);
                timerCheckPLM.Enabled = true;
            }
        }

        private void Network_StandardMessageReceived(object sender, StandardMessageReceivedArgs e)
        {
            string desc = e.Description;
            string address = e.PeerId.ToString();
            int level;

            if (debugInsteon)
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    Logging.writeLog("Insteon:  Debug - received from '" + address + "' message '" + desc + "'");
                }));
            }

            if (address == Properties.Settings.Default.trayAddress)
            {
                level = insteonProcessDimmerMessage(desc, address);
                if (level >= 0)
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        Logging.writeLog("Insteon:  Received Tray dimmer update from PLM - level '" + level.ToString() + "'");
                        formMain.trackBarTray.Value = level;
                    }));
                }
            }
            else if (address == Properties.Settings.Default.potsAddress)
            {
                level = insteonProcessDimmerMessage(desc, address);
                if (level >= 0)
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        Logging.writeLog("Insteon:  Received Pots dimmer update from PLM - level '" + level.ToString() + "'");
                        formMain.trackBarPots.Value = level;
                    }));
                }
            }
            else if (address == Properties.Settings.Default.motionSensorAddress)
            {
                int state = insteonProcessMotionSensorMessage(desc, address);
                if (state == 1) //Motion Detected
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.insteonDoMotion(true);
                    }));
                }
                else if (state == 0)  //No Motion Detected
                { 
                    formMain.BeginInvoke(new Action(() =>
                    {
                        if (formMain.labelMotionSensorStatus.Text != "No Motion")
                        {
                            Logging.writeLog("Insteon:  Motion Sensor reported 'No Motion Detected'");
                            formMain.progressBarInsteonMotionLatch.Value = formMain.progressBarInsteonMotionLatch.Maximum;
                            formMain.insteonMotionLatchExpires = DateTime.Now.AddMinutes(Properties.Settings.Default.insteonMotionLatch);
                            formMain.insteonMotionLatchActive = true;
                            formMain.labelMotionSensorStatus.Text = "No Motion";
                        }
                    }));
                }
            }
            else if (address == Properties.Settings.Default.doorSensorAddress)
            {
                int state = insteonProcessMotionSensorMessage(desc, address);

                if (state == -1)
                {
                    return;
                }
                else if (state == 1)  //Door Open Detected
                { 
                    formMain.BeginInvoke(new Action(() =>
                    {
                        
                        formMain.toolStripStatus.Text = "Door Opened";
                        if (areLightsOff())
                        {
                            Logging.writeLog("Insteon:  Door Opened - Turning On Lights");
                            formMain.insteonDoMotion(true);
                            lightsOn();
                        }
                        else
                        {
                            Logging.writeLog("Insteon:  Door Opened - Lights are already on - no change");
                        }
                    }));
                }
                else if (state == 0) //Door Closed
                { 
                    formMain.BeginInvoke(new Action(() =>
                    {
                        Logging.writeLog("Insteon:  Door Closed");
                        formMain.toolStripStatus.Text = "Door Closed";
                    }));
                }
            }
           
            else
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    Logging.writeLog("Insteon:  Received unknown device message from address '" + address + "' message '" + desc + "'");
                }));
            }
        }

        public int insteonGetLightLevel(string address)
        {
            int level = -1;
            DeviceBase device;
            if (address == string.Empty) return 0;
            if (powerlineModem.Network.TryConnectToDevice(address, out device))
            {
                var lightingControl = device as DimmableLightingControl;
                byte onLevel;

                lightingControl.TryGetOnLevel(out onLevel);

                int integerLevel = Convert.ToInt32(onLevel);
                float decLevel = (float)integerLevel / 254 * 10;

                level = (int)decLevel;
            }
            return level;
        }

        public int insteonGetSwitchStatus(string address)
        {
            DeviceBase device;
            if (address == string.Empty) return -1;
            if (powerlineModem.Network.TryConnectToDevice(address, out device))
            {
                var switchControl = device as SwitchedLightingControl;
                byte onLevel;

                switchControl.TryGetOnLevel(out onLevel);
                int integerLevel = Convert.ToInt32(onLevel);

                return integerLevel;
            }

            return -1;
        }

        public void insteonSetLightLevel(string address, int level)
        {
            DeviceBase device;
            if (address == string.Empty) return;
            if (powerlineModem != null && powerlineModem.Network.TryConnectToDevice(address, out device))
            {
                bool finished = false;
                int counter = 0;

                while (!finished && counter < 3)
                {
                    var lightingControl = device as DimmableLightingControl;
                    float theVal = (level * 254 / 10) + 1;
                    int toInt = (int)theVal;
                    finished = lightingControl.RampOn((byte)toInt);
                    if (!finished)
                    {
                        Logging.writeLog("Insteon:  Could not set light '" + address + "' to level '" + level.ToString() + "'");
                    }
                    else
                    {
                        lights[address] = -1;
                        Logging.writeLog("Insteon:  Set light '" + address + "' to level '" + level.ToString() + "'");
                        return;
                    }
                    counter++;
                }
            }
            toolStripStatus.Text = "Could not connect to light '" + address + "'";
            Logging.writeLog("Insteon:  Error setting light '" + address + "' to level '" + level.ToString() + "'");
        }

        public void insteonSetRelay(string address, bool turnOn)
        {
            DeviceBase device;
            if (address == string.Empty) return;
            if (powerlineModem != null && powerlineModem.Network.TryConnectToDevice(address, out device))
            {
                bool finished = false;
                int counter = 0;

                while (!finished && counter < 3)
                {
                    var relayControl = device as SwitchedLightingControl;
                    if (turnOn)
                    {
                        finished = relayControl.TurnOn();
                    }
                    else
                    {
                        finished = relayControl.TurnOff();
                    }
                    if (!finished)
                    {
                        Logging.writeLog("Insteon:  Could not set relay '" + address + "' to '" + turnOn.ToString() + "'");
                    }
                    else
                    {
                        Logging.writeLog("Insteon:  Set relay '" + address + "' to '" + turnOn.ToString() + "'");
                        return;
                    }
                    counter++;
                }
            }
            toolStripStatus.Text = "Could not connect to relay '" + address + "'";
            Logging.writeLog("Insteon:  Error setting relay '" + address + "' to '" + turnOn.ToString() + "'");
        }

        private void PowerlineModem_OnError(object sender, EventArgs e)
        {
            if (powerlineModem.Exception.GetType() == typeof(TimeoutException))
            {
                plmConnected = false;
                formMain.BeginInvoke(new Action(() =>
                {
                    labelPLMstatus.Text = "Disconnected";
                    Logging.writeLog("Insteon:  Error connecting to PLM");
                    labelPLMstatus.ForeColor = System.Drawing.Color.Maroon;
                    timerPLMreceive.Enabled = false;
                    timerCheckPLM.Enabled = true;
                }));
            }
        }

        private void timerCheckPLM_Tick(object sender, EventArgs e)
        {
            plmConnected = true;
            timerCheckPLM.Enabled = false;
            formMain.insteonPoll();
        }

        private void timerInsteonMotionLatch_Tick(object sender, EventArgs e)
        {
            if (insteonMotionLatchActive)
            {
                if (labelKodiPlaybackStatus.Text == "Stopped")
                {
                    DateTime rightNow = DateTime.Now;
                    if (insteonMotionLatchExpires < rightNow)
                    {
                        insteonMotionLatchActive = false;
                        Logging.writeLog("Insteon:  Latch timer expired - setting room vacant");
                        labelRoomOccupancy.Text = "Vacant";
                        labelMotionSensorStatus.Text = "No Motion";
                    }
                    else
                    {
                        float secondsDiff = (float)(insteonMotionLatchExpires - rightNow).TotalSeconds;
                        float totalSecs = Properties.Settings.Default.insteonMotionLatch * 60;
                        float percentage = (secondsDiff / totalSecs) * 100;
                        progressBarInsteonMotionLatch.Value = Convert.ToInt32(percentage);
                        return;
                    }
                }
            }
            progressBarInsteonMotionLatch.Value = progressBarInsteonMotionLatch.Minimum;
        }

        private void insteonDoMotion(bool explicitMotion = true)
        {
            if (!explicitMotion)
            {
                Logging.writeLog("Insteon:  Implied room occupancy detected");
            }
            else if (labelMotionSensorStatus.Text != "Motion Detected")
            {
                Logging.writeLog("Insteon:  Motion Sensor reported 'Motion Detected'");
                labelMotionSensorStatus.Text = "Motion Detected";
            }
            labelRoomOccupancy.Text = "Occupied";
            insteonMotionLatchActive = false;
        }

        private async void insteonPoll()
        {
            bool connected = false;
            int level;
            formMain.BeginInvoke(new Action(() =>
            {
                level = insteonGetLightLevel(Properties.Settings.Default.trayAddress);
                if (level >= 0)
                {
                    formMain.trackBarTray.Value = level;
                    connected = true;
                }
            }));
            await doDelay(1200);
            formMain.BeginInvoke(new Action(() =>
            {
                level = insteonGetLightLevel(Properties.Settings.Default.potsAddress);
                if (level >= 0)
                {
                    formMain.trackBarPots.Value = level;
                    connected = true;
                }
            }));
            if (connected && formMain.labelPLMstatus.Text != "Connected")
            {
                formMain.BeginInvoke(new Action(() =>
                {

                    formMain.labelPLMstatus.Text = "Connected";
                    Logging.writeLog("Insteon:  Connected to PLM");
                    formMain.labelPLMstatus.ForeColor = System.Drawing.Color.ForestGreen;

                }));
            }
        }

        private void timerInsteonPoll_Tick(object sender, EventArgs e)
        {
            if (labelPLMstatus.Text == "Connected")
            {
                toolStripStatus.Text = "Polling for Insteon status";
                insteonPoll();
            }
        }
    }
}
