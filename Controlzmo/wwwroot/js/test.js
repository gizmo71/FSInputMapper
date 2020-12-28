"use strict";

function errorHandler(err) {
    return console.error(err.toString());
}

var connection = new signalR.HubConnectionBuilder().withUrl("/hub/light").build();

connection.on("ShowMessage", function (message) {
    var messagesList = $("#messagesList");
    messagesList.children("li").slice(0, 1 - 5).remove();
    messagesList.append($("<li/>", { text: message }))
});

connection.start().then(function () {
    // Called when connection established - may want to disable things until this is received
    connection.invoke("SendAll").catch(errorHandler);
    $(".sendSomet").on("change", function (event) {
        connection.invoke("ChangedSomet", event.target.id + " is " + event.target.checked).catch(errorHandler);
        //event.preventDefault();
    });
}).catch(errorHandler);
