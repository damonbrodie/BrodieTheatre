using System;
using System.Windows.Forms;
using GoogleCast;
using GoogleCast.Channels;
using GoogleCast.Models.Media;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        private async void ConnectReceiver()
        {
            var receivers = await new DeviceLocator().FindReceiversAsync();

            foreach (var receiver in receivers)
            {
                if (receiver.Id == Properties.Settings.Default.SmartSpeaker)
                {
                    googleHomeReceiver = receiver;
                }
            }
        }

        private async void Announce(string text)
        {
            string speechAudioFile = text_to_mp3(text, googleCloudChannel);
            var castSender = new Sender();

            await castSender.ConnectAsync(googleHomeReceiver);
            var mediaChannel = castSender.GetChannel<IMediaChannel>();
            await castSender.LaunchAsync(mediaChannel);
            string url = "http://" + localIP +  ":" + Properties.Settings.Default.webServerPort + "/cast/" + speechAudioFile;
            Logging.writeLog("Google Cast:  Serving announcement '" + text + "' at: " + url);

            try
            {
                var mediaStatus = await mediaChannel.LoadAsync(new MediaInformation() { ContentId = url });
            }
            catch (Exception ex)
            {
                Logging.writeLog("Google Cast:  Timeout casting: " + ex);
            }
        }
    }
}
