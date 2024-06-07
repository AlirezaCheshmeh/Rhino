using Application.Services.TelegramServices.BaseMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.TelegramServices.ConstVariable
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

        public static class Report
        {
            public const string InBound = "inboundreport";
            public const string InBoundTodayReport = "inboundtodayreport";
            public const string InboundToday = "inboundtoday";
            public const string InboundYesterday = "inboundyesterday";
            public const string InboundLastWeek = "inboundlastweek";
            public const string InboundLastMonth = "inboundlastmonth";
            public const string OutboundToday = "outboundtoday";
            public const string OutboundYesterday = "outboundyesterday";
            public const string OutboundLastWeek = "outboundlastweek";
            public const string OutboundLastMonth = "outboundlastmonth";
            public const string InBoundTodaySummaryReport = "inboundtodaysummaryreport";
            public const string OutBound = "outboundreport";
            public const string OutBoundTodayReport = "outboundtodayreport";
            public const string OutBoundTodaySummaryReport = "outboundtodaysummaryreport";
            public const string InBoundYesterdayReport = "inboundyesterdayreport";
            public const string InBoundYesterdaySummaryReport = "inboundyesterdaysummaryreport";
            public const string OutBoundYesterdayReport = "outboundyesterdayreport";
            public const string OutBoundYesterdaySummaryReport = "outboundyesterdaysummaryreport";
            public const string InBoundLastWeekReport = "inboundlastweekreport";
            public const string InBoundLastWeekSummaryReport = "inboundlastweeksummaryreport";
            public const string OutBoundLastWeekReport = "outboundlastweekreport";
            public const string OutBoundLastWeekSummaryReport = "outboundlastweeksummaryreport";
            public const string InBoundLastMonthReport = "inboundlastmonthreport";
            public const string InBoundLastMonthSummaryReport = "inboundlastmonthsummaryreport";
            public const string OutBoundLastMonthReport = "outboundlastmonthreport";
            public const string OutBoundLastMonthSummaryReport = "outboundlastmonthsummaryreport";
        }

        public static class Reminder
        {
            public const string ChooseReminderMonth = "remindermonth";
            public const string RemindMeAgain = "remindmeagain-";
            public const string ChooseReminderDay = "reminderday";
            public const string RemindDescription = "reminddescription";
            public const string RemindAmount = "remindamount";
        }

        public static class OutboundTransaction
        {
            public const string Daily = "daily";
            public const string OutBoundSpecificDate = "outboundSpecificDate";
            public const string Amount = "Amount";
            public const string OutBoundSpeseficMonth = "outboundmonth";
            public const string OutBoundSpeseficDay = "outboundday";

        }

        public static class InboundTransaction
        {
            public const string Daily = "inbounddaily";
            public const string InBoundSpecificDate = "inboundSpecificDate";
            public const string Amount = "inboundAmount";
            public const string InboundSpeseficMonth = "inboundmonth";
            public const string InBoundSpeseficDay = "inboundday";
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

        public static class ReminderPreview
        {
            public const string Submit = "ReminderSubmit";
            public const string Cancel = "ReminderCancel";
        }
        public static class Global
        {
            public const string Back = "back";
            public const string BackToMenu = "backToMenu";
        }

    }
}
