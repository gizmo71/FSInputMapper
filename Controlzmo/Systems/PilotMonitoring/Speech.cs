using System.Speech.Synthesis;

namespace Controlzmo.Systems.PilotMonitoring
{
    [Component]
    public class Speech
    {
        private readonly SpeechSynthesizer synth = new SpeechSynthesizer();

        public Speech() { synth.SetOutputToDefaultAudioDevice(); }

        public void Say(string words) => synth.SpeakAsync(words);

        //TODO: also support SSML?
        // https://www.w3.org/TR/speech-synthesis/
        // Would this allow some of the more awkward things to sound more natural?
    }
}
