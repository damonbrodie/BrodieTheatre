using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Collections.Generic;


/*
   Numbering for JSON Messages
  
   99 - GetPlayer
   98 - Show Image of behind projector
  
*/

namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        public static string kodiBehindScreen = @"smb://10.0.0.7/Pictures/ht_0.jpg";
        public static bool kodiIsConnected = false;
        public static bool connectKodiLocalhost = false;
        public static int currentKodiPort = 0;
        public static TcpClient tcpClient;
        public static NetworkStream kodiSocketStream;
        public static StreamReader kodiStreamReader;
        public static StreamWriter kodiStreamWriter;
        public static char[] kodiReadBuffer = new char[1000000];
        public static int kodiReadBufferPos = 0;

        private static void kodiConnect()
        {
            if (Properties.Settings.Default.kodiJSONPort != 0)
            {
                try
                {
                    kodiStatusDisconnect(false);
                    tcpClient = new TcpClient();
                    tcpClient.ReceiveTimeout = 500;
                    // Localhost is hardcoded below.  If we want to support Kodi on a regular IP, then we should read the 
                    // Kodi Localhost flag here.
                    var result = tcpClient.BeginConnect("127.0.0.1", Properties.Settings.Default.kodiJSONPort, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                    if (success)
                    {
                        kodiSocketStream = tcpClient.GetStream();
                        kodiStreamReader = new StreamReader(kodiSocketStream);
                        kodiStreamWriter = new StreamWriter(kodiSocketStream);
                        kodiSocketStream.Flush();
                        Thread thread = new Thread(kodiReadStream);
                        thread.Start();
                        if (!kodiIsConnected) // Only write the log if we were previously disconnected
                        {
                            Logging.writeLog("Kodi:  Connected to Kodi JSON port");
                        }
                        kodiIsConnected = true;
                        connectKodiLocalhost = Properties.Settings.Default.connectKodiLocalhost;
                        currentKodiPort = Properties.Settings.Default.kodiJSONPort;
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelKodiStatus.Text = "Connected";
                            formMain.labelKodiStatus.ForeColor = System.Drawing.Color.ForestGreen;
                            formMain.timerKodiConnect.Interval = 2000;
                        }));
                        return;
                    }
                }
                catch { }
            }
            kodiStatusDisconnect(); 
        }

        public static async void kodiReadStream()
        {
            char[] buffer = new char[100000];
            bool ended = false;
            bool gotSome = false;
            while (!ended)
            {
                try
                {
                    int bytesRead = await kodiStreamReader.ReadAsync(buffer, 0, 100000);
                    Array.Copy(buffer, 0, kodiReadBuffer, kodiReadBufferPos, bytesRead);
                    kodiReadBufferPos += bytesRead;
                    if (bytesRead > 0)
                    {
                        gotSome = true;
                    }
                    else
                    {
                        gotSome = false;
                    }
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.kodiFindJson();
                    }));
                }
                catch
                {
                    ended = true;
                }
                if (gotSome)  // After we read from the buffer, don't wait as long to check again. 
                {
                    await doDelay(100);
                }
                else
                {
                    await doDelay(1000);
                }
            }
            formMain.BeginInvoke(new Action(() =>
            {
                Logging.writeLog("Kodi:  Exiting JSON read thread");
            }));
        }

        public void kodiFindJson()
        {
            int braces = 0;
            bool inQ = false;
            char lastB = ' ';

            int curPos = 0;
            int startPos = 0;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            while (curPos < kodiReadBufferPos)
            {
                char b = kodiReadBuffer[curPos];
                curPos += 1;
                sb.Append(b);

                if (b == '"' && lastB != '\\')
                {
                    inQ = !inQ;
                }
                else if (b == '{' && !inQ)
                {
                    braces += 1;
                }
                else if (b == '}' && !inQ)
                {
                    braces -= 1;
                }
                lastB = (char)b;
                if (braces == 0)
                {
                    int newBufferLength = kodiReadBufferPos - curPos;
                    string currJson = sb.ToString();
                    sb = new System.Text.StringBuilder();
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.kodiProcessJson(currJson);
                    }));
                    startPos = curPos;
                }
            }
            if (braces > 0)
            {
                int newBufferLength = kodiReadBufferPos - startPos;
                Array.Copy(kodiReadBuffer, startPos, kodiReadBuffer, 0, newBufferLength);
                kodiReadBufferPos = newBufferLength;
            }
            else
            {
                kodiReadBufferPos = 0;
            }
        }

        public void kodiProcessJson(string jsonText)
        {      
            Dictionary<string, dynamic> result = null;
            if (debugKodi)
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    Logging.writeLog("Kodi:  Raw JSON:  " + jsonText);
                }));
            }
            try
            {
                result = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonText);
            }
            catch
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    Logging.writeLog("Kodi:  Unable to decode JSON:  " + jsonText);
                }));
                return;
            }
            if (result.ContainsKey("id") && result["id"] == "98")
            {
                // Return after showing transparent screen image.  Process no further.
            }
            else if (result.ContainsKey("id") && result["id"] == "99")
            {
                // Our submitted request for Player Get Properties.  Examples:
                // PLAYING {"id":"99","jsonrpc":"2.0","result":{"currentvideostream":{"codec":"vc1","height":1080,"index":0,"language":"","name":"FraMeSToR VC-1 Video","width":1920},"speed":1,"type":"video"}}
                // PAUSED  {"id":"99","jsonrpc":"2.0","result":{"currentvideostream":{"codec":"vc1","height":1080,"index":0,"language":"","name":"FraMeSToR VC-1 Video","width":1920},"speed":0,"type":"video"}}
                // STOPPED {"id":"99","jsonrpc":"2.0","result":{"currentvideostream":{"codec":"","height":0,"index":0,"language":"","name":"","width":0},"speed":0,"type":"video"}}
                try
                {
                    if (result["result"]["currentvideostream"]["codec"] == string.Empty && labelKodiPlaybackStatus.Text != "Stopped")
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            labelKodiPlaybackStatus.Text = "Stopped";
                            Logging.writeLog("Kodi:  Playback status incorrect - No players active");
                        }));
                    }
                    else if (result["result"]["currentvideostream"]["codec"] != string.Empty && result["result"]["speed"] != 0 && labelKodiPlaybackStatus.Text != "Playing")
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            labelKodiPlaybackStatus.Text = "Playing";
                            Logging.writeLog("Kodi:  Playback status incorrect - Player is running");
                        }));
                    }
                    else if (result["result"]["currentvideostream"]["codec"] != string.Empty && result["result"]["speed"] == 0 && labelKodiPlaybackStatus.Text != "Paused")
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            labelKodiPlaybackStatus.Text = "Paused";
                            Logging.writeLog("Kodi:  Playback status incorrect - Player is paused");
                        }));
                    }
                }
                catch
                {
                    Logging.writeLog("Kodi:  Error parsing Kodi JSON: '" + jsonText + "'");
                }
            }
            else if (result.ContainsKey("method"))
            {
                switch (result["method"])
                {
                    case "Player.OnPause":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            Logging.writeLog("Kodi:  Kodi status changed to 'Paused'");
                            insteonDoMotion(false);
                            labelKodiPlaybackStatus.Text = "Paused";
                            lightsToPausedLevel();
                        }));
                        break;
                    case "Player.OnPlay":
                    case "Player.OnResume":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            Logging.writeLog("Kodi:  Kodi status changed to 'Playing'");
                            insteonDoMotion(false);
                            labelKodiPlaybackStatus.Text = "Playing";
                            lightsToPlaybackLevel();
                        }));
                        break;
                    case "Player.OnStop":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            Logging.writeLog("Kodi:  Kodi status changed to 'Stopped'");
                            insteonDoMotion(false);
                            labelKodiPlaybackStatus.Text = "Stopped";
                            lightsToStoppedLevel();
                        }));
                        break;
                    case "System.OnQuit":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            Logging.writeLog("Kodi:  Kodi is exiting");
                            kodiStatusDisconnect();
                        }));
                        break;
                    // Example of Brodietheatre JSON {"jsonrpc":"2.0","method":"Other.aspectratio","params":{"data":"2.39","sender":"brodietheatre"}}
                    case "Other.aspectratio":
                        if (result["params"]["sender"] == "brodietheatre")
                        {
                            string kodiAspectRatio = result["params"]["data"];
                            float ar = 0;
                            try
                            {
                                ar = float.Parse(kodiAspectRatio);
                            }
                            catch (FormatException)
                            {
                                formMain.BeginInvoke(new Action(() =>
                                {
                                    Logging.writeLog("Kodi:  Invalid Aspect Ratio: '" + kodiAspectRatio + "'");
                                }));
                            }
   
                            formMain.BeginInvoke(new Action(() =>
                            {
                                projectorQueueChangeAspect(ar);
                                Logging.writeLog("Kodi:  Received Aspect Ratio: '" + kodiAspectRatio + "'");
                            }));

                        }
                        break;
                }
            }
            else
            {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        Logging.writeLog("Kodi:  Received unknown JSON:  " + jsonText);
                    }));
            }
        }

        public static void kodiSendJson(string command)
        {
            try
            {
                kodiStreamWriter.WriteLine(command);
                kodiStreamWriter.Flush();
            }
            catch (IOException)
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    formMain.timerKodiConnect.Enabled = true;
                }));
            }
            catch (NullReferenceException) { }
        }

        public static void kodiStatusDisconnect(bool enableTimer = true)
        {
            if (kodiIsConnected)
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    Logging.writeLog("Kodi:  Connection closed to Kodi JSON port");
                }));
            }
            kodiIsConnected = false;
            formMain.BeginInvoke(new Action(() =>
            {
                formMain.labelKodiStatus.Text = "Disconnected";
                formMain.labelKodiStatus.ForeColor = System.Drawing.Color.Maroon;
                formMain.timerKodiConnect.Enabled = enableTimer;
            }));
        }

        private void kodiPlaybackControl(string command, string media=null)
        {
            // It seems the Active Player is always "1".  Use this if we need to query it.
            //  "{\"jsonrpc\": \"2.0\", \"method\": \"Player.GetActivePlayers\", \"id\": \"1\"}"
            switch (command)
            {
                case "Pause":
                    kodiSendJson("{\"jsonrpc\": \"2.0\", \"method\": \"Player.PlayPause\", \"params\": { \"playerid\" : 1 }, \"id\": \"1\"}");
                    break;
                case "Play":
                    kodiSendJson("{\"jsonrpc\": \"2.0\", \"method\": \"Player.PlayPause\", \"params\": { \"playerid\" : 1 }, \"id\": \"1\"}");
                    break;
                case "Stop":
                    kodiSendJson("{\"jsonrpc\": \"2.0\", \"method\": \"Player.Stop\", \"params\": { \"playerid\" : 1 }, \"id\": \"1\"}");
                    break;
            }
        }

        public static void kodiShowBehindScreen()
        {
            kodiSendJson("{\"jsonrpc\": \"2.0\", \"method\": \"Player.Open\", \"params\": { \"item\": {\"file\": \"" + kodiBehindScreen + "\" }}, \"id\": \"98\"}");
            Logging.writeLog("Kodi:  Showing behind the screen picture");
        }

        private void timerKodiConnect_Tick(object sender, EventArgs e)
        {
            kodiConnect();
        }

        private void timerKodiPoll_Tick(object sender, EventArgs e)
        {
            // Periodically poll Kodi and retrieve the Player properties.  Use this to keep consistency
            // in our view of the current Kodi status

            if (labelKodiStatus.Text == "Connected")
            {
                kodiSendJson("{\"jsonrpc\": \"2.0\", \"method\": \"Player.GetProperties\", \"params\": {\"playerid\": 1, \"properties\" : [\"type\", \"currentvideostream\", \"speed\"]}, \"id\": \"99\"}");
            }
        }
    }
}
