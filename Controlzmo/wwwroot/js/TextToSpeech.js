// https://developer.mozilla.org/en-US/docs/Web/API/SpeechSynthesis
var synth = window.speechSynthesis;

function testTextToSpeech() {
    var utterance = new SpeechSynthesisUtterance('eighty knots V1');
    //utterance.pitch = 1; // 0 to 2, with 1 as default
    utterance.rate = 1.25; // 0.1 to 10, with 1 as default
    synth.speak(utterance);

    utterance = new SpeechSynthesisUtterance('Rotate');
    utterance.rate = 1.25; // 0.1 to 10, with 1 as default
    synth.speak(utterance);
}

/*
If this works on the phone, consider getting L:AIRLINER_V1_SPEED and L:AIRLINER_VR_SPEED and calling them.
Maybe 80 knots too, all using "AIRSPEED INDICATED" and something to detect takeoff mode.
Would have to have a WASM module to send client events we could RX.
On landing, would be good to detect spoilers popping, reversers, and decel light.
https://github.com/flybywiresim/a32nx/blob/autopilot/docs/a320-simvars.md
https://github.com/flybywiresim/a32nx/blob/autopilot/docs/a320-events.md
*/