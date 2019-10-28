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
            this.StatusLabel = new System.Windows.Forms.Label();
            this.compressionLabel = new System.Windows.Forms.Label();
            this.conversionProgressLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.CompressionSign = new System.Windows.Forms.Label();
            this.compressionTypeLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.imagepreview)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
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
            // StatusLabel
            // 
            this.StatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Location = new System.Drawing.Point(-4, -2);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(131, 23);
            this.StatusLabel.TabIndex = 6;
            this.StatusLabel.Text = "Idle";
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // compressionLabel
            // 
            this.compressionLabel.AutoSize = true;
            this.compressionLabel.Location = new System.Drawing.Point(408, 0);
            this.compressionLabel.Name = "compressionLabel";
            this.compressionLabel.Size = new System.Drawing.Size(47, 23);
            this.compressionLabel.TabIndex = 4;
            this.compressionLabel.Text = "0.000";
            this.compressionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // conversionProgressLabel
            // 
            this.conversionProgressLabel.AutoSize = true;
            this.conversionProgressLabel.Location = new System.Drawing.Point(228, 0);
            this.conversionProgressLabel.Name = "conversionProgressLabel";
            this.conversionProgressLabel.Size = new System.Drawing.Size(64, 23);
            this.conversionProgressLabel.TabIndex = 5;
            this.conversionProgressLabel.Text = "00,0ms";
            this.conversionProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.compressionLabel, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.CompressionSign, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.conversionProgressLabel, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.compressionTypeLabel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.StatusLabel, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 70);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(458, 23);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // CompressionSign
            // 
            this.CompressionSign.AutoSize = true;
            this.CompressionSign.Location = new System.Drawing.Point(298, 0);
            this.CompressionSign.Name = "CompressionSign";
            this.CompressionSign.Size = new System.Drawing.Size(102, 17);
            this.CompressionSign.TabIndex = 7;
            this.CompressionSign.Text = "Compression =";
            this.CompressionSign.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CompressionTypeLabel
            // 
            this.compressionTypeLabel.AutoSize = true;
            this.compressionTypeLabel.Location = new System.Drawing.Point(129, -1);
            this.compressionTypeLabel.Name = "CompressionTypeLabel";
            this.compressionTypeLabel.Size = new System.Drawing.Size(54, 17);
            this.compressionTypeLabel.TabIndex = 8;
            this.compressionTypeLabel.Text = "0";
            this.compressionTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 453);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.imagepreview);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonOpen);
            this.MinimumSize = new System.Drawing.Size(320, 250);
            this.Name = "Form1";
            this.Text = "APIF";
            ((System.ComponentModel.ISupportInitialize)(this.imagepreview)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOpen;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.PictureBox imagepreview;
        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.Label compressionLabel;
        private System.Windows.Forms.Label conversionProgressLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label CompressionSign;
        private System.Windows.Forms.Label compressionTypeLabel;
    }
}

