﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InvoiceAnalyserMainUI
{
    class Pdf
    {

        //make contructor to handle init
        //private Dictionary<string, string> info ;
        int designationColumn;
        int quantityColumn;
        int itemNumberColumn;
        int itemCount;
        internal string headerKeyword;
        string fotterKeyword;
        string date;
        string commandNumber;
        int[] pos;
        bool perUnit;
        bool tvaInclu;
        private Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None);
        private Regex alphaNum_regex = new Regex(@"[^a-zA-Z0-9._/\\]", RegexOptions.None);
        internal bool provider_Exist;
        

        public Dictionary<string, string> info { get; private set; }

        public Pdf()
        {
            // init
            designationColumn = -1;

            info = new Dictionary<string, string>();
            info["factureDate"] = "";
            info["Idate"] = "";
            info["Icommande"] = "";
            info["commande"] = "";
            info["BVR"] = "------------";
            info["prix"] = "0.00";

        }

        public Pdf(string[] prefs)
        {
            // init with values from screen
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

            info = new Dictionary<string, string>();
            info["factureDate"] = "";
            info["Idate"] = "";
            info["Icommande"] = "";
            info["commande"] = "";
            info["BVR"] = "------------";
            info["prix"] = "0.00";
            provider_Exist = true;

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
                    foreach (string line in content.Where(line => !string.IsNullOrWhiteSpace(line.Trim())))
                    {
                        foreach (string word in line.Split(new char[] {' ','-' }))
                        {
                            if (appSettings.AllKeys.Contains(word.ToLowerInvariant()))// make frequency
                            {
                                
                                return word;

                            }
                        }

                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
            }
           
                return "";           
            
        }

        //load provider setting using the provider name as key.
        public void ReadProviderSettings(string provider)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
             
                if (string.IsNullOrEmpty(provider.Trim()))
                {
                    Console.WriteLine("Configuration is null");
                    provider_Exist = false;
                    //To do :show message box
                }
                else
                {
                    provider_Exist = true;
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
                provider_Exist = false;
            }
        }


        static string GetUntil(string text, string stopAt = "0")
        {
            int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

            if (charLocation > 0)
            {
                return text.Substring(charLocation);
            }
            return text;
        }
        // process contents using prefs
        public void ProcessContent(String[] content)// extracted info will be saved in a collection(dictionary in UI cs)
        {
            if (provider_Exist)
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
                decimal price_tv = 0;
                string tv = "8";
                bool look_under = false;
                bool p = false;
                
                StringBuilder infoItem = new StringBuilder();
                bool general_Date = true;
                DateTime Idate = new DateTime();

                foreach (string line in content)
                {
                    if (!string.IsNullOrWhiteSpace(line.Trim()))
                    {
                        if (checkdate)
                        {
                            

                            string word = Process.Next_word_after_keyword(line, date);
                            //Console.WriteLine(word);
                            if (!string.IsNullOrWhiteSpace(word) && word != "Not Found")
                            {
                                info["factureDate"] = word;
                                checkdate = false;
                            }
                            else
                            {
                                if (line.ToUpperInvariant().Contains(date.ToUpperInvariant()))
                                {
                                    string sub = regex.Replace((line.Substring(line.ToUpperInvariant().IndexOf(date.ToUpperInvariant()) + date.Length).Trim()), "\t");
                                    info["factureDate"] = sub.Split('\t').Where(term => term.Trim().Length > 4).ToList()[0];
                                    checkdate = false;
                                }

                                if (general_Date)
                                {
                                    foreach (string words in line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Where(i => i.Trim().Length > 6))
                                    {

                                        if(DateTime.TryParse(words.Trim(), out Idate))
                                        {
                                            info["Idate"] = words.Trim();
                                            general_Date = false;
                                        }
                                    }
                                }
                            }
                        }

                        if (checkorder)
                        {                           
                            string word = Process.Next_word_after_keyword(line, commandNumber);
                            if (line.ToLowerInvariant().Contains(commandNumber.ToLowerInvariant()) && string.IsNullOrWhiteSpace(info["Icommande"]))
                            {
                                string sub = regex.Replace((line.Substring(line.ToLowerInvariant().IndexOf(commandNumber.ToLowerInvariant()) + commandNumber.Length)), "\t");
                                if (!string.IsNullOrWhiteSpace(sub))
                                {
                                    info["Icommande"] = sub.Split('\t').Where(term => term.Trim().Length > 4).ToList()[0];
                                }
                                
                            }

                            if (!string.IsNullOrWhiteSpace(word))
                            {
                                if (!word.Any(char.IsDigit))
                                {

                                    look_under = true;
                                    continue;
                                }

                                info["commande"] = word;
                                checkorder = false;
                            }
                               
                           
                            if (look_under) //next lines
                            {
                                //first words
                                string firs_word = line.Split(' ')[0];
                                if (firs_word.Any(char.IsDigit))
                                {
                                    info["commande"] = firs_word;
                                    look_under = false;
                                }
                                
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
                                string w = GetUntil(word.Trim(), "0");
                                if (w.StartsWith("0") && w.Contains(">") && w.EndsWith(">"))
                                {
                                    info["BVR"] = w;
                                    check = false;                                 


                                    // get invoice toal from the BVR number
                                    if (w.IndexOf('>') != w.LastIndexOf('>'))
                                    {
                                        string total = w.Replace(" ","").Substring(2, w.IndexOf('>') - 2);
                                        info["prix_total"] = total.Insert(total.Length - 3, ".");
                                    }
                                    break;
                                }
                            }
                        }
                        //header section
                        if ( !hfound && Process.isheaader(line, headerKeyword))
                        {
                            hfound = true;
                            Console.WriteLine("<table header> \n" + line);
                            continue;
                        }
                        // footer section
                        if (hfound && !ffound && Process.isfooter(line, fotterKeyword))
                        {
                            ffound = true;
                            // no date found until footer, so filter dates                            

                            Console.WriteLine("<table end found> \n" + line);
                            Console.WriteLine("Itemline fotter - {0}", line_p);
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
                                line_p = line.Replace("  . ", " ").Replace("*"," ");
                                line_p = regex.Replace(line_p, "\t");

                                // get positions from pos and adjute pos in line

                                if (pos != null && line_p.Split('\t').Length >= itemCount - pos.Length)
                                {
                                    Array.Sort(pos);
                                    //insert pos after pos number
                                    string[] terms = line_p.Split(new char[] { '\t' },StringSplitOptions.RemoveEmptyEntries);
                                    foreach (int ipos in pos)
                                    {
                                        terms = line_p.Split('\t');
                                        if (terms[ipos - 1].Trim().IndexOf(" ") != -1)
                                            terms[ipos - 1] = terms[ipos - 1].Insert(terms[ipos - 1].Trim().IndexOf(" "), " \t");
                                        Console.WriteLine("Positions {0}", terms[ipos - 1]);
                                        line_p = string.Join("\t", terms);
                                    }
                                    line_p = string.Join("\t", terms);
                                }
                                //find the item line and item elements
                                line_p = regex.Replace(line_p, "\t");


                                if (line_p.Split('\t').Length >= itemCount && itemCount != 0) // item section start
                                {
                                    try
                                    {
                                        Console.WriteLine("Itemline - {0}", line_p);
                                        checkserial = false;
                                        if (!string.IsNullOrWhiteSpace(name)|| !string.IsNullOrWhiteSpace(item_number))
                                        {
                                            infoItem.AppendLine(name + "|" + quantity + "|" + item_number + "|" + price_ht + "|" +
                                            string.Format("{0:0.00}", price_tv) + "|" + tv + "|" + serial);
                                        }
                                        name = "";
                                        quantity = "";
                                        item_number = "";
                                        price_ht = " ";
                                        serial = "";
                                        line_p = line_p.Replace("_", "-").Replace("—", "-").Replace("*","");

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
                                            price_ht = items[itemCount - 1].Replace(',', '.');
                                            if (!price_ht.Contains('.') | price_ht.Contains(','))
                                            {
                                                price_ht = price_ht.Insert(price_ht.Length - 2, ".");
                                            }
                                            else
                                            {
                                                price_ht = price_ht.Substring(0, price_ht.IndexOf('.') + 3);
                                            }
                                                
                                        }
                                        // logic for getting maximum decimal value in item line for price ht
                                        decimal result;
                                        decimal price = 0;
                                        if (!decimal.TryParse(price_ht, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result))
                                        {
                                            for (int i = items.Length-2; i > 1 ; i--)
                                            {
                                                if ((i != designationColumn - 1 || i != quantityColumn - 1 || i != itemNumberColumn - 1) &&
                                                    decimal.TryParse(items[i], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result))
                                                {
                                                    if (result > price && result.ToString().Length >2)
                                                    {
                                                        price_ht = items[i];
                                                        if(!price_ht.Contains("."))
                                                        price_ht = price_ht.Insert(price_ht.Length - 2, ".");
                                                        
                                                    }
                                                }
                                            }

                                        }
                                        // if last item is not decimal, so strip 

                                        // get price(based on qunatity and rabais)
                                        if (perUnit)
                                        {
                                            int rab_col = itemCount - 2;
                                            decimal rabais = 0;
                                            if ((rab_col != designationColumn - 1 && rab_col != quantityColumn - 1) && rab_col != itemNumberColumn - 1)
                                            {
                                                
                                                try
                                                {
                                                    rabais = decimal.Parse(items[rab_col]);
                                                    if (rabais != decimal.Parse(price_ht) && rabais < 100)
                                                    {
                                                        rabais = rabais / 100 * decimal.Parse(price_ht);
                                                    }
                                                    else
                                                    {
                                                        rabais = 0;
                                                    }
                                                }
                                                catch (Exception)
                                                {
                                                    rabais = 0;
                                                }
                                            }
                                                try
                                                {
                                                    Console.WriteLine("discount = ",rabais);
                                                    decimal prix = (decimal.Parse(price_ht) - rabais) * int.Parse(quantity);

                                                    prix = Math.Round(prix * 20) / 20;

                                                    price_ht = string.Format("{0:0.00}", prix);
                                                }
                                                catch (Exception)
                                                {

                                                }

                                        }
                                        
                                        decimal tva = decimal.Parse(tv) / 100 * decimal.Parse(price_ht);
                                        tva = Math.Round(tva * 20) / 20;
                                        //get price with tax 
                                        if (tvaInclu)
                                        {
                                            price_tv = Math.Truncate(decimal.Parse(price_ht) * 20) / 20; 

                                            price_ht = string.Format("{0:0.00}", decimal.Parse(price_ht) - tva);
                                        }
                                        else
                                        {
                                            price_tv = decimal.Parse(price_ht)  + tva;
                                            price_tv = Math.Truncate(price_tv * 20) / 20;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        price_ht = "0";
                                        price_tv = 0;
                                        
                                    }

                                }
                                // find serial number if switch is on
                                // if (serialSwitch.Value)
                                //{ problem with hard coded keywords
                                //end of item line
                                if (line.Contains("Seriennr.") | line.Contains("Numéro de série") | line.Contains("N.DE SERIE"))
                                {
                                    checkserial = true;
                                }
                                if (checkserial)
                                {
                                    string serial_key_idx = "";
                                    int idxs = line.IndexOf("Seriennr.");
                                    int idxn = line.IndexOf("Numéro de série");
                                    int idxN = line.IndexOf("N.DE SERIE");
                                    if (idxn > -1)
                                    {
                                        serial_key_idx = line.Substring(idxn + 15).Trim(' ', ':').Split(' ')[0];
                                        if (string.IsNullOrWhiteSpace(serial_key_idx) || !serial_key_idx.Any(char.IsDigit))
                                            continue;
                                    }
                                    else if (idxs > -1)
                                    {
                                        serial_key_idx = line.Substring(idxs + 10).Trim(' ', ':').Split(' ')[0];
                                        if (string.IsNullOrWhiteSpace(serial_key_idx) || !serial_key_idx.Any(char.IsDigit))
                                            continue;
                                    }
                                    else if (idxN > -1)
                                    {
                                        serial_key_idx = line.Substring(idxN + 10).Trim(' ', ':').Split(' ')[0];
                                        if (string.IsNullOrWhiteSpace(serial_key_idx) || !serial_key_idx.Any(char.IsDigit))
                                            continue;
                                    }
                                    else
                                    {
                                        string serial_line = regex.Replace(line, "\t");
                                        if (serial_line.Split('\t').Count()  == 0 || serial_line.Split('\t').Count() > 2)
                                            continue;
                                    }
                                    //problem on how to exit form non serial number line which are not items
                                    
                                    if ((string.IsNullOrWhiteSpace(serial_key_idx) || !serial_key_idx.Any(char.IsDigit)) && line.Split(' ')[0].Any(char.IsDigit)) 
                                        //&& serial.Split(';').Count() < int.Parse(quantity)) // the next word is probably not the serial so is beneath
                                    {
                                        Console.WriteLine("check for serial on {0} ", line);
                                        serial += line.Split(' ')[0].Replace('?','7') + ";";
                                    }
                                    else
                                        serial += serial_key_idx ;
                                    //  }
                                }
                            }
                        }
                        
                        // if fotter found then look for total prix, default look for last item in line with toal chf
                        if (ffound)
                        {
                            decimal result;
                            //line having TVA
                            if (line.Contains("TVA"))
                            {
                                //if it contains 8 it's 8
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
                                total = total.Replace("-", string.Empty).Replace("_", string.Empty).Replace("—", string.Empty).Replace("'", string.Empty);
                                if (!decimal.TryParse(total, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result))
                                {
                                    p = true;// total is not on the line, maybe after
                                    continue;
                                }
                                else
                                {
                                    total = total.Replace("-", string.Empty).Replace("_", string.Empty).Replace("—", string.Empty).Replace("'", string.Empty);
                                    if (decimal.TryParse(total, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result))
                                    {
                                        p = false;
                                        if ((!total.Contains('.') | total.Contains(',')) && total.Length > 2 )
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
                                    //Console.WriteLine(total);
                                    total = total.Replace("-", string.Empty).Replace("_", string.Empty).Replace("—", string.Empty).Replace("'", string.Empty);
                                    if (decimal.TryParse(total, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result))
                                    {
                                        p = false;
                                        try
                                        {
                                            if (!total.Contains('.') | total.Contains(','))
                                                total = total.Insert(total.Length - 2, ".");
                                            info["prix"] = total;
                                        }
                                        catch (Exception)
                                        {
                                           
                                        }
                                    }

                                }
                            }

                        }
                    }
                }
                info["item"] = infoItem.ToString();                
                System.Windows.Forms.Application.UseWaitCursor = false;
            }
            else
            {
                Console.WriteLine("Provider not determined");
                System.Windows.Forms.Application.UseWaitCursor = false;
            }
        }
        // for each invoice present to the UI.
        // get short invoice name,(use as key), make buttons dynamically and add events
    }
}
