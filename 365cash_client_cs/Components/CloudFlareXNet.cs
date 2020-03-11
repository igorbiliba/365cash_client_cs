using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using _365cash_client_cs.Drivers;
using Leaf.xNet;
using Leaf.xNet.Services.Cloudflare;


namespace _365cash_client_cs.Components
{
    public class CloudFlareXNet
    {
        public Leaf.xNet.HttpResponse Get(string url, ref Leaf.xNet.HttpRequest req)
        {
            Leaf.xNet.HttpResponse con = null;
            var JSEngine = new Jint.Engine();
            var uri = new Uri(url);

            try
            {
                req.Cookies = new CookieStorage();
                req.UserAgent = Http.ChromeUserAgent();
                //Делаем запрос к сайту с защитой CF, вероятнее всего он выдаст ошибку 503 и отправимся мы в блок Catch
                con = req.Get(url);
            }
            catch (Exception)
            {
                //В переменную Con заносим ответ от сайта и парсим нужную инфу
                con = req.Response;
                var challenge = Regex.Match(con.ToString(), "name=\"jschl_vc\" value=\"(\\w+)\"").Groups[1].Value;
                var challenge_pass = Regex.Match(con.ToString(), "name=\"pass\" value=\"(.+?)\"").Groups[1].Value;

                var builder = Regex.Match(con.ToString(), @"setTimeout\(function\(\){\s+(var t,r,a,f.+?\r?\n[\s\S]+?a\.value =.+?)\r?\n").Groups[1].Value;
                builder = Regex.Replace(builder, @"a\.value =(.+?) \+ .+?;", "$1");
                builder = Regex.Replace(builder, @"\s{3,}[a-z](?: = |\.).+", "");
                builder = Regex.Replace(builder, @"[\n\\']", "");

                //Выполняем JS
                long solved = long.Parse(JSEngine.Execute(builder).GetCompletionValue().ToObject().ToString());
                solved += uri.Host.Length; //add the length of the domain to it.

                //Ждем 3 сек, иначе CF пошлет нас к хуям
                Thread.Sleep(3000);

                //Генерируем запрос
                string cookie_url = string.Format("{0}://{1}/cdn-cgi/l/chk_jschl", uri.Scheme, uri.Host);
                var uri_builder = new UriBuilder(cookie_url);
                var query = HttpUtility.ParseQueryString(uri_builder.Query);
                query["jschl_vc"] = challenge;
                query["pass"] = challenge_pass;
                query["jschl_answer"] = solved.ToString();
                uri_builder.Query = query.ToString();

                req.AllowAutoRedirect = false;
                req.Referer = url;

                //Отправляем запрос
                con = req.Get(uri_builder.Uri);
            }

            return con;
        }
    }
}
