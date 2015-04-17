// jsonWebsocket客户端
function jsonWebsocket(url) {
    var ws;
    var packetId = 0;
    var callbackTable = [];

    // 是否连接
    this.connected = false;

    // 关闭时触发
    this.onclose = function (code, reason) {
    };

    // 握手成功后触发
    this.onopen = function () {
    };

    // 绑定给服务器来调用的api
    this.bindApi = function (name, func) {
        this[name] = func;
    }

    // 调用服务器实现api  {api名，[参数1，参数2，参数n],返回数据回调}
    this.invkeApi = function (api, parameters, callback) {
        if (this.connected == false) {
            return;
        }

        packetId = packetId + 1;
        var packet = { api: api, id: packetId, body: parameters || [] };
        var json = JSON.stringify(packet);

        if (callback) {
            callbackTable.push({ id: packetId, call: callback });
        }
        ws.send(json);
    };

    // 获取回调
    function getCallback(id) {
        for (var i = 0; i < callbackTable.length; i++) {
            var callBack = callbackTable[i];
            if (callBack.id == id) {
                callbackTable.splice(i, 1);
                return callBack.call;
            }
        }
    }

    // 收到文本消息时
    function onmessage(e) {
        var packet = JSON.parse(e.data);
        if (!packet.fromClient) {
            callApi.apply(this, [packet]);
            return;
        }

        var callback = getCallback(packet.id);
        if (callback) {
            callback(packet.body);
        }
    }

    // 调用自身实现的api
    function callApi(packet) {
        var api = packet.api;
        var define = this[api] && typeof this[api] == "function";
        if (!define) {
            setRemoteException(packet, '请求的api不存在：' + api);
            return;
        }

        try {
            var result = this[api].apply(this, packet.body);
            setApiResult(packet, result);
        } catch (e) {
            setRemoteException(packet, e.message);
        }
    }

    // 将异常信息发送到服务端
    function setRemoteException(packet, message) {
        packet.state = false;
        packet.body = message;
        var json = JSON.stringify(packet);
        ws.send(json);
    }

    // 将api调用结果发送到服务端
    function setApiResult(packet, result) {
        packet.body = result;
        var json = JSON.stringify(packet);
        ws.send(json);
    }

    // 初始化
    function init() {
        var $this = this;
        ws = new (window.WebSocket || window.MozWebSocket)(url);
        ws.onclose = function (code, reason) {
            $this.connected = false;
            $this.onclose(code, reason);
        };
        ws.onopen = function () {
            $this.connected = true;
            $this.onopen();
        };
        ws.onmessage = function (e) {
            onmessage.apply($this, [e]);
        };
    }

    init.apply(this);
}
