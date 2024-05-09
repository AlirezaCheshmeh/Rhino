using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.ConstMessages
{
    public static class ConstMessage
    {
        public static string Introduction => "برنامه Rhino یک برنامه کوچک حسابداری در تلگرام است که به کاربران امکان مدیریت هزینه‌ها و درآمدهای خود را به‌صورت ساده و سریع می‌دهد. این برنامه به‌عنوان یک بات تلگرام، به کاربران اجازه می‌دهد که با استفاده از دستورات متنی، هزینه‌ها و درآمدهای خود را وارد کنند و گزارش‌های مالی خود را بررسی کنند.";

        public static string InsertAmount => "💰 <b>لطفا مبلغ پرداختی را وارد کنید</b> 💰 ";

        public static string InsertDescription => "<b>لطفا بابت پراختی را واردکنید</b> 📔";
        public static string ChooseBank => "<b>بانک خود را انتخاب کنید</b> 🏧";
        public static string OutboundTransactionType => "<b>پرداخت را در چه تاریخی انجام داده اید</b> ⏳";
        public static string Back => "بازگشت 🔙";
        public static string CancelButton => "لغو ❌";
        public static string Submit => "تایید ✅";
        public static string OutboundTransactionPreview => "⚠ تراکنش ایجاد شده شما :";
        public static string Today => "امروز";
        public static string SpecificDate => "تاریخ مشخص";
        public static string Success => "<b>عملیات با موفقیت انجام شد</b> ✏";

        public static string Menu => "      🚀 <b>     حسابدار راینو    </b> 🚀\n" +
                                     " 🔹 گزارش گیری روزانه\n" +
                                     " 🔹 یادآوری تاریخ وصول چک\n" +
                                     " 🔹 محاسبه بیمه ومالیات براساس حقوق";
        public static string Cancel => "<b>عملیات لغو شد</b> ⛔";

    }
}
