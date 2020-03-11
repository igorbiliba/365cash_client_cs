using _365cash_client_cs.Components;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _365cash_client_cs.Parsers
{
    public class ParseFinalPage
    {
        HtmlParser parser;
        IHtmlDocument document;
        IHtmlCollection<IElement> fields;

        public ParseFinalPage(string content)
        {
            parser   = new HtmlParser();
            document = parser.ParseDocument(content);
            fields   = document.GetElementsByClassName("field-group");
        }

        IElement GetFieldBy(string neelde)
        {
            foreach (var field in fields)
                if (field.OuterHtml.IndexOf(neelde) != -1)
                    return field;

            return null;
        }

        /**
         * 
         *  <span class="pull-right text-right">
         *   <span class="status-text color-0">В обработке</span>
         *   <br>Ожидает оплаты            
         *  </span>
         * 
         * */
        public string GetStatus()
        {
            try
            {
                const string NEEDLE = "В обработке";

                var field = GetFieldBy(NEEDLE);
                var span = field.GetElementsByTagName("span")[0];
                return span
                    .TextContent
                    .Replace(NEEDLE, "")
                    .Trim();
            }
            catch (Exception) { return String.Empty; }
        }

        public string GetNumberTicket()
        {
            try
            {
                const string NEEDLE = "Номер заявки:";

                var field = GetFieldBy(NEEDLE);
                var b = field.GetElementsByTagName("b")[0];
                return b
                    .TextContent
                    .Trim();
            }
            catch (Exception) { return String.Empty; }
        }
        
        public string GetRubAmount()
        {
            try
            {
                const string NEEDLE = "Отдаю Qiwi:";

                var field = GetFieldBy(NEEDLE);
                var b = field.GetElementsByTagName("b")[0];
                return b
                    .TextContent
                    .Replace("RUB", "")
                    .Trim();
            }
            catch (Exception) { return String.Empty; }
        }

        public string GetBtcAmount()
        {
            try
            {
                const string NEEDLE = "Получаю Bitcoin:";

                var field = GetFieldBy(NEEDLE);
                var b = field.GetElementsByTagName("b")[0];
                return b
                    .TextContent
                    .Replace("BTC", "")
                    .Trim();
            }
            catch (Exception) { return String.Empty; }
        }

        public string GetBtcAddr()
        {
            try
            {
                const string NEEDLE = "На счет:";

                var field = GetFieldBy(NEEDLE);
                var b = field.GetElementsByTagName("div")[0];
                return b
                    .TextContent
                    .Trim();
            }
            catch (Exception) { return String.Empty; }
        }

        public string GetCardId()
        {   
            const string PREFIX = "hb-in-card-number-";
            var items = document.GetElementsByTagName("span");

            foreach(var item in items)
            {
                try
                {
                    if (item.GetAttribute("id").IndexOf(PREFIX) == -1)
                        continue;
                }
                catch (Exception) { continue; }

                return item
                    .GetAttribute("id")
                    .Replace(PREFIX, "")
                    .Trim();
            }
                

            return String.Empty;
        }
    }
}
