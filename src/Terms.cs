using VeryNaiveDatalog;


public record Constant : Term
{
    public override Term Apply(Substitution env) => this;
}


public sealed record String(string Value) : Constant
{
    public override string ToString() => Value;
}

public sealed record Boolean(bool Value) : Constant
{
    public override string ToString() => Value.ToString();

    public static bool operator &(Boolean one, Boolean two) => one.Value && two.Value;
    public static bool operator |(Boolean one, Boolean two) => one.Value || two.Value;
    public static bool operator !(Boolean value) => !value.Value;
    public static bool operator true(Boolean value) => value.Value;
    public static bool operator false(Boolean value) => value.Value;
}

public sealed record Integer(long Value) : Constant
{
    public override string ToString() => Value.ToString();
}

public sealed record Date(ulong Timestamp) : Constant
{
    public override string ToString() => DateTime.ToLongDateString();
    public DateTime DateTime => FromTAI64(Timestamp);

    public static bool operator <(Date one, Date two) => one.Timestamp < two.Timestamp;
    public static bool operator >(Date one, Date two) => one.Timestamp > two.Timestamp;
    public static bool operator <=(Date one, Date two) => one.Timestamp <= two.Timestamp;
    public static bool operator >=(Date one, Date two) => one.Timestamp >= two.Timestamp;

    public static DateTime FromTAI64(ulong timestamp)
    {
        //TODE make more robust
        //taken from https://github.com/paulhammond/tai64/blob/main/tai64n.go
        var unixTimeStamp = EpochTime(timestamp - (2^62));

        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds( unixTimeStamp ).ToLocalTime();
        return dateTime;
    }

    public static ulong ToTAI64(DateTime dateTime)
    {
        var universal = dateTime.ToUniversalTime();
        var unixTimeStamp = (ulong)universal.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

        return unixTimeStamp + (2^62);
    }

    // EpochTime returns the time.Time at secs seconds and nsec nanoseconds since
    // the beginning of January 1, 1970 TAI.
    public static ulong EpochTime(ulong secs) 
    {
	    var offset = leapSeconds.Length + 10;
        foreach(var leap in leapSeconds)
        {
            offset--;
            if(secs > leap)
                break;
        }
        return secs- (ulong)offset;
    }

    // This is a list of all leap seconds added since 1972, in TAI seconds since
    // the unix epoch. It is derived from
    // http://www.ietf.org/timezones/data/leap-seconds.list
    // http://hpiers.obspm.fr/eop-pc/earthor/utc/UTC.html
    // http://maia.usno.navy.mil/leapsec.html
    static ulong[] leapSeconds = new ulong[] {
        // subtract 2208988800 to convert from NTP datetime to unix seconds
        // then add number of previous leap seconds to get TAI-since-unix-epoch
        1483228836,
        1435708835,
        1341100834,
        1230768033,
        1136073632,
        915148831,
        867715230,
        820454429,
        773020828,
        741484827,
        709948826,
        662688025,
        631152024,
        567993623,
        489024022,
        425865621,
        394329620,
        362793619,
        315532818,
        283996817,
        252460816,
        220924815,
        189302414,
        157766413,
        126230412,
        94694411,
        78796810,
        63072009,
    };
}