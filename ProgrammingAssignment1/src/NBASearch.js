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
                var baseHTML = "" +
                    "<div class='No-Results'>" +
                    "   <div>" +
                    "       <h2>" +
                    "        Sorry! Your Search Returned No Results" +
                    "       </h2>" +
                    "   </div>" +
                    "</div>";
                hideLoad();
                $("#results").append(baseHTML);

            },
            beforeSend: function() {
                console.log('loading..');
                showLoading();
            },
            dataType: 'JSON'
        });
    }

    function hideLoad() {
        $('#results').empty();
    }

    function printResults(data) {
        hideLoad();
        for(var i = 0; i < data.length; i++) {
            var baseHTML = "" +
            "<div class='player'>" +
            "   <div>" +
            "      <img class='profile-pic' " +
            " src='" +  data[i].ImageURL + "' onError='this'>" +
            "       <h2 class='PlayerName'>" +
                        data[i].PlayerName +
            "       </h2>"+
            "       <table class='table table-bordered stat-table'>" +
            "       <thead class='bg-primary'>" +
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

            $('.profile-pic').one('error', function() {
                this.src = 'src/generic-avatar-390x390.png';
            });
        }
    }

    function showLoading() {
        $('#results').html('<img class="loading" src="src/loading-blue.gif" />');
    }



})();