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
    let bird_audio = document.getElementById("bird-audio");

    $.ajax({
        type: "POST",
        url: "/BirdVoice/GetRandomActiveBird",
        success: function(data){
            bird_text.textContent = bird_text.textContent.replace("%BIRD", `${data.german} (${data.latin})`);
            let url = get_audio_url_from_xeno_canto(data.Latin);
            bird_audio.src = url;
        }
    });
}

//AAAAANND xeno-canto doesn't return any Access-Control-Allow-Origin
//so same-origin-policy prevents this from happening :'(
function get_audio_url_from_xeno_canto(latin_name) {
    $.ajax({
        type: "GET",
        dataType: "json",
        url: "https://www.xeno-canto.org/api/2/recordings?query=" + latin_name + "+q:A+cnt:germany",
        success: function(data) {
            return `https:${data.recordings[0].file}`;
        }
    });
}

function bird_study_yes() {

}

function bird_study_now() {

}