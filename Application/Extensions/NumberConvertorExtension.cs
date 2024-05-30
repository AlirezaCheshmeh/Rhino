namespace Application.Extensions
{
    public static class NumbersConvertorExtension
    {
        static char[][] persianChars = new char[][]
        {
        "0123456789".ToCharArray(), "۰۱۲۳۴۵۶۷۸۹".ToCharArray()
        };

        static char[][] arabicChars = new char[][]
        {
        "0123456789".ToCharArray(), "٠١٢٣٤٥٦٧٨٩".ToCharArray()
        };


        public static string ToEnglishNumber(this string src)
        {
            if (string.IsNullOrEmpty(src)) return null;
            for (int x = 0; x <= 9; x++)
            {
                src = src.Replace(persianChars[1][x], persianChars[0][x]);
            }

            for (int x = 0; x <= 9; x++)
            {
                src = src.Replace(arabicChars[1][x], arabicChars[0][x]);
            }

            return src;
        }


        public static string ToPersianNumber(this string src)
        {
            if (string.IsNullOrEmpty(src)) return null;
            for (int x = 0; x <= 9; x++)
            {
                src = src.Replace(persianChars[0][x], persianChars[1][x]);
            }

            return src;
        }


        public static string ConvertDayOfWeekToPersian(this string DayInEnglish)
        {
            switch (DayInEnglish)
            {
                case "Monday":
                    return "دوشنبه";
                case "Tuesday":
                    return "سه شنبه";
                case "Wednesday":
                    return "چهار شنبه";
                case "Thursday":
                    return "پنج شنبه";
                case "Friday":
                    return "جمه";
                case "Sunday":
                    return "یکشنبه";
                case "Saturday":
                    return "شنبه";
                default:
                    return null;
            }
        }
    }
}
