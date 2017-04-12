using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.Text.RegularExpressions;
using HtmlDiff;
using System.Collections;
using System.Globalization;

namespace Differ
{
    class Program
    {
        static string cleanup(string strin)
        {
            string text = strin.Trim();  //trim the text 
            string result = Regex.Replace(text, @"\r\n?|\n", " "); //replace the line breaks with empty string
            //result = Regex.Replace(result, "<.*?>", string.Empty); //replace unwanted html with empty string 
            result = Regex.Replace(result, "&nbsp;", " "); // replace &nbsp; with space(“ “)
            result = Regex.Replace(result, @"\s+", " "); //replace multiple spaces with single space 
            return result;
        }

        static void Main1(string[] args)
        {
            string oldText = "This is a date Septermber 4, 2016 that will change";
            string newText = "This is a date March 5 2017 that won't change";
            var diff = new global::HtmlDiff.HtmlDiff(oldText, newText);

            // purposefully cause an overlapping expression
            // var pattern = @"(Jan|Feb)[\s]*[\d]{1,2}[\s]*[\d]{4}";

            var ex = diff.Build();
            Console.WriteLine(ex);
            Console.Read();
            return;
        }

        static void Main2(string[] args)
        {
            string oldText = "this is 123,34.78% abc.sdfg";
            string newText = "this is 234,35.6% abc.ert";
            var diff = new global::HtmlDiff.HtmlDiff(oldText, newText);

            // purposefully cause an overlapping expression
            diff.AddBlockExpression(new Regex(@"\d+\.?\d+", RegexOptions.IgnoreCase));

            var ex = diff.Build();
            Console.WriteLine(ex);
            Console.Read();
            return;
        }


        static void Main(string[] args)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            //string urlnew = "http://globaldocuments.morningstar.com/DocumentLibrary/Document/81d1472d3c8f11f430218f84ac24deab.msdoc/original/d284756d10q.htm";
            //string urlold = "http://globaldocuments.morningstar.com/DocumentLibrary/Document/b4acd5a4e7f551a2532e6ada39c37ae8.msdoc/original/d229830d10q.htm";

            string urlold = "https://www.sec.gov/Archives/edgar/data/789019/000156459017000654/msft-10q_20161231.htm";
            string urlnew = "https://www.sec.gov/Archives/edgar/data/789019/000119312516742796/d245252d10q.htm";
            string newFiling = wc.DownloadString(urlnew);
            string oldFiling = wc.DownloadString(urlold);

            newFiling = cleanup(newFiling);
            oldFiling = cleanup(oldFiling);

            HtmlAgilityPack.HtmlDocument htmlDocNew = new HtmlAgilityPack.HtmlDocument();
            htmlDocNew.LoadHtml(newFiling);
            var title = htmlDocNew.DocumentNode.SelectSingleNode("//head/title");
            Console.WriteLine(title.InnerText);

            HtmlAgilityPack.HtmlDocument htmlDocOld = new HtmlAgilityPack.HtmlDocument();
            htmlDocOld.LoadHtml(oldFiling);

            StreamWriter swTablesNew = new StreamWriter("tablesNew.html");

            Hashtable htDataTablesNew = new Hashtable();
            int tableSequenceNew = 0;
            foreach (HtmlNode table in htmlDocNew.DocumentNode.SelectNodes("//table"))
            {
                ++tableSequenceNew;
                swTablesNew.WriteLine(table.OuterHtml + "<br/><br/><br/>");
                int rows = 0;
                bool dataTable = false;
                if (table.SelectNodes("tr") != null)
                {
                    foreach (HtmlNode row in table.SelectNodes("tr"))
                    {
                        if (++rows > 1)
                        {
                            dataTable = true;
                            break;
                        }

                    }
                }
                if (dataTable)
                {
                    string table_Id = "Table_Sequence_Id_" + tableSequenceNew.ToString();
                    htDataTablesNew.Add(table_Id, table);
                    HtmlNode spaceHolderNode = HtmlNode.CreateNode("<Place_Holder tableId=\"" + table_Id + "\"></Place_Holder>");
                    table.ParentNode.ReplaceChild(spaceHolderNode, table);
                }
            }
            swTablesNew.Flush();

