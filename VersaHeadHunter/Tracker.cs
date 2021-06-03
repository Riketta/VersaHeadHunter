using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace VersaHeadHunter
{
    public class Tracker
    {
        private static readonly Logger logger = Logger.GetLogger();

        public delegate void NewPlayerDataEventHandler(object sender, NewPlayerDataEventArgs e);
        public static event NewPlayerDataEventHandler NewPlayerData;

        int postDelay = 72 * 60 * 60; // spam prevention delay (if profile updated too often)
        int outdatedThreshold = 24 * 60 * 60; // threshold to not post older players that was already published (like they went back from second page)
        Timer timer = null;
        string url = null;

        /// <summary>
        /// Default tracker for dedicated page (realm)
        /// </summary>
        /// <param name="url">wowprogress page to track</param>
        /// <param name="interval">interval to attempt to parse in minutes</param>
        public Tracker(string url, int interval = 15)
        {
            this.url = url;
            timer = new Timer(interval * 60 * 1000);
        }

        public void Start()
        {
            logger.Info($"Starting tracker for \"{url}\"");

            Timer_Elapsed(null, null);

            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        public void Stop()
        {
            logger.Info($"Stopping tracker for \"{url}\"");
            timer.Stop();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                logger.Info($"Parsing data for \"{url}\"");
                string data = Downloader.DownloadURL(url);
                var players = Parser.ParseList(data);

                foreach (var p in players)
                {
                    Player localPlayer = null;
                    if (Storage.Players.ContainsKey(url))
                        localPlayer = Storage.Players[url].FirstOrDefault(x => x.Name == p.Name);

                    if ((localPlayer == null || (localPlayer.Timestamp != p.Timestamp && int.Parse(p.Timestamp) - int.Parse(localPlayer.Timestamp) > postDelay)) &&
                        new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() - int.Parse(p.Timestamp) <= outdatedThreshold)
                    {
                        logger.Info($"New player data found for \"{p.Name}\"");
                        p.URL = new Uri(new Uri(url), p.URL).ToString();
                        string htmlPlayer = Downloader.DownloadURL(p.URL);
                        Parser.ParsePlayer(p, htmlPlayer);

                        if (!string.IsNullOrEmpty(p.GuildURL))
                        {
                            p.GuildURL = new Uri(new Uri(url), p.GuildURL).ToString();
                            string htmlGuild = Downloader.DownloadURL(p.GuildURL);
                            Parser.ParseGuild(p, htmlGuild);
                        }

                        logger.Debug($"Sending player data via event invoke for \"{p.Name}\"");
                        NewPlayerData?.Invoke(this, new NewPlayerDataEventArgs(url, p));
                    }
                }

                Storage.Players[url] = players;
                Storage.Save();
                logger.Info($"Local storage updated for \"{url}\"");
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }
    }

    public class NewPlayerDataEventArgs : EventArgs
    {
        public string TrackerURL { get; }
        public Player Player { get; }
        public NewPlayerDataEventArgs(string trackerUrl, Player player)
        {
            TrackerURL = trackerUrl;
            Player = player;
        }
    }
}
