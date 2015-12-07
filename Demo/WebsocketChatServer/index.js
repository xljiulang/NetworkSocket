var chatUI = new function () {
    // websocket的flash文件，给老IE用的
    WEB_SOCKET_SWF_LOCATION = "/js/WebSocketMain.swf";
    WEB_SOCKET_DEBUG = false;


    this.showOthersChatMessage = function (name, message, time) {
        var li = $(".template").clone();
        li.removeClass().addClass("text-left").find("span:first").html(name).next().html(time).next().html(message);
        li.appendTo(".chat-list");
    }

    this.showMyChatMessage = function (dom) {
        var message = $(dom).prev().val();
        var li = $(".template").clone();
        li.removeClass().addClass("text-right").find("div").html(message);
        li.appendTo(".chat-list");
        $(dom).prev().empty();
    }

    this.showNickName = function (data) {
        if (data.state) {
            $(".alert-success").hide();
            document.title += ("[" + data.name + "]");
            var li = "<li>" + data.name + "</li>";
            $(li).appendTo(".member-list");
        } else {
            win.alertEx(data.message);
        }
    }

    this.showAllMembers = function (data) {
        $(".member-list").empty();
        for (var i = 0; i < data.length; i++) {
            var li = "<li>" + data[i] + "</li>";
            $(li).appendTo(".member-list");
        }
    }

    this.showConnected = function () {
        $(".alert-success").show().prev().hide();
    }

    this.showDisConnected = function () {
        $(".alert-error").show().next().hide();
    }
};