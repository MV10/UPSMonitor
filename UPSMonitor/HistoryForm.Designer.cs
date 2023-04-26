namespace UPSMonitor
{
    partial class HistoryForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HistoryForm));
            lstMessages = new ListBox();
            txtDetails = new TextBox();
            label1 = new Label();
            label2 = new Label();
            SuspendLayout();
            // 
            // lstMessages
            // 
            lstMessages.FormattingEnabled = true;
            lstMessages.ItemHeight = 15;
            lstMessages.Location = new Point(12, 27);
            lstMessages.Name = "lstMessages";
            lstMessages.Size = new Size(170, 289);
            lstMessages.TabIndex = 0;
            lstMessages.SelectedIndexChanged += lstMessages_SelectedIndexChanged;
            // 
            // txtDetails
            // 
            txtDetails.Enabled = false;
            txtDetails.Location = new Point(204, 27);
            txtDetails.Multiline = true;
            txtDetails.Name = "txtDetails";
            txtDetails.Size = new Size(342, 289);
            txtDetails.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(58, 15);
            label1.TabIndex = 2;
            label1.Text = "Messages";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(204, 9);
            label2.Name = "label2";
            label2.Size = new Size(91, 15);
            label2.TabIndex = 3;
            label2.Text = "Message Details";
            // 
            // HistoryForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(560, 327);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtDetails);
            Controls.Add(lstMessages);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "HistoryForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "UPS Monitor Message History";
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListBox lstMessages;
        private TextBox txtDetails;
        private Label label1;
        private Label label2;
    }
}