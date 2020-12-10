using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web;
using System.Web.Http;
using Dapper;
using System.Web.Configuration;
using Newtonsoft.Json.Linq;
using System.Runtime.Caching;
using System.Reflection;
using Newtonsoft.Json;
using System.IO;

namespace WebMES.Controllers
{
    /// <summary>
    /// 與帳戶控制相關資料操作
    /// </summary>
    public class AccountController : ApiController
    {
        GeneralController general;

        public AccountController()
        {
            general = new GeneralController();
        }

        /// <summary>
        /// 登入動作
        /// 密碼未加密;密碼修改安全性;信箱認證等水太深以後再做
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject LoginAction(JObject data)
        {
            JObject returnMsg = new JObject();
            int result = 0;

            try
            {
                //1.檢查帳號密碼
                string id = data["id"].ToString();
                string password = data["password"].ToString();

                string sqlStr = "SELECT name FROM users WHERE id = @userId AND password = @userPassword";
                dynamic queryResult;

                using (var conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["MES"].ConnectionString))
                {
                    queryResult = conn.Query(sqlStr, new
                    {
                        userId = id,
                        userPassword = password
                    });
                }

                foreach (var row in queryResult)
                {
                    var fields = row as IDictionary<string, object>;
                    result += 1;
                }

                if (result > 0)
                {
                    //2.清除重複登入
                    CheckSameIdAndForceLogout(id);

                    //3.建立登入金鑰
                    var loginKey = new HttpCookie("loginKey", GenerateLoginKey());
                    loginKey.Expires = DateTime.Now.AddHours(1);
                    loginKey.Domain = Request.RequestUri.Host;
                    loginKey.Path = "/";

                    //4.儲存登入金鑰
                    HttpContext.Current.Response.Cookies.Add(loginKey);

                    sqlStr = @"INSERT INTO userLoginKey (loginkey,userId,expireTime,loginTime) 
                                VALUES(@loginkey,@userId,@expireTime,@loginTime);";

                    using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["MES"].ConnectionString))
                    {
                        conn.Execute(sqlStr, new
                        {
                            loginkey = loginKey.Value,
                            userId = id,
                            expireTime = loginKey.Expires,
                            loginTime = DateTime.Now
                        });
                    }

