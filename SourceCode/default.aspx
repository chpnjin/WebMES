<%@ Page Language="C#" %>

<%@ Import Namespace="WebMES.Controllers" %>
<%@ Import Namespace="System.Runtime" %>

<script runat="server">
    void Page_Load(object sender, System.EventArgs e)
    {
        var api = new AccountController();
        var valid = api.LoginStatusCheck();

        if ((bool)valid["result"] == true)
        {
            DateTime expiresTime = HttpContext.Current.Request.Cookies["loginKey"].Expires;

            Server.Transfer("index.html");
        }
        else
        {
            Server.Transfer("login.html");
        }
    }
</script>
