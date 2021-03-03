// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function add_new_protocol_entry() {
    var i = $(".ProtocolEntryForm").length;
    $.ajax({
        type: "POST",
        url: '/ActivityProtocol/AddProtocolEntry?index=' + i,
        success: function (data) {
            $('#dynEntries').append(data);

            //remove validator from the form
            $("form").removeData("validator").removeData("unobtrusiveValidation");
            //add validator again (parses the new html we just added)
            $.validator.unobtrusive.parse("form");
        }
    });
}