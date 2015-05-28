$(function () {
    console.log("ready!");

    $(".load").click(Load);
    $(".start").click(Start);
    $(".stop").click(Stop);
    $(".clear-index").click(ClearIndex);
    $(".clear-queue").click(ClearQueue);

    $('.add_button').click(AddUrlToQueue);
    $('.search_for_index').click(SearchResults);
    $('.custom_crawl').click(CustomCrawl);




    window.setInterval(function () {
        NewResults();
    }, 10000);

    window.setInterval(function () {
        GetErrorList();
    }, 5000);

    window.setInterval(function () {
        ErrorCount();
        IndexCount();
        var errors = parseInt($(".error_count").html());
        var indexed =  parseInt($(".index_count").html());
        var total = errors + indexed
        $(".crawl_count").empty().append(total);

        var percentage = Math.floor(parseInt(indexed / total * 100));
        console.log(total);
        $(".success_rate").empty().append(percentage + "%");
    }, 10000);

    window.setInterval(function () {
        SystemStatus();
    }, 10000);

    window.setInterval(function () {
        LeftToProcess();
    }, 5000);

    window.setInterval(function () {
        GetMemory();
    }, 10000);

    window.setInterval(function () {
        GetCPU();
    }, 10000);



    function NewResults() {
        $.ajax({
            type: "POST",
            url: "/admin.asmx/LastTenVisitedUrls",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                $(".lastTen").empty();
                ToList(".lastTen", msg);
            },
            error: function (msg) {
                console.log(msg['response']);
            }
        });
    };

    function CustomCrawl() {
        $.ajax({
            type: "POST",
            url: "/admin.asmx/CustomCrawl",
            data: JSON.stringify
                ({
                    'crawl_string': ($(".crawl_request").val() + "=" + $(".crawl_bound").val())
                }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                $(".lastTen").empty();
                ToList(".lastTen", msg);
            },
            error: function (msg) {
                console.log(msg['response']);
            }
        });
    };

    function ClearQueue() {
        $.ajax({
            type: "POST",
            url: "/admin.asmx/ClearQueue",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {

            },
            error: function (msg) {
                console.log(msg['response']);
            }
        });
    };

    function SearchResults() {
        $.ajax({
            type: "POST",
            url: "/admin.asmx/SearchResults",
            data: JSONData(".search_query"),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                $(".search_results").empty();
                ToList(".search_results", msg);
            },
            error: function (msg) {
                console.log(msg['response']);
            }
        });
    };

    function IndexCount() {
        $.ajax({
            type: "POST",
            url: "/admin.asmx/IndexCount",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var total = getData(msg)
                $(".index_count").empty().append(total);
            },
            error: function (msg) {
                console.log(msg['response']);
            }
        });
    };

    function ErrorCount() {
        $.ajax({
            type: "POST",
            url: "/admin.asmx/ErrorCount",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var total = getData(msg)
                $(".error_count").empty().append(total);
            },
            error: function (msg) {
                console.log(msg['response']);
            }
        });
    }

    function SystemStatus() {
        $.ajax({
            type: "POST",
            url: "/admin.asmx/SystemRunHistory",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                ToCommandLog(msg);
            },
            error: function (msg) {
                console.log(msg['response']);
            }
        });
    };


    function LeftToProcess() {
        $.ajax({
            type: "POST",
            url: "/admin.asmx/LeftToProcess",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                $(".queue_count").empty().append(getData(msg));
            },
            error: function (msg) {
                console.log("Left To Process Broke");
            }
        });
    };

    function GetErrorList() {
        $.ajax({
            type: "POST",
            url: "/admin.asmx/GetErrorMessages",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                ToErrorLog(msg);
            },
            error: function (msg) {
                console.log(msg['response']);
            }
        });
    }


    function Start() {
        $.ajax({
            type: "POST",
            url: "/admin.asmx/Start",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                //ToCommandLog(msg);
            },
            error: function (msg) {
                console.log(msg['response']);
            }
        });
    };

    function AddUrlToQueue() {
        $.ajax({
            type: "POST",
            url: "/admin.asmx/AddUrlToQueue",
            data: JSONData(".add_url"),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                //ToCommandLog(msg);
            },
            error: function (msg) {
                console.log(msg['response']);
            }
        });
    };


    function Load() {
        $.ajax({
            type: "POST",
            url: "/admin.asmx/Load",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                //ToCommandLog(msg);
            },
            error: function (msg) {

            }
        });
    };

    function Stop() {
        $.ajax({
            type: "POST",
            url: "/admin.asmx/Stop",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {     
                //ToCommandLog(msg);
            },
            error: function (msg) {

            }
        });
    };

    function ClearIndex() {
        $.ajax({
            type: "POST",
            url: "/admin.asmx/ClearIndex",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                //ToCommandLog(msg);
            },
            error: function (msg) {

            }
        });
    };


    function GetCPU() {
        $.ajax({
            type: "POST",
            url: "/admin.asmx/GetCPU",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                $(".cpu_stat").empty().append(getData(msg));
            },
            error: function (msg) {

            }
        });
    };


    function GetMemory() {
        $.ajax({
            type: "POST",
            url: "/admin.asmx/GetMemory",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                $(".memory_stat").empty().append(getData(msg));
            },
            error: function (msg) {
            }
        });
    }

    function ToList(selector, msg) {
        var data = getData(msg);

        jQuery.each(data, function (rec) {
            $(selector).append("<li>" + data[rec] + "</li>");
        });
    }

    function ToCommandLog(msg) {
        $(".log").empty()
        var data = getData(msg);

        $(".status").empty().append(data[0]);

        jQuery.each(data, function (rec) {
            $(".log").append("<li>" + data[rec] + "</li>");
        });
    };

    function ToErrorLog(msg) {
        $(".errors").empty();
        ToLog(".errors", msg);
    };

    function ToLog(selector, msg) {
        var data = getData(msg);

        jQuery.each(data, function (rec) {
            $(selector).append("<li>" + data[rec] + "</li>");
        });
    };

    function getData(msg) {
        return eval(msg['d']);
    };

    function JSONData(selector) {
        var prefix = $(selector).val();
        var Obj =
            {
                'url': prefix
            };

        return JSON.stringify(Obj);
    };


});