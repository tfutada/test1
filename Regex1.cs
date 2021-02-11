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
    public static void Main()
    {
        var stories = new List<string>(){
            "2021年2月9日 イーロンマスクがリップルを、1,234,567円買った",
            "GameStopがドージコインを売った 2月10日(水)",
            "2021/02/01 ロビンフッドがドージコインを¥1.23売った",
            "トヨタ自動車、3Q累計税引前1,869,989百万。",
            "10日の東京株式市場で日経平均株価は4日続伸し、前日比57円00銭（0.19%）高の2万9562円93銭で終えた",
            "14時時点は1ﾄﾞﾙ=104円56銭近辺と、前日17時時点と比べ21銭の円高・ドル安だった",
            "２０２１年度一般会計税収は、２０年度当初予算比９．５％減の５７兆４４８０億円となる見込みだ",
        };

        var tm = new Stopwatch();
        tm.Start();
        var st = new SystemTrader();
        var tasks = new List<Task>();

        foreach (var v in stories)
        {
            var story = v;

            tasks.Add(Task.Run(() =>
            {
                st.ParseTweet(story); // spawn a heavy job 
            })); // like go func but thread
        }

        Console.WriteLine("a: {0}", tm.Elapsed);

        Task t = Task.WhenAll(tasks);

        Console.WriteLine("b: {0}", tm.Elapsed);

        try
        {
            t.Wait(); // wait all
        }
        catch { }

        Console.WriteLine("c: {0}", tm.Elapsed);


        // // Parallel
        // tm.Reset(); tm.Start();

        // var po = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };
        // Parallel.ForEach(stories, po, story =>
        // {
        //     st.ParseTweet(story);
        // });
        // tm.Stop(); Console.WriteLine("Parallel: {0}", tm.Elapsed);

    }
    // study how to use Regex to parse date, digit and currency
    private class SystemTrader
    {
        private static Regex re = new Regex(@"^イーロンマスクが(.+)を(.+)った$", RegexOptions.Compiled);
        public void ParseTweet(String tweet)
        {
            Match mt = re.Match(tweet);
            if (mt.Success)
            {
                Console.WriteLine("{0}:{1}", mt.Groups[1].Value, mt.Groups[2].Value);
            }
        }
    }
}