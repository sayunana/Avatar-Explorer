namespace Avatar_Explorer.Forms
{
    partial class SelectSupportedAvatar
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
            AvatarList = new ListBox();
            label1 = new Label();
            ConfirmButton = new Button();
            SuspendLayout();
            // 
            // AvatarList
            // 
            AvatarList.FormattingEnabled = true;
            AvatarList.ItemHeight = 15;
            AvatarList.Location = new Point(12, 70);
            AvatarList.Name = "AvatarList";
            AvatarList.Size = new Size(776, 349);
            AvatarList.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI", 12F);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(359, 42);
            label1.TabIndex = 1;
            label1.Text = "対応アバターを選択してください。\r\n選択されてなければ、全アバター対応として扱われます！\r\n";
            // 
            // ConfirmButton
            // 
            ConfirmButton.Location = new Point(657, 425);
            ConfirmButton.Name = "ConfirmButton";
            ConfirmButton.Size = new Size(131, 41);
            ConfirmButton.TabIndex = 2;
            ConfirmButton.Text = "確定";
            ConfirmButton.UseVisualStyleBackColor = true;
            ConfirmButton.Click += ConfirmButton_Click;
            // 
            // SelectSupportedAvatar
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 474);
            Controls.Add(ConfirmButton);
            Controls.Add(label1);
            Controls.Add(AvatarList);
            Name = "SelectSupportedAvatar";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SelectSupportedAvatar";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListBox AvatarList;
        private Label label1;
        private Button ConfirmButton;
    }
}