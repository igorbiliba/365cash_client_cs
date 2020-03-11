using AngleSharp.Dom;
using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace _365cash_client_cs.Components
{
    public class Helper
    {
        public static RequestParams ConvertToRequestParams(List<ParserInputData> inputs, List<string> excludeKeys = null)
        {
            var formParams = new RequestParams();

            foreach (var input in inputs)
            {
                if (excludeKeys != null && excludeKeys.IndexOf(input.name) != -1)
                    continue;

                string valParam = input.value;
                string keyParam = input.name;

                formParams.Add(new KeyValuePair<string, string>(keyParam, valParam));
            }

            return formParams;
        }

        public static Leaf.xNet.HttpResponse Submit(
            string formId,
            Leaf.xNet.HttpResponse response,
            string baseUrl,
            ref Leaf.xNet.HttpRequest httpRequest,
            List<KeyValuePair<string, string>> changeInputs)
        {
            string responeContent = response.ToString();
            Parser parser = new Parser(responeContent);
            IElement form = parser.GetForm(formId);
            List<ParserInputData> inputs = parser.GetInputsByForm(form);

            if(changeInputs != null)
                foreach(var changeInput in changeInputs)
                {
                    parser
                        .ModifyInputs(
                            ref inputs,
                            changeInput.Key,
                            changeInput.Value
                        );
                }

            string action = baseUrl + form.GetAttribute("action").Substring(1);

            return httpRequest.Post(action, Helper.ConvertToRequestParams(inputs));
        }

        public static bool IsBanForever(Leaf.xNet.HttpResponse response)
        {
            string content = response.ToString();

            return content.ToUpper().IndexOf("IP ЗАБЛОКИРОВАН НАВСЕГДА".ToUpper()) != -1;
        }

        public static bool IsBan(Leaf.xNet.HttpResponse response)
            => response.ToString().IndexOf("БАН IP") != -1;
    }
}
