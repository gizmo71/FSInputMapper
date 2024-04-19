"use strict";

$(function () {
    // Only do this if mobile?
    if ($(window).width() < 1500) $("#tabs").tabs();
});

var noSleep = new NoSleep();
document.addEventListener('click', function enableNoSleep() {
    document.removeEventListener('click', enableNoSleep, false);
    noSleep.enable();
}, false);

class ComFrequency extends HTMLInputElement {
    constructor() {
        super();
        this.setAttribute('type', 'number');
        this.setAttribute('size', '7');
        this.setAttribute('min', '118.0');
        this.setAttribute('max', '136.975');
        this.setAttribute('step', '0.005');
        // 00, 05, 10, 15, 25, 30, 35, 40, 50, 55, 60, 65, 75, 80, 85, 90
        // Does nothing in Firefox: this.setAttribute('pattern', '1[123]\d\.\d(\d[05]?)?');
    }
}
customElements.define('com-frequency', ComFrequency, { extends: "input" });

function errorHandler(err) {
    return console.error(err.toString());
}

var connection = new signalR.HubConnectionBuilder().withUrl("/hub/connectzmo").withAutomaticReconnect().build();

connection.on("SetFromSim", function (name, newValue) {
    var jqInput = $("#" + name).add($('input[type="radio"][name="' + name + '"]'));
    if (newValue != null) {
        var type = jqInput.prop("type");
        if (type == 'checkbox') {
            jqInput.prop('checked', newValue);
        } else if (type == 'radio') {
            jqInput = jqInput.filter(function () { return this.value == newValue; });
            jqInput.prop('checked', true);
        } else {
            jqInput.prop('value', newValue);
        }
    }
    jqInput.prop('disabled', newValue == null);
});

connection.on("SetMcduType", function (type) {
    $('#tabs-mcdu').attr('src', 'http://controlzmo.aquarium.davegymer.org:' + (type == 'fenix' ? '8083/#/mcdu' : '8380/interfaces/mcdu/'));
});

function speak(text) {
    // https://developer.mozilla.org/en-US/docs/Web/API/SpeechSynthesis
    var utterance = new SpeechSynthesisUtterance(text);
    //utterance.pitch = 1; // 0 to 2, with 1 as default
    utterance.rate = 1.25; // 0.1 to 10, with 1 as default
    window.speechSynthesis.speak(utterance);
}

connection.on("Speak", speak);

function linkSendStyles() {
    $(".sendBoolean").on("change", function (event) {
        connection.invoke("SetInSim", event.target.id, event.target.checked).catch(errorHandler);
        event.preventDefault();
    });
    $(".sendRadio").on("change", function (event) {
        connection.invoke("SetInSim", event.target.name, event.target.value).catch(errorHandler);
        event.preventDefault();
    });
    $(".sendText").on("change", function (event) {
        if (event.target.reportValidity()) {
            connection.invoke("SetInSim", event.target.id, event.target.value).catch(errorHandler);
        }
        event.preventDefault();
        $(event.target).blur();
        window.scrollTo(0, 0);
    });
    $(".sendButton").on("click", function (event) {
        connection.invoke("SetInSim", event.target.id, event.target.value).catch(errorHandler);
        event.preventDefault();
    });
    linkSendStyles = function () { }; // Don't do it again, even if reconnected.
}

function startSignalR() {
    connection.start().then(function () {
        // Called when connection established - may want to disable things until this is received.
        connection.invoke("SendAll");
        linkSendStyles();
    }).catch(errorHandler);
}
function reconnect() {
    connection.stop().then(startSignalR, startSignalR);
}

startSignalR();

function testCallout() {
    connection.invoke("SetInSim", 'resetMcdu', true);
    recognition.start();
    speak('Monitoring');
}
