"use strict";
(function() {
    $(document).ready(function() {
        $('.search-button').click(search);

        console.log("Document Ready And Loaded");
    });


    function search() {

        $.ajax({
            type: "POST",
            url: './code/Controller/Requests.php',
            data: 'name=' + $('#search-box').val(),
            success: function(data) {
                console.log("Success");
                printResults(data);
            },
            error: function(message) {
                console.log("Failure");
                $('#results').text($.parseJSON(message));
            },
            dataType: 'JSON'
        });
    }

    function printResults(data) {
        for(var i = 0; i < data.length; i++) {
            var baseHTML = "" +
            "<div class='player'>" +
            "   <div class='container'>" +
            "      <img class='profile-pic' src='src/generic-avatar-390x390.png' />" +
            "       <h2 class='PlayerName'>" +
                        data[i].PlayerName +
            "       </h2>"+
            "       <table class='table table-bordered'>" +
            "       <thead>" +
            "            <tr>" +
            "                <th>GP</th>" +
            "                <th>FGP</th>" +
            "                <th>TPP</th>" +
            "                <th>FTP</th>" +
            "                <th>PPG</th>" +
            "           </tr>"+
            "        </thead>" +
            "        <tbody>" +
                        "<tr class='stat'>" + data[i].GP +
                        "</tr>" +
                        "<tr class='stat'>" + data[i].FGP +
                        "</tr>" +
                        "<tr class='stat'>" + data[i].TPP +
                        "</tr>" +
                        "<tr class='stat'>" + data[i].FTP +
                        "</tr>" +
                        "<tr class='stat'>" + data[i].PPG +
            "           </tr>" +
            "       </tbody>" +
            "       </table>" +
            "   </div>" +
            "</div>";

            $('#results').append(baseHTML);
        }
    }


})();