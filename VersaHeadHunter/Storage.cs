using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace VersaHeadHunter
{
    class Storage
    {
        private static readonly Logger logger = Logger.GetLogger();

        private static readonly object __lock = "_";
        private static readonly string file = "storage.json";

        /// <summary>
        /// Players lists per tracker URL
        /// </summary>
        public static Dictionary<string, Player[]> Players = new Dictionary<string, Player[]>();
        public static Queue<Player> PublishQueue = new Queue<Player>();
        public static Timer QueueTimer = null;

        static Storage()
        {
            Tracker.NewPlayerData += Tracker_NewPlayerData;

            QueueTimer = new Timer(3 * 60 * 1000);

            QueueTimer.Elapsed += Timer_Elapsed;
            QueueTimer.Start();
        }

        private static void Tracker_NewPlayerData(object sender, NewPlayerDataEventArgs e)
        {
            lock (PublishQueue)
            {
                if (PublishQueue.Count(x => e.Player.Name == x.Name) > 0)
                    logger.Warn($"Player with name \"{e.Player.Name}\" already in publish queue!");
                else
                    PublishQueue.Enqueue(e.Player);
            }
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
        }

        public static void Load()
        {
            if (!File.Exists(file))
            {
                logger.Warn("No storage file found!");
                return;
            }

            string json = "";
            using (StreamReader reader = new StreamReader(file))
                json = reader.ReadToEnd();

            if (!string.IsNullOrEmpty(json))
                Players = JsonConvert.DeserializeObject<Dictionary<string, Player[]>>(json);
        }

        public static void Save()
        {
            lock (__lock)
            {
                string json = JsonConvert.SerializeObject(Players, Formatting.Indented);
                using (StreamWriter writer = new StreamWriter(file))
                    writer.Write(json);
            }
        }
    }
}
