using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.ConstVariable
{
    public static class ConstCallBackData
    {
        public static class Menu
        {
            public const string InboundTransaction = "InboundTransaction";
            public const string OutboundTransaction = "OutboundTransaction";
            public const string OnceReminder = "OnceReminder";
            public const string PeriodicReminder = "PeriodicReminder";
            public const string Settings = "Settings";
            public const string Calculator = "Calculator";
            public const string Supporter = "Supporter";
            public const string Guide = "Guide";
            public const string BuyAccount = "BuyAccount";
        }

        public static class OutboundTransaction
        {
            public const string Daily = "daily";
            public const string SpecificDate = "SpecificDate";
        }

        public static class DailyOrSpecificDate
        {
            public const string Bank = "bank-";
        }

        public static class OutboundTransactionPreview
        {
            public const string Submit = "outboundTransactionSubmit";
            public const string Cancel = "outboundTransactionCancel";
        }
        public static class Global
        {
            public const string Back = "back";
        }

    }
}
