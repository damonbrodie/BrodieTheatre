using System;
using System.Windows.Forms;
using System.Collections.Generic;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {

        public string projectorLastCommand;

        public string currentProjectorPort;
        public class ProjectorLensChange
        {
            public int newLensMemory = 0;
            public bool force = false;
            public string powerCommand = null;
        }

        public ProjectorLensChange projectorCommand = new ProjectorLensChange();

        public List<string> panasonic_pj_codes = new List<string> 
        {
            "VXX:LMLI0=+00000",
            "VXX:LMLI0=+00001",
            "VXX:LMLI0=+00002",
            "VXX:LMLI0=+00003",
            "VXX:LMLI0=+00004",
            "VXX:LMLI0=+00005"
        };

        private void projectorConnect()
        {
            try
            {
                // If the port is open close it
                projectorDisconnect();

                serialPortProjector.PortName = Properties.Settings.Default.projectorPort;
                if (!serialPortProjector.IsOpen)
                {
                    serialPortProjector.Open();
                    formMain.BeginInvoke(new Action(() =>
                    {
                        Logging.writeLog("Projector:  Connecting Serial Port " + Properties.Settings.Default.projectorPort + " to Projector");
                    }));
                }
                serialPortProjector.DataReceived += SerialPortProjector_DataReceived;
                projectorCheckPower();
            }
            catch
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    Logging.writeLog("Projector:  Unable to connect to Projector");
                }));
                toolStripStatus.Text = "Could not open projector serial port";
                projectorDisconnect();
            }
        }

        private void projectorDisconnect()
        {
            if (serialPortProjector.IsOpen)
            {
                serialPortProjector.DataReceived -= SerialPortProjector_DataReceived;
                serialPortProjector.Close();
                Logging.writeLog("Projector:  Closing connection to Projector");
            }
            labelProjectorStatus.Text = "Disconnected";
            labelProjectorStatus.ForeColor = System.Drawing.Color.Maroon;
        }

        private void projectorCheckPower()
        {
            projectorLastCommand = "Power";
            if (debugProjector)
            {
                Logging.writeLog("Projector:  Sending Check Power query to projector");
            }
            projectorSendCommand("", "QPW");
        }

        private void projectorSendCommand(string logMessage, string command)
        {
            int startByte = 2;
            int endByte = 3;

            char start = (char)startByte;
            char end = (char)endByte;

            string full_command = start + command + end;
            if (serialPortProjector.IsOpen)
            {
                if (logMessage != "" && debugProjector)
                {
                    Logging.writeLog("Projector:  " + logMessage);
                }
                serialPortProjector.Write(full_command);
            }
        }

        private void SerialPortProjector_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            string response = serialPortProjector.ReadExisting();
            if (debugProjector)
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    Logging.writeLog("Projector:  Received from Projector - " + response);
                }));
            }
            switch (projectorLastCommand)
            {
                case "Power":
                    // Projector is in Power On State
                    if (response.Contains("001"))
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            if (debugProjector)
                           { 
                                Logging.writeLog("Projector:  Received 'Power is on' from Projector");
                            }
                            formMain.labelProjectorPower.Text = "On";
                            formMain.buttonProjectorPower.Text = "Power Off";
                            if (labelProjectorStatus.Text != "Connected")
                            {
                                labelProjectorStatus.Text = "Connected";
                                labelProjectorStatus.ForeColor = System.Drawing.Color.ForestGreen;
                                currentProjectorPort = Properties.Settings.Default.projectorPort;
                                Logging.writeLog("Projector:  Connected to Projector");
                            }
                        }));
                    }
                    //  Projector is in Power off State
                    else if (response.Contains("000"))
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            if (debugProjector)
                            {
                                Logging.writeLog("Projector:  Received 'Power is off' from Projector");
                            }
                            formMain.labelProjectorPower.Text = "Off";
                            formMain.buttonProjectorPower.Text = "Power On";
                            if (labelProjectorStatus.Text != "Connected")
                            {
                                labelProjectorStatus.Text = "Connected";
                                labelProjectorStatus.ForeColor = System.Drawing.Color.ForestGreen;
                                currentProjectorPort = Properties.Settings.Default.projectorPort;
                                Logging.writeLog("Projector:  Connected to Projector");
                            }
                        }));
                    }
                    break;
                case "Lens":
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.toolStripStatus.Text = "Lens change - received: " + response;
                        Logging.writeLog("Projector:  Received lens change response '" + response + "'");
                    }));
                    break;
            }
        }

        private void buttonProjectorPower_Click(object sender, EventArgs e)
        {
            if (buttonProjectorPower.Text == "Power On")
            {
                projectorPowerOn();
            }
            else
            {
                projectorPowerOff();
            }
        }

        private void timerCheckProjector_Tick(object sender, EventArgs e)
        {
            if (labelProjectorStatus.Text == "Connected")
            {
                projectorCheckPower();
            }
        }

        private void projectorChangeAspect(int index, bool force=false)
        {

            timerProjectorControl.Enabled = true;
            comboBoxProjectorLensMemory.SelectedIndex = index;
            projectorLastCommand = "Lens";
            projectorSendCommand("Change zoom to: " + panasonic_pj_labels[index], panasonic_pj_codes[index]);
            projectorCommand.force = false;
        }

        // "force" is used to request a change even in the app believes the lens is already
        // at that setting.  Used at startup time to ensure the lens isn't in the wrong state
        // from the previous projector shutdown
        private void projectorQueueChangeAspect(int index, bool force=false)
        {
            if (labelProjectorPower.Text == "On")
            { 
                if (timerProjectorControl.Enabled == true)
                {
                    // Wait for the last Aspect change to finish
                    Logging.writeLog("Projector:  Queueing Lens Memory change - " + panasonic_pj_labels[index]);
                    projectorCommand.newLensMemory = index;
                    projectorCommand.force = force;
                }
                else  // Nothing is queued - go ahead and change it now.
                {
                    projectorChangeAspect(index, force);
                }                
            }
        }

        private int projectorGetLetMemoryFromAR(decimal decimalAspect)
        {
            int index = -1;
            int compareLow = Decimal.Compare(decimalAspect, 1.0m);
            int compareHigh = Decimal.Compare(decimalAspect, Properties.Settings.Default.projectorARRangeHigh1);
            if (compareLow >= 0 && compareHigh <= 0)
            {
                index = 0;
            }
            compareLow = Decimal.Compare(decimalAspect, Properties.Settings.Default.projectorARRangeHigh1);
            compareHigh = Decimal.Compare(decimalAspect, Properties.Settings.Default.projectorARRangeHigh2);
            if (compareLow > 0 && compareHigh <= 0)
            {
                index = 1;

            }
            compareLow = Decimal.Compare(decimalAspect, Properties.Settings.Default.projectorARRangeHigh2);
            compareHigh = Decimal.Compare(decimalAspect, Properties.Settings.Default.projectorARRangeHigh3);
            if (compareLow > 0 && compareHigh <= 0)
            {
                index = 2;
            }
            compareLow = Decimal.Compare(decimalAspect, Properties.Settings.Default.projectorARRangeHigh3);
            compareHigh = Decimal.Compare(decimalAspect, Properties.Settings.Default.projectorARRangeHigh4);
            if (compareLow > 0 && compareHigh <= 0)
            {
                index = 3;
            }
            compareLow = Decimal.Compare(decimalAspect, Properties.Settings.Default.projectorARRangeHigh4);
            compareHigh = Decimal.Compare(decimalAspect, Properties.Settings.Default.projectorARRangeHigh5);
            if (compareLow > 0 && compareHigh <= 0)
            {
                index = 4;
            }
            if (index < 0)
            {
                Logging.writeLog("Projector:  Error - index out of range in projectorChangeAspect");
            }
            Logging.writeLog("Projector:  Aspect Ratio " + decimalAspect.ToString() + " matches to index: " + index.ToString());
            return index;
            
        }

        private void timerProjectorControl_Tick(object sender, EventArgs e)
        {       
            if (projectorCommand.powerCommand != null)
            {
                if (projectorCommand.powerCommand == "001")
                {
                    projectorSendCommand("Power On", "PON");                   
                }
                else if (projectorCommand.powerCommand == "000")
                {
                    projectorSendCommand("Power Off", "POF");
                }
                projectorCommand.powerCommand = null;
            }
            else if (projectorCommand.newLensMemory >= 0)
            {
                // A queued projector lens aspect ratio change is waiting
                projectorChangeAspect(projectorCommand.newLensMemory, projectorCommand.force);
                projectorCommand.newLensMemory = -1;
                projectorCommand.force = false;
                comboBoxProjectorLensMemory.Enabled = true;
            }
            if (projectorCommand.powerCommand == null && projectorCommand.newLensMemory < 0)
            {
                timerProjectorControl.Enabled = false;
            }
        }

        private void projectorPowerOn()
        {
            if (labelProjectorPower.Text == "On" || labelProjectorPower.Text == "Powering On")
            {
                return;
            }
            
            labelProjectorPower.Text = "Powering On";
            // Set the Projector to the current AR in the UI to ensure we are in sync.
            projectorCommand.newLensMemory = 0;
            projectorCommand.force = true;
            if (timerProjectorControl.Enabled == true)
            {
                Logging.writeLog("Projector:  Queueing Powering On");
                projectorCommand.powerCommand = "001";
            }
            else
            {
                projectorSendCommand("Power On", "PON");     
                timerProjectorControl.Enabled = true;
            }
        }

        private void projectorPowerOff()
        {
            if (labelProjectorPower.Text == "Off" || labelProjectorPower.Text == "Powering Off")
            { 
                return;
            }
            labelProjectorPower.Text = "Powering Off";
            
            if (timerProjectorControl.Enabled == true)
            {
                Logging.writeLog("Projector:  Queueing Powering Off");
                projectorCommand.powerCommand = "000";
            }
            else
            {
                projectorSendCommand("Power Off", "POF");
                timerProjectorControl.Enabled = true;
            }           
        }

        private void comboBoxProjectorLensMemory_SelectedIndexChanged(object sender, EventArgs e)
        {
            projectorQueueChangeAspect(comboBoxProjectorLensMemory.SelectedIndex);
        }

        private void labelProjectorStatus_TextChanged(object sender, EventArgs e)
        {
            if (labelProjectorStatus.Text == "Disconnected")
            {
                comboBoxProjectorLensMemory.Enabled = false;
                buttonProjectorPower.Enabled = false;
            }
            else if (labelProjectorStatus.Text == "Connected")
            {
                if (labelProjectorStatus.Text == "On")
                {
                    comboBoxProjectorLensMemory.Enabled = true;
                }
                else
                {
                    comboBoxProjectorLensMemory.Enabled = false;
                }
                buttonProjectorPower.Enabled = true;
            }
        }
        private void LabelProjectorPower_TextChanged(object sender, EventArgs e)
        {
            if (labelProjectorPower.Text == "On")
            {
                comboBoxProjectorLensMemory.Enabled = true;
            }
            else
            {
                comboBoxProjectorLensMemory.Enabled = false;
            }
        }
    }
}
