using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaHeadHunter
{
    class Parser
    {
        private static readonly Logger logger = Logger.GetLogger();

        public static Player[] ParseList(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            List<HtmlNode> rawPlayers = doc.DocumentNode.Descendants()
                .Where(x => x.Name == "tr" && x.ParentNode.ParentNode.Attributes["class"] != null && x.ParentNode.ParentNode.Attributes["class"].Value.Contains("rating")).ToList();

            List<Player> players = new List<Player>();
            foreach (var p in rawPlayers)
            {
                var data = p.Descendants("td").ToArray();

                Player player = new Player();
                // player column
                player.Name = data[0].InnerText;
                player.Class = data[0].Descendants().ToArray()[0].Attributes["aria-label"].Value;
                player.URL = data[0].Descendants().ToArray()[0].Attributes["href"].Value;

                // guild column
                player.GuildName = data[1]?.InnerText;
                if (player.GuildName != "&nbsp;")
                {
                    player.GuildFaction = data[1].Descendants().ToArray()[0].Attributes["class"].Value;
                    player.GuildURL = data[1].Descendants().ToArray()[0].Attributes["href"].Value;
                }
                else
                    player.GuildName = "";

                // raid column is data[2]
                // ...

                    // ilvl column
                player.ilvl = data[3].InnerText;

                // date column
                player.Timestamp = data[4].Descendants("span").ToArray()[0].Attributes["data-ts"].Value;

                players.Add(player);
            }

            return players.ToArray();
        }

        public static Player ParsePlayer(Player player, string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var descDiv = doc.DocumentNode.Descendants().FirstOrDefault(x => x.Name == "div" && x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("charCommentary"));
            if (descDiv != null)
                player.Description = descDiv.InnerText;

            int killedBosses = 0;
            int totalBosses = 0;
            var divRaidTier = doc.DocumentNode.Descendants().FirstOrDefault(x => x.Name == "div" && x.Attributes["id"] != null && x.Attributes["id"].Value.StartsWith("tier_"));
            if (divRaidTier != null)
            {
                List<HtmlNode> rawKilledBosses = divRaidTier.Descendants()
                    .Where(x => x.Name == "span" && x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("progress_heroic")).ToList();

                List<HtmlNode> rawTotalBosses = divRaidTier.Descendants()
                    .Where(x => x.Name == "span" && x.Attributes["class"] != null
                    && (x.Attributes["class"].Value.Contains("progress_heroic") || x.Attributes["class"].Value.Contains("progress_normal") || x.Attributes["class"].Value.Contains("progress_none"))).ToList();
                
                killedBosses = rawKilledBosses.Count;
                totalBosses = rawTotalBosses.Count;
            }
            player.Progress = $"{killedBosses}/{totalBosses} (M)";

            player.Specs = doc.DocumentNode.Descendants().FirstOrDefault(x => x.Name == "div" && x.InnerText.StartsWith("Specs playing: "))?.InnerText.Replace("Specs playing: ", "");
            player.BattleNet = doc.DocumentNode.Descendants().FirstOrDefault(x => x.Name == "div" && x.InnerText.StartsWith("Battletag: "))?.InnerText.Replace("Battletag: ", "");

            return player;
        }

        internal static Player ParseGuild(Player player, string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            player.GuildProgress = doc.DocumentNode.Descendants().First(x => (x.Name == "span" && x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("ratingProgress"))).InnerText.Trim();

            return player;
        }
    }
}
