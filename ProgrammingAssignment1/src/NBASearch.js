"use strict";
(function() {
    $(document).ready(function() {
        $('.search-button').click(search);

        console.log("Document Ready And Loaded");
    });


    function search() {
        $.ajax({
            type: "POST",
            url: './code/Controller/Requests.php?name=',
            data: $('#search-box').val(),
            success: function(data) {
                console.log("Success");
                $('#test-output').text(data[0].PlayerName);
                console.log(JSON.stringify(data));
            },
            error: function(message) {
                console.log("Failure");
                $('#test-output').text($.parseJSON(message));
            },
            dataType: 'JSON'
        });
    }


})();