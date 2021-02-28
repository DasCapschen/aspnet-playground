// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
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

function bird_study_next() {
    let bird_text = document.getElementById("bird-name");

    $.ajax({
        type: "POST",
        url: "/BirdVoice/GetRandomActiveBird",
        success: function(data){
            bird_text.textContent = `You are listening to ${data.german} (${data.latin})`;
            get_audio_from_xeno_canto(data.latin);
        }
    });
}

function get_audio_from_xeno_canto(latin_name) {
    let bird_audio = document.getElementById("bird-audio");

    $.ajax({
        type: "POST",
        url: "/BirdVoice/GetXenoCanto?latin_name=" + latin_name,
        success: function(data_str) {
            let data = JSON.parse(data_str);
            try {
                let len = data.recordings.length;
                let url = `https:${data.recordings[random_int(0,len)].file}`;
                bird_audio.src = url;
            } catch {
                alert("Xeno-Canto does not have a recording of this bird :(");
            }
        }
    });
}

function random_int(min, max) {
    return Math.floor(Math.random() * (max - min)) + min;
}

function bird_study_yes() {

}

function bird_study_now() {

}