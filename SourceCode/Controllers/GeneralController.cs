using Dapper;
using System;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Web.Http;

namespace WebMES.Controllers
{
    /// <summary>
    /// 全網站通用資料操作
    /// </summary>
    public class GeneralController : ApiController
    {
        /// <summary>
        /// 儲存伺服器錯誤紀錄
        /// </summary>
        /// <param name="msg">錯誤訊息</param>
        /// <param name="name">產生錯誤的來源函式名稱</param>
        public void SaveErrorLog(string msg,string name)
        {
            try
            {
                //1.存至DB
                string sqlStr = @"INSERT INTO errorLog VALUES(@errMessage,@funcName,@errTime);";

                using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["MES"].ConnectionString))
                {
                    conn.Execute(sqlStr, new
                    {
                        errMessage = msg,
                        funcName = name,
                        errTime = DateTime.Now
                    });
                }

                //2.寄送至系統管理員E-mail


            }
            catch (Exception ex)
            {

            }
        }
    }
}
