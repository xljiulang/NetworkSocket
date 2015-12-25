##### 下载当前版本(Download the latest version)
程序包管理器控制台：
<br>PM> `Install-Package NetworkSocket`

##### 项目主页和文档（Project homepage and Documentation）
非常感谢网友[少林扫地僧](http://zhangyihui.cnblogs.com/)无偿提供文档托管，[项目主页和文档](http://networksocket.nginx.online)

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


 
