// AltRank measures a coin's performance relative to the entire crypto market.
// The score combines altcoin price performance relative to Bitcoin with other
// social activity indicators across the crypto market.
// AltRank pairs -- https://lunarcrush.com/markets?metric=alt_rank

// The Galaxy Score measures how a coin is performing relative to its own past
// social and market performance. The score combines a variety of performance
// indicators across crypto markets and social channels. Scores over 50 are
// more bullish while scores below 50 are more bearish.
// Galaxy Score pairs -- https://lunarcrush.com/markets

using System;
using System.Linq;
using XCommas.Net;
using XCommas.Net.Objects;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Configuration;

namespace update_pairs
{
    class Program
    {
        //update with your 3c api key/secret and exchange info
        static string key = ConfigurationManager.AppSettings["3c_api_key"];
        static string secret = ConfigurationManager.AppSettings["3c_api_secret"];
        static string market = ConfigurationManager.AppSettings["3c_market"];
        static int accountId = Convert.ToInt32(ConfigurationManager.AppSettings["3c_accountId"]);
        static int botId = Convert.ToInt32(ConfigurationManager.AppSettings["3c_botId"]);
        static string quote = ConfigurationManager.AppSettings["quote"];

        public static XCommasApi api;
        public static HttpClient hc = new HttpClient();

        public static int MAX_ALTRANK_PAIRS = 15;
        public static int MAX_GALAXY_PAIRS = 15;

        static void Main() { MainAsync().GetAwaiter().GetResult(); }

        static async Task MainAsync()
        {
            api = new XCommasApi(key, secret, default, UserMode.Real);

            Console.WriteLine("API successfully connected, please standby ...");
            
            // Get list of all accounts, and find the one we want
            var accts = await api.GetAccountsAsync();
            foreach (var acct in accts.Data)
            {
                if (acct.MarketCode == market) { accountId = acct.Id; break; }
            }

            // get list of ALL market pairs available for account as well as the current blacklist
            var marketPairs = await api.GetMarketPairsAsync(market);
            var marketPairsBlacklisted = await api.GetBotPairsBlackListAsync();

            // Add each pair to a collection, unless its blacklisted
            HashSet<string> allPairsOnExchange = new HashSet<string>();
            foreach (string p in marketPairs.Data)
            {
                if (!marketPairsBlacklisted.Data.Pairs.Contains(p))
                    allPairsOnExchange.Add(p);
            }

            while (true)
            {
                try
                {
                    LunarCrushRoot res = await GetJSON<LunarCrushRoot>("https://api.lunarcrush.com/v2?data=market&type=fast&sort=acr&limit=1000&key=asdf");
                    HashSet<string> pairsToUpdate = new HashSet<string>();
                    foreach (Datum d in res.data)
                    {
                        if (pairsToUpdate.Count >= MAX_ALTRANK_PAIRS) break;
                        string pair = $"{quote}_{d.s}";
                        if (allPairsOnExchange.Contains(pair) && !pairsToUpdate.Contains(pair)) pairsToUpdate.Add(pair);
                    }
                    int altRankPairsCount = pairsToUpdate.Count;

                    res = await GetJSON<LunarCrushRoot>("https://api.lunarcrush.com/v2?data=market&type=fast&sort=gs&limit=1000&key=asdf&desc=True");
                    //Console.Write("Updating Galaxy Score pairs: ");
                    foreach (Datum d in res.data)
                    {
                        if (pairsToUpdate.Count >= altRankPairsCount + MAX_GALAXY_PAIRS) break;
                        string pair = $"{quote}_{d.s}";
                        if (allPairsOnExchange.Contains(pair) && !pairsToUpdate.Contains(pair)) pairsToUpdate.Add(pair);
                    }

                    var sb = await api.ShowBotAsync(botId: botId);
                    Bot bot = sb.Data;

                    var pairsRemoving = bot.Pairs.Except(pairsToUpdate);
                    var pairsAdding = pairsToUpdate.Except(bot.Pairs);

                    bot.Pairs = pairsToUpdate.ToArray();

                    var ub = await api.UpdateBotAsync(botId, new BotUpdateData(bot));
                    if (ub.IsSuccess)
                    {
                        Console.WriteLine($"Successfully updated {bot.Name} with {pairsToUpdate.Count} total pairs..");
                        Console.WriteLine("  Removed: " + string.Join(" ", pairsRemoving));
                        Console.WriteLine("    Added: " + string.Join(" ", pairsAdding));
                        Console.WriteLine("");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: " + ex.Message);
                    api = new XCommasApi(key, secret, default, UserMode.Real);
                    hc = new HttpClient();
                }
                //update every five minutes
                await Task.Delay(1000 * 60 * 5);
            }
        }

        static async Task<dynamic> GetJSON<T>(string endpoint)
        {
            var res = await hc.GetAsync(endpoint);
            if (res.StatusCode == HttpStatusCode.OK)
            {
                string json = await res.Content.ReadAsStringAsync();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            return null;
        }
    }
}
