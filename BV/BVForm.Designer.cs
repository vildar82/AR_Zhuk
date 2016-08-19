namespace BeetlyVisualisation
{
    partial class BVForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BVForm));
            this.textBoxXMLInfoFile = new System.Windows.Forms.TextBox();
            this.labelXMLInfoFile = new System.Windows.Forms.Label();
            this.buttonSetXMLFile = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.buttonPreview = new System.Windows.Forms.Button();
            this.groupBoxPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxXMLInfoFile
            // 
            this.textBoxXMLInfoFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxXMLInfoFile.Location = new System.Drawing.Point(12, 39);
            this.textBoxXMLInfoFile.Name = "textBoxXMLInfoFile";
            this.textBoxXMLInfoFile.Size = new System.Drawing.Size(429, 20);
            this.textBoxXMLInfoFile.TabIndex = 3;
            // 
            // labelXMLInfoFile
            // 
            this.labelXMLInfoFile.AutoSize = true;
            this.labelXMLInfoFile.Location = new System.Drawing.Point(12, 20);
            this.labelXMLInfoFile.Name = "labelXMLInfoFile";
            this.labelXMLInfoFile.Size = new System.Drawing.Size(184, 13);
            this.labelXMLInfoFile.TabIndex = 4;
            this.labelXMLInfoFile.Text = "XML файл с результатами расчёта";
            // 
            // buttonSetXMLFile
            // 
            this.buttonSetXMLFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSetXMLFile.Location = new System.Drawing.Point(447, 38);
            this.buttonSetXMLFile.Name = "buttonSetXMLFile";
            this.buttonSetXMLFile.Size = new System.Drawing.Size(75, 23);
            this.buttonSetXMLFile.TabIndex = 5;
            this.buttonSetXMLFile.Text = "Select...";
            this.buttonSetXMLFile.UseVisualStyleBackColor = true;
            this.buttonSetXMLFile.Click += new System.EventHandler(this.buttonSetXMLFile_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(447, 65);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "Generate";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Controls.Add(this.pictureBoxPreview);
            this.groupBoxPreview.Location = new System.Drawing.Point(12, 105);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(510, 341);
            this.groupBoxPreview.TabIndex = 7;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Preview";
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.Location = new System.Drawing.Point(6, 19);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(498, 316);
            this.pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxPreview.TabIndex = 0;
            this.pictureBoxPreview.TabStop = false;
            // 
            // buttonPreview
            // 
            this.buttonPreview.Location = new System.Drawing.Point(366, 65);
            this.buttonPreview.Name = "buttonPreview";
            this.buttonPreview.Size = new System.Drawing.Size(75, 23);
            this.buttonPreview.TabIndex = 8;
            this.buttonPreview.Text = "Preview";
            this.buttonPreview.UseVisualStyleBackColor = true;
            this.buttonPreview.Click += new System.EventHandler(this.buttonPreview_Click);
            // 
            // BVForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(533, 458);
            this.Controls.Add(this.buttonPreview);
            this.Controls.Add(this.groupBoxPreview);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonSetXMLFile);
            this.Controls.Add(this.labelXMLInfoFile);
            this.Controls.Add(this.textBoxXMLInfoFile);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "BVForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Жучки - генератор изображений";
            this.Load += new System.EventHandler(this.BVForm_Load);
            this.groupBoxPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxXMLInfoFile;
        private System.Windows.Forms.Label labelXMLInfoFile;
        private System.Windows.Forms.Button buttonSetXMLFile;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Button buttonPreview;
    }
}