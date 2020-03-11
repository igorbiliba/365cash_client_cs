using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _365cash_client_cs.Response
{
    public class RateCache365Response
    {
        public double rate;
        public double balance;

        public string toJson() => Newtonsoft.Json.JsonConvert.SerializeObject(this);
    }
}
