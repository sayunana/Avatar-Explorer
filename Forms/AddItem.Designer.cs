namespace Avatar_Explorer.Forms
{
    sealed partial class AddItem
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
            label2 = new Label();
            label3 = new Label();
            FolderTextBox = new TextBox();
            BoothURLTextBox = new TextBox();
            AddButton = new Button();
            label4 = new Label();
            TypeComboBox = new ComboBox();
            label5 = new Label();
            SelectAvatar = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI", 12F);
            label1.Location = new Point(22, 64);
            label1.Name = "label1";
            label1.Size = new Size(58, 21);
            label1.TabIndex = 0;
            label1.Text = "フォルダ";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Yu Gothic UI", 12F);
            label2.Location = new Point(22, 109);
            label2.Name = "label2";
            label2.Size = new Size(84, 21);
            label2.TabIndex = 1;
            label2.Text = "Booth URL";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Yu Gothic UI", 20F);
            label3.Location = new Point(22, 9);
            label3.Name = "label3";
            label3.Size = new Size(174, 37);
            label3.TabIndex = 2;
            label3.Text = "アイテムの追加";
            // 
            // FolderTextBox
            // 
            FolderTextBox.AllowDrop = true;
            FolderTextBox.Font = new Font("Yu Gothic UI", 12F);
            FolderTextBox.Location = new Point(124, 61);
            FolderTextBox.Name = "FolderTextBox";
            FolderTextBox.Size = new Size(378, 29);
            FolderTextBox.TabIndex = 3;
            FolderTextBox.DragDrop += FolderTextBox_DragDrop;
            FolderTextBox.DragEnter += FolderTextBox_DragEnter;
            // 
            // BoothURLTextBox
            // 
            BoothURLTextBox.Font = new Font("Yu Gothic UI", 12F);
            BoothURLTextBox.Location = new Point(124, 101);
            BoothURLTextBox.Name = "BoothURLTextBox";
            BoothURLTextBox.Size = new Size(378, 29);
            BoothURLTextBox.TabIndex = 4;
            // 
            // AddButton
            // 
            AddButton.Location = new Point(380, 191);
            AddButton.Name = "AddButton";
            AddButton.Size = new Size(122, 36);
            AddButton.TabIndex = 5;
            AddButton.Text = "追加";
            AddButton.UseVisualStyleBackColor = true;
            AddButton.Click += AddButton_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Yu Gothic UI", 12F);
            label4.Location = new Point(26, 146);
            label4.Name = "label4";
            label4.Size = new Size(45, 21);
            label4.TabIndex = 7;
            label4.Text = "タイプ";
            // 
            // TypeComboBox
            // 
            TypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            TypeComboBox.FormattingEnabled = true;
            TypeComboBox.Items.AddRange(new object[] { "アバター", "衣装", "テクスチャ", "ギミック", "アクセサリー", "髪型", "アニメーション", "ツール", "シェーダー" });
            TypeComboBox.Location = new Point(124, 148);
            TypeComboBox.Name = "TypeComboBox";
            TypeComboBox.Size = new Size(186, 23);
            TypeComboBox.TabIndex = 8;
            TypeComboBox.SelectedIndexChanged += TypeComboBox_SelectedIndexChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Yu Gothic UI", 12F);
            label5.Location = new Point(26, 191);
            label5.Name = "label5";
            label5.Size = new Size(121, 21);
            label5.TabIndex = 9;
            label5.Text = "対応アバター選択";
            // 
            // SelectAvatar
            // 
            SelectAvatar.Location = new Point(170, 184);
            SelectAvatar.Name = "SelectAvatar";
            SelectAvatar.Size = new Size(140, 38);
            SelectAvatar.TabIndex = 10;
            SelectAvatar.Text = "選択";
            SelectAvatar.UseVisualStyleBackColor = true;
            SelectAvatar.Click += SelectAvatar_Click;
            // 
            // AddItem
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(519, 242);
            Controls.Add(SelectAvatar);
            Controls.Add(label5);
            Controls.Add(TypeComboBox);
            Controls.Add(label4);
            Controls.Add(AddButton);
            Controls.Add(BoothURLTextBox);
            Controls.Add(FolderTextBox);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AddItem";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AddItem";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox FolderTextBox;
        private TextBox BoothURLTextBox;
        private Button AddButton;
        private Label label4;
        private ComboBox TypeComboBox;
        private Label label5;
        private Button SelectAvatar;
    }
}