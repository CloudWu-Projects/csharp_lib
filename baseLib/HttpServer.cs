using System.Net;
using System;
using System.IO;
using System.Net.Sockets;
using wu_jiaxing20220115;
using System.Security.Policy;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using HeiFei_20220103;
using System.Web;

namespace csharp_lib.baseLib
{
    public delegate void RouteAction(HttpListenerContext ctx, Dictionary<string, string> data);
    internal class Router 
    {
        private Dictionary<string, RouteAction> _routes_GET = new Dictionary<string, RouteAction>();

        private Dictionary<string, RouteAction> _routes_POST = new Dictionary<string, RouteAction>();


        public void AddPOST(string route, RouteAction handler)
        {
            _routes_POST.Add(route.ToLower(), handler);
        }
        public void AddGET(string route, RouteAction handler)
        {
            _routes_GET.Add(route.ToLower(), handler);
        }
        public bool TryGetValue_GET(string localPath, out RouteAction handler, out Dictionary<string, string> data)
        {
            return TryGetValue(_routes_GET,localPath,out handler, out data);
        }

        public bool TryGetValue_POST(string localPath, out RouteAction handler, out Dictionary<string, string> data)
        {
            return TryGetValue(_routes_POST, localPath, out handler, out data);
        }
        bool TryGetValue(Dictionary<string, RouteAction> _routes, string localPath, out RouteAction handler, out Dictionary<string, string> data)
        {
            handler = null;
            data = null;
            return _routes.TryGetValue(localPath.ToLower(),out handler);
        }
    };
    public class ServerHelper
    {
        HttpListener httpListener = new HttpListener();
        MyLogger logger = MyLogger.GetLogger("ServerHelper");
        Router router = new Router();
       
         ServerHelper(string url)
        {
            // httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
           // httpListener.Prefixes.Add(string.Format("http://*:{0}/", port));//如果发送到8080 端口没有被处理，则这里全部受理，+是全部接收
           if(!url.EndsWith("/")) 
                url += "/";
            httpListener.Prefixes.Add(url);
            foreach (var a in httpListener.Prefixes)
            {
                //netsh http add urlacl url=http://+:8080/ user=Everyone
                logger.Info($"startHttp server: {a.ToString()}");
                logger.Info($"netsh http add urlacl url={a.ToString()} user=Everyone");
            }
        }
        public ServerHelper(List<string> urls)
        {
            foreach (var urlA in urls)
            {
                string url = urlA;
                if (!url.EndsWith("/"))
                    url += "/";
                httpListener.Prefixes.Add(url);
            }
            foreach (var a in httpListener.Prefixes)
            {
                //netsh http add urlacl url=http://+:8080/ user=Everyone
                logger.Info($"startHttp server: {a.ToString()}");
                logger.Info($"netsh http add urlacl url={a.ToString()} user=Everyone");
            }
        }
        string defaultRouterHtml = "";
        public ServerHelper AddPostHandler(String param1, RouteAction param2)
        {
            defaultRouterHtml += $"<li> POST:<a href={param1}>{param1}</a> </li>";
            router.AddPOST(param1, param2);
            return this;
        }
        public ServerHelper AddGetHandler(String param1, RouteAction param2)
        {
            defaultRouterHtml += $"<li> GET:<a href={param1}>{param1}</a> </li>";
            router.AddGET(param1, param2);
            return this;
        }
        public void Start()
        {
            try { 
                httpListener.Start();//开启服务
                Receive();//异步接收请求
            }
            catch(Exception ex)
            {
                logger.Error($"{ex.Message} {ex.ToString()}");

               
            }
        }
        public void Stop()
        {
            httpListener.Stop();
            logger?.Dispose();

        }
        private void Receive()
        {
            httpListener.BeginGetContext(new AsyncCallback(EndReceive), null);
        }

        void EndReceive(IAsyncResult ar)
        {
            var context = httpListener.EndGetContext(ar);
            Dispather(context);//解析请求
            Receive();
        }

