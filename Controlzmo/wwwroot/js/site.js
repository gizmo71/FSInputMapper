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
        // Does nothing in Firefox: this.setAttribute('pattern', '1[13]\d\.\d(\d[05]?)?');
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

connection.start().then(function () {
    // Called when connection established - may want to disable things until this is received.
    connection.invoke("SendAll").catch(errorHandler);
    $(".sendBoolean").on("change", function (event) {
        connection.invoke("SetInSim", event.target.id, event.target.checked).catch(errorHandler);
        event.preventDefault();
    });
    $(".sendText").on("change", function (event) {
        if (event.target.reportValidity()) {
            connection.invoke("SetInSim", event.target.id, event.target.value).catch(errorHandler);
        }
        event.preventDefault();
    });
    $(".sendButton").on("click", function (event) {
        connection.invoke("SetInSim", event.target.id, event.target.value).catch(errorHandler);
        event.preventDefault();
    });
    /*$("#frottle").on("keypress", function (event) {
        if (event.which == 13) {
            connection.invoke("SetFrottle", Number.parseInt(event.target.value)).catch(errorHandler);
            event.preventDefault();
        }
    });*/
}).catch(errorHandler);
