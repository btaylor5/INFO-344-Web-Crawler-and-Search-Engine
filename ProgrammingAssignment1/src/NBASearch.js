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
                $('#results').empty();
                console.log("Success");
                printResults(data);
            },
            error: function(message) {
                console.log("Failure");
                $('#results').text($.parseJSON(message));
            },
            beforeSend: function() {
                console.log('loading..');
                showLoading();
            },
            dataType: 'JSON'
        });
    }

    function printResults(data) {
        for(var i = 0; i < data.length; i++) {
            var baseHTML = "" +
            "<div class='player'>" +
            "   <div>" +
            "      <img class='profile-pic' src='src/generic-avatar-390x390.png'>" +
            "       <h2 class='PlayerName'>" +
                        data[i].PlayerName +
            "       </h2>"+
            "       <table class='table table-bordered stat-table'>" +
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
            "       <tr>" +
            "           <td class='stat'>" + data[i].GP +
            "           </td>" +
            "           <td class='stat'>" + data[i].FGP +
            "           </td>" +
            "           <td class='stat'>" + data[i].TPP +
            "           </td>" +
            "           <td class='stat'>" + data[i].FTP +
            "           </td>" +
            "           <td class='stat'>" + data[i].PPG +
            "           </td>" +
            "       </tr>" +
            "      </tbody>" +
            "       </table>" +
            "   </div>" +
            "</div>";

            $('#results').append(baseHTML);
        }
    }

    function showLoading() {
        $('#results').html('<img class="loading" src="src/loading-blue.gif" />');
    }


})();