//
// Licensed under the Apache
// by Takashi Futada

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Globalization;

#nullable enable

public class Test
{
    public interface IParse
    {
        public void ParseTweet(string tweet);
    }
    public static void Main()
    {
        var stories = new List<string>(){
            "2021年2月9日 イーロンマスクがリップルを、1,２３４,567円買った",
            "abc令和2年3月8日GameStopがドージコインを売った",
            "2021/02/01 ロビンフッドがドージコインを¥1.23売った",
            "トヨタ自動車、3Q累計税引前1,869,989百万円。",
            "10日の東京株式市場で日経平均株価は4日続伸し、前日比57円00銭（0.19%）高の2万9562円93銭で終えた",
            "14時時点は1ﾄﾞﾙ=104円56銭近辺と、前日17時時点と比べ21銭の円高・ドル安だった",
            "２０２１年度一般会計税収は、２０年度当初予算比９．５％減の５７兆４４８０億円となる見込みだ",
        };

        var tm = new Stopwatch();
        tm.Start();
        IParse st = new SystemTrader(locale: "ja-JP");
        var tasks = new List<Task>();

        // parse each story in parallel
        foreach (var v in stories)
        {
            var story = v;

            tasks.Add(Task.Run(() =>
            {
                st.ParseTweet(tweet: story); // spawn a heavy job
            })); // like go func but thread
        }

        // Console.WriteLine("a: {0}", tm.Elapsed);

        Task t = Task.WhenAll(tasks);

        // Console.WriteLine("b: {0}", tm.Elapsed);

        try
        {
            t.Wait(); // wait all
        }
        catch { }

        // Console.WriteLine("c: {0}", tm.Elapsed);
    }

    /// <summary>
    /// Class to parse date and currency strings.
    /// </summary>
    private class SystemTrader : IParse
    {
        private List<(int, string, Regex)> regexes;
        private System.Globalization.CultureInfo culture;
        private static ParallelOptions po = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };

        /// <summary>
        /// Constructor
        /// </summary>
        public SystemTrader(string locale)
        {
            regexes = new List<(int, string, Regex)>(){
                (1, "Currency with 円", new Regex(@"[\d,兆億百万]+円\d*銭?", RegexOptions.Compiled)),
                (2, "Currency with ¥", new Regex(@"¥[\d,\.兆億百万]+", RegexOptions.Compiled)),
                (3, "Date with 元号", new Regex(@"(昭和|平成|令和)\d*年?\d*月?\d*日?", RegexOptions.Compiled)),
            };

            culture = new System.Globalization.CultureInfo(locale, true);
            culture.DateTimeFormat.Calendar = new JapaneseCalendar();
        }

        // parse a string using regex.
        // The given string should be a string representation of date or currency in Japanese.
        public void ParseTweet(string tweet)
        {
            Parallel.ForEach(regexes, po, regex =>
            {
                (int code, string pattern, Regex re) = regex;
                MatchCollection matches = re.Matches(tweet);
                foreach (Match m in matches)
                {
                    if (code == 3)
                    {
                        string? ret = ToDate(japaneseDate: m.Value);
                        Console.WriteLine("{0}: {1}", pattern, ret ?? "parse error");
                    }
                    else
                    {
                        Console.WriteLine("{0}: {1}", pattern, m.Value);
                    }
                }
            });
        }

        // Convert the Japanese(Wareki) calendar to the Western calendar.
        // gg represents a Wareki such as 令和
        private string? ToDate(string japaneseDate)
        {
            if (DateTime.TryParseExact(japaneseDate, "ggyy年M月d日", culture, DateTimeStyles.AssumeLocal, out DateTime result))
            {
                return result.ToLongDateString();
            }
            else
            {
                return null;
            }
        }
    }
}