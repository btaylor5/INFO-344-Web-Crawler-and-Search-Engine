﻿$(function () {
    $("#search").click(function () {
        NBAPlayer();
        //Search();
    });

function NBAPlayer() {
        $.ajax({
            url: 'http://ec2-54-69-7-140.us-west-2.compute.amazonaws.com/code/Controller/Requests.php',
            data: { name: $('#search-box').val },
            dataType: 'jsonp',
            jsonp: 'callback',
            jsonpCallback: 'callback',
            success: function () {
                console.log("success");
            }
        });
}

function callback(data) {
    printResults(data);
}

function showLoading() {
    $('#results').html('<img class="loading" src="/loading-blue.gif" />');
}

function hideLoad() {
    $('#results').empty();
}

function printResults(data) {
    hideLoad();
    for (var i = 0; i < data.length; i++) {
        var baseHTML = "" +
        "<div class='player'>" +
        "   <div>" +
        "      <img class='profile-pic' " +
        " src='" + data[i].ImageURL + "' onError='this'>" +
        "       <h2 class='PlayerName'>" +
                    data[i].PlayerName +
        "       </h2>" +
        "       <table class='table table-bordered stat-table'>" +
        "       <thead class='bg-primary'>" +
        "            <tr>" +
        "                <th>GP</th>" +
        "                <th>FGP</th>" +
        "                <th>TPP</th>" +
        "                <th>FTP</th>" +
        "                <th>PPG</th>" +
        "           </tr>" +
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

        $('.profile-pic').one('error', function () {
            this.src = 'src/generic-avatar-390x390.png';
        });
    }
}

});