// https://developer.mozilla.org/en-US/docs/Web/API/SpeechSynthesis
var synth = window.speechSynthesis;
var utterance = new SpeechSynthesisUtterance('V1');
utterance.pitch = 1; // 0 to 2, with 1 as default
utterance.rate = 1.5; // 0.1 to 10, with 1 as default
synth.speak(utterance);
