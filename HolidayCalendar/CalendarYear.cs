﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CalendarCalculator
{

    class CalendarYear
    {
        private readonly int _year;

        public CalendarYear(int year)
        {
            _year = year;
        }
        public DateTime SchoolyearBegin => CalcDateOnNextWeekday(GetDate(9, 1), DayOfWeek.Monday);
        public DateTime SemesterHolidayBegin => CalcDateOnNextWeekday(GetDate(2, 1), DayOfWeek.Monday);
        public DateTime MainHolidayBegin => CalcDateOnNextWeekday(GetDate(6, 28), DayOfWeek.Saturday);
        public DateTime EasterSunday => CalcEasterSunday();
        public DateTime ChristiHimmelfahrt => EasterSunday.AddDays(39);
        public DateTime PfingstSunday => EasterSunday.AddDays(49);
        public DateTime Fronleichnam => EasterSunday.AddDays(60);

        /// <summary>
        /// Liefert ein Array aller Tage eines Jahres mit den entsprechenden Anmerkungen für
        /// Feiertage und Schulferien.
        /// </summary>
        public CalendarDay[] GetCalendarDays()
        {
            /* Arbeitsruhegesetz
             * https://www.ris.bka.gv.at/GeltendeFassung.wxe?Abfrage=Bundesnormen&Gesetzesnummer=10008541
             *
             * (2) Feiertage im Sinne dieses Bundesgesetzes sind:
             *     1. Jänner (Neujahr), 6. Jänner (Heilige Drei Könige), Ostermontag, 1. Mai (Staatsfeiertag), 
             *     Christi Himmelfahrt, Pfingstmontag, Fronleichnam, 15. August (Mariä Himmelfahrt),
             *     26. Oktober (Nationalfeiertag), 1. November (Allerheiligen),
             *     8. Dezember (Mariä Empfängnis), 25. Dezember (Weihnachten), 26. Dezember (Stephanstag).
            */
            var semesterHolidayBegin = SemesterHolidayBegin;
            var easterSunday = EasterSunday;
            var mainHolidayBegin = MainHolidayBegin;
            var schoolyearBegin = SchoolyearBegin;
            var xmaxHolidayBegin = GetDate(12, 23).DayOfWeek == DayOfWeek.Monday ? GetDate(12, 23) : GetDate(12, 24);

            var days = new CalendarDay[new DateTime(_year, 12, 31).DayOfYear];

            // Gesetzliche Feiertage
            AddDays(days, GetDate(1, 1), d => new CalendarDay(d, true, "Neujahr"));
            AddDays(days, GetDate(1, 6), d => new CalendarDay(d, true, "Heilige 3 Könige"));
            AddDays(days, easterSunday.AddDays(1), d => new CalendarDay(d, true, "Ostermontag"));
            AddDays(days, GetDate(5, 1), d => new CalendarDay(d, true, "Staatsfeiertag"));
            AddDays(days, easterSunday.AddDays(39), d => new CalendarDay(d, true, "Christi Himmelfahrt"));
            AddDays(days, easterSunday.AddDays(50), d => new CalendarDay(d, true, "Pfingstmontag"));
            AddDays(days, easterSunday.AddDays(60), d => new CalendarDay(d, true, "Fronleichnam"));
            AddDays(days, GetDate(8, 15), d => new CalendarDay(d, true, "Mariä Himmelfahrt"));
            AddDays(days, GetDate(10, 26), d => new CalendarDay(d, true, "Nationalfeiertag"));
            AddDays(days, GetDate(11, 1), d => new CalendarDay(d, true, "Allerheiligen"));
            AddDays(days, GetDate(12, 8), d => new CalendarDay(d, true, "Mariä Empfängnis"));
            AddDays(days, GetDate(12, 25), d => new CalendarDay(d, true, "Weihnachten"));
            AddDays(days, GetDate(12, 26), d => new CalendarDay(d, true, "Stephanstag"));

            // Immer frei, aber mit einem Text versehen
            AddDays(days, easterSunday, d => new CalendarDay(d, false, "Ostersonntag"));
            AddDays(days, easterSunday.AddDays(49), d => new CalendarDay(d, false, "Pfingstsonntag"));

            // Zusätzlich schulfreie Tage nach § 2 Schulzeitgesetz, https://www.ris.bka.gv.at/GeltendeFassung.wxe?Abfrage=Bundesnormen&Gesetzesnummer=10009575
            AddDays(days, GetDate(1, 2), GetDate(1, 6), d => new CalendarDay(d, false, "Weihnachtsferien"));
            AddDays(days, semesterHolidayBegin, semesterHolidayBegin.AddDays(6), d => new CalendarDay(d, false, "Semesterferien"));
            // Novelle BGBl. I Nr. 49/2019: DI nach Ostern und Pfingsten ist ab 2020 nicht mehr frei.
            AddDays(days, easterSunday.AddDays(-8), easterSunday.AddDays(_year < 2020 ? 3 : 2), d => new CalendarDay(d, false, "Osterferien"));
            AddDays(days, easterSunday.AddDays(48), easterSunday.AddDays(_year < 2020 ? 52 : 51), d => new CalendarDay(d, false, "Pfingstferien"));
            AddDays(days, mainHolidayBegin, schoolyearBegin, d => new CalendarDay(d, false, "Sommerferien"));
            AddDays(days, GetDate(11, 2), d => new CalendarDay(d, false, "Allerseelen"));
            AddDays(days, GetDate(11, 15), d => new CalendarDay(d, false, "Heiliger Lepopld"));
            AddDays(days, xmaxHolidayBegin, GetDate(12, 31).AddDays(1), d => new CalendarDay(d, false, "Weihnachtsferien"));
            // Novelle BGBl. I Nr. 49/2019: Herbstferien vom 27.10. bis 31.10.
            if (_year >= 2020)
            {
                AddDays(days, GetDate(10, 27), GetDate(11, 1), d => new CalendarDay(d, false, "Herbstferien"));
            }

            // Alle anderen Tage sind normale Arbeitstage
            AddDays(days, GetDate(1, 1), GetDate(12, 31).AddDays(1));
            return days;
        }

        /// <summary>
        /// Liefert ein Datum im Jahr des Objektes
        /// </summary>
        private DateTime GetDate(int month, int day) => new DateTime(_year, month, day);
        /// <summary>
        /// Fügt einen einzelnen Tag in ein Array von Jahrestagen ein, wenn noch kein Tag eingefügt wurde.
        /// </summary>
        private void AddDays(CalendarDay[] days, DateTime day, Func<DateTime, CalendarDay> converter)
        {
            int dayOfYear = day.DayOfYear - 1;
            if (days[dayOfYear] is null)
                days[dayOfYear] = converter(day);
        }
        /// <summary>
        /// Fügt eine Range von Tagen (begin bis exklusive end) in ein Array von Jahrestagen ein,
        /// wenn noch kein Tag eingefügt wurde.
        /// </summary>
        private void AddDays(CalendarDay[] days, DateTime begin, DateTime end)
            => AddDays(days, begin, end, d => new CalendarDay(d));
        /// <summary>
        /// Fügt eine Range von Tagen (begin bis exklusive end) in ein Array von Jahrestagen ein,
        /// wenn noch kein Tag eingefügt wurde.
        /// </summary>
        private void AddDays(CalendarDay[] days, DateTime begin, DateTime end, Func<DateTime, CalendarDay> converter)
        {
            for (; begin < end; begin = begin.AddDays(1))
            {
                int dayOfYear = begin.DayOfYear - 1;
                if (days[dayOfYear] is null)
                    days[dayOfYear] = converter(begin);
            }
        }
        /// <summary>
        /// Liefert den nächsten Tag, an dem der angegebene Wochentag eintritt. Wird für die
        /// Berechnung der Schulferien benötigt.
        /// DI, 15.11.2022, gesucht ist der nächste DI  --> 15.11.2022
        /// MI, 16.11.2022, gesucht ist der nächste DI  --> 22.11.2022
        /// </summary>
        private DateTime CalcDateOnNextWeekday(DateTime date, DayOfWeek dayOfWeek)
        {
            int dayOfWeekZeroBased = dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1;
            int dateDayOfWeekZeroBased = date.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)date.DayOfWeek - 1;
            return date.AddDays((dayOfWeekZeroBased - dateDayOfWeekZeroBased + 7) % 7);
        }

        /// <summary>
        /// Berechnet den Tag des Ostersonntags im Jahr des Objektes.
        /// Nach Spencer Jones, vgl. Meeus Astronomical Algorithms, 2nd Ed, S67
        /// </summary>
        private DateTime CalcEasterSunday()
        {
            int a = _year % 19;
            int b = _year / 100;
            int c = _year % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4;
            int k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = (a + 11 * h + 22 * l) / 451;
            int x = h + l - 7 * m + 114;
            int n = x / 31;
            int p = x % 31;
            return new DateTime(_year, n, p + 1);
        }
    }
}
