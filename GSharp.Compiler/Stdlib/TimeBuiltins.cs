using System.Globalization;
using System.Reflection;

namespace GSharp.Compiler.Stdlib;

// A G# date is a boxed DateTime (GsType.DateType). println shows it in the machine's regional
// format. time.now is local time; time.utcNow is UTC. Dates compare with the ordinary
// < > <= >= == != operators (see RuntimeHelpers.Compare); they do not support + or -.
public static class TimeBuiltins
{
    public static void Register(Dictionary<string, MethodInfo> builtins)
    {
        builtins["time.now"] = typeof(TimeBuiltins).GetMethod(nameof(Now))!;
        builtins["time.utcNow"] = typeof(TimeBuiltins).GetMethod(nameof(UtcNow))!;
        builtins["time.make"] = typeof(TimeBuiltins).GetMethod(nameof(Make))!;
        builtins["time.parse"] = typeof(TimeBuiltins).GetMethod(nameof(Parse))!;
        builtins["time.format"] = typeof(TimeBuiltins).GetMethod(nameof(Format))!;
        builtins["time.year"] = typeof(TimeBuiltins).GetMethod(nameof(Year))!;
        builtins["time.month"] = typeof(TimeBuiltins).GetMethod(nameof(Month))!;
        builtins["time.day"] = typeof(TimeBuiltins).GetMethod(nameof(Day))!;
        builtins["time.hour"] = typeof(TimeBuiltins).GetMethod(nameof(Hour))!;
        builtins["time.minute"] = typeof(TimeBuiltins).GetMethod(nameof(Minute))!;
        builtins["time.second"] = typeof(TimeBuiltins).GetMethod(nameof(Second))!;
        builtins["time.weekday"] = typeof(TimeBuiltins).GetMethod(nameof(Weekday))!;
        builtins["time.addDays"] = typeof(TimeBuiltins).GetMethod(nameof(AddDays))!;
        builtins["time.addHours"] = typeof(TimeBuiltins).GetMethod(nameof(AddHours))!;
        builtins["time.addMinutes"] = typeof(TimeBuiltins).GetMethod(nameof(AddMinutes))!;
        builtins["time.addSeconds"] = typeof(TimeBuiltins).GetMethod(nameof(AddSeconds))!;
        builtins["time.diffDays"] = typeof(TimeBuiltins).GetMethod(nameof(DiffDays))!;
        builtins["time.diffSeconds"] = typeof(TimeBuiltins).GetMethod(nameof(DiffSeconds))!;
    }

    public static object Now()
    {
        return DateTime.Now;
    }

    public static object UtcNow()
    {
        return DateTime.UtcNow;
    }

    // Midnight of the given year/month/day.
    public static object Make(object year, object month, object day)
    {
        return new DateTime((int)year, (int)month, (int)day);
    }

    // Same "no try-parse" philosophy as string.toInt: malformed input throws FormatException.
    // Parsed with the invariant culture so ISO 8601 text ("2026-09-19") behaves identically
    // on every machine.
    public static object Parse(object text)
    {
        return DateTime.Parse((string)text, CultureInfo.InvariantCulture);
    }

    public static object Format(object date, object pattern)
    {
        return ((DateTime)date).ToString((string)pattern);
    }

    public static object Year(object date) => ((DateTime)date).Year;
    public static object Month(object date) => ((DateTime)date).Month;
    public static object Day(object date) => ((DateTime)date).Day;
    public static object Hour(object date) => ((DateTime)date).Hour;
    public static object Minute(object date) => ((DateTime)date).Minute;
    public static object Second(object date) => ((DateTime)date).Second;

    // 0 = Sunday.
    public static object Weekday(object date) => (int)((DateTime)date).DayOfWeek;

    public static object AddDays(object date, object days) => ((DateTime)date).AddDays((int)days);
    public static object AddHours(object date, object hours) => ((DateTime)date).AddHours((int)hours);
    public static object AddMinutes(object date, object minutes) => ((DateTime)date).AddMinutes((int)minutes);
    public static object AddSeconds(object date, object seconds) => ((DateTime)date).AddSeconds((int)seconds);

    // later - earlier, truncated to whole units (negative when `later` is before `earlier`).
    public static object DiffDays(object later, object earlier)
    {
        return (int)((DateTime)later - (DateTime)earlier).TotalDays;
    }

    public static object DiffSeconds(object later, object earlier)
    {
        return (int)((DateTime)later - (DateTime)earlier).TotalSeconds;
    }
}
