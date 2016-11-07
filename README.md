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
}

public class FastMathService : FastApiService
{
    [Api]
    public UserInfo[] UserInfo(string name)
    {
        return new UserInfo[0];
    }
}

public class WebSocketSystemService : JsonWebSocketApiService
{
    [Api]
    public UserInfo[] GetUsers(string name)
    {
        return new UserInfo[0];
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
// 浏览器请求
$.post("/Home/GetUsers",{name:"admin",fAdmin:true});

// fastClient请求
var client = new FastTcpClient();
client.Connect(IPAddress.Loopback, 1212);
var users = await client.InvokeApi<Int32>("GetUsers", "admin");

// websocket客户端请求
var ws = new jsonWebSocket('ws://127.0.0.1:1212/);
ws.invokeApi("GetUsers", ['admin'], function (data) {
    alert(data.length == 0)
});
```

##### 欢迎入群
Q群 439800853


 
