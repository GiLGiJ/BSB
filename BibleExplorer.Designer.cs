namespace BSB
{
    partial class BibleExplorer
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cboWords = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // cboWords
            // 
            this.cboWords.FormattingEnabled = true;
            this.cboWords.Location = new System.Drawing.Point(59, 55);
            this.cboWords.Name = "cboWords";
            this.cboWords.Size = new System.Drawing.Size(121, 21);
            this.cboWords.TabIndex = 0;
            // 
            // BibleExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(470, 447);
            this.Controls.Add(this.cboWords);
            this.Name = "BibleExplorer";
            this.Text = "BibleExplorer";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cboWords;




    }
}