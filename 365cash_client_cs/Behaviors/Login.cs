using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using _365cash_client_cs.Components;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using Leaf.xNet;
using Leaf.xNet.Services.Cloudflare;
using _365cash_client_cs.Behaviors;
using _365cash_client_cs.Drivers;
using System.Threading;

namespace _365cash_client_cs.Behaviors
{
    public class Login
    {
        public string LOGIN_ACTION;

        public Leaf.xNet.HttpRequest httpRequest;
        public string BASE_URL;
        public string uname;
        public string passwrd;

        Leaf.xNet.HttpResponse initResponse;
        Leaf.xNet.HttpResponse loginResponse;

        public bool DoLogin()
        {
            //1. инициалицирует csrf и получает форму
            initResponse = httpRequest.Get(BASE_URL + LOGIN_ACTION);
            string initResponseContent = initResponse.ToString();

            //2. авторизация
            Parser parser = new Parser(initResponseContent);
            IElement form = parser.GetForm("login-form");
            List<ParserInputData> inputs = parser.GetInputsByForm(form);
            parser.ModifyInputs  (ref inputs, "LoginForm[identity]", uname)
                    .ModifyInputs(ref inputs, "LoginForm[password]", passwrd);

            loginResponse = httpRequest.Post(BASE_URL + LOGIN_ACTION, Helper.ConvertToRequestParams(inputs));
            string loginResponseContent = loginResponse.ToString();

            return loginResponseContent.IndexOf("/user/sign-in/logout") != -1;
        }
    }
}
