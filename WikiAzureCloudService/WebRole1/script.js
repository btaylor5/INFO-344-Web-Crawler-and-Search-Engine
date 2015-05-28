$(function () {

    $("#search").bind("keyup", Search);


    function Search() {

        $("#suggestions").empty();

        $.ajax({
            type: "POST",
            url: "/WikiSuggest.asmx/GetSuggestions",
            data: JSONData(),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
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
        var prefix = $("#search").val();
        var Obj =
            {
                'prefix': prefix
            };

        return JSON.stringify(Obj);
        
    }


});

