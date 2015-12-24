##### 下载当前版本
程序包管理器控制台：
<br>PM> `Install-Package NetworkSocket`

##### 下载帮助文档
[V1.6.0文档](http://files.cnblogs.com/files/kewei/V1.6.0.0doc.rar)，希望有好心人愿意无偿托管帮助文档

##### 服务端代码
```
public class HomeController : HttpController
{
    [HttpPost]
    public ActionResult Index(User user, bool fAdmin = false)
    {
        return Json(new { state = true });
    }
}

public class FastMathService : FastApiService
{
    [Api]
    public int GetSum(int x, int y, int z)
    {
        return x + y + z;
    }
}

public class WebSocketSystemService : JsonWebSocketApiService
{
    [Api]
    public UserInfo[] SearchUsers(string name)
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
$.post("/home/index",{account:"admin",password:"123456",fAdmin:true});
// fastClient请求
var client = new FastTcpClient();
client.Connect(IPAddress.Loopback, 1212);
var sum = client.InvokeApi<Int32>("GetSum", 1, 2, 3).Result;
// websocket客户端请求
var ws = new jsonWebSocket('ws://127.0.0.1:1212/);
ws.invokeApi("SearchUsers", ['张三'], function (data) {
    alert(data.length == 0)
});
```

##### 欢迎入群
Q群 439800853


 
