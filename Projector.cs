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
            public float newAspect = 0;
            public bool force = false;
            public string powerCommand = null;
        }

        public ProjectorLensChange projectorCommand = new ProjectorLensChange();

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
                            Logging.writeLog("Projector:  Received 'Power is on' from Projector");
                            formMain.labelProjectorPower.Text = "On";
                            formMain.buttonProjectorPower.Text = "Power Off";
                            labelProjectorStatus.Text = "Connected";
                            labelProjectorStatus.ForeColor = System.Drawing.Color.ForestGreen;
                            currentProjectorPort = Properties.Settings.Default.projectorPort;
                        }));
                    }
                    //  Projector is in Power off State
                    else if (response.Contains("000"))
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            Logging.writeLog("Projector:  Received 'Power is off' from Projector");
                            formMain.labelProjectorPower.Text = "Off";
                            formMain.buttonProjectorPower.Text = "Power On";
                            labelProjectorStatus.Text = "Connected";
                            labelProjectorStatus.ForeColor = System.Drawing.Color.ForestGreen;
                            currentProjectorPort = Properties.Settings.Default.projectorPort;
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

        private void buttonProjectorChangeAspect_Click(object sender, EventArgs e)
        {
            if (buttonProjectorChangeAspect.Text == "Narrow Aspect")
            {
                projectorQueueChangeAspect((float)1.0); //Narrow
            }
            else
            {
                projectorQueueChangeAspect((float)2.0); //Wide
            }
        }

        private void timerCheckProjector_Tick(object sender, EventArgs e)
        {
            if (labelProjectorStatus.Text == "Connected")
            {
                projectorCheckPower();
            }
        }

        private void projectorChangeAspect(float aspect, bool force=false)
        {
            if (aspect <= 0)
            {
                return;
            }
            timerProjectorControl.Enabled = true;
            buttonProjectorChangeAspect.Enabled = false;
            List<string> pj_codes = new List<string> {
                "VXX:LMLI0=+00000",
                "VXX:LMLI0=+00001",
                "VXX:LMLI0=+00002",
                "VXX:LMLI0=+00003",
                "VXX:LMLI0=+00004",
                "VXX:LMLI0=+00005"
            };
            projectorLastCommand = "Lens";
            if (aspect < 1.9 && (force || labelProjectorLensAspect.Text != "Narrow"))
            {
                projectorSendCommand("Change to narrow aspect", pj_codes[0]);
                labelProjectorLensAspect.Text = "Narrow";
                Logging.writeLog("Projector:  Changing lens aspect ratio to 'narrow'");
            }
            else if (aspect >= 1.9 && (force || labelProjectorLensAspect.Text != "Wide"))
            {
                projectorSendCommand("Change to wide aspect", pj_codes[1]);
                labelProjectorLensAspect.Text = "Wide";
                Logging.writeLog("Projector:  Changing lens aspect ratio to 'wide'");
            }
            projectorCommand.force = false;
        }

        // "force" is used to request a change even in the app believes the lens is already
        // at that setting.  Used at startup time to ensure the lens isn't in the wrong state
        // from the previous projector shutdown
        private void projectorQueueChangeAspect(float aspect, bool force=false)
        {
            if (labelProjectorPower.Text == "On")
            {
                if (timerProjectorControl.Enabled == true)
                {
                    // Wait for the last Aspect change to finish
                    Logging.writeLog("Projector:  Queueing Aspect Ratio change - " + aspect.ToString());
                    projectorCommand.newAspect = aspect;
                    projectorCommand.force = force;
                }
                else
                {
                    projectorChangeAspect(aspect, force);
                }
            }
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
            else if (projectorCommand.newAspect > 0)
            {
                // A queued projector lens aspect ratio change is waiting
                projectorChangeAspect(projectorCommand.newAspect, projectorCommand.force);
                projectorCommand.newAspect = 0;
                projectorCommand.force = false;
                buttonProjectorChangeAspect.Enabled = true;
            }
            if (projectorCommand.powerCommand == null && projectorCommand.newAspect == 0)
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
            projectorCommand.newAspect = 1;
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

        private void labelLensAspect_TextChanged(object sender, EventArgs e)
        {
            if (labelProjectorLensAspect.Text == "Narrow")
            {
                buttonProjectorChangeAspect.Text = "Wide Aspect";
            }
            else
            {
                buttonProjectorChangeAspect.Text = "Narrow Aspect";
            }
        }
        private void labelProjectorStatus_TextChanged(object sender, EventArgs e)
        {
            if (labelProjectorStatus.Text == "Disconnected")
            {
                buttonProjectorChangeAspect.Enabled = false;
                buttonProjectorPower.Enabled = false;
            }
            else if (labelProjectorStatus.Text == "Connected")
            {
                buttonProjectorChangeAspect.Enabled = true;
                buttonProjectorPower.Enabled = true;
            }
        }
    }
}
