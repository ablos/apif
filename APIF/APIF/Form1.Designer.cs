namespace APIF
{
    partial class Form1
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
            this.buttonOpen = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.imagepreview = new System.Windows.Forms.PictureBox();
            this.conversionProgressBar = new System.Windows.Forms.ProgressBar();
            this.compressionLabel = new System.Windows.Forms.Label();
            this.conversionProgressLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.imagepreview)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOpen
            // 
            this.buttonOpen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpen.Location = new System.Drawing.Point(12, 12);
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.Size = new System.Drawing.Size(458, 23);
            this.buttonOpen.TabIndex = 0;
            this.buttonOpen.Text = "Open";
            this.buttonOpen.UseVisualStyleBackColor = true;
            this.buttonOpen.Click += new System.EventHandler(this.OpenFile);
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(12, 41);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(458, 23);
            this.buttonSave.TabIndex = 1;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.SaveFile);
            // 
            // imagepreview
            // 
            this.imagepreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imagepreview.Location = new System.Drawing.Point(12, 99);
            this.imagepreview.Name = "imagepreview";
            this.imagepreview.Size = new System.Drawing.Size(458, 342);
            this.imagepreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imagepreview.TabIndex = 2;
            this.imagepreview.TabStop = false;
            // 
            // conversionProgressBar
            // 
            this.conversionProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.conversionProgressBar.Location = new System.Drawing.Point(12, 70);
            this.conversionProgressBar.Name = "conversionProgressBar";
            this.conversionProgressBar.Size = new System.Drawing.Size(254, 23);
            this.conversionProgressBar.Step = 1;
            this.conversionProgressBar.TabIndex = 3;
            // 
            // compressionLabel
            // 
            this.compressionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.compressionLabel.Location = new System.Drawing.Point(327, 70);
            this.compressionLabel.Name = "compressionLabel";
            this.compressionLabel.Size = new System.Drawing.Size(143, 23);
            this.compressionLabel.TabIndex = 4;
            this.compressionLabel.Text = "compression = 0.000";
            this.compressionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // conversionProgressLabel
            // 
            this.conversionProgressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.conversionProgressLabel.Location = new System.Drawing.Point(272, 70);
            this.conversionProgressLabel.Name = "conversionProgressLabel";
            this.conversionProgressLabel.Size = new System.Drawing.Size(58, 23);
            this.conversionProgressLabel.TabIndex = 5;
            this.conversionProgressLabel.Text = "00,0ms";
            this.conversionProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 453);
            this.Controls.Add(this.conversionProgressBar);
            this.Controls.Add(this.conversionProgressLabel);
            this.Controls.Add(this.imagepreview);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonOpen);
            this.Controls.Add(this.compressionLabel);
            this.MinimumSize = new System.Drawing.Size(250, 250);
            this.Name = "Form1";
            this.Text = "APIF";
            ((System.ComponentModel.ISupportInitialize)(this.imagepreview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOpen;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.PictureBox imagepreview;
        private System.Windows.Forms.ProgressBar conversionProgressBar;
        private System.Windows.Forms.Label compressionLabel;
        private System.Windows.Forms.Label conversionProgressLabel;
    }
}

