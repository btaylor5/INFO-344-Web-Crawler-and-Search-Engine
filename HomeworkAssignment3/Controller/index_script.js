$(function () {
    $("#search").click(function () {
        NBAPlayer();
        Search();
    });

    $("#search-box").bind("keyup", function () {
        Suggest();
    });

    $("#search-box").keyup(function () {
        delay(function () {
            Search();
            NBAPlayer();
        }, 250);
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

window.callback = function (data) {
    console.log(data);
    printResults(data);
};

function showLoading() {
    $('#all-results').html('<img class="loading" src="/loading-blue.gif" />');
}

function hideLoad() {
    $('#all-results').empty();
}
        
function printResults(data) {

    baseHTML = "";
    if (data != "" ) {
        console.log("Returned NBA Result");
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

        $('.profile-pic').one('error', function () {
            this.src = 'generic-avatar-390x390.png';
        });
    }

    $('.nba-result').empty().append(baseHTML);

    //hideLoad();
        
}

var delay = (function () {
    var timer = 0;
    return function (callback, ms) {
        clearTimeout(timer);
        timer = setTimeout(callback, ms);
    };
})();

function Search() {
        $.ajax({
            type: "POST",
            url: "/admin.asmx/Search",
            data:
                '{"query":"' +  $('#search-box').val() + '","n":"' + 10 + '"}',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                console.log("Search Success");
                console.log(msg);
                var data = getData(msg);
                console.log(data);
                $('.results-div').empty();
                for (var i = 0; i < data.length - 1; i++) {
                    PrintSearchResults(data[i]);
                }
                //jQuery.each(data, function (rec) {
                //    console.log(rec);
                //    PrintSearchResults(rec);
                //});
            },
            error: function (msg) {
                console.log("Left To Process Broke");
            }
        });
}


function PrintSearchResults(rec) {
    baseHTML = "";
    if (rec != "") {
        baseHTML =
            "<div class='result well'>" +
            "<h4>" +
            "<a href='" +
             rec.URL +
            "'>" +
            rec.Title +
            "       </a>" +
        "</h4>" +
        "<p>" +
        rec.Date +
        "</p>" +
        "<p>" +
            rec.Body +
        "</p>" +
        "</div>";
    }

    $('.results-div').append(baseHTML);

}

function Suggest() {
    $.ajax({
        type: "POST",
        url: "/WikiSuggest.asmx/GetSuggestions",
        data: JSONData(),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $("#suggestions").empty();
            var data = eval(msg['d']);
            jQuery.each(data, function (rec) {
                $("#suggestions").append("<li>" + data[rec] + "</li>");
            });

        },
        error: function (msg) {
            console.log(msg['response']);
        }
    });
};

function JSONData() {
    var prefix = $("#search-box").val();
    var Obj =
        {
            'prefix': prefix
        };

    return JSON.stringify(Obj);

};

function getData(msg) {
    return eval(msg['d']);
};



});