using Microsoft.AspNetCore.Mvc;

namespace Asu19.Areas.Account.MyResult
{
    public class HtmlResult : IActionResult
    {
        string htmlCode;
        public HtmlResult(string html)
        {
            htmlCode = "<h2>" + string.Join("</h2><h2>", html.Split(";")) + "</h2>";
        }
        public async Task ExecuteResultAsync(ActionContext context)
        {
            string fullHtmlCode =
                @$"<!DOCTYPE html>
                <html>
                    <head>
                        <title>Ошибка</title>
                        <meta charset=utf-8 />
                    </head>
                    <body>
                        <h1>Список ошибок</h1>
                        {htmlCode}
                        <a href=""/login"">Вернуться к аутентификации</a>
                    </body>
                </html>";
            await context.HttpContext.Response.WriteAsync(fullHtmlCode);
        }
    }
}
