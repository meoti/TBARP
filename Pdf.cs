using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InvoiceAnalyserMainUI
{
    class Pdf
    {

        //make contructor to handle init
        private Dictionary<string, int> dict = new Dictionary<string, int>();
        //private Dictionary<string, string> info ;
        int designationColumn;
        int quantityColumn;
        int itemNumberColumn;
        int itemCount;
        string headerKeyword;
        string fotterKeyword;
        string date;
        string commandNumber;
        int[] pos;
        bool perUnit;
        bool tvaInclu;
        private Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None);

        public Dictionary<string, string> info { get; private set; }

        public Pdf()
        {
            // init
            designationColumn = -1;

            info = new Dictionary<string, string>();
            info["factureDate"] = "";
            info["commande"] = "";
            info["BVR"] = "------------";
            info["prix"] = "0.00";

        }

        // handle drag and drop

        // get list of pdf files to process, each is an invoice-
        //  --- convert image , find array and ocr async(method exist)


        // feed as argument content string split by sapce.
        public string GetProviderName(string[] content)
        {
            // get all existing provider names from config xml
            try
            {
                var appSettings = ConfigurationManager.AppSettings;// move up or make local?

                if (appSettings.Count == 0)
                {
                    // To-do
                    Console.WriteLine("No template-- make appropraite message.");
                }
                else
                {
                    foreach (string line in content)
                    {
                        foreach (string word in line.Split(' '))
                        {
                            if (appSettings.AllKeys.Contains(word.ToLowerInvariant()))// make frequency
                            {
                                if (!dict.ContainsKey(word))
                                {
                                    dict.Add(word, 1);
                                }
                                else
                                {
                                    dict[word]++;
                                }
                            }
                        }

                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
            }
            // key provider of max freq.
            // if all providers is none handle(case provider not found)
            
                if(dict.Count > 0)
                return dict.OrderBy(x => x.Value).First().Key;
            else
            {
                return " ";
            }
            
        }

        //load provider setting using the provider name as key.
        public void ReadProviderSettings(string provider)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

                if (string.IsNullOrEmpty(provider.Trim()))
                {
                    Console.WriteLine("Provider Not found");
                    //To do :show message box
                }
                else
                {

                    // split list by tabs for each setting value( split pos by ,)
                    Console.WriteLine("Key: {0} Value: {1}", provider, appSettings[provider]);
                    string[] prefs = appSettings[provider].Split('\t');
                    designationColumn = int.Parse(prefs[1]);
                    quantityColumn = int.Parse(prefs[2]);
                    itemNumberColumn = int.Parse(prefs[3]);
                    itemCount = int.Parse(prefs[5]);
                    headerKeyword = prefs[0];
                    fotterKeyword = prefs[8];
                    date = prefs[9];
                    commandNumber = prefs[10];
                    string s_pos = prefs[4];
                    if (!string.IsNullOrEmpty(s_pos))
                    {
                        pos = s_pos.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                    }
                    perUnit = bool.Parse(prefs[6]);
                    tvaInclu = bool.Parse(prefs[7]);


                }
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
            }
        }

        // process contents using prefs
        public void ProcessContent(String[] content)// extracted info will be saved in a collection(dictionary in UI cs)
        {
            // process content will always take the string content(from the ocr getcontent or the texarea) process the spaces and dashes 
            // and update the contents array as well as the text area
            System.Windows.Forms.Application.UseWaitCursor = true;
            Console.WriteLine("processing");
            bool hfound = false;
            bool ffound = false;
            bool check = true;
            bool checkdate = true;
            bool checkorder = true;
            bool checkserial = false;
            string serial = "";
            string line_p = null;
            string name = "";
            string quantity = "";
            string item_number = "";
            string price_ht = " ";
            double price_tv = 0;
            string tv = "8";
            bool n = false;
            bool p = false;

            StringBuilder infoItem = new StringBuilder();

            foreach (string line in content)
            {
                if (checkdate)
                {
                    string word = Process.Next_word_after_keyword(line, date);
                    //Console.WriteLine(word);
                    if (!string.IsNullOrEmpty(word))
                    {
                        info["factureDate"] = word;
                        checkdate = false;
                    }
                }

                if (checkorder)
                {
                    string word = Process.Next_word_after_keyword(line, commandNumber);
                    // Console.WriteLine(word);
                    if (!string.IsNullOrEmpty(word))
                    {
                        if (!word.Any(char.IsDigit))
                        {
                            n = true;
                            continue;

                        }

                        info["commande"] = word;
                        checkorder = false;
                    }
                    if (n)
                    {
                        info["commande"] = line.Split(' ')[0];
                        checkorder = false;
                    }
                }


                //get bvr - found
                if (check) // stop checking for the bvr once found
                {
                    Regex rex = new Regex(@"[ ]{3,}", RegexOptions.None);
                    string line_ = rex.Replace(line, "\t");
                    string[] words = line_.Split('\t');
                    foreach (string word in words)
                    {
                        if (word.StartsWith("0") && word.Contains(">") && word.EndsWith(">"))
                        {
                            info["BVR"] = word;
                            check = false;
                            break;
                        }
                    }
                }
                //header section
                if (!hfound && Process.isheaader(line, headerKeyword))
                {
                    hfound = true;
                    Console.WriteLine("<table header> \n" + line);
                    continue;
                }
                // footer section
                if (Process.isfooter(line, fotterKeyword))
                {
                    ffound = true;
                    Console.WriteLine("<table end found> \n" + line);
                    if (!string.IsNullOrEmpty(name))
                        infoItem.AppendLine(name + "|" + quantity + "|" + item_number + "|" + price_ht + "|" +
                                string.Format("{0:0.00}", price_tv) + "|" + tv + "|" + serial);
                    checkserial = false;
                }

                line_p = null;
                //item section
                if (!headerKeyword.Equals("None", StringComparison.OrdinalIgnoreCase))
                {
                    //try
                    //{
                    if (hfound && !ffound)
                    {
                        line_p = line.Replace("  . ", " ");
                        line_p = regex.Replace(line_p, "\t");

                        // get positions from pos and adjute pos in line

                        if (pos != null && line_p.Split('\t').Length >= itemCount - pos.Length)
                        {
                            Array.Sort(pos);
                            //insert pos after pos number
                            string[] terms = line_p.Split('\t');
                            foreach (int ipos in pos)
                            {
                                terms = line_p.Split('\t');
                                if (terms[ipos - 1].Trim().IndexOf(" ") != -1)
                                    terms[ipos - 1] = terms[ipos - 1].Insert(terms[ipos - 1].Trim().IndexOf(" "), " \t");
                                Console.WriteLine(terms[ipos - 1]);
                                line_p = string.Join("\t", terms);
                            }
                            line_p = string.Join("\t", terms);
                        }
                        //find the item line and item elements
                        line_p = regex.Replace(line_p, "\t");

                        if (line_p.Split('\t').Length >= itemCount) // item section start
                        {
                            Console.WriteLine("Itemline - {0}", line_p);
                            checkserial = false;
                            if (!string.IsNullOrEmpty(name))
                                infoItem.AppendLine(name + "|" + quantity + "|" + item_number + "|" + price_ht + "|" +
                                string.Format("{0:0.00}", price_tv) + "|" + tv + "|" + serial);
                            name = "";
                            quantity = "";
                            item_number = "";
                            price_ht = " ";
                            serial = "";

                            string[] items = line_p.Split('\t');
                            if (designationColumn != -1)
                            {
                                name = items[designationColumn - 1];
                            }

                            if (quantityColumn != -1)
                            {
                                quantity = items[quantityColumn - 1];
                                quantity = Regex.Replace(quantity, "[^0-9.]", "");
                            }

                            if (itemNumberColumn != -1)
                            {
                                item_number = items[itemNumberColumn - 1];
                            }

                            if (itemCount != -1)
                            {
                                price_ht = items[itemCount - 1];
                            }

                            double result;
                            double price = 0;
                            if (!double.TryParse(price_ht, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result))
                            {
                                for (int i = 1; i < items.Length; i++)
                                {
                                    if ((i != designationColumn - 1 || i != quantityColumn - 1 || i != itemNumberColumn - 1) &&
                                        double.TryParse(items[i], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result))
                                    {
                                        if (result > price)
                                        {
                                            price_ht = items[i];
                                        }
                                    }
                                }

                            }
                            // if last item is not double, so strip 

                            // get price(based on qunatity and rabais)
                            if (perUnit)
                            {
                                int rab_col = itemCount - 2;
                                if ((rab_col != designationColumn - 1 || rab_col != quantityColumn - 1) || rab_col != itemNumberColumn - 1)
                                {
                                    double rabais = double.Parse(items[rab_col]);
                                    if (rabais != double.Parse(price_ht) && rabais < 100)
                                    {
                                        Console.WriteLine(rabais);
                                        double prix = (double.Parse(price_ht) - (rabais / 100 * double.Parse(price_ht))) * int.Parse(quantity);

                                        prix = Math.Round(prix * 20) / 20;

                                        price_ht = string.Format("{0:0.00}", prix);
                                    }
                                }
                            }
                            double tva = double.Parse(tv) / 100 * double.Parse(price_ht);
                            tva = Math.Round(tva * 20) / 20;
                            //get price with tax or without
                            if (tvaInclu)
                            {
                                price_tv = double.Parse(price_ht);
                                price_ht = string.Format("{0:0.00}", double.Parse(price_ht) - tva);
                            }
                            else
                            {
                                price_tv = double.Parse(price_ht) + tva;
                            }

                        }
                        // find serial number if switch is on
                        // if (serialSwitch.Value)
                        //{ problem with hard coded keywords
                        if (line.Contains("Seriennr.") | line.Contains("Numéro de série") | line.Contains("N.DE SERIE"))
                        {
                            checkserial = true;
                        }
                        if (checkserial)
                        {
                            string s = "";
                            int idxs = line.IndexOf("Seriennr.");
                            int idxn = line.IndexOf("Numéro de série");
                            int idxN = line.IndexOf("N.DE SERIE");
                            if (idxn > -1)
                            {
                                s = line.Substring(idxn + 15).Trim(' ', ':').Split(' ')[0];
                                if (string.IsNullOrEmpty(s) || !s.Any(char.IsDigit))
                                    continue;
                            }
                            else if (idxs > -1)
                            {
                                s = line.Substring(idxs + 10).Trim(' ', ':').Split(' ')[0];
                                if (string.IsNullOrEmpty(s) || !s.Any(char.IsDigit))
                                    continue;
                            }
                            else if (idxN > -1)
                            {
                                s = line.Substring(idxN + 10).Trim(' ', ':').Split(' ')[0];
                                if (string.IsNullOrEmpty(s) || !s.Any(char.IsDigit))
                                    continue;
                            }
                            //problem on how to exit form non serial number line which are not items
                            if ((string.IsNullOrEmpty(s) || !s.Any(char.IsDigit))) // the next word is probably not the serial so is beneath
                            {
                                Console.WriteLine("check for serial on {0}", line);
                                serial += line.Split(' ')[0] + ";";
                            }
                            else
                                serial += s;
                            //  }
                        }
                        //make item ine

                    }
                    //}
                    /*catch (Exception ex)
                    {
                        //Trace.TraceError(ex.ToString());
                        Console.WriteLine("Unexpected Error: \n" + ex.Message);
                    }*/
                }
                // if fotter found then look for total prix, default look for last item in line with toal chf
                if (ffound)
                {
                    double result;
                    //line having TVA
                    if (line.Contains("TVA"))
                    {
                        //if it contains 8 it 8
                        if (line.Contains("8"))
                            tv = "8.00";
                        //if not substring after tva , and strip all letters and take number
                        else
                        {
                            tv = line.Substring(line.IndexOf("TVA") + 3);
                            tv = Regex.Replace(tv, "[^0-9.]", "");

                        }
                    }
                    //line having total after fotter
                    if (line.ToLowerInvariant().Contains("total"))
                    {
                        string total = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Last();
                        if (string.IsNullOrEmpty(total) || !total.Any(char.IsDigit))
                        {
                            p = true;
                            continue;
                        }
                        else
                        {
                            total = total.Replace("-", string.Empty).Replace("_", string.Empty).Replace("—", string.Empty);
                            if (double.TryParse(total, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result))
                            {
                                p = false;
                                if (!total.Contains('.'))
                                    total = total.Insert(total.Length - 2, ".");
                                info["prix"] = total;
                            }
                        }

                    }
                    else
                    {
                        if (p)
                        {
                            string total = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Last();
                            Console.WriteLine(total);
                            total = total.Replace('-', ',').Replace('_', ',').Replace('—', ',');
                            if (double.TryParse(total, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result))
                            {
                                p = false;
                                if (!total.Contains('.') | total.Contains(','))
                                    total = total.Insert(total.Length - 2, ".");
                                info["prix"] = total;
                            }

                        }
                    }


                }
            }
            info["item"] = infoItem.ToString();
            Console.WriteLine("done");
            System.Windows.Forms.Application.UseWaitCursor = false;
        }

        // for each invoice present to the UI.
        // get short invoice name,(use as key), make buttons dynamically and add events
    }
}
