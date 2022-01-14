using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace update_pairs
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Performance
    {
        public double? hour { get; set; }
        public double? day { get; set; }
        public double? week { get; set; }
        public double? month { get; set; }
        public double? year { get; set; }
        public double? min15 { get; set; }
        public double? min5 { get; set; }
    }

    public class Usd
    {
        public double price { get; set; }
        public object marketcap { get; set; }
        public object volume { get; set; }
        public Performance performance { get; set; }
    }

    public class Aud
    {
        public double price { get; set; }
        public object marketcap { get; set; }
        public object volume { get; set; }
        public Performance performance { get; set; }
    }

    public class Cad
    {
        public double price { get; set; }
        public object marketcap { get; set; }
        public object volume { get; set; }
        public Performance performance { get; set; }
    }

    public class Eur
    {
        public double price { get; set; }
        public object marketcap { get; set; }
        public object volume { get; set; }
        public Performance performance { get; set; }
    }

    public class Gbp
    {
        public double price { get; set; }
        public object marketcap { get; set; }
        public object volume { get; set; }
        public Performance performance { get; set; }
    }

    public class Pln
    {
        public double price { get; set; }
        public object marketcap { get; set; }
        public object volume { get; set; }
        public Performance performance { get; set; }
    }

    public class Rub
    {
        public double price { get; set; }
        public object marketcap { get; set; }
        public object volume { get; set; }
        public Performance performance { get; set; }
    }

    public class Inr
    {
        public double price { get; set; }
        public object marketcap { get; set; }
        public object volume { get; set; }
        public Performance performance { get; set; }
    }

    public class Try
    {
        public double price { get; set; }
        public object marketcap { get; set; }
        public object volume { get; set; }
        public Performance performance { get; set; }
    }

    public class Brl
    {
        public double price { get; set; }
        public object marketcap { get; set; }
        public object volume { get; set; }
        public Performance performance { get; set; }
    }

    public class Btc
    {
        public double price { get; set; }
        public int marketcap { get; set; }
        public int volume { get; set; }
        public Performance performance { get; set; }
    }

    public class Eth
    {
        public double price { get; set; }
        public int marketcap { get; set; }
        public int volume { get; set; }
        public Performance performance { get; set; }
    }

    public class Data
    {
        public Usd usd { get; set; }
        public Aud aud { get; set; }
        public Cad cad { get; set; }
        public Eur eur { get; set; }
        public Gbp gbp { get; set; }
        public Pln pln { get; set; }
        public Rub rub { get; set; }
        public Inr inr { get; set; }
        public Try @try { get; set; }
        public Brl brl { get; set; }
        public Btc btc { get; set; }
        public Eth eth { get; set; }
    }

    public class Bubble500Root
    {
        public string id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public int rank { get; set; }
        public string symbol { get; set; }
        public string binanceSymbol { get; set; }
        public string kucoinSymbol { get; set; }
        public string gateioSymbol { get; set; }
        public string coinbaseSymbol { get; set; }
        public string ftxSymbol { get; set; }
        public string image { get; set; }
        public double dominance { get; set; }
        public Data data { get; set; }
    }


}