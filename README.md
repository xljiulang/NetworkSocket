##### 下载当前版本
程序包管理器控制台：
<br>PM> `Install-Package NetworkSocket`

##### 示例代码
Http协议服务
```
public class HomeController : HttpController
{
    [HttpPost]
    public ActionResult Index(User user, bool fAdmin = false)
    {
        return Json(new { state = true });
    }
}
// 启动服务
var httpServer = new HttpServer();
httpServer.BindController<HomeController>();
httpServer.StartListen(7777);
// 客户端请求
$.post("/home/index",{account:"admin",password:"123456",fAdmin:true});
```
Fast协议服务
```
public class MathService : FastApiService
{
    [Api]
    public int GetSum(int x, int y, int z)
    {
        return x + y + z;
    }
}
// 启动服务
var server = new FastTcpServer();
server.BindService<MathService>();
server.StartListen(10086);
// 客户端调用
var client = new FastTcpClient();
client.Connect(IPAddress.Loopback, 10086);
var sum = client.InvokeApi<Int32>("GetSum", 1, 2, 3).Result;
```
jsonWebsocket协议服务
```
public class SystemApiService : JsonWebSocketApiService
{
    [Api]
    public UserInfo[] SearchUsers(string name)
    {
        return new UserInfo[0];
    }
}
// 启动服务
var server = new JsonWebSocketServer();
server.BindService<SystemApiService>();
server.StartListen(10010);
// 客户端调用
var ws = new jsonWebSocket('ws://127.0.0.1:10010/);
ws.invokeApi("SearchUsers", ['张三'], function (data) {
    alert(data.length == 0)
});
```
自定义协议服务
```
public class SimpleServer : TcpServerBase<SessionBase>
{
    protected sealed override SessionBase OnCreateSession()
    {
        return new SessionBase();
    }

    // 收到数据时
    protected sealed override void OnReceive(SessionBase session, ReceiveStream buffer)
    {
        var bytes = buffer.ReadArray();
        session.Send(bytes);
    }

    // 连接时
    protected sealed override void OnConnect(SessionBase session)
    {
        var connectedCount = this.AllSessions.Count();
    }

    // 连接时
    protected sealed override void OnDisconnect(SessionBase session)
    {
        var connectedCount = this.AllSessions.Count();
    }
}
```
##### 功能列表
1、提供Tcp服务器抽象类和客户端抽象类，以及相关的流读写功能类，字节和位操作等功能类。所有基于tcp的标准协议和个人自定义协议的服务都基于此继承来开发，抽象类已实现很多最基础功能。

2、内置flash和silverlight策略服务类，支持继承重写部分功能。

3、内置http服务模块，编写http服务时风格习惯与Asp.Net MVC极其一致，只差不支持视图。

4、内置websocket服务模块，同时封装基于json的jsonWebsocket服务和客户端脚本，从IE6到Chrome的web双工通讯不再是梦，编写jsonWebsocket服务时风格习惯和MVC一致。

5、内置名为fast的自定义个人协议，开发.Net内部使用的系统，可无视通讯知识就能编写tcp双工服务，编写服务风格习惯和MVC一致，客户端不需额外编写。

6、内置也能在EF下使用的Model验证特性，编写各种服务时，Model验证不在是恶心的一坨占了半个函数的if。

7、提供功能强大的Filter，AOP思想可以在http、jsonWebsocket、fast服务编写时大显身手，淋漓尽致，日志、异常和权限验证变得如此简单。

8、提供依赖注入支持，可以替换默认的依赖解析提供着，比如使用Autofac等优秀的依赖注入提供者来减少耦合或生命周期管理，Filter也支持依赖注入。

##### 欢迎入群
Q群 439800853


 
