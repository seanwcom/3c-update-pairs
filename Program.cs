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
        static string baseType = ConfigurationManager.AppSettings["baseType"];
        static bool usePerp = Boolean.Parse(ConfigurationManager.AppSettings["usePerp"] ?? "false");
        static int botId = Convert.ToInt32(ConfigurationManager.AppSettings["3c_botId"]);
        static int accountId = Convert.ToInt32(ConfigurationManager.AppSettings["3c_accountId"]);


        public static XCommasApi api;
        public static HttpClient hc = new HttpClient();

        public static int MAX_BUBBLE_PAIRS = 50;

        static void Main() { MainAsync().GetAwaiter().GetResult(); }

        static async Task MainAsync()
        {
            string perp = ((usePerp) ? "-PERP" : "");

            api = new XCommasApi(key, secret, default, UserMode.Real);

            Console.WriteLine("API successfully connected, please standby ...");
            
            // Get list of all accounts, and find the one we want
            var accts = await api.GetAccountsAsync();
            if (accountId != 0)
            {
                foreach (var acct in accts.Data)
                {
                    if (acct.MarketCode == market) { accountId = acct.Id; break; }
                }
            }

            // get list of ALL market pairs available for account as well as the current blacklist
            var marketPairs = await api.GetMarketPairsAsync(market);
            var marketPairsBlacklisted = await api.GetBotPairsBlackListAsync();

            // Add each pair to a collection, unless its blacklisted
            HashSet<string> pairs = new HashSet<string>();
            foreach (string p in marketPairs.Data)
            {
                if (!marketPairsBlacklisted.Data.Pairs.Contains(p))
                    pairs.Add(p + perp);
            }

            while (true)
            {
                try
                {
                    #region LunarCrush
                    /*
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
                    foreach (Datum d in res.data)
                    {
                        if (pairsToUpdate.Count >= altRankPairsCount + MAX_GALAXY_PAIRS) break;
                        string pair = $"{quote}_{d.s}";
                        if (allPairsOnExchange.Contains(pair) && !pairsToUpdate.Contains(pair)) pairsToUpdate.Add(pair);
                    }
                    */
                    #endregion

                    #region CryptoBubbles
                    Console.WriteLine("Adding cryptobubble pairs...");
                    List<Bubble500Root> bubbles = await GetJSON<List<Bubble500Root>>("https://cryptobubbles.net/backend/data/currentBubbles500.json");

                    bubbles = bubbles.OrderByDescending(x => x.data.usd.performance.min5).ToList();

                    HashSet<string> pairsToUpdate = new HashSet<string>();
                    int idx = 1;
                    foreach (Bubble500Root bubble in bubbles)
                    {
                        if (pairsToUpdate.Count >= MAX_BUBBLE_PAIRS) break;
                        string pair = $"{baseType}_{bubble.symbol}{perp}";
                        if (!pairsToUpdate.Contains(pair))
                        {
                            if (pairs.Contains(pair))
                            {
                                pairsToUpdate.Add(pair);
                                //Console.WriteLine($"{idx}) Added {pair} on {market}, performance: {bubble.data.usd.performance.min5}");
                            }
                        }
                        idx++;
                    }
                    #endregion

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
                    else
                    {
                        //I couldn't find the market code for ftx futures although it probably exists
                        if (ub.Error.Contains("No market data for this pair"))
                        {
                            string[] badPairs = ub.Error.Split(": ").Select(p => p.Substring(0, p.IndexOf('"'))).ToArray();
                            foreach (string badPair in badPairs)
                                if (pairsToUpdate.Contains(badPair))
                                {
                                    Console.WriteLine($"Removed {badPair} on {market} because it only exists on spot");
                                    pairsToUpdate.Remove(badPair);
                                }
                                else if (badPair.Contains(baseType)) Console.WriteLine(badPair + " malformed?");
                            bot.Pairs = pairsToUpdate.ToArray();
                            ub = await api.UpdateBotAsync(botId, new BotUpdateData(bot));
                            if (ub.IsSuccess) Console.WriteLine($"\nSuccessfully updated {bot.Name} with {pairsToUpdate.Count} new pairs..");
                            else Console.WriteLine($"ERROR: {ub.Error}");
                        }
                        else
                        {
                            Console.WriteLine($"ERROR: {ub.Error}");
                        }
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
