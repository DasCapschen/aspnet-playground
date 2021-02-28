﻿// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function add_new_protocol_entry() {
    var i = $(".ProtocolEntryForm").length;
    $.ajax({
        type: "POST",
        url: 'AddProtocolEntry?index=' + i,
        success: function (data) {
            $('#dynEntries').append(data)
        }
    });
}

function move_to_active_birds() {
    let available = document.getElementById("select-available");
    let active = document.getElementById("select-active");

    let index = available.selectedIndex;
    let value = available.value;

    if(available == null || active == null || index < 0) 
        return;

    $.ajax({
        type: "POST",
        url: "/BirdVoice/AddActiveBird?id=" + value,
        success: function () {
            active.add(available.children[index]);
        }
    });
}

function move_to_available_birds() {
    let available = document.getElementById("select-available");
    let active = document.getElementById("select-active");

    let index = active.selectedIndex;
    let value = active.value;

    if(available == null || active == null || index < 0) 
        return;

    $.ajax({
        type: "POST",
        url: "/BirdVoice/RemoveActiveBird?id=" + value,
        success: function () {
            available.add(active.children[index]);
        }
    });
}