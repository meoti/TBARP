
using System;


namespace InvoiceAnalyserMainUI
{
    class Listbutton : Bunifu.Framework.UI.BunifuFlatButton
    {
        public Listbutton(string btnName, string btntext)
        {
            InitializeComponent(btnName, btntext);
        }

        private void InitializeComponent(String btnName, string btnText)
        {
           
            Activecolor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(25)))), ((int)(((byte)(36)))));
            BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(25)))), ((int)(((byte)(36)))));
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            BorderRadius = 0;
            ButtonText = btnText;
            Cursor = System.Windows.Forms.Cursors.Hand;
            DisabledColor = System.Drawing.Color.Gray;
            Font = new System.Drawing.Font("Comic Sans MS", 7.8F);
            ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(90)))), ((int)(((byte)(120)))));
            Iconcolor = System.Drawing.Color.Transparent;
            Iconimage = global::banifu_forms_main_ui.Properties.Resources.pdf_file_3_32;
            Iconimage_Selected = global::banifu_forms_main_ui.Properties.Resources.selected;
            
            Iconimage_right_Selected = global::banifu_forms_main_ui.Properties.Resources.selected;
            

            Iconimage_right = null;
            Iconimage_right_Selected = null;
            Iconimage_Selected = null;
            IconMarginLeft = 0;
            IconMarginRight = 0;
            IconRightVisible = true;
            IconRightZoom = 40D;
            IconVisible = true;
            IconZoom = 60D;
           IsTab = true;
            //Location = new System.Drawing.Point(3, 48);
            Name = btnName;
            Normalcolor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(25)))), ((int)(((byte)(36)))));
            OnHovercolor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(16)))), ((int)(((byte)(24)))));
            OnHoverTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(104)))), ((int)(((byte)(184)))));
            //selected = false;
            Size = new System.Drawing.Size(349, 44);            
            Text = btnText;
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            Textcolor = System.Drawing.Color.Gainsboro;
            TextFont = new System.Drawing.Font("Century Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        }
    }
}
