using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using WebMES.Controllers;

namespace WebMES
{
    public class WebApiApplication : HttpApplication
    {
        /// <summary>
        /// 程式進入點
        /// </summary>
        protected void Application_Start()
        {
            //WebAPI 路由註冊
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}