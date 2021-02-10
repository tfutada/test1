//
// Licensed under the Apache
// by Takashi Futada

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class Test
{
    public static void Main()
    {
        var stories = new List<string>(){
            "2021年2月9日 イーロンマスクがビットコインを買った",
            "GameStopがドージコインを売った 2月10日(水)",
            "2021/02/01 ロビンフッドがドージコインを売った",
            "令和3年 税制改革",
            "日経平均 3万円 230円高",
            "ドル円",
            "アメリカ　バイデン　金融緩和 ",
            "アメリカ　国債 ",
        };

        var st = new SystemTrader();

        foreach (string story in stories)
        {
            st.ParseTweet(story);
        }

        // Async/wait

        // Parallel
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