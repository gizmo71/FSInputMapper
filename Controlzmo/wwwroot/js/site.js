"use strict";

var noSleep = new NoSleep();
document.addEventListener('click', function enableNoSleep() {
    document.removeEventListener('click', enableNoSleep, false);
    noSleep.enable();
}, false);

function errorHandler(err) {
    return console.error(err.toString());
}

var connection = new signalR.HubConnectionBuilder().withUrl("/hub/connectzmo").build();

connection.on("ShowMessage", function (message) {
    var messagesList = $("#messagesList");
    messagesList.children("li").slice(0, 1 - 5).remove();
    var timestamp = new Date().toLocaleTimeString([], { timeStyle: 'medium' });
    messagesList.append($("<li/>", { text: timestamp + ": " + message }))
});

connection.on("SetFromSim", function (name, value) {
    var jqInput = $("#" + name);
    if (value != null) {
        if (jqInput.type == 'checkbox')
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
