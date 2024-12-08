namespace Avatar_Explorer.Forms
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            UndoButton = new Button();
            PathTextBox = new TextBox();
            AvatarSearchFilterList = new TabControl();
            AvatarPage = new TabPage();
            AvatarAuthorPage = new TabPage();
            AddItemButton = new Button();
            ExplorerList = new TabControl();
            AvatarItemExplorer = new TabPage();
            StartLabel = new Label();
            AvatarSearchFilterList.SuspendLayout();
            ExplorerList.SuspendLayout();
            AvatarItemExplorer.SuspendLayout();
            SuspendLayout();
            // 
            // UndoButton
            // 
            UndoButton.Location = new Point(23, 15);
            UndoButton.Name = "UndoButton";
            UndoButton.Size = new Size(97, 23);
            UndoButton.TabIndex = 0;
            UndoButton.Text = "戻る";
            UndoButton.UseVisualStyleBackColor = true;
            UndoButton.Click += UndoButton_Click;
            // 
            // PathTextBox
            // 
            PathTextBox.Location = new Point(148, 16);
            PathTextBox.Name = "PathTextBox";
            PathTextBox.Size = new Size(806, 23);
            PathTextBox.TabIndex = 2;
            PathTextBox.Text = "ここには現在のパスが表示されます";
            // 
            // AvatarSearchFilterList
            // 
            AvatarSearchFilterList.Controls.Add(AvatarPage);
            AvatarSearchFilterList.Controls.Add(AvatarAuthorPage);
            AvatarSearchFilterList.Location = new Point(16, 45);
            AvatarSearchFilterList.Name = "AvatarSearchFilterList";
            AvatarSearchFilterList.SelectedIndex = 0;
            AvatarSearchFilterList.Size = new Size(243, 443);
            AvatarSearchFilterList.TabIndex = 3;
            // 
            // AvatarPage
            // 
            AvatarPage.AutoScroll = true;
            AvatarPage.Location = new Point(4, 24);
            AvatarPage.Name = "AvatarPage";
            AvatarPage.Padding = new Padding(3);
            AvatarPage.Size = new Size(235, 415);
            AvatarPage.TabIndex = 0;
            AvatarPage.Text = "アバター";
            AvatarPage.UseVisualStyleBackColor = true;
            // 
            // AvatarAuthorPage
            // 
            AvatarAuthorPage.Location = new Point(4, 24);
            AvatarAuthorPage.Name = "AvatarAuthorPage";
            AvatarAuthorPage.Padding = new Padding(3);
            AvatarAuthorPage.Size = new Size(235, 415);
            AvatarAuthorPage.TabIndex = 1;
            AvatarAuthorPage.Text = "作者";
            AvatarAuthorPage.UseVisualStyleBackColor = true;
            // 
            // AddItemButton
            // 
            AddItemButton.Location = new Point(793, 490);
            AddItemButton.Name = "AddItemButton";
            AddItemButton.Size = new Size(161, 32);
            AddItemButton.TabIndex = 5;
            AddItemButton.Text = "アイテムの追加";
            AddItemButton.UseVisualStyleBackColor = true;
            AddItemButton.Click += AddItemButton_Click;
            // 
            // ExplorerList
            // 
            ExplorerList.Controls.Add(AvatarItemExplorer);
            ExplorerList.Location = new Point(269, 45);
            ExplorerList.Name = "ExplorerList";
            ExplorerList.SelectedIndex = 0;
            ExplorerList.Size = new Size(699, 439);
            ExplorerList.TabIndex = 6;
            // 
            // AvatarItemExplorer
            // 
            AvatarItemExplorer.AutoScroll = true;
            AvatarItemExplorer.Controls.Add(StartLabel);
            AvatarItemExplorer.Location = new Point(4, 24);
            AvatarItemExplorer.Name = "AvatarItemExplorer";
            AvatarItemExplorer.Padding = new Padding(3);
            AvatarItemExplorer.Size = new Size(691, 411);
            AvatarItemExplorer.TabIndex = 0;
            AvatarItemExplorer.Text = "アイテム";
            AvatarItemExplorer.UseVisualStyleBackColor = true;
            // 
            // StartLabel
            // 
            StartLabel.AutoSize = true;
            StartLabel.Font = new Font("Yu Gothic UI", 15F);
            StartLabel.Location = new Point(116, 159);
            StartLabel.Name = "StartLabel";
            StartLabel.Size = new Size(481, 56);
            StartLabel.TabIndex = 7;
            StartLabel.Text = "左のメニューからアイテムを選択してください\r\nそれか、右下の\"アイテムの追加\"でアイテムを追加してください";
            StartLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(980, 534);
            Controls.Add(ExplorerList);
            Controls.Add(AddItemButton);
            Controls.Add(AvatarSearchFilterList);
            Controls.Add(PathTextBox);
            Controls.Add(UndoButton);
            Name = "Main";
            Text = "Avatar Explorer";
            AvatarSearchFilterList.ResumeLayout(false);
            ExplorerList.ResumeLayout(false);
            AvatarItemExplorer.ResumeLayout(false);
            AvatarItemExplorer.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button UndoButton;
        private TextBox PathTextBox;
        private TabControl AvatarSearchFilterList;
        private TabPage AvatarPage;
        private TabPage AvatarAuthorPage;
        private Button AddItemButton;
        private TabControl ExplorerList;
        private TabPage AvatarItemExplorer;
        private Label StartLabel;
    }
}
