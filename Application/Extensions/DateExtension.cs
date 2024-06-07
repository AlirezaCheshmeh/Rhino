﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Extensions
{
    public static class DateExtension
    {
        public static DateTime ConvertToEnglishDate(string Date)
        {
            Calendar persian = new PersianCalendar();
            int[] yyyy = Date.Split('/').Select(x => int.Parse(x)).ToArray();
            DateTime date = new DateTime(yyyy[0], yyyy[1], yyyy[2], persian);
            return date;
        }
        /// <summary>
        /// yyy/MM/dd string format
        /// </summary>
        /// <param name="Date"></param>
        /// <param name="ForSave"></param>
        /// <returns></returns>
        public static string ConvertToPersianDate(string Date, bool ForSave = false)
        {
            DateTime date = DateTime.ParseExact(Date, "yyyy/MM/dd", CultureInfo.InvariantCulture);

            int year = date.Year;
            int month = date.Month;
            int day = date.Day;

            PersianCalendar persianCalendar = new PersianCalendar();
            if (!ForSave)
            {
                int persianYear = persianCalendar.GetYear(date);
                string[] persianMonths = new string[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };
                string persianMonth = persianMonths[persianCalendar.GetMonth(date) - 1];
                int persianDay = persianCalendar.GetDayOfMonth(date);
                return $"{persianYear}/{persianMonth}/{persianDay}";
            }
            else
            {
                int persianYear = persianCalendar.GetYear(date);
                var persianMonth = persianCalendar.GetMonth(date);
                int persianDay = persianCalendar.GetDayOfMonth(date);
                return $"{persianYear}/{persianMonth}/{persianDay}";
            }


        }

        public static int ToPersianYear(this int gregorianYear)
        {
            var gregorianDate = new DateTime(gregorianYear, 1, 1, new GregorianCalendar());
            PersianCalendar persianCalendar = new PersianCalendar();
            var year = persianCalendar.GetYear(gregorianDate);
            year++;
            return year;
        }

        public static string ConvertDayOfWeekToPersian(this DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return "دوشنبه";
                case DayOfWeek.Tuesday:
                    return "سه‌شنبه";
                case DayOfWeek.Wednesday:
                    return "چهارشنبه";
                case DayOfWeek.Thursday:
                    return "پنج‌شنبه";
                case DayOfWeek.Friday:
                    return "جمعه";
                case DayOfWeek.Saturday:
                    return "شنبه";
                case DayOfWeek.Sunday:
                    return "یکشنبه";
                default:
                    return null;
            }
        }
    }
}
