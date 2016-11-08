##### 下载当前版本(Download the latest version)
程序包管理器控制台：
<br>PM> `Install-Package NetworkSocket`


##### 服务端代码
```
public class HomeController : HttpController
{
    [HttpGet]
    public UserInfo[] GetUsers(string name)
    {
        return new UserInfo[0];
    }
    
    [HttpGet]
    public async Task<string> AboutAsync(string name)
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        return "Http";
    }
}

public class FastService : FastApiService
{
    [Api]
    public UserInfo[] UserInfo(string name)
    {
        return new UserInfo[0];
    }
    
    [Api]
    public async Task<string> AboutAsync(string name)
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        return "Fast";
    }
}

public class JsonWebSocketService : JsonWebSocketApiService
{
    [Api]
    public UserInfo[] GetUsers(string name)
    {
        return new UserInfo[0];
    }
    
    [Api]
    public async Task<string> AboutAsync(string name)
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        return "WebSocket";
    }
}

var listener = new TcpListener();
listener.Use<HttpMiddleware>();
listener.Use<JsonWebSocketMiddleware>();
listener.Use<FastMiddleware>();            
listener.Start(1212);
```

##### 客户端代码
```
// http js请求
$.getJSON("/Home/GetUsers",{name:"admin"},function(data){
    alert(data.length == 0)
});
$.getJSON("/Home/About",{},function(data){
    alert(data == "Http")
});

// fastClient请求
var client = new FastTcpClient();
client.Connect(IPAddress.Loopback, 1212);
var users = await client.InvokeApi<UserInfo[]>("GetUsers", "admin");
var about = await client.InvokeApi<string>("About"); // about == "Fast"

// websocket js请求
var ws = new jsonWebSocket('ws://127.0.0.1:1212/);
ws.invokeApi("GetUsers", ['admin'], function (data) {
    alert(data.length == 0)
});
ws.invokeApi("About", [], function (data) {
    alert(data == "WebSocket")
});

// websocketClient请求
var client = new JsonWebSocketClient();
await client.ConnectAsync(IPAddress.Loopback, 1212);
var users = await client.InvokeApi<UserInfo[]>("GetUsers", "admin");
var about = await client.InvokeApi<string>("About"); // about == "WebSocket"
```

##### Async Void支持
FilterAttributer提供void OnXXX(ActionContext filterContext)的重写方法，当重写为
`async` void OnXXX(ActionContext filterContext)之后，不影响Filter的等待流程，但是执行线程不是在异步等待执行，
而是转为同步等待执行。
##### 欢迎入群
Q群 439800853


 
