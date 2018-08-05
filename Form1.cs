using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Bunifu.Framework.UI;
using System.Configuration;
using System.Collections.ObjectModel;
using System.Threading;
using System.Diagnostics;

namespace InvoiceAnalyserMainUI
{
    public partial class Form1 : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private string[] contents;
        private List<int> pos = new List<int>();
        private List<Listbutton> dynaPdfBtns = new List<Listbutton>();
        private Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None);
        private Regex return_regex = new Regex("[\r\n]{2,}", RegexOptions.None);
        private string tv;
        private ObservableCollection<string> _files = new ObservableCollection<string>();
        private Dictionary<string, string> pdfData = new Dictionary<string, string>();
        private int i = 0;

        // to handle top panel dragging- i have no idea how the hell it does it
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();


        public Form1()
        {
            InitializeComponent();

        }

        private void bunifuImageButton5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnMenu_Click(object sender, EventArgs e)
        {
            // expanding
            // get width if its 45 expand to 219
            // show logo
            if (sideMenu.Width == 38)
            {
                Expand();
            }
            else
            {
                Minimize();
            }
        }

        static string GetUntil( string text, string stopAt = "0")
        {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(charLocation);
                }
            return text;
        }
            private void ProcessContent(String[] content)
        {
            // process content will always take the string content(from the ocr getcontent or the texarea) process the spaces and dashes 
            // and update the contents array as well as the text area
            Application.UseWaitCursor = true;
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
            tv = "8";
            bool n = false;
            bool p = false;

            foreach (string line in content)
            {
                if (!string.IsNullOrWhiteSpace(line.Trim()))
                {

                    if (checkdate)
                    {
                        string word = Process.Next_word_after_keyword(line, date_de_factureTextbox.Text);
                        //Console.WriteLine(word);
                        if (!string.IsNullOrEmpty(word))
                        {
                            factureDate_label.Text = word;
                            checkdate = false;
                        }
                    }

                    if (checkorder)
                    {
                        string word = Process.Next_word_after_keyword(line, commandeTextbox.Text);
                        // Console.WriteLine(word);
                        if (!string.IsNullOrEmpty(word))
                        {
                            if (!word.Any(char.IsDigit))
                            {
                                n = true;
                                continue;

                            }

                            commande_Label.Text = word;
                            checkorder = false;
                        }
                        if (n)
                        {
                            commande_Label.Text = line.Split(' ')[0];
                            checkorder = false;
                        }
                    }


                    //get bvr - found
                    if (check)
                    {
                        Regex rex = new Regex(@"[ ]{3,}", RegexOptions.None);
                        string line_ = rex.Replace(line, "\t");
                        string[] words = line_.Split('\t');
                        foreach (string word in words)
                        {
                            string w = GetUntil(word.Trim(), "0");
                            if (w.StartsWith("0") && w.Contains(">") && w.EndsWith(">"))
                            {
                                BVR_Label.Text = w;
                                check = false;
                                break;
                            }
                        }
                    }
                    //header section
                    if (!hfound && Process.isheaader(line, header_nameTextbox.Text))
                    {
                        hfound = true;
                        Console.WriteLine("<table header> \n" + line);
                        continue;
                    }
                    // footer section
                    if (Process.isfooter(line, fotterNameTextBox.Text))
                    {
                        ffound = true;
                        Console.WriteLine("<table end found> \n" + line);
                        if (!string.IsNullOrEmpty(name))
                            bunifuCustomDataGrid2.Rows.Add(name, quantity, item_number, price_ht, string.Format("{0:0.00}", price_tv), tv, serial);
                        checkserial = false;
                    }

                    line_p = null;
                    //item section
                    if (!header_nameTextbox.Text.Equals("None", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            if (hfound && !ffound)
                            {
                                line_p = line.Replace("  . ", " ");
                                line_p = regex.Replace(line_p, "\t");

                                // get positions from pos and adjute pos in line
                                pos.Sort();
                                if (pos.Count != 0 && line_p.Split('\t').Length >= int.Parse(itemCountTextbox.Text) - pos.Count)
                                {
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

                                if (line_p.Split('\t').Length >= int.Parse(itemCountTextbox.Text.Trim())) // item section start
                                {
                                    Console.WriteLine("Itemline - {0}", line_p);
                                    checkserial = false;
                                    if (!string.IsNullOrEmpty(name))
                                        bunifuCustomDataGrid2.Rows.Add(name, quantity, item_number, price_ht, string.Format("{0:0.00}", price_tv), tv, serial);
                                    name = "";
                                    quantity = "";
                                    item_number = "";
                                    price_ht = " ";
                                    serial = "";
                                    //put in the datgrid name, quanantitie, itemnumber , price t, price c, tva, serial number                        

                                    // validate for non-existing columns
                                    string[] items = line_p.Split('\t');
                                    if (!string.IsNullOrEmpty(designationTextbox.Text) || !designationTextbox.Text.Equals("-1"))
                                    {
                                        name = items[int.Parse(designationTextbox.Text) - 1];
                                    }

                                    if (!string.IsNullOrEmpty(quantityTextbox.Text) || !quantityTextbox.Text.Equals("-1"))
                                    {
                                        quantity = items[int.Parse(quantityTextbox.Text) - 1];
                                        quantity = Regex.Replace(quantity, "[^0-9.]", "");
                                    }

                                    if (!string.IsNullOrEmpty(itemNumberTextbox.Text) || !itemNumberTextbox.Text.Equals("-1"))
                                    {
                                        item_number = items[int.Parse(itemNumberTextbox.Text) - 1];
                                    }

                                    if (!string.IsNullOrEmpty(itemCountTextbox.Text) || !itemCountTextbox.Text.Equals("-1"))
                                    {
                                        price_ht = items[int.Parse(itemCountTextbox.Text) - 1];
                                    }

                                    double result;
                                    double price = 0;
                                    if (!double.TryParse(price_ht, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result))
                                    {
                                        for (int i = 1; i < items.Length; i++)
                                        {
                                            if ((i != int.Parse(designationTextbox.Text) - 1 || i != int.Parse(quantityTextbox.Text) - 1 || i != int.Parse(itemNumberTextbox.Text) - 1) &&
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
                                    if (per_unitSwitch.Value)
                                    {
                                        int rab_col = int.Parse(itemCountTextbox.Text) - 2;
                                        if ((rab_col != int.Parse(designationTextbox.Text) - 1 || rab_col != int.Parse(quantityTextbox.Text) - 1) || rab_col != int.Parse(itemNumberTextbox.Text) - 1)
                                        {
                                            double rabais = 0;
                                            try
                                            {
                                                rabais = double.Parse(items[rab_col]);
                                                if (rabais != double.Parse(price_ht) && rabais < 100)
                                                {
                                                    Console.WriteLine(rabais);
                                                    double prix = (double.Parse(price_ht) - (rabais / 100 * double.Parse(price_ht))) * int.Parse(quantity);
                                                    prix = Math.Round(prix * 20) / 20;
                                                    price_ht = string.Format("{0:0.00}", prix);
                                                }
                                            }
                                            catch (Exception)
                                            {

                                            }

                                        }
                                    }
                                    double tva = double.Parse(tv) / 100 * double.Parse(price_ht);
                                    tva = Math.Round(tva * 20) / 20;
                                    //get price with tax or without
                                    if (tvaSwitch.Value)
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
                            }
                        }
                        catch (Exception ex)
                        {
                            //Trace.TraceError(ex.ToString());
                            Console.WriteLine("Unexpected Error: \n" + ex.Message);
                        }
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
                                    prix_label.Text = total;
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
                                    prix_label.Text = total;
                                }

                            }
                        }


                    }
                }
            }
            //Console.WriteLine("done");
            Application.UseWaitCursor = false;
        }

        
        private void maxButton_Click(object sender, EventArgs e)
        {
            maximize();
        }
        private void maximize()
        {
            panelTransition1.HideSync(maxBtn);
            content_panel.Visible = false;
            content_panel.Width = 844;//from 806
            homeTransition.ShowSync(content_panel);
            logoTransition.ShowSync(hideBtn);
        }

        private void Expand()
        {
            if(sideMenu.Width < 116)
            //logoTransition.Hide(logo_small);
            sideMenu.Visible = false;
            sideMenu.Width = 116;

            panelTransition1.ShowSync(sideMenu);
            //logoTransition.ShowSync(logo);
        }

        private void Minimize()
        {

            //minimize
            // logoTransition.Hide(logo);
            sideMenu.Visible = false;
            sideMenu.Width = 38;
            panelTransition.ShowSync(sideMenu);
            // logoTransition.ShowSync(logo_small);
        }
        private void hideButton_Click(object sender, EventArgs e)
        {
            panelTransition1.HideSync(hideBtn);
            content_panel.Visible = false;
            content_panel.Width = 30;
            panelTransition1.ShowSync(content_panel);
            logoTransition.ShowSync(maxBtn);
        }

        private void processButton_Click(object sender, EventArgs e)
        {
            // label cleaning
            factureDate_label.Text = "Not found";
            commande_Label.Text = "Not found";
            BVR_Label.Text = "Not found";
            prix_label.Text = "0.00";
            bunifuCustomDataGrid2.Rows.Clear();
            //ProcessContent(contents);

            string[] pref = new string[]{ header_nameTextbox.Text,designationTextbox.Text, quantityTextbox.Text, itemNumberTextbox.Text, string.Join(",", pos),
            itemCountTextbox.Text, per_unitSwitch.Value.ToString(),tvaSwitch.Value.ToString(), fotterNameTextBox.Text,date_de_factureTextbox.Text, commandeTextbox.Text};

            Pdf pdf = new Pdf(pref);
            //string provider = pdf.GetProviderName(contents);
            //pdf.ReadProviderSettings(provider);
            //providerTextbox.Text = provider;
            pdf.ProcessContent(contents);

            var info = pdf.info;
            //put info in UI
            Print_To_Screen(info);

        }
        private async void loadBtn_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.           
            if (result == DialogResult.OK)
            {
                pos = new List<int>();
                //UI cleaning 
                content_Box.Clear();
                content_Box.Visible = false;
                close_Button.Visible = false;
                //load_Button.Enabled = false;
                maximize();
                loading_Box.Visible = true;
                feed_label.Visible = true;
                feed_label.Text = "PW Processing invoice ...";
                // async OCR
                StringBuilder scontent = await DolazyOcr(openFileDialog1.FileName);
                //move strings to the content array
                string[] delim = { Environment.NewLine, "\n" };
                string txt = scontent.ToString();

                txt = txt.Replace("\n\r\n\t\n\n\r\n", "");
                txt = txt.Replace("\n\n\r\n", Environment.NewLine);
                txt = txt.Replace("\n\r\n", Environment.NewLine);
                //txt = return_regex.Replace(txt, Environment.NewLine);
                contents = txt.Split(delim, StringSplitOptions.None);
                //Console.WriteLine(txt);
                txt = regex.Replace(txt, "\t");
                content_Box.AppendText(txt.Replace(" ", "-"));
                //UI cleaning 
                loading_Box.Visible = false;
                feed_label.Visible = false;
                content_Box.Visible = true;
                close_Button.Visible = true;
                processButton.Visible = true;
                //load_Button.Enabled = true;
            }
        }

        private Task<StringBuilder> DolazyOcr(string pdffile)
        {
            // make an ocr instance and call it on a tread to getcontent
            OCR ocr = new OCR();
            //start ocring with a new task
            return Task.Run(() =>
            {
                List<string> imgs = new List<string>(ocr.PDFToImage(pdffile));
                this.Invoke((Action)delegate
                {
                    feed_label.Text = "PW Getting text ...";
                }
                        );
                return ocr.v3_GetContents(imgs);
            });
        }

        private void templateFlatButton_Click(object sender, EventArgs e)
        {
            if (sideMenu.Width > 38)
            {
                Minimize();
            }
            this.homePanel.Visible = false;
            this.dynaPdfListPanel.Visible = false;
        }

        private void headerPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }


        private void panel4_Click(object sender, EventArgs e)
        {
            this.ActiveControl = titlePanel;
        }

        private void content_Box_KeyPress(object sender, KeyPressEventArgs e)
        {
            // if the change is  a space allow in content area.(Allow backsapace too)
            e.Handled = !((e.KeyChar == (char)Keys.Space || e.KeyChar == (char)Keys.Back));

        }

        private void content_Box_TextChanged(object sender, EventArgs e)
        {
            //.WriteLine(e.GetType());
            if (!dynaPdfListPanel.Visible )           
            {

                // if its item line (can't check since we dont know anything about item lines yet)                            
                // get text up to cursor
                string textUpToCaret = regex.Replace(content_Box.Text.Substring(0, content_Box.SelectionStart), "\t");
                // find the word before the cursor if empty space dont process
                int lastSpaceIndex = textUpToCaret.LastIndexOf("\t");
                if (lastSpaceIndex < 0)
                    lastSpaceIndex = 0;
                string word_before_caret = textUpToCaret.Substring(lastSpaceIndex).Split('\n').Last().Replace('-', ' ').Trim();
                if (!(String.IsNullOrEmpty(word_before_caret) || String.IsNullOrWhiteSpace(word_before_caret)))
                {
                    string line = content_Box.Lines[content_Box.GetLineFromCharIndex(content_Box.SelectionStart)];
                    //process line if it's item line proceed

                    // diff btn the start of line and the cursor positin for substring the current line
                    int column = content_Box.SelectionStart - content_Box.GetFirstCharIndexFromLine(content_Box.GetLineFromCharIndex(content_Box.SelectionStart));
                    string text_before_caret = line.Substring(0, column).Trim();

                    // remove separator and process double space to tabs, then tab split and count
                    text_before_caret = text_before_caret.Replace('-', ' ');
                    text_before_caret = regex.Replace(text_before_caret, "\t");
                    int pos_new = text_before_caret.Split('\t').Length;

                    //ways it can go wrong, if not done from left to right - solve( increment when new pos is less than previous pos
                    if (!pos.Contains(pos_new))
                    {
                        for (int i = 0; i < pos.Count; i++)
                        {
                            if (pos_new < pos[i])
                            {
                                pos[i]++;
                            }
                        }
                        pos.Add(pos_new);
                    }
                }
            }
            else
            {
                processButton.Visible = true;
            }
        }

        private void panel5_MouseClick(object sender, MouseEventArgs e)
        {
            this.ActiveControl = label22;
        }

        private void header_nameTextbox_OnValueChanged(object sender, EventArgs e)
        {
            var activecolor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(105)))), ((int)(((byte)(100)))));
            var inactivecolor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            //if the value is special word for no item, disable stuff.  108, 105, 100
            if (header_nameTextbox.Text.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                designationTextbox.Enabled = false;
                quantityTextbox.Enabled = false;
                itemNumberTextbox.Enabled = false;
                itemCountTextbox.Enabled = false;
                per_unitSwitch.Enabled = false;
                tvaSwitch.Enabled = false;
            }
            else
            {
                designationTextbox.Enabled = true;
                quantityTextbox.Enabled = true;
                itemNumberTextbox.Enabled = true;
                itemCountTextbox.Enabled = true;
                per_unitSwitch.Enabled = true;
                tvaSwitch.Enabled = true;
            }
        }

        private void itemCount_Textbox_Validating(object sender, CancelEventArgs e)
        {
            IntegerValidate(itemCountTextbox, e);
        }

        private void IntegerValidate(BunifuMetroTextbox Textbox, CancelEventArgs e)
        {
            Int16 num;
            if (!Int16.TryParse(Textbox.Text, out num))
            {
                // Cancel the event and select the text to be corrected by the user.
                e.Cancel = true;
                Textbox.Select();

                // Set the ErrorProvider error with the text to display. 
                this.errorProvider1.SetError(Textbox, "Numeric only");

            }
        }
        private void IntegerValidate(BunifuMaterialTextbox Textbox, CancelEventArgs e)
        {
            Int16 num;
            if (!Int16.TryParse(Textbox.Text, out num))
            {
                // Cancel the event and select the text to be corrected by the user.
                e.Cancel = true;
                Textbox.Select();

                // Set the ErrorProvider error with the text to display. 
                this.errorProvider1.SetError(Textbox, "Numeric only");

            }
        }

        private void designationTextbox_Validated(object sender, EventArgs e)
        {
            errorProvider1.SetError(designationTextbox, "");
        }

        private void designationTextbox_Validating(object sender, CancelEventArgs e)
        {
            IntegerValidate(designationTextbox, e);
        }

        private void quantityTextbox_Validating(object sender, CancelEventArgs e)
        {
            IntegerValidate(quantityTextbox, e);
        }

        private void item_numberTextbox_Validating(object sender, CancelEventArgs e)
        {
            IntegerValidate(itemNumberTextbox, e);
        }

        private void itemCount_Textbox_Validated(object sender, EventArgs e)
        {
            errorProvider1.SetError(itemCountTextbox, "");
        }

        private void quantityTextbox_Validated(object sender, EventArgs e)
        {
            errorProvider1.SetError(quantityTextbox, "");
        }

        private void item_numberTextbox_Validated(object sender, EventArgs e)
        {
            errorProvider1.SetError(itemNumberTextbox, "");
        }

        private void saveButton_Click(object sender, EventArgs e)
        {

            // validate the name textbox
            providerTextbox.Focus();
            saveButton.Focus();

            //save all preferences under provider name as key - value pair
            // provider - header, column nos(designation,quantity,itemnumber)[item spaceing-pos] itemcount, switch, fotter, date,commandenumber
            string pref = $"{ header_nameTextbox.Text}\t{designationTextbox.Text }\t{quantityTextbox.Text }\t{itemNumberTextbox.Text }" +
                $"\t{string.Join(",", pos)}\t{itemCountTextbox.Text }\t{per_unitSwitch.Value.ToString() }\t" +
                $"{tvaSwitch.Value.ToString() }\t{fotterNameTextBox.Text }" +
                $"\t{date_de_factureTextbox.Text }\t{commandeTextbox.Text}";

            if (!string.IsNullOrEmpty(providerTextbox.Text))
            {
                AddUpdateAppSettings(providerTextbox.Text, pref);

            }
        }
        static void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);

            }
            catch (ConfigurationErrorsException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private void providerTextbox_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(providerTextbox.Text))
            {
                // Cancel the event and select the text to be corrected by the user.
                e.Cancel = true;

                // Set the ErrorProvider error with the text to display. 
                this.errorProvider1.SetError(providerTextbox, "Input provider name");

            }
        }

        private void providerTextbox_Validated(object sender, EventArgs e)
        {
            this.errorProvider1.SetError(providerTextbox, "");
        }

        private void pdfFlatButton_Click(object sender, EventArgs e)
        {
            if (pdfData.Count > 0)
            {
                this.dynaPdfListPanel.Visible = true;
                this.homePanel.Visible = false;
            }
            else
            {
                homeButton_Click(sender, e);
                homeButton.Focus();
                homeButton.selected = true;
            }          
        }

        // no need to thread this
        private void DynaDataExtract(object sender, EventArgs e)
        {
            Listbutton button = sender as Listbutton;
            foreach(Control c in dynaPdfListPanel.Controls)
            {
                Listbutton a = (Listbutton)c;
                a.Iconimage_right = null;
            }

            button.selected = true;
            button.Iconimage_right = global::banifu_forms_main_ui.Properties.Resources.selected;           
            bunifuCustomDataGrid2.Rows.Clear();


            // get contents and process            
            string txt = pdfData[button.Name];

            //should be a better way to filter whitespace 
            txt = txt.Replace("\n\r\n\t\n\n\r\n", "");
            txt = txt.Replace("\n\n\r\n", Environment.NewLine);
            txt = txt.Replace("\n\r\n", Environment.NewLine);
            //txt = return_regex.Replace(txt, Environment.NewLine);
            txt = txt.Replace('|', 'I');
            contents = txt.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None);
            txt = regex.Replace(txt, "\t");
            content_Box.Text = (txt.Replace(" ", "-"));
            
            //get from saved data
            Pdf pdf = new Pdf();
            string provider = pdf.GetProviderName(contents);
            pdf.ReadProviderSettings(provider);
            providerTextbox.Text = provider;
            pdf.ProcessContent(contents);

            var info = pdf.info;
            //put info in UI
            Print_To_Screen(info);
        }

        private void Print_To_Screen(Dictionary<string,string> info)
        {           
            factureDate_label.Text = info["factureDate"];
            commande_Label.Text = info["commande"];
            prix_label.Text = info["prix"];
            BVR_Label.Text = info["BVR"];
            try
            {
                string[] items = info["item"].Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries);
                bunifuCustomDataGrid2.Rows.Clear();
                foreach (string item in items)
                {
                    bunifuCustomDataGrid2.Rows.Add(item.Split('|'));
                }

            }
            catch (Exception)
            {

            }
        }

        private void DropBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;

                this.DropBox.BorderStyle = BorderStyle.None;
                this.borderpanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(205)))));
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void DropBox_DragLeave(object sender, EventArgs e)
        {
            this.DropBox.BorderStyle = BorderStyle.FixedSingle;
            this.borderpanel.BackColor = System.Drawing.Color.Transparent;
        }
        private void DropBox_MouseLeave(object sender, EventArgs e)
        {
            this.DropBox.BorderStyle = BorderStyle.FixedSingle;
            this.borderpanel.BackColor = System.Drawing.Color.Transparent;
        }

        private void DropBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string filePath in files)
                {
                    if (!_files.Contains(filePath))
                        _files.Add(filePath);
                    DropBox.Items.Add(filePath);
                }
                if (!executeButton.Visible)
                {
                    executeButton.Visible = true;
                }
            }

        }

        private async void ExecuteButton_ClickAsync(object sender, EventArgs e)
        {
            i = 0;
            animatepanel.Visible = true;
            labelf.Text = "Processing PDF ...";
            Thread.Sleep(1);
            circleProgressbar.Value = 1;
            
            // _files contains all the files.
            // call pdf to convert each file async
            // Thread th = new Thread(work);
            // th.Start();

            await WorkAsync();
            this.circleProgressbar.Value = 100;
            Thread.Sleep(100);
            animatepanel.Visible = false;
            homePanel.Visible = false;

            // add dynamically the custom buttons
            dynaPdfListPanel.Controls.Clear();
            dynaPdfBtns.Clear();
            System.Drawing.Point newLoc = new System.Drawing.Point(-2, 6);
            foreach (string pdfname in pdfData.Keys)
            {
                var b = new Listbutton(pdfname, pdfname);
                newLoc.Offset(0, b.Height + 2);
                b.Click += new EventHandler(DynaDataExtract);
                
                this.dynaPdfListPanel.Controls.Add(b);
                
                //dynaPdfBtns.Add(b);
                b.Location = newLoc;
            }
            //dynaPdfListPanel.Visible = true;
            homeTransition.ShowSync(dynaPdfListPanel);
            pdfFlatButton.selected = true;
            homeButton.selected = false;
        }

        private Task WorkAsync()
        {
            return Task.Run(() =>
            {
                Parallel.ForEach(_files, new ParallelOptions() { MaxDegreeOfParallelism = 4 },
                    file =>
                    {
                        DoOcr(file);
                    });
            });
        }
      
        private void DoOcr(string pdffile)
        {
            // get filename
            string filename = System.IO.Path.GetFileNameWithoutExtension(pdffile);
            OCR ocr = new OCR();
            i += 1;
            List<string> imgs = new List<string>(ocr.PDFToImage(pdffile));
            this.Invoke((Action)delegate
            {
                this.circleProgressbar.Value = (int)(i / (double)_files.Count * 50); 
                //animatepanel.Refresh();
            });
            pdfData[filename] = ocr.v3_GetContents(imgs).ToString();
            i += 1;
            this.Invoke((Action)delegate
            {

                this.circleProgressbar.Value = (int)(i / (double)_files.Count * 50) ;
                //Console.WriteLine(i );
                if(i + 1 - _files.Count > 0)
                labelf.Text = " Reading Content " + (i+1 -_files.Count) + " of " + _files.Count;                
                //animatepanel.Refresh();
                if(i+1 == _files.Count)
                {
                    this.circleProgressbar.Value = 96;
                }
            });
        }

        private void homeButton_Click(object sender, EventArgs e)
        {            
            //DropBox.Items.Clear();
            executeButton.Visible = false;
            homeTransition.ShowSync(homePanel);
            Expand();
        }

        private void DropBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Delete)
            {
                _files.RemoveAt(DropBox.SelectedIndex);
                DropBox.Items.RemoveAt(DropBox.SelectedIndex);
            }
            
        }

        private void DropBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && DropBox.SelectedIndex >= 0)
            {
                _files.RemoveAt(DropBox.SelectedIndex);
                DropBox.Items.RemoveAt(DropBox.SelectedIndex);
            }
        }
    }

}
