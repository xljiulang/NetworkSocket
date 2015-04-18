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

    function isFunction(func) {
        return !!func && typeof func == "function";
    }

    // 调用服务器实现api，返回是否可以正常调用
    // api：远程端api名，不区分大小写
    // parameters：参数值数组，注意参数顺序(可选)
    // doneFunc：服务端返回api结果后触发的回调(可选)
    // exFunc：服务端返回异常信息后触发的回调(可选)    
    this.invkeApi = function (api, parameters, doneFunc, exFunc) {
        if (this.connected == false) {
            return false;
        }

        packetId = packetId + 1;
        var packet = { api: api, id: packetId, body: parameters || [] };
        var json = JSON.stringify(packet);

        if (isFunction(doneFunc) || isFunction(exFunc)) {
            callbackTable.push({ id: packetId, callback: { doneFunc: doneFunc, exFunc: exFunc } });
        }
        ws.send(json);
        return true;
    };

    // 获取回调
    function getCallback(id) {
        for (var i = 0; i < callbackTable.length; i++) {
            var item = callbackTable[i];
            if (item.id == id) {
                callbackTable.splice(i, 1);
                return item.callback;
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
        if (!callback) {
            return;
        }

        var func = packet.state ? callback.doneFunc : callback.exFunc;
        if (isFunction(func)) {
            func(packet.body);
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