            StreamWriter swTablesOld = new StreamWriter("tablesOld.html");

            Hashtable htDataTablesOld = new Hashtable();
            int tableSequenceOld = 0;
            foreach (HtmlNode table in htmlDocOld.DocumentNode.SelectNodes("//table"))
            {
                ++tableSequenceOld;
                swTablesOld.WriteLine(table.OuterHtml + "<br/><br/><br/>");
                int rows = 0;
                bool dataTable = false;
                if (table.SelectNodes("tr") != null)
                {
                    foreach (HtmlNode row in table.SelectNodes("tr"))
                    {
                        if (++rows > 1)
                        {
                            dataTable = true;
                            break;
                        }

                    }
                }
                if (dataTable)
                {
                    string table_Id = "Table_Sequence_Id_" + tableSequenceOld.ToString();
                    htDataTablesOld.Add(table_Id, table);
                    HtmlNode spaceHolderNode = HtmlNode.CreateNode("<Place_Holder tableId=\"" + table_Id + "\"></Place_Holder>");
                    table.ParentNode.ReplaceChild(spaceHolderNode, table);
                }
            }
            swTablesOld.Flush();

            StreamWriter swold = new StreamWriter("c:/temp/old.html");
            swold.Write(htmlDocOld.DocumentNode.OuterHtml);
            swold.Flush();
            StreamWriter swnew = new StreamWriter("c:/temp/new.html");
            swnew.Write(htmlDocNew.DocumentNode.OuterHtml);
            swnew.Flush();
            global::HtmlDiff.HtmlDiff diff = new global::HtmlDiff.HtmlDiff(htmlDocOld.DocumentNode.OuterHtml, htmlDocNew.DocumentNode.OuterHtml);

            diff.AddBlockExpression(new Regex(@"(Jan(uary)?|Feb(ruary)?|Ma(r(ch)?|y)|Apr(il)?|Ju((ly?)|(ne?))|Aug(ust)?|Oct(ober)?|(Sep(?=\b|t)t?|Nov|Dec)(ember)?)[\s]*[\d]{1,2}[\s]*[\d]{4}", RegexOptions.IgnoreCase));
            diff.AddBlockExpression(new Regex(@"(Jan(uary)?|Feb(ruary)?|Ma(r(ch)?|y)|Apr(il)?|Ju((ly?)|(ne?))|Aug(ust)?|Oct(ober)?|(Sep(?=\b|t)t?|Nov|Dec)(ember)?)[\s]*[\d]{1,2}[\,\.]{1}[\s]*[\d]{4}", RegexOptions.IgnoreCase));
            diff.AddBlockExpression(new Regex(@"(\$\d+.\d+)"));
            diff.AddBlockExpression(new Regex(@"(\d+,\d+)"));
            //diff.AddBlockExpression(new Regex(@"(\d+.\d+%)"));
            diff.IgnoreWhitespaceDifferences = true;
            diff.OrphanMatchThreshold = 0.1;
            var delta = diff.Build();
            
            HtmlDocument htmlDocDelta = new HtmlDocument();
            htmlDocDelta.LoadHtml(delta);

            foreach (HtmlNode table in htmlDocDelta.DocumentNode.SelectNodes("//place_holder"))
            {
                table.ParentNode.ReplaceChild((HtmlNode)htDataTablesNew[table.Attributes[0].Value], table); ;
            }
            //HtmlTextNode Hnode = htmlDocDelta.DocumentNode.SelectSingleNode("//head/title") as HtmlTextNode;
            //Hnode.Text = title.InnerText;

            string head="<head><link href=\"App_Themes/default/styles.css\" type=\"text/css\" rel=\"stylesheet\" /></head>";
            StreamWriter sw = new StreamWriter("c:/temp/mytest.html");
            sw.Write(head+htmlDocDelta.DocumentNode.OuterHtml);
            sw.Flush();
            //Console.Read();
        }
    }
}
