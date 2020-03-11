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
using System.IO;
using _365cash_client_cs.Parsers;

namespace _365cash_client_cs.Behaviors
{
    public class CreateTicket
    {
        public string TICKET_ACTION;

        public Leaf.xNet.HttpRequest httpRequest;
        public string BASE_URL;
        public string phone;
        public double amount;
        public string btcAddr;

        CloudFlareXNet cloudFlare = new CloudFlareXNet();

        Leaf.xNet.HttpResponse initResponse;
        Leaf.xNet.HttpResponse step1Response;        
        Leaf.xNet.HttpResponse step2Response;
        Leaf.xNet.HttpResponse step3Response;
        Leaf.xNet.HttpResponse finalStepResponse;

        public string finalStepContent
        {
            get { return finalStepResponse.ToString(); }
        }

        static double GetBtcRateByTicketForm(IElement form)
        {
            var p = Cash365.GetById(form, "p", "pre-order-form-course");
            string val = p.GetElementsByTagName("span")[0].InnerHtml;

            try
            {
                return double.Parse(val);
            }
            catch (Exception) { }

            return double.Parse(val.Replace('.', ','));
        }

        public static double GetAmountRecive(double amount, double costBtc)
        {
            return amount / costBtc;
        }

        public void MainPage()
        {
            //1. инициалицирует csrf и получает форму
            initResponse = httpRequest.Get(BASE_URL + TICKET_ACTION);
            if (Helper.IsBan(initResponse))
                throw new Exception("БАН");
            
            //2. заполняем поля и посылаем форму
            List<KeyValuePair<string, string>> changeInputs = new List<KeyValuePair<string, string>>();

            Parser parser = new Parser(initResponse.ToString());
            IElement form = parser.GetForm("order-form");
            double costBtc   = GetBtcRateByTicketForm(form);
            double amountBtc = GetAmountRecive(amount, costBtc);

            changeInputs.Add(new KeyValuePair<string, string>("SellForm[sell_amount]", amount.ToString().Replace(',', '.'))    );
            changeInputs.Add(new KeyValuePair<string, string>("SellForm[sell_source]", phone)                                  );
            changeInputs.Add(new KeyValuePair<string, string>("BuyForm[buy_amount]",   amountBtc.ToString().Replace(',', '.')) );
            changeInputs.Add(new KeyValuePair<string, string>("BuyForm[buy_target]",   btcAddr)                                );

            step1Response = Helper.Submit("order-form", initResponse, BASE_URL, ref httpRequest, changeInputs);
            if (Helper.IsBan(step1Response))
                throw new Exception("БАН");
        }

        public void Step1Page()
        {
            step2Response = Helper.Submit("order-form", step1Response, BASE_URL, ref httpRequest, null);

            if (Helper.IsBan(step2Response))
                throw new Exception("БАН");
        }

        public void Step2Page(string email = "")
        {
            List<KeyValuePair<string, string>> changeInputs = new List<KeyValuePair<string, string>>();
            changeInputs.Add(new KeyValuePair<string, string>("PreOrderForm[email]", email));

            step3Response = Helper.Submit(
                "order-form",
                step2Response,
                BASE_URL,
                ref httpRequest,
                email == "" ? null : changeInputs
            );
            
            if (Helper.IsBan(step3Response))
                throw new Exception("БАН");
        }

        public void Step3Page()
        {
            finalStepResponse = Helper.Submit("order-form", step3Response, BASE_URL, ref httpRequest, null);

            if (Helper.IsBan(finalStepResponse))
                throw new Exception("БАН");
        }

        public class QIWINumberRespone
        {
            public bool success { get; set; }
            public string html { get; set; }

            public string GetPhone()
            {
                try
                {
                    int start = html.IndexOf(">");
                    int end = html.IndexOf("<", start);

                    if (start == -1 || end == -1) return html;

                    return html
                        .Substring(start, end - start)
                        .Replace("<", "")
                        .Replace(">", "")
                        .Replace(" ", "");
                }
                catch (Exception) { }

                return html;
            }
        }

        public QIWINumberRespone GetQIWINumber()
        {
            string id = new ParseFinalPage(
                    finalStepResponse.ToString()
                )
                .GetCardId();

            httpRequest.AddXmlHttpRequestHeader();
            Leaf.xNet.HttpResponse qiwiNumberResponse = httpRequest
                .Get(BASE_URL + "/order/ajax?method=showHB&id=" + id + "&value=in-card-number");

            if (Helper.IsBan(qiwiNumberResponse))
                throw new Exception("БАН");

            return Newtonsoft.Json.JsonConvert.DeserializeObject<QIWINumberRespone>(qiwiNumberResponse.ToString());
        }
    }
}
