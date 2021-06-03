using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace VersaHeadHunter
{
    class Program
    {
        private static readonly Logger logger = Logger.GetLogger();

        // TODO: replace hardcoded stuff with config args
        // replace this with your realm address
        private readonly static string url = "https://www.wowprogress.com/gearscore/eu/%D1%80%D0%B5%D0%B2%D1%83%D1%89%D0%B8%D0%B9-%D1%84%D1%8C%D0%BE%D1%80%D0%B4?lfg=1&sortby=ts";
        private readonly static string token = "___";
        private readonly static ulong channelId = 649913439556206592; // channel used to post messages into

        private readonly DiscordSocketClient _client;

        static void Main(string[] args)
        {
            try
            {
                var awaiter = new Program().MainAsync().GetAwaiter();

                Storage.Load();
                Tracker tracker = new Tracker(url);
                tracker.Start();

                awaiter.GetResult();
            }
            catch (Exception ex)
            {
                FatalError.Exception(ex);
            }
        }

        public Program()
        {
            _client = new DiscordSocketClient();

            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;

            Storage.QueueTimer.Elapsed += SendNewPlayerDataAsync;
        }

        private void QueueTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public async Task MainAsync()
        {
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log)
        {
            logger.Info(log.ToString());
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            logger.Info($"{_client.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        private void SendNewPlayerDataAsync(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                logger.Info("Sending new player data to channels");

                lock (Storage.PublishQueue)
                    while (Storage.PublishQueue.Count > 0)
                    {
                        var p = Storage.PublishQueue.Dequeue();
                        foreach (var g in _client.Guilds)
                            g.GetTextChannel(channelId).SendMessageAsync(p.ToString());
                    }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }
    }
}
