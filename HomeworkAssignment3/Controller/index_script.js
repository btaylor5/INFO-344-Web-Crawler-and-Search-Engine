$(function () {
    $("#search").click(function () {
        NBAPlayer();
        //Search();
    });

function NBAPlayer() {
        $.ajax({
            url: 'http://ec2-54-69-7-140.us-west-2.compute.amazonaws.com/code/Controller/Requests.php',
            data: { name: $('#search-box').val() },
            dataType: 'jsonp',
            jsonp: 'callback',
            jsonpCallback: 'callback',
            success: function () {
                console.log("success");
            }
        });
}

window.callback = function(data) {
    console.log(data);
    printResults(data);
};

function showLoading() {
    $('#results').html('<img class="loading" src="/loading-blue.gif" />');
}

function hideLoad() {
    $('#results').empty();
}

function printResults(data) {
    console.log(data);
    //hideLoad();
        var baseHTML = "" +
        "<div class='player'>" +
        "   <div>" +
        "      <img class='profile-pic' " +
        " src='" + data.ImageURL + "' onError='this'>" +
        "       <h2 class='PlayerName'>" +
                    data.PlayerName +
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
        "           <td class='stat'>" + data.GP +
        "           </td>" +
        "           <td class='stat'>" + data.FGP +
        "           </td>" +
        "           <td class='stat'>" + data.TPP +
        "           </td>" +
        "           <td class='stat'>" + data.FTP +
        "           </td>" +
        "           <td class='stat'>" + data.PPG +
        "           </td>" +
        "       </tr>" +
        "      </tbody>" +
        "       </table>" +
        "   </div>" +
        "</div>";

        $('#results').append(baseHTML);

        $('.profile-pic').one('error', function () {
            this.src = 'generic-avatar-390x390.png';
        });
}

});