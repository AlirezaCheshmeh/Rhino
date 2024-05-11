using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.ConstMessages
{
    public static class ConstMessage
    {
        //intro
        public static string Introduction => "برنامه Rhino یک برنامه کوچک حسابداری در تلگرام است که به کاربران امکان مدیریت هزینه‌ها و درآمدهای خود را به‌صورت ساده و سریع می‌دهد. این برنامه به‌عنوان یک بات تلگرام، به کاربران اجازه می‌دهد که با استفاده از دستورات متنی، هزینه‌ها و درآمدهای خود را وارد کنند و گزارش‌های مالی خود را بررسی کنند.";

        //transaction
        public static string InsertAmount => "💰 <b>لطفا مبلغ پرداختی را وارد کنید</b> 💰 ";
        public static string InsertDescription => "<b>لطفا بابت پراختی را واردکنید</b> 📔";
        public static string ChooseCategory => "<b>دسته بندی مورد نظر را انتخاب کنید</b> 📔";

        //errors
        public static string Error => "<b>خطایی رخ داده است دوباره تلاش کنبد</b> ⚠";


        public static string ChooseBank => "<b>بانک خود را انتخاب کنید</b> 🏧";
        public static string OutboundTransactionType => "<b>پرداخت را در چه تاریخی انجام داده اید</b> ⏳";

        //menu and global
        public static string Back => "بازگشت 🔙";
        public static string BackToMenu => "بازگشت به منو 🔙";
        public static string CancelButton => "لغو ❌";
        public static string Submit => "تایید ✅";
        public static string Menu => "      🚀 <b>     حسابدار راینو    </b> 🚀\n" +
                                     " 🔹 گزارش گیری روزانه\n" +
                                     " 🔹 یادآوری تاریخ وصول چک\n" +
                                     " 🔹 محاسبه بیمه ومالیات براساس حقوق";
        public static string Cancel => "<b>عملیات لغو شد</b> ⛔";
        public static string BuyAccount => "💰 <b>قیمت های اشتراک راینو به شرح زیر می‌باشد</b> 💰";
        public static string OutboundTransactionPreview => "⚠ تراکنش ایجاد شده شما :";
        public static string Today => "امروز";
        public static string SpecificDate => "تاریخ مشخص";
        public static string Success => "<b>عملیات با موفقیت انجام شد</b> ✏";


        //validation
        public const string AmountValidationErrorMEssage = " ⚠ <b>مقدار قیمت به درستی وارد نشده است</b> \n <b>دوباره تلاش کنید</b>";
        public const string AccountError = " ⚠ <b>شما اشتراک فعالی ندارید</b>";
        


        //setting and bank message
        public static string Settings => "<b>تنظیمات مورد نظر خود را انتخاب کنید</b> ⚙";

        public static string InsertNewBank => "<b>نام بانک خود را وارد کنید</b>";

    }
}
