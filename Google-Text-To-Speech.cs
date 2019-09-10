using System;
using System.IO;
using System.Windows.Forms;
using Google.Cloud.TextToSpeech.V1;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        public string text_to_mp3(string text, Grpc.Core.Channel channel)
        {
            TextToSpeechClient client = TextToSpeechClient.Create(channel);

            var input = new SynthesisInput
            {
                Text = text

            };

            var voiceSelection = new VoiceSelectionParams
            {
                LanguageCode = Properties.Settings.Default.textToSpeechLanguage,
                SsmlGender = SsmlVoiceGender.Female

            };
            var audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };

            var response = client.SynthesizeSpeech(input, voiceSelection, audioConfig);

            string filename = Guid.NewGuid().ToString() + ".mp3";
            Logging.writeLog("Google:  Text-To-Speech for '" + text + "' at:  " + filename);

            MemoryStream newTextToSpeech = new MemoryStream();
            response.AudioContent.WriteTo(newTextToSpeech);

            textToSpeechFiles[filename] = newTextToSpeech;

            return filename;
        }
    }
}
