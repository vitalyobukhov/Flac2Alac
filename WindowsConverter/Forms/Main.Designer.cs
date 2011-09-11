namespace WindowsConverter.Forms
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.lblDropYourMusicHere = new System.Windows.Forms.Label();
            this.fdbOutputDirectory = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // lblDropYourMusicHere
            // 
            this.lblDropYourMusicHere.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblDropYourMusicHere.AutoSize = true;
            this.lblDropYourMusicHere.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblDropYourMusicHere.Location = new System.Drawing.Point(72, 120);
            this.lblDropYourMusicHere.Name = "lblDropYourMusicHere";
            this.lblDropYourMusicHere.Size = new System.Drawing.Size(159, 20);
            this.lblDropYourMusicHere.TabIndex = 0;
            this.lblDropYourMusicHere.Text = "Drop your music here";
            // 
            // fdbOutputDirectory
            // 
            this.fdbOutputDirectory.Description = "Please select output directory";
            this.fdbOutputDirectory.RootFolder = System.Environment.SpecialFolder.DesktopDirectory;
            // 
            // Main
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(304, 282);
            this.Controls.Add(this.lblDropYourMusicHere);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(320, 320);
            this.Name = "Main";
            this.Text = "Converter";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Main_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Main_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblDropYourMusicHere;
        private System.Windows.Forms.FolderBrowserDialog fdbOutputDirectory;
    }
}