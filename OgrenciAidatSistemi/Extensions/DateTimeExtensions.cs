using System.Globalization;

public static class DateTimeExtensions
{
    public static int GetWeekOfYear(this DateTime date)
    {
        var calendar = CultureInfo.InvariantCulture.Calendar;
        var weekRule = CalendarWeekRule.FirstFourDayWeek;
        var firstDayOfWeek = DayOfWeek.Monday;

        return calendar.GetWeekOfYear(date, weekRule, firstDayOfWeek);
    }
}
