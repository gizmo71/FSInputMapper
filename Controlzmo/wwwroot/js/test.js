"use strict";

function errorHandler(err) {
    return console.error(err.toString());
}

var connection = new signalR.HubConnectionBuilder().withUrl("/hub/light").build();

connection.on("ShowMessage", function (message) {
    var messagesList = $("#messagesList");
    messagesList.children("li").slice(0, 1 - 5).remove();
    var timestamp = new Date().toLocaleTimeString([], { timeStyle: 'medium' });
    messagesList.append($("<li/>", { text: timestamp + ": " + message }))
});

connection.on("SetFromSim", function (name, value) {
    var jqCheckbox = $("#lights" + name);
    if (value != null)
        jqCheckbox.prop('checked', value);
    jqCheckbox.prop('disabled', value == null);
});

connection.start().then(function () {
    // Called when connection established - may want to disable things until this is received
    connection.invoke("SendAll").catch(errorHandler);
    $(".sendLights").on("change", function (event) {
        var id = event.target.id.replace(/^lights/, '');
        connection.invoke("SetInSim", id, event.target.checked).catch(errorHandler);
        event.preventDefault();
    });
}).catch(errorHandler);
