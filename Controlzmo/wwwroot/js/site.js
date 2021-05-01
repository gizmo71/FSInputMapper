"use strict";

$(function () {
    $("#tabs").tabs();
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

var connection = new signalR.HubConnectionBuilder().withUrl("/hub/connectzmo").build();

connection.on("SetFromSim", function (name, value) {
    var jqInput = $("#" + name);
    if (value != null) {
        if (jqInput.prop("type") == 'checkbox')
            jqInput.prop('checked', value);
        else
            jqInput.prop('value', value);
    }
    jqInput.prop('disabled', value == null);
});

connection.on("Speak", function (text) {
    // https://developer.mozilla.org/en-US/docs/Web/API/SpeechSynthesis
    var utterance = new SpeechSynthesisUtterance(text);
    //utterance.pitch = 1; // 0 to 2, with 1 as default
    utterance.rate = 1.25; // 0.1 to 10, with 1 as default
    window.speechSynthesis.speak(utterance);
});

connection.start().then(function () {
    // Called when connection established - may want to disable things until this is received.
    connection.invoke("SendAll").catch(errorHandler);
    $(".sendBoolean").on("change", function(event) {
        connection.invoke("SetInSim", event.target.id, event.target.checked).catch(errorHandler);
        event.preventDefault();
    });
    $(".sendText, .clickText").on("change", function(event) {
        if (event.target.reportValidity()) {
            connection.invoke("SetInSim", event.target.id, event.target.value).catch(errorHandler);
        }
        event.preventDefault();
    });
    $(".clickText").on("click", function(event) {
        var label = $("label[for='" + event.target.id + "']").text();
        var newValue = window.prompt(label, event.target.value);
        if (newValue && event.target.value != newValue) {
            event.target.value = newValue;
            $(event.target).trigger('change');
        }
        $(event.target).blur();
        event.preventDefault();
    });
    $(".sendButton").on("click", function(event) {
        connection.invoke("SetInSim", event.target.id, event.target.value).catch(errorHandler);
        event.preventDefault();
    });
}).catch(errorHandler);
