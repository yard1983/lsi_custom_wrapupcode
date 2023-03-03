using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Configuration;
using Genesys.PS.DataAccess;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.SelfHost;
using System.Web.Http.Cors;
using System.Web.Http;
using System.Net.Http;
using System.IO;
using System.Reflection;
using System.Net.Http.Headers;

namespace Genesys.PS.SelfHost
{
    public partial class WindowsService : ServiceBase
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();      
        private static string serviceAddress;
        public WindowsService()
        {
            InitializeComponent();
        }

        public void onStart()
        {
            this.OnStart(null);
        }
        protected override void OnStart(string[] args)
        {
            var serviceName = System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString();
            var serviceVersion = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
           
            //Get connection string
            SqlServerProvider.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["CustomWrapupCodeDatabase"].ConnectionString;

            try
            {
                serviceAddress = ConfigurationManager.AppSettings["serviceAddress"];
                _log.Info("serviceAddress -> " + serviceAddress);
                createServer(serviceAddress);
                _log.Info("create host - success");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "create host - error");
                return;
            }
            
            _log.Info("Service started: " + serviceName + "-" + serviceVersion);           
        }

        protected override void OnStop()
        {
            NLog.LogManager.Shutdown();            
        }
        void createServer(string serviceAddress)
        {
            var config = new HttpSelfHostConfiguration(serviceAddress);
            config.EnableCors(new EnableCorsAttribute("*", headers: "*", methods: "*"));
            config.MessageHandlers.Add(new FileHandler());
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute("default",
                                "api/{controller}/{action}/{id}",
                                new { controller = "WrapupCode", id = RouteParameter.Optional });

            var server = new HttpSelfHostServer(config);
            var task = server.OpenAsync();
            task.Wait();
        }       
        
    }

    public class FileHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.AbsolutePath.StartsWith("/api/"))
                return base.SendAsync(request, cancellationToken);
            return Task<HttpResponseMessage>.Factory.StartNew(() =>
            {
                var response = request.CreateResponse();
                var baseFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Static";
                var suffix = (request.RequestUri.AbsolutePath == "" || request.RequestUri.AbsolutePath == "/") ? "index.html" : request.RequestUri.AbsolutePath.Substring(1);
                var fullPath = Path.Combine(baseFolder, suffix);
                Console.WriteLine("Serving file {0}", fullPath);
                string extension = Path.GetExtension(fullPath);
                response.Content = new StreamContent(new FileStream(fullPath, FileMode.Open));
                if (!string.IsNullOrWhiteSpace(extension)) {

                    response.Headers.CacheControl = new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromSeconds(60 * 60 * 24 * 365) };

                    if (extension.Contains("html")) {
                        response.Headers.CacheControl = null;
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                    }
                    else if(extension.Contains("css"))
                    {
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/css");
                    }
                    else if (extension.Contains("js"))
                    {
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/javascript");

                        if (fullPath.Contains("app.js"))
                        {
                            response.Headers.CacheControl = new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromSeconds(60 * 60 * 24 * 7) };
                        }
                    }
                    else if (extension.Contains("png"))
                    {
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    }
                    else if (extension.Contains("json"))
                    {
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    }                    
                }               
                return response;
            });

        }

    }
}