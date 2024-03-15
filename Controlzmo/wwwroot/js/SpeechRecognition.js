// https://developer.mozilla.org/en-US/docs/Web/API/SpeechRecognitionResult
// Turn on both things in FireFox which have webspeech.recognition in their name

var recognition = new webkitSpeechRecognition();

recognition.continuous = true;
recognition.interimResults = false;

recognition.lang = "en-GB";

recognition.onresult = function (e) {
    speak("Heard " + e.results[0][0].transcript);
};

recognition.onerror = function (e) {
    recognition.stop();
};
