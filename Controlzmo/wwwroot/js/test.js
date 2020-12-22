"use strict";

function errorHandler(err) {
    return console.error(err.toString());
}

var connection = new signalR.HubConnectionBuilder().withUrl("/hub/light").build();

connection.on("TestMessage", function(message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = "someone says " + msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});

connection.start().then(function() {
    // Called when connection established - may want to disable things until this is received
}).catch(errorHandler);

["light1", "light2"].forEach(id =>
    document.getElementById(id).addEventListener("change", function (event) {
        connection.invoke("TestMessage", /*event.srcElement.*/id + " is " + event.srcElement.checked).catch(errorHandler);
        //event.preventDefault();
    })
);
