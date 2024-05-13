using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.ConstVariable
{
    public static class ConstCallBackData
    {
        public static class BankMenu
        {
            public const string InsertNewBank = "InsertNewBank";
            public const string GetBankList = "GetBankList";
        }
        public static class Menu
        {
            public const string InboundTransaction = "InboundTransaction";
            public const string Reports = "Reports";
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
            public const string Amount = "Amount";
           
        }

        public static class InboundTransaction
        {
            public const string Daily = "inbounddaily";
            public const string SpecificDate = "inboundSpecificDate";
            public const string Amount = "inboundAmount";

        }

        public static class DailyCategory
        {
            //does not need - in string handle in set dynamic buttons
            public const string Category = "outboundcategory";
        }

        public static class DailyOrSpecificDate
        {
            //does not need - in string handle in set dynamic buttons
            public const string Bank = "outboundbank";
        }

        public static class InboundDailyCategory
        {
            //does not need - in string handle in set dynamic buttons
            public const string Category = "inboundcategory";
        }

        public static class InboundDailyOrSpecificDate
        {
            //does not need - in string handle in set dynamic buttons
            public const string Bank = "inboundbank";
        }


        public static class OutboundTransactionPreview
        {
            public const string Submit = "outboundTransactionSubmit";
            public const string Cancel = "outboundTransactionCancel";
        }

        public static class InboundTransactionPreview
        {
            public const string Submit = "inboundTransactionSubmit";
            public const string Cancel = "intboundTransactionCancel";
        }
        public static class Global
        {
            public const string Back = "back";
            public const string BackToMenu = "backToMenu";
        }

    }
}
