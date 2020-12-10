$(function () {
    var routes = {};
    var defaultRoute = 'home'; //預設路由名稱


    //根據登入者所屬權限設定可訪問頁面
    $.ajax({
        type: "POST",
        url: "api/Account/GetAccessRightsByUserGroup",
        dataType: "json",
        async: false,
        success: function (routers) {
            var navbar = "<ul>";

            //建立路由與生成導覽列
            $.each(routers, function (mainTitle, val) {
                var haveSubMenu = Array.isArray(val);
                var link = "";

                //有子選項
                if (haveSubMenu) {
                    link = "<li>";
                    link += "<a href='#'><span>@mainTitle</span></a>";
                    link = link.replace('@mainTitle', mainTitle);
                    link += "<ul>";

                    val.forEach(function (subItem) {
                        debugger;
                        link += "<li><a href='@href'><span>@title</span></a></li>";
                        var routerName = subItem.templateUrl.replace(/^.*[\\\/]/, '');

                        routerName = routerName.split('.').slice(0, -1).join('.');

                        link = link.replace('@href', '#/' + routerName);
                        link = link.replace('@title', subItem.title);

                        routes[routerName] = {
                            url: '#/' + routerName,
                            templateUrl: subItem.templateUrl
                        };
                    });

                    link += "</ul></li>";
                }
                else { //單獨連結
                    link = "<li><a href='@href'><span>@title</span></a></li>";
                    var routerName = val.templateUrl.replace(/^.*[\\\/]/, '');

                    routerName = routerName.split('.').slice(0, -1).join('.');

                    link = link.replace('@href', '#/' + routerName);
                    link = link.replace('@title', val.title);

                    routes[routerName] = {
                        url: '#/' + routerName,
                        templateUrl: val.templateUrl
                    };
                }

                navbar += link;
            });

            navbar += "</ul>";
            $('#cssmenu').append(navbar);
        }
    });

    //頁面完全顯示完畢後
    $.when($.ready)
        .then(function () {
            //設定路由基本資料
            $.router
                .setData(routes)
                .setDefault(defaultRoute);

            $.router.run('#content', 'home');
        });

    //事件：路由切換前觸發
    $.router.onRouteBeforeChange(function (e, route, params) {
        //console.log("route Before changed");

        $.ajax({
            type: "POST",
            url: "api/Account/LoginStatusCheck",
            contentType: 'application/json; charset=utf-8',
            dataType: "json",
            success: function (response) {
                if (response.result == false) {
                    location.replace("/");
                }
            }
        });
    });
});