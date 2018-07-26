using System;
using System.Text.RegularExpressions;

namespace InvoiceAnalyserMainUI
{
    public static class Process
    {
        

        /// <summary>
        /// Returns the number of steps required to transform the source string
        /// into the target string.
        /// </summary>
        public static int ComputeLevenshteinDistance(string source, string target)
       {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
       }

        /// <summary>
        /// Calculate percentage similarity of two strings
        /// <param name="source">Source String to Compare with</param>
        /// <param name="target">Targeted String to Compare</param>
        /// <returns>Return Similarity between two strings from 0 to 1.0</returns>
        /// </summary>
       public static double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }

        public static string Next_word_after_keyword(string contents, string keyword)
        {
            Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None);
            contents = contents.Replace('-', ' ').Replace('_', ' ').Replace('—', ' ').Replace(':', ' ');
            keyword = keyword.Replace('-', ' ').Replace('_', ' ').Replace('—', ' ').Replace(':', ' ');
            string word = " ";
            // split line and check each word for sim. if above threshold, 
            string[] line_terms = regex.Replace(contents, "\t").Split('\t');
            foreach (string term in line_terms)
            {
                //Console.WriteLine(term + "-vs-" + keyword);
                //Console.WriteLine(Process.CalculateSimilarity(term.ToUpperInvariant(), keyword.ToUpperInvariant()));
                if (Process.CalculateSimilarity(term.ToUpperInvariant(), keyword.ToUpperInvariant()) > 0.8)
                {
                    //Console.WriteLine(pline);  // find the word after the keyword and that is the order code cant check for numbers or pattern it keeps changing

                    word = contents.Substring(contents.ToUpperInvariant().IndexOf(term.ToUpperInvariant()) + term.Length).Trim();
                    // Console.WriteLine("word after key is {0}", word);
                    word = regex.Replace(word, "\t");
                    //Console.WriteLine("expected word will be {0}", word.Trim().Split('\t')[0]);
                    break;

                }
            }
            return word.Trim().Split('\t')[0];
        }

        public static bool KeywordIn(string pline, string keyword)
        {
            pline = pline.Replace('-', ' ').Replace('_', ' ').Replace('—', ' ').Replace(':', ' ').Replace('ê', 'e');
            keyword = keyword.Replace('-', ' ').Replace('_', ' ').Replace('—', ' ').Replace(':', ' ').Replace('ê', 'e');
            // split line and check each word for sim. if above threshold, 
            string[] line_terms =  pline.Split('\t'); //regex.Replace(pline, "\t").Split('\t');
            foreach (string term in line_terms)
            {

                if (Process.CalculateSimilarity(term.ToUpperInvariant(), keyword.ToUpperInvariant()) > 0.8)
                {
                    //Console.WriteLine("word {0} is in {1}", term, pline);  
                    // find the word after the keyword and that is the order code cant check for numbers or pattern it keeps changing
                    return true;

                }
            }
            return false;
        }

        public static bool isheaader(string line, string key)
        {
            line = line.Replace('-', ' ').Replace('é', 'e').Replace('—', ' ').Replace('è', 'e').Replace('ê', 'e').ToUpperInvariant();
             key = key.Replace('-', ' ').Replace('é', 'e').Replace('—', ' ').Replace('è', 'e').Replace('ê', 'e').ToUpperInvariant();

            if (line.Contains(key))
            {

                return true;
            }
            return false;
        }

        public static bool isfooter(string line, string key)
        {
            line = line.Replace('-', ' ').Replace('_', ' ').Replace('—', ' ').Replace(':', ' ').Replace('ê', 'e').ToUpperInvariant();
             key = key.Replace('-', ' ').Replace('_', ' ').Replace('—', ' ').Replace(':', ' ').Replace('ê', 'e').ToUpperInvariant();
            if (line.Contains(key))
            {
                return true;
            }
            return false;
        }
    }
    // find the general info section
    //if (!hfound)

    /*so after all the time and thinking to make this code its useless 
     * Regex rex = new Regex(@"[ ]{3,}", RegexOptions.None);
    //facture date - found
    if (line.ToUpperInvariant().Contains(date_de_factureTextbox.Text.ToUpperInvariant()))
    {
        string dline = line.Replace(':', ' ');
        Console.WriteLine(dline);
        dline = rex.Replace(dline, "\t");
        string date = dline.Substring(dline.ToUpperInvariant().IndexOf(date_de_factureTextbox.Text.ToUpperInvariant())
            + date_de_factureTextbox.Text.Trim().Length).Trim();

        //dline = dline.Replace(line, "\t");
        //dline = dline.Replace(':', ' ');// weird cos first one is not stripping the seperator oh so smart me working on a different varaible and expecting change
        Console.WriteLine(date);
        int idx = date.IndexOf('\t');
        if (idx != -1)
        {
            factureDate_label.Text = date.Substring(0, idx).Trim();
        }
        else
            factureDate_label.Text = date.Trim();
    }
    // commande number 
    // different forms of number formats... (potentially fixed)
    //string line_rep = line.Replace('-', ' ').Replace('_', ' ').Replace('—', ' ').Replace(':', ' ');
    //string key_rep = commande_Textbox.Text.Replace('-', ' ').Replace('_', ' ').Replace('—', ' ').Replace(':', ' ');
    ocr have problem recogniznig the various forms of hyphenation and dashes(pauntuation causes problems
    if (line_rep.ToUpperInvariant(). Contains(key_rep.ToUpperInvariant()))
    {                
        Console.WriteLine(line_rep);  // find the word after the keyword and that is the order code cant check for numbers or pattern it keeps changing
        line_rep = regex.Replace(line_rep, "\t");
        string order = line_rep.Substring(line_rep.ToUpperInvariant().IndexOf(key_rep.ToUpperInvariant()) + key_rep.Length).Trim();
        Console.WriteLine(order);
        /*int idx = order.IndexOf(" ");
        if (idx != -1)
        {
            commande_Label.Text = order.Substring(0, idx).Trim();
        }
        else
            commande_Label.Text = order.Trim();
        commande_Label.Text = order.Split('\t')[0];
    }*/

    // save name of provider separately for lookup
    /*List<string> providers = new List<string>(); 
        providers.Add(Settings.Default["providers"].ToString());
    if (!providers.Contains(providerTextbox.Text))
    {
        providers.Add(providerTextbox.Text);

    }
    StringBuilder builder = new StringBuilder();
    foreach (string provider in providers) // Loop through all strings
    {
        if (!string.IsNullOrEmpty(provider))
        {
            builder.Append(provider).Append("|");
        }
         // Append string to StringBuilder
    }
    Settings.Default["providers"] = builder.ToString(); // Get string from StringBuilder
    Settings.Default.Save();*/


}
