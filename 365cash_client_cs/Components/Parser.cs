using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace _365cash_client_cs.Components
{
    public class Parser
    {
        string content;
        public Parser(string content)
        {
            this.content = content;
        }

        public IElement GetForm(string id)
        {
            var parser = new HtmlParser();
            var document = parser.ParseDocument(content);

            return document.GetElementById(id);
        }

        public List<ParserInputData> GetInputsByForm(IElement form)
        {
            List<ParserInputData> list = new List<ParserInputData>();

            foreach (IElement input in form.QuerySelectorAll("input"))
            {
                list.Add( new ParserInputData(input) );
            }

            return list;
        }

        public string GetCsrf(List<ParserInputData> inputs)
        {
            try
            {
                return inputs.Where(el => el.name == "_csrf")
                    .First()
                    .value;
            }
            catch (Exception) { }

            return null;
        }

        public Parser ModifyInputs(ref List<ParserInputData> inputs, string name, string value)
        {
            for (int i = 0; i < inputs.Count(); i++)
                if (inputs[i].name.ToLower().Trim() == name.ToLower().Trim())
                    inputs[i].value = value;

            return this;
        }
    }
}