        RequestHelper RequestHelper;
        ResponseHelper ResponseHelper;
        void Dispather(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            try
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                RouteAction routeAction;
                Dictionary<string, string> data;
                logger.Info($"{request.HttpMethod} {request.Url.LocalPath}");
                if (request.HttpMethod == "POST")
                {
                    if (router.TryGetValue_POST(request.Url.LocalPath, out routeAction, out data))
                    {
                        routeAction(context, data);
                    }   
                }
                else if (request.HttpMethod == "GET")
                {
                    if (string.Equals(request.Url.LocalPath, "/"))
                    {
                        var response = context.Response;
                        response.StatusCode = (int)HttpStatusCode.OK;
                        response.ContentType = "text/html";
                        using(var writer = new StreamWriter(response.OutputStream,Encoding.UTF8))
                        {
                            writer.Write(defaultRouterHtml);
                            writer.Close();
                            response.Close();
                            return;
                        }
                    }
                    if (router.TryGetValue_GET(request.Url.LocalPath, out routeAction, out data))
                    {
                        routeAction(context, data);
                    }
                    else if (string.Equals(request.Url.LocalPath, "/getfile"))
                    {
                        string query = request.Url.Query;
                        var queryParams = HttpUtility.ParseQueryString(query);
                        if (queryParams.Count == 0)
                        {
                            return;
                        }
                        var filePath1 = queryParams[0];
                        var filePath = System.Web.HttpUtility.UrlDecode(filePath1);
                        HttpListenerResponse response = context.Response;
                        if (File.Exists(filePath))
                        {
                            // 返回静态文件内容
                            byte[] fileBytes = File.ReadAllBytes(filePath);
                            response.ContentType = GetContentType(filePath);
                            response.ContentLength64 = fileBytes.Length;
                            response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                        }
                        else
                        {
                            response.StatusCode = 404;
                            byte[] errorBytes = System.Text.Encoding.UTF8.GetBytes("File not found");
                            response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
                        }
                        response.OutputStream.Close();
                        //HttpListenerResponse response = context.Response;
                        //RequestHelper = new RequestHelper(request, logger);
                        //ResponseHelper = new ResponseHelper(response);
                        //RequestHelper.DispatchResources(fs =>
                        //{
                        //    ResponseHelper.WriteToClient(fs);// 对相应的请求做出回应
                        //});
                    }
                
                }
            }
            catch (Exception ex)
            {
                logger.Error($"{ex.StackTrace} {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            try
            {
                Console.WriteLine($"aaaa-->{request.HttpMethod} {request.Url.LocalPath} {context.Response.StatusCode}");
                context.Response.Close();
            }
            catch (Exception ex) { 
                logger.Error($"{ex.StackTrace} {ex.Message}");}
        }

        // 根据文件扩展名返回 Content-Type
        static string GetContentType(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            return ext switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }
    public class RequestHelper
    {
        private HttpListenerRequest request;
        private MyLogger _logger = null;
        public RequestHelper(HttpListenerRequest request, MyLogger logger)
        {
            this.request = request;
            this._logger = logger;
        }
        public Stream RequestStream { get; set; }
        public void ExtracHeader()
        {
            RequestStream = request.InputStream;
        }

        public delegate void ExecutingDispatch(FileStream fs);
        public void DispatchResources(ExecutingDispatch action)
        {
            var rawUrl = request.RawUrl;//资源默认放在执行程序的wwwroot文件下，默认文档为index.html
            if (rawUrl.Length == 1)
            {
                //filePath = string.Format(@"{0}/wwwroot/index.html", Environment.CurrentDirectory);//默认访问文件
                return;
            }
            string query= request.Url.Query;
            var queryParams = HttpUtility.ParseQueryString(query);
            if (queryParams.Count==0)
            {
                return;
            }
            var filePath1 = queryParams[0];
            var filePath = System.Web.HttpUtility.UrlDecode(filePath1);
            if (_logger != null)
                _logger.Info($"http recv request {filePath}");
            try
            {
                if (File.Exists(filePath))
                {
                    FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);

                    action?.Invoke(fs);

                }
            }
            catch { return; }
        }
        public void ResponseQuerys()
        {
            var querys = request.QueryString;
            foreach (string key in querys.AllKeys)
            {
                VarityQuerys(key, querys[key]);
            }
        }

        private void VarityQuerys(string key, string value)
        {
            switch (key)
            {
                case "pic": Pictures(value); break;
                case "text": Texts(value); break;
                default: Defaults(value); break;
            }
        }

        private void Pictures(string id)
        {

        }

        private void Texts(string id)
        {

        }

        private void Defaults(string id)
        {

        }

    }

    public class ResponseHelper
    {
        private HttpListenerResponse response;
        public ResponseHelper(HttpListenerResponse response)
        {
            this.response = response;
            OutputStream = response.OutputStream;

        }
        public Stream OutputStream { get; set; }
        public class FileObject
        {
            public FileStream fs;
            public byte[] buffer;
        }
        public void WriteToClient(FileStream fs)
        {
            response.StatusCode = 200;
            response.ContentType = "image/jpeg";
            byte[] buffer = new byte[fs.Length];
            FileObject obj = new FileObject() { fs = fs, buffer = buffer };
            fs.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(EndWrite), obj);
        }
        void EndWrite(IAsyncResult ar)
        {
            var obj = ar.AsyncState as FileObject;
            var num = obj.fs.EndRead(ar);
            OutputStream.Write(obj.buffer, 0, num);
            obj.fs.Close(); ;
            OutputStream.Close(); ;
            if (num < 1)
            {
                //obj.fs.Close(); //关闭文件流　　　　　　　　　　OutputStream.Close();//关闭输出流，如果不关闭，浏览器将一直在等待状态 　　　　　　　　　　return; 　　　　　　　　}
                obj.fs.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(EndWrite), obj);
            }
        }
    }
}