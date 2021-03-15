using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaHeadHunter
{
    public class Player
    {
        public string Timestamp;

        public string URL;
        public string Name;
        public string Class;
        public string Specs;
        public string ilvl;
        public string Progress;
        public string BattleNet;
        public string Description;
        public string Realm; // TODO: add
        public bool Transfer; // TODO: add

        public string GuildURL;
        public string GuildName;
        public string GuildFaction;
        public string GuildProgress;

        public override string ToString()
        {
            string guild = string.IsNullOrEmpty(GuildName) ? "without guild " : $"from \"{GuildName}\" with progress {GuildProgress} ";
            string description = string.IsNullOrEmpty(Description) ? "" : $"```fix\n{Description}```";
            string specs = string.IsNullOrEmpty(Specs) ? "" : $"{{{Specs}}} - ";
            //string battlenet = string.IsNullOrEmpty(BattleNet) ? "" : $"({BattleNet}) ";

            return $"```apache\n{Name} ({Class} - {specs}{ilvl} - {Progress}) {guild}published.```{description}{URL}";
        }
    }
}
