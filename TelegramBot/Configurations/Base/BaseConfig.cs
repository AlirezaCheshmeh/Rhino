using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Configurations.Base
{
    public abstract class BaseConfig
    {
        private const string StageBot = "6996481741:AAGtEvzB_ek90PLbezp2p1phyAsUFQLJd7g";
        private const string Production = "7034758059:AAGRepl9FygWlLTMC3UrlXLquvul3yer2l4";
        public string TelegramKey { get; private set; } = StageBot;
    }
}
