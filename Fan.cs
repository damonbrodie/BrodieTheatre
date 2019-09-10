using System;
using System.Windows.Forms;

namespace BrodieTheatre
{  
    public partial class FormMain : Form
    {
        public DateTime fanDelayShutoffTime;

        private void updateFanStatus(bool isPowered = false)
        {
            if (! isPowered)
            {
                formMain.labelFanStatus.Text = "Powered Off";
                formMain.buttonFanPower.Text = "Power On";
            }
            else
            {
                formMain.labelFanStatus.Text = "Powered On";
                formMain.buttonFanPower.Text = "Power Off";
            }
        }

        private void timerExhaustFanDelay_Tick(object sender, EventArgs e)
        {
            if (HarmonyIsActivityStarted())
            {
                DateTime now = DateTime.Now;
                DateTime exhaustDelayStart = fanDelayShutoffTime.AddMinutes(Properties.Settings.Default.fanDelayOff * -1);
                var totalSeconds = (fanDelayShutoffTime - exhaustDelayStart).TotalSeconds;
                var progress = (now - exhaustDelayStart).TotalSeconds;
                int percentage = Math.Abs(100 - (Convert.ToInt32((progress / totalSeconds) * 100) + 1));
                formMain.progressBarExhaustFan.Value = percentage;
    
                if (fanDelayShutoffTime < now)
                {
                    formMain.progressBarExhaustFan.Value = progressBarExhaustFan.Minimum;
                    formMain.timerExhaustFanDelay.Enabled = false;
                    fanPowerOff();
                }
            }
            else
            {
                // While the AV gear is on, don't count down.
                formMain.progressBarExhaustFan.Value = progressBarExhaustFan.Minimum;
            }
        }

        private void fanDelayPowerOff()
        {
            if (Properties.Settings.Default.fanDelayOff == 0)
            {
                fanPowerOff();
            }
            else
            {
                fanDelayShutoffTime = DateTime.Now.AddMinutes(Properties.Settings.Default.fanDelayOff);
                formMain.progressBarExhaustFan.Value = formMain.progressBarExhaustFan.Maximum;
                formMain.timerExhaustFanDelay.Enabled = true;
                Logging.writeLog("Fan:  Queueing Exhaust Fan power off");
            }
        }

        private void fanPowerOn()
        {
            insteonSetRelay(Properties.Settings.Default.fanAddress, true);
            Logging.writeLog("Fan:  Powering on Exhaust Fan");
            updateFanStatus(true);
        }

        private void fanPowerOff()
        {
            insteonSetRelay(Properties.Settings.Default.fanAddress, false);
            formMain.timerExhaustFanDelay.Enabled = false;
            formMain.progressBarExhaustFan.Value = formMain.progressBarExhaustFan.Minimum;
            Logging.writeLog("Fan:  Powering off Exhaust Fan");

            updateFanStatus(false);
        }

        private void buttonFanPower_Click(object sender, EventArgs e)
        {
            if (formMain.buttonFanPower.Text == "Power On")
            {
                fanPowerOn();
            }
            else
            {
                fanPowerOff();
            }
        }
    }
}
