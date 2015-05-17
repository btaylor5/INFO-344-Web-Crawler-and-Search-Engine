$(function () {
    console.log("ready!");

    $(".load").click(Load);
    $(".start").click(Start);
    $(".stop").click(Stop);
    $(".clear-index").click(ClearIndex);

    window.setInterval(function () {
        NewResults();
    }, 5000);

    window.setInterval(function () {
        LeftToProcess();
    }, 2000);

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
                LastVisited(msg);
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
                ToLog(msg);
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
                ToLog(msg);
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
                ToLog(msg);
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
                
                ToLog(msg);
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
                ToLog(msg);
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

    function LastVisited(msg) {
        var data = getData(msg);

        jQuery.each(data, function (rec) {
            $(".lastTen").append("<li>" + data[rec] + "</li>");
        });
    };

    function ToLog(msg) {
        var data = getData(msg);
        $(".log").append("<li>" + data + "</li>");
    };

    function getData(msg) {
        return eval(msg['d']);
    };


});