using System.Diagnostics;
using System.Linq;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Net;

namespace HtmlDiff
{
    public class Operation
    {
        public Operation(Action action, int startInOld, int endInOld, int startInNew, int endInNew)
        {
            Action = action;
            StartInOld = startInOld;
            EndInOld = endInOld;
            StartInNew = startInNew;
            EndInNew = endInNew;
        }

        public Action Action { get; set; }
        public int StartInOld { get; set; }
        public int EndInOld { get; set; }
        public int StartInNew { get; set; }
        public int EndInNew { get; set; }


#if DEBUG

        public void PrintDebugInfo(string[] oldWords, string []newWords)
        {
            var oldText = string.Join("", oldWords.Where((s, pos) => pos >= this.StartInOld && pos < this.EndInOld).ToArray());
            var newText = string.Join("", newWords.Where((s, pos) => pos >= this.StartInNew && pos < this.EndInNew).ToArray());
            Debug.WriteLine(string.Format(@"Operation: {0}, Old Text: '{1}', New Text: '{2}'", Action.ToString(), oldText, newText));

        }

#endif
        static string cleanup(string strin)
        {
            string text = strin.Trim();  //trim the text 
            string result = Regex.Replace(text, @"\r\n?|\n", " "); //replace the line breaks with blank space
            result = Regex.Replace(result, "<.*?>", string.Empty); //replace unwanted html with empty string 
            result = Regex.Replace(result, "&nbsp;", " "); // replace &nbsp; with space(“ “)
            result = Regex.Replace(result, @"\s+", " "); //replace multiple spaces with single space 
            return result;
        }
        //This function is filter out some operations that may only bring noise to the compared results.
        //1. filter out one character replacement.
        //2. filter out dates.
        public void SmartFilter(string[] oldWords, string[] newWords)
        {
            if (Action != Action.Replace)
                return;

            var oldText = string.Join("", oldWords.Where((s, pos) => pos >= this.StartInOld && pos < this.EndInOld).ToArray());
            var newText = string.Join("", newWords.Where((s, pos) => pos >= this.StartInNew && pos < this.EndInNew).ToArray());
            oldText = cleanup(oldText);
            newText = cleanup(newText);
            if (oldText.Contains("June"))
                Console.WriteLine(oldText + "\t" + newText);
            //public static string DeEntitize(string text)
            oldText = WebUtility.HtmlDecode(oldText);
            newText = WebUtility.HtmlDecode(newText);
            if(oldText ==  newText)
            {
                Action = Action.Equal;
                Console.WriteLine(oldText + "**" + newText);

            }
            if (oldText.Length == 1 && newText.Length == 1)
            {
                Regex intregex = new Regex(@"[\d]{1}");
                if (!intregex.IsMatch(oldText) && !intregex.IsMatch(newText))
                {
                    Action = Action.Equal;
                    Console.WriteLine(oldText + "**" + newText);
                }
            }
            Regex dateregex = new Regex(@"^(Jan(uary)?|Feb(ruary)?|Ma(r(ch)?|y)|Apr(il)?|Ju((ly?)|(ne?))|Aug(ust)?|Oct(ober)?|(Sep(?=\b|t)t?|Nov|Dec)(ember)?)[\s]*[\d]{1,2}[\,\.]{1}[\s]*[\d]{4}$", RegexOptions.IgnoreCase);
            Regex dateregex2 = new Regex(@"^(Jan(uary)?|Feb(ruary)?|Ma(r(ch)?|y)|Apr(il)?|Ju((ly?)|(ne?))|Aug(ust)?|Oct(ober)?|(Sep(?=\b|t)t?|Nov|Dec)(ember)?)[\s]*[\d]{1,2}$", RegexOptions.IgnoreCase);
            if ((dateregex.IsMatch(oldText) && dateregex.IsMatch(newText)) || (dateregex2.IsMatch(oldText) && dateregex2.IsMatch(newText)) )
            {
                Action = Action.Equal;
                Console.WriteLine(oldText + "\t" + newText);
            }
            //string[] formats = { "MMMM dd", "MMMM dd, YYYY" };
            //try
            //{
            //    var oldd = DateTime.ParseExact(oldText, formats, new CultureInfo("en-US"), DateTimeStyles.None);
            //    var newd = DateTime.ParseExact(newText, formats, new CultureInfo("en-US"), DateTimeStyles.None);
            //    //Debug.WriteLine(string.Format("**** {0}, {1}", oldd, newd));
            //    Action = Action.Replace;
            //    Console.WriteLine(oldText+"\t"+ newText);
            //    return ;
            //}
            //catch (Exception) { }

            //try
            //{
            //    var oldd = float.Parse(oldText);
            //    var newd = float.Parse(newText);
            //    //Debug.WriteLine(string.Format("**** {0}, {1}", oldd, newd));
            //    Action = Action.Equal;
            //    Console.WriteLine(oldText + "\t" + newText);
            //    return;
            //}
            //catch (Exception) { }
        }
    }
}