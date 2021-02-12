//
// Licensed under the Apache
// by Takashi Futada

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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
        IParse st = new SystemTrader();
        var tasks = new List<Task>();

        // parse each story in parallel
        foreach (var v in stories)
        {
            var story = v;

            tasks.Add(Task.Run(() =>
            {
                st.ParseTweet(story); // spawn a heavy job
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
    // study how to use Regex to parse date, digit and currency
    private class SystemTrader : IParse
    {
        private List<(string, Regex)> regexes;
        private static ParallelOptions po = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };

        public SystemTrader()
        {
            regexes = new List<(string, Regex)>(){
                ("Currency with 円", new Regex(@"[\d,兆億百万]+円\d*銭?", RegexOptions.Compiled)),
                ("Currency with ¥", new Regex(@"¥[\d,\.兆億百万]+", RegexOptions.Compiled)),
                ("Date with 元号", new Regex(@"(昭和|平成|令和)\d*年?\d*月?\d*日?", RegexOptions.Compiled)),
                // new Regex(@"(\d+年\d+月\d+日|\d+年\d+月|\d+年)", RegexOptions.Compiled),
            };
        }
        public void ParseTweet(string tweet)
        {
            Parallel.ForEach(regexes, po, regex =>
            {
                (string pattern, Regex re) = regex;
                MatchCollection matches = re.Matches(tweet);
                foreach (Match m in matches)
                {
                    Console.WriteLine("{0}: {1}", pattern, m.Value);
                }
            });
        }
    }
}