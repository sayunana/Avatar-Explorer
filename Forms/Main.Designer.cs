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
            SearchBox = new TextBox();
            label1 = new Label();
            SearchResultLabel = new Label();
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
            PathTextBox.Font = new Font("Noto Sans JP", 10F);
            PathTextBox.Location = new Point(148, 16);
            PathTextBox.Name = "PathTextBox";
            PathTextBox.Size = new Size(733, 27);
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
            AvatarSearchFilterList.Size = new Size(333, 586);
            AvatarSearchFilterList.TabIndex = 3;
            // 
            // AvatarPage
            // 
            AvatarPage.AllowDrop = true;
            AvatarPage.AutoScroll = true;
            AvatarPage.Location = new Point(4, 24);
            AvatarPage.Name = "AvatarPage";
            AvatarPage.Padding = new Padding(3);
            AvatarPage.Size = new Size(325, 558);
            AvatarPage.TabIndex = 0;
            AvatarPage.Text = "アバター";
            AvatarPage.UseVisualStyleBackColor = true;
            AvatarPage.DragDrop += AvatarPage_DragDrop;
            AvatarPage.DragEnter += AvatarPage_DragEnter;
            // 
            // AvatarAuthorPage
            // 
            AvatarAuthorPage.AutoScroll = true;
            AvatarAuthorPage.Location = new Point(4, 24);
            AvatarAuthorPage.Name = "AvatarAuthorPage";
            AvatarAuthorPage.Padding = new Padding(3);
            AvatarAuthorPage.Size = new Size(325, 558);
            AvatarAuthorPage.TabIndex = 1;
            AvatarAuthorPage.Text = "作者";
            AvatarAuthorPage.UseVisualStyleBackColor = true;
            // 
            // AddItemButton
            // 
            AddItemButton.Location = new Point(1093, 641);
            AddItemButton.Name = "AddItemButton";
            AddItemButton.Size = new Size(161, 46);
            AddItemButton.TabIndex = 5;
            AddItemButton.Text = "アイテムの追加";
            AddItemButton.UseVisualStyleBackColor = true;
            AddItemButton.Click += AddItemButton_Click;
            // 
            // ExplorerList
            // 
            ExplorerList.Controls.Add(AvatarItemExplorer);
            ExplorerList.Location = new Point(355, 45);
            ExplorerList.Name = "ExplorerList";
            ExplorerList.SelectedIndex = 0;
            ExplorerList.Size = new Size(899, 590);
            ExplorerList.TabIndex = 6;
            // 
            // AvatarItemExplorer
            // 
            AvatarItemExplorer.AllowDrop = true;
            AvatarItemExplorer.AutoScroll = true;
            AvatarItemExplorer.Controls.Add(StartLabel);
            AvatarItemExplorer.Location = new Point(4, 24);
            AvatarItemExplorer.Name = "AvatarItemExplorer";
            AvatarItemExplorer.Padding = new Padding(3);
            AvatarItemExplorer.Size = new Size(891, 562);
            AvatarItemExplorer.TabIndex = 0;
            AvatarItemExplorer.Text = "アイテム";
            AvatarItemExplorer.UseVisualStyleBackColor = true;
            AvatarItemExplorer.DragDrop += AvatarItemExplorer_DragDrop;
            AvatarItemExplorer.DragEnter += AvatarItemExplorer_DragEnter;
            // 
            // StartLabel
            // 
            StartLabel.AutoSize = true;
            StartLabel.Font = new Font("Noto Sans JP", 15F);
            StartLabel.Location = new Point(165, 198);
            StartLabel.Name = "StartLabel";
            StartLabel.Size = new Size(591, 58);
            StartLabel.TabIndex = 7;
            StartLabel.Text = "左のメニューからアイテムを選択してください\r\nそれか、右下の\"アイテムの追加\"でアイテムを追加してください";
            StartLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // SearchBox
            // 
            SearchBox.Font = new Font("Noto Sans JP", 10F);
            SearchBox.Location = new Point(980, 16);
            SearchBox.Name = "SearchBox";
            SearchBox.Size = new Size(270, 27);
            SearchBox.TabIndex = 7;
            SearchBox.TextChanged += SearchBox_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Noto Sans JP", 12F);
            label1.Location = new Point(932, 20);
            label1.Name = "label1";
            label1.Size = new Size(42, 23);
            label1.TabIndex = 8;
            label1.Text = "検索";
            // 
            // SearchResultLabel
            // 
            SearchResultLabel.AutoSize = true;
            SearchResultLabel.Font = new Font("Noto Sans JP", 10F);
            SearchResultLabel.Location = new Point(971, 45);
            SearchResultLabel.Name = "SearchResultLabel";
            SearchResultLabel.Size = new Size(0, 20);
            SearchResultLabel.TabIndex = 9;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1266, 699);
            Controls.Add(SearchResultLabel);
            Controls.Add(label1);
            Controls.Add(SearchBox);
            Controls.Add(ExplorerList);
            Controls.Add(AddItemButton);
            Controls.Add(AvatarSearchFilterList);
            Controls.Add(PathTextBox);
            Controls.Add(UndoButton);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Main";
            Text = "Avatar Explorer";
            FormClosing += Main_FormClosing;
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
        private TextBox SearchBox;
        private Label label1;
        private Label SearchResultLabel;
    }
}
