"use strict";

function errorHandler(err) {
    return console.error(err.toString());
}

var connection = new signalR.HubConnectionBuilder().withUrl("/hub/light").build();

function addMessage(message) {
    var messagesList = $("#messagesList");
    messagesList.children("li").slice(0, 1 - 5).remove();
    var timestamp = new Date().toLocaleTimeString([], { timeStyle: 'medium' });
    messagesList.append($("<li/>", { text: timestamp + ": " + message }))
}

connection.on("ShowMessage", addMessage);
connection.on("SetLandingLights", function (value) {
    $("#lightLanding").prop('checked', value);
});
connection.on("SetStrobeLights", function (value) {
    if (value != null)
        $("#lightStrobes").prop('checked', value);
    $("#lightStrobes").prop('disabled', value == null);
});

connection.start().then(function () {
    // Called when connection established - may want to disable things until this is received
    connection.invoke("SendAll").catch(errorHandler);
    $(".sendSomet").on("change", function (event) {
        connection.invoke("ChangedSomet", event.target.id, event.target.checked).catch(errorHandler);
        event.preventDefault();
    });
}).catch(errorHandler);