                    returnMsg.Add("result", "done");
                }
                else
                {
                    throw new Exception("failed");
                }
            }
            catch (Exception ex)
            {
                general.SaveErrorLog(ex.Message, MethodBase.GetCurrentMethod().DeclaringType.Name);
                returnMsg.Add("result", ex.Message);
            }

            return returnMsg;
        }

        /// <summary>
        /// 檢查登入狀態
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JObject LoginStatusCheck()
        {
            var returnMsg = new JObject();

            try
            {
                var loginKey = HttpContext.Current.Request.Cookies["loginKey"];
                DateTime expireTime = new DateTime();
                var logoutTime = new object();

                //無登入金鑰表示未登入
                if (loginKey == null)
                {
                    returnMsg.Add("result", false);
                }
                else
                {
                    //有金鑰檢查金鑰有效性
                    string sql = "SELECT expireTime,logoutTime ";
                    sql += "FROM userLoginKey ";
                    sql += "WHERE loginkey = @loginkey";

                    dynamic queryResult;

                    using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["MES"].ConnectionString))
                    {
                        queryResult = conn.Query(sql, new
                        {
                            loginkey = loginKey.Value
                        });
                    }

                    foreach (var row in queryResult)
                    {
                        var fields = row as IDictionary<string, object>;
                        expireTime = (DateTime)fields["expireTime"];
                        logoutTime = fields["logoutTime"];
                    }

                    //此金鑰已登出
                    if (logoutTime != null)
                    {
                        returnMsg.Add("result", false);
                    }
                    else
                    {
                        if (expireTime.Year != 1)
                        {
                            //檢查金鑰有效期限
                            int diff = DateTime.Compare(DateTime.Now, expireTime);

                            returnMsg.Add("result", diff < 0 ? true : false);
                        }
                        else
                        {
                            //查無金鑰
                            returnMsg.Add("result", false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                general.SaveErrorLog(ex.Message, MethodBase.GetCurrentMethod().DeclaringType.Name);
                returnMsg.Add("result", ex.Message);
            }

            return returnMsg;
        }

        /// <summary>
        /// 根據登入金鑰取得使用者資料並存至快取中
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JObject LoadUserData()
        {
            var userData = new JObject();
            var loginKey = HttpContext.Current.Request.Cookies["loginKey"];
            ObjectCache cache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy();
            dynamic queryResult;
            string permission = "";

            try
            {
                string sqlStr = "SELECT A.id ,A.name,B.groupId ";
                sqlStr += "FROM users A ";
                sqlStr += "INNER JOIN r_userGroup B ON A.id = B.userId ";
                sqlStr += "INNER JOIN userLoginKey C ON A.id = C.userId ";
                sqlStr += "WHERE C.loginkey = @key";

                using (var conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["MES"].ConnectionString))
                {
                    queryResult = conn.Query(sqlStr, new
                    {
                        key = loginKey.Value
                    });
                }

                foreach (var row in queryResult)
                {
                    var fields = row as IDictionary<string, object>;

                    //登入者姓名存入回傳資料
                    if (userData.Count == 0)
                    {
                        userData.Add("name", fields["name"].ToString());
                    }

                    //使用者權限群組
                    permission += fields["groupId"].ToString();
                }

                policy.AbsoluteExpiration = DateTime.Now.AddHours(2);
                cache.Add("permissions", permission, policy);
            }
            catch (Exception ex)
            {
                general.SaveErrorLog(ex.Message, MethodBase.GetCurrentMethod().DeclaringType.Name);
                userData.Add("errMsg", ex.Message);
            }

            return userData;
        }

        /// <summary>
        /// 依照登入金鑰取得對應帳號操作權限
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JObject GetAccessRightsByUserGroup()
        {
            var returnMsg = new JObject();
            ObjectCache cache = MemoryCache.Default;
            List<int> canAccessPages = new List<int>();
            List<int> mainItems = new List<int>();
            List<SubItem> subItems = new List<SubItem>();
            string permissions;

            try
            {
                string sqlStr = "";
                dynamic queryResult;

                //1.從Server快取中取得使用者所屬權限群組
                permissions = cache.Get("permissions").ToString();

                //2.根據權限群組取得可訪問頁面
                sqlStr = "SELECT A.pageId,B.mainNavId ";
                sqlStr += "FROM r_pageGroup A ";
                sqlStr += "LEFT JOIN navSubItem B ON B.pageId = A.pageId ";
                sqlStr += "WHERE groupId LIKE @groupId;";

                using (var conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["MES"].ConnectionString))
                {
                    queryResult = conn.Query(sqlStr, new
                    {
                        groupId = "%" + permissions + "%"
                    });
                }

                foreach (var row in queryResult)
                {
                    var fields = row as IDictionary<string, object>;

                    //紀錄該權限群組所有可訪問頁面
                    canAccessPages.Add((int)fields["pageId"]);

                    //紀錄掛載於父導覽項目的頁面(查詢用)
                    if (fields["mainNavId"] != null)
                    {
                        int mainIdx = (int)fields["mainNavId"];
                        if (mainItems.FindIndex(x => x == mainIdx) < 0)
                        {
                            mainItems.Add(mainIdx);
                        }
                    }
                }

                //3.取得附屬於主導覽項目之子頁面資料
                sqlStr = "SELECT A.mainNavId,A.sortIdx,A.title,B.pageFileloc ";
                sqlStr += "FROM navSubItem A INNER JOIN page B ON B.id = A.pageId ";
                sqlStr += "WHERE B.id IN @pages ";
                sqlStr += "ORDER BY A.mainNavId,A.sortIdx;";

                using (var conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["MES"].ConnectionString))
                {
                    subItems = (List<SubItem>)conn.Query<SubItem>(sqlStr, new
                    {
                        pages = canAccessPages
                    });
                }

                //4.將路由資料組成頁面導覽列結構
                sqlStr = "(SELECT A.sortIdx,A.id,A.title,B.pageFileloc ";
                sqlStr += "FROM navBar A INNER JOIN page B ON B.id = A.pageId ";
                sqlStr += "WHERE B.id IN @canAccessPages ";
                sqlStr += "UNION ";
                sqlStr += "SELECT A.sortIdx,A.id,A.title,null AS pageFileloc ";
                sqlStr += "FROM navBar A WHERE A.haveSubItem = 1 ";
                sqlStr += "AND id IN @mainItems)";
                sqlStr += "ORDER BY sortIdx;";

                using (var conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["MES"].ConnectionString))
                {
                    queryResult = conn.Query(sqlStr, new
                    {
                        canAccessPages,
                        mainItems
                    });
                }

                foreach (var row in queryResult)
                {
                    var fields = row as IDictionary<string, object>;

                    if (fields["pageFileloc"] == null) //包含子項目主選單
                    {
                        var mainMenu = new JArray();

                        foreach (var item in subItems.FindAll(x => x.mainNavId == (int)fields["id"]))
                        {
                            mainMenu.Add(JToken.FromObject(new
                            {
                                title = item.title,
                                templateUrl = item.pageFileloc
                            }));
                        }
                        returnMsg.Add(new JProperty(fields["title"].ToString(), mainMenu));
                    }
                    else //單獨連結
                    {
                        var router = JObject.FromObject(new
                        {
                            title = fields["title"].ToString(),
                            templateUrl = fields["pageFileloc"].ToString()
                        });
                        var routerName = Path.GetFileNameWithoutExtension(fields["pageFileloc"].ToString());

                        returnMsg.Add(new JProperty(routerName, router));
                    }
                }
            }
            catch (Exception ex)
            {
                general.SaveErrorLog(ex.Message, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }

            return returnMsg;
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JObject Logout()
        {
            JObject returnMsg = new JObject();
            HttpCookie loginKey = HttpContext.Current.Request.Cookies["loginKey"];

            try
            {
                if (loginKey != null)
                {
                    //移除客戶端金鑰
                    loginKey.Expires = DateTime.Now;
                    HttpContext.Current.Response.Cookies.Remove("loginKey");

                    //移除帳號對應權限紀錄
                    ObjectCache cache = MemoryCache.Default;
                    cache.Remove("permissions");

                    //更新帳號登出紀錄
                    string sqlStr = @"UPDATE userLoginKey SET logoutTime = @logoutTime ";
                    sqlStr += "WHERE loginkey = @key";

                    using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["MES"].ConnectionString))
                    {
                        conn.Execute(sqlStr, new
                        {
                            logoutTime = DateTime.Now,
                            key = loginKey.Value
                        });
                    }
                }

                returnMsg.Add("result", "done");
            }
            catch (Exception ex)
            {
                general.SaveErrorLog(ex.Message, MethodBase.GetCurrentMethod().DeclaringType.Name);
                returnMsg.Add("result", ex.Message);
            }

            return returnMsg;
        }

        /// <summary>
        /// 檢查同Id登入紀錄,並強制登出
        /// </summary>
        /// <returns></returns>
        public bool CheckSameIdAndForceLogout(string id)
        {
            bool result = false;

            string sqlStr = "UPDATE userLoginKey SET logoutTime = @Now ";
            sqlStr += "WHERE userId = @userId AND logoutTime IS NULL;";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["MES"].ConnectionString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        var rowsAffected = conn.Execute(
                            sqlStr, new
                            {
                                userId = id,
                                DateTime.Now
                            }, tran);

                        if (rowsAffected == 1)
                        {
                            tran.Commit();
                            result = true;
                        }
                        else
                        {
                            throw new Exception("failed");
                        }
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 生成登入金鑰
        /// </summary>
        /// <returns></returns>
        string GenerateLoginKey()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= (b + 1);
            }
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }

    }

    class SubItem
    {
        public int mainNavId { get; set; } //所屬父階層導覽項目編號
        public int sortIdx { get; set; }   //子階層排序
        public string title { get; set; }  //連結標題
        public string pageFileloc { get; set; } //實體頁面檔案路徑
    }

    class Router
    {
        public string title { get; set; }
        public string templateUrl { get; set; }
    }
}