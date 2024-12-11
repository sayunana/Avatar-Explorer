namespace Avatar_Explorer.Forms
{
    partial class EditItemMeta
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
            label1 = new Label();
            SaveButton = new Button();
            label2 = new Label();
            label3 = new Label();
            TitleTextBox = new TextBox();
            AuthorTextBox = new TextBox();
            BoothLinkLabel = new Label();
            AuthorLink = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI", 15F);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(141, 28);
            label1.TabIndex = 0;
            label1.Text = "メタデータの編集";
            // 
            // SaveButton
            // 
            SaveButton.Font = new Font("Yu Gothic UI", 13F);
            SaveButton.Location = new Point(460, 206);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new Size(125, 36);
            SaveButton.TabIndex = 1;
            SaveButton.Text = "確定";
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Yu Gothic UI", 12F);
            label2.Location = new Point(12, 66);
            label2.Name = "label2";
            label2.Size = new Size(57, 21);
            label2.TabIndex = 4;
            label2.Text = "タイトル";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Yu Gothic UI", 12F);
            label3.Location = new Point(27, 102);
            label3.Name = "label3";
            label3.Size = new Size(42, 21);
            label3.TabIndex = 5;
            label3.Text = "作者";
            // 
            // TitleTextBox
            // 
            TitleTextBox.Font = new Font("Yu Gothic UI", 10F);
            TitleTextBox.Location = new Point(86, 66);
            TitleTextBox.Name = "TitleTextBox";
            TitleTextBox.Size = new Size(499, 25);
            TitleTextBox.TabIndex = 6;
            // 
            // AuthorTextBox
            // 
            AuthorTextBox.Font = new Font("Yu Gothic UI", 10F);
            AuthorTextBox.Location = new Point(86, 102);
            AuthorTextBox.Name = "AuthorTextBox";
            AuthorTextBox.Size = new Size(499, 25);
            AuthorTextBox.TabIndex = 7;
            // 
            // BoothLinkLabel
            // 
            BoothLinkLabel.AutoSize = true;
            BoothLinkLabel.Font = new Font("Yu Gothic UI", 12F);
            BoothLinkLabel.Location = new Point(12, 143);
            BoothLinkLabel.Name = "BoothLinkLabel";
            BoothLinkLabel.Size = new Size(91, 21);
            BoothLinkLabel.TabIndex = 9;
            BoothLinkLabel.Text = "Booth Link: ";
            // 
            // AuthorLink
            // 
            AuthorLink.AutoSize = true;
            AuthorLink.Font = new Font("Yu Gothic UI", 12F);
            AuthorLink.Location = new Point(12, 164);
            AuthorLink.Name = "AuthorLink";
            AuthorLink.Size = new Size(98, 21);
            AuthorLink.TabIndex = 10;
            AuthorLink.Text = "Author Link: ";
            // 
            // EditItemMeta
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(605, 254);
            Controls.Add(AuthorLink);
            Controls.Add(BoothLinkLabel);
            Controls.Add(AuthorTextBox);
            Controls.Add(TitleTextBox);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(SaveButton);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "EditItemMeta";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "EditItemMeta";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Button SaveButton;
        private Label label2;
        private Label label3;
        private TextBox TitleTextBox;
        private TextBox AuthorTextBox;
        private Label BoothLinkLabel;
        private Label AuthorLink;
    }
}