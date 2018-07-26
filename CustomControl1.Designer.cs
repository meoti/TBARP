namespace banifu_forms_main_ui
{
    partial class CustomControl1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomControl1));
            this.listButton = new Bunifu.Framework.UI.BunifuFlatButton();
            this.SuspendLayout();
            // 
            // listButton
            // 
            this.listButton.Activecolor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(16)))), ((int)(((byte)(24)))));
            this.listButton.BackColor = System.Drawing.Color.Transparent;
            this.listButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.listButton.BorderRadius = 0;
            this.listButton.ButtonText = "pdf_filename";
            this.listButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.listButton.DisabledColor = System.Drawing.Color.Gray;
            this.listButton.Font = new System.Drawing.Font("Comic Sans MS", 7.8F);
            this.listButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(90)))), ((int)(((byte)(120)))));
            this.listButton.Iconcolor = System.Drawing.Color.Transparent;
            this.listButton.Iconimage = ((System.Drawing.Image)(resources.GetObject("listButton.Iconimage")));
            this.listButton.Iconimage_right = null;
            this.listButton.Iconimage_right_Selected = null;
            this.listButton.Iconimage_Selected = null;
            this.listButton.IconMarginLeft = 0;
            this.listButton.IconMarginRight = 10;
            this.listButton.IconRightVisible = true;
            this.listButton.IconRightZoom = 0D;
            this.listButton.IconVisible = true;
            this.listButton.IconZoom = 75D;
            this.listButton.IsTab = false;
            this.listButton.Location = new System.Drawing.Point(0, 0);
            this.listButton.Name = "listButton";
            this.listButton.Normalcolor = System.Drawing.Color.Transparent;
            this.listButton.OnHovercolor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(16)))), ((int)(((byte)(24)))));
            this.listButton.OnHoverTextColor = System.Drawing.Color.WhiteSmoke;
            this.listButton.selected = false;
            this.listButton.Size = new System.Drawing.Size(344, 46);
            this.listButton.TabIndex = 0;
            this.listButton.Text = "pdf_filename";
            this.listButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.listButton.Textcolor = System.Drawing.Color.Gainsboro;
            this.listButton.TextFont = new System.Drawing.Font("Century Gothic", 10.2F);
            this.ResumeLayout(false);

        }

        #endregion

        public Bunifu.Framework.UI.BunifuFlatButton listButton;
    }
}
