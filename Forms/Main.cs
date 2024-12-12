using System.Diagnostics;
using Avatar_Explorer.Classes;

namespace Avatar_Explorer.Forms
{
    public sealed partial class Main : Form
    {
        // Current Version
        private const string CurrentVersion = "v1.0.0";

        // Items Data
        public Item[] Items;

        // Current Path
        public CurrentPath CurrentPath = new();

        // Search Mode
        private bool _authorMode;

        public Main()
        {
            Items = Helper.LoadItemsData();
            InitializeComponent();
            GenerateAvatarList();
            GenerateAuthorList();

            Text = $"VRChat Avatar Explorer {CurrentVersion} by ぷこるふ";
        }

        // Generate List (LEFT)
        private void GenerateAvatarList()
        {
            AvatarPage.Controls.Clear();
            var index = 0;
            foreach (Item item in Items.Where(item => item.Type == ItemType.Avatar))
            {
                Button button = Helper.CreateButton(item.ImagePath, item.Title, "作者: " + item.AuthorName, true);
                button.Location = new Point(0, (70 * index) + 7);
                button.Click += (_, _) =>
                {
                    CurrentPath.CurrentSelectedAvatar = item.Title;
                    CurrentPath.CurrentSelectedAuthor = null;
                    CurrentPath.CurrentSelectedCategory = ItemType.Unknown;
                    CurrentPath.CurrentSelectedItemCategory = null;
                    CurrentPath.CurrentSelectedItem = null;
                    _authorMode = false;
                    GenerateCategoryList();
                    PathTextBox.Text = GeneratePath();
                };
                AvatarPage.Controls.Add(button);
                index++;
            }
        }

        private void GenerateAuthorList()
        {
            AvatarAuthorPage.Controls.Clear();
            var index = 0;

            var authors = Array.Empty<Author>();
            foreach (Item item in Items)
            {
                if (authors.Any(author => author.AuthorName == item.AuthorName)) continue;
                authors = authors.Append(new Author
                {
                    AuthorName = item.AuthorName,
                    AuthorImagePath = item.AuthorImageFilePath
                }).ToArray();
            }

            foreach (var author in authors)
            {
                Button button = Helper.CreateButton(author.AuthorImagePath, author.AuthorName,
                    Items.Count(item => item.AuthorName == author.AuthorName) + "個の項目", true);
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    CurrentPath.CurrentSelectedAuthor = author;
                    CurrentPath.CurrentSelectedAvatar = null;
                    CurrentPath.CurrentSelectedCategory = ItemType.Unknown;
                    CurrentPath.CurrentSelectedItemCategory = null;
                    CurrentPath.CurrentSelectedItem = null;
                    _authorMode = true;
                    GenerateCategoryList();
                    PathTextBox.Text = GeneratePath();
                };
                AvatarAuthorPage.Controls.Add(button);
                index++;
            }
        }

        // Generate List (RIGHT)
        private void GenerateCategoryList()
        {
            ResetAvatarList();

            var index = 0;
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                if (itemType is ItemType.Unknown) continue;
                var itemCount = _authorMode
                    ? Items.Count(item =>
                        item.Type == itemType && item.AuthorName == CurrentPath.CurrentSelectedAuthor?.AuthorName)
                    : Items.Count(item =>
                        item.Type == itemType && (item.SupportedAvatar.Contains(CurrentPath.CurrentSelectedAvatar) ||
                                                  item.SupportedAvatar.Length == 0));
                if (itemCount == 0) continue;
                Button button = Helper.CreateButton("./Datas/FolderIcon.png", Helper.GetCategoryName(itemType),
                    itemCount + "個の項目");
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    CurrentPath.CurrentSelectedCategory = itemType;
                    GenerateItems();
                    PathTextBox.Text = GeneratePath();
                };
                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        private void GenerateItems()
        {
            ResetAvatarList();

            var filteredItems = _authorMode
                ? Items.Where(item =>
                    item.Type == CurrentPath.CurrentSelectedCategory &&
                    item.AuthorName == CurrentPath.CurrentSelectedAuthor?.AuthorName)
                : Items.Where(item =>
                    item.Type == CurrentPath.CurrentSelectedCategory &&
                    (item.SupportedAvatar.Contains(CurrentPath.CurrentSelectedAvatar) ||
                     item.SupportedAvatar.Length == 0));

            var index = 0;
            foreach (Item item in filteredItems)
            {
                Button button = Helper.CreateButton(item.ImagePath, item.Title, "作者: " + item.AuthorName);
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    CurrentPath.CurrentSelectedItem = item;
                    GenerateItemCategoryList();
                    PathTextBox.Text = GeneratePath();
                };

                ContextMenuStrip contextMenuStrip = new();
                ToolStripMenuItem toolStripMenuItem = new("Boothリンクのコピー");
                toolStripMenuItem.Click += (_, _) =>
                {
                    Clipboard.SetText("https://booth.pm/ja/items/" + item.BoothId);
                };
                ToolStripMenuItem toolStripMenuItem2 = new("削除");
                toolStripMenuItem2.Click += (_, _) =>
                {
                    Items = Items.Where(i => i.Title != item.Title).ToArray();
                    GenerateItems();
                };
                ToolStripMenuItem toolStripMenuItem3 = new("編集");
                toolStripMenuItem3.Click += (_, _) =>
                {
                    AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, true, item, null);
                    addItem.ShowDialog();
                    GenerateAvatarList();
                };
                contextMenuStrip.Items.Add(toolStripMenuItem);
                contextMenuStrip.Items.Add(toolStripMenuItem2);
                contextMenuStrip.Items.Add(toolStripMenuItem3);
                button.ContextMenuStrip = contextMenuStrip;
                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        private void GenerateItemCategoryList()
        {
            var types = new[]
            {
                "改変用データ",
                "テクスチャ",
                "ドキュメント",
                "Unityパッケージ"
            };
            if (CurrentPath.CurrentSelectedItem == null) return;
            ItemFolderInfo itemFolderInfo = Helper.GetItemFolderInfo(CurrentPath.CurrentSelectedItem.ItemPath);
            CurrentPath.CurrentSelectedItemFolderInfo = itemFolderInfo;

            ResetAvatarList();

            var index = 0;
            foreach (var itemType in types)
            {
                var itemCount = itemFolderInfo.GetItemCount(itemType);
                if (itemCount == 0) continue;
                Button button = Helper.CreateButton("./Datas/FolderIcon.png", itemType, itemCount + "個の項目");
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    CurrentPath.CurrentSelectedItemCategory = itemType;
                    GenerateItemFiles();
                    PathTextBox.Text = GeneratePath();
                };
                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        private void GenerateItemFiles()
        {
            ResetAvatarList();

            var index = 0;
            foreach (var file in CurrentPath.CurrentSelectedItemFolderInfo.GetItems(CurrentPath
                         .CurrentSelectedItemCategory))
            {
                var imagePath = file.FileExtension is ".png" or ".jpg" ? file.FilePath : "./Datas/FileIcon.png";
                Button button = Helper.CreateButton(imagePath, file.FileName,
                    file.FileExtension.Replace(".", "") + "ファイル");
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    Process.Start(new ProcessStartInfo("explorer.exe", " /select, " + file.FilePath));
                };
                ToolTip toolTip = new();
                toolTip.SetToolTip(button, "開くファイルのパス: " + file.FilePath);
                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        // Add Item Form
        private void AddItemButton_Click(object sender, EventArgs e)
        {
            AddItem addItem = new AddItem(this, CurrentPath.CurrentSelectedCategory, false, null, null);
            addItem.ShowDialog();
            GenerateAvatarList();
        }

        // Generate Path
        private string GeneratePath()
        {
            if (!_authorMode)
            {
                if (CurrentPath.CurrentSelectedAvatar == null) return "";
                if (CurrentPath.CurrentSelectedCategory == ItemType.Unknown)
                    return Helper.RemoveFormat(CurrentPath.CurrentSelectedAvatar);
                if (CurrentPath.CurrentSelectedItem == null)
                    return Helper.RemoveFormat(CurrentPath.CurrentSelectedAvatar) + "/" +
                           Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory);
                if (CurrentPath.CurrentSelectedItemCategory == null)
                    return Helper.RemoveFormat(CurrentPath.CurrentSelectedAvatar) + "/" +
                           Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory) + "/" +
                           Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title);

                return Helper.RemoveFormat(CurrentPath.CurrentSelectedAvatar) + "/" +
                       Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory) + "/" +
                       Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title) + "/" +
                       Helper.RemoveFormat(CurrentPath.CurrentSelectedItemCategory);
            }

            if (CurrentPath.CurrentSelectedAuthor == null) return "";
            if (CurrentPath.CurrentSelectedCategory == ItemType.Unknown)
                return Helper.RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName);
            if (CurrentPath.CurrentSelectedItem == null)
                return Helper.RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName) + "/" +
                       Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory);
            if (CurrentPath.CurrentSelectedItemCategory == null)
                return Helper.RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName) + "/" +
                       Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory) + "/" +
                       Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title);

            return Helper.RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName) + "/" +
                   Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory) + "/" +
                   Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title) + "/" + CurrentPath.CurrentSelectedItemCategory;
        }

        // Undo Button
        private void UndoButton_Click(object sender, EventArgs e)
        {
            if (CurrentPath.CurrentSelectedItemCategory != null)
            {
                CurrentPath.CurrentSelectedItemCategory = null;
                GenerateItemCategoryList();
                PathTextBox.Text = GeneratePath();
                return;
            }

            if (CurrentPath.CurrentSelectedItem != null)
            {
                CurrentPath.CurrentSelectedItem = null;
                GenerateItems();
                PathTextBox.Text = GeneratePath();
                return;
            }

            if (CurrentPath.CurrentSelectedCategory != ItemType.Unknown)
            {
                CurrentPath.CurrentSelectedCategory = ItemType.Unknown;
                GenerateCategoryList();
                PathTextBox.Text = GeneratePath();
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Helper.SaveItemsData(Items);
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            if (SearchBox.Text == "")
            {
                SearchResultLabel.Text = "";
                if (CurrentPath.CurrentSelectedItemCategory != null)
                {
                    GenerateItemFiles();
                    return;
                }

                if (CurrentPath.CurrentSelectedItem != null)
                {
                    GenerateItemCategoryList();
                    return;
                }

                if (CurrentPath.CurrentSelectedCategory != ItemType.Unknown)
                {
                    GenerateItems();
                    return;
                }

                if (CurrentPath.CurrentSelectedAvatar != null || CurrentPath.CurrentSelectedAuthor != null)
                {
                    GenerateCategoryList();
                    return;
                }

                ResetAvatarList(true);
                return;
            }

            string[] searchWords = SearchBox.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            GenerateFilteredItem(searchWords);
        }

        private void GenerateFilteredItem(string[] searchWords)
        {
            ResetAvatarList();

            var filteredItems = Items.Where(item =>
                searchWords.Any(word => item.Title.ToLower().Contains(word.ToLower()) ||
                    item.AuthorName.ToLower().Contains(word.ToLower()) ||
                    item.SupportedAvatar.Any(avatar => avatar.ToLower().Contains(word.ToLower())) ||
                    item.BoothId.ToString().Contains(word.ToLower()))
            );

            SearchResultLabel.Text = "検索結果: " + filteredItems.Count() + "件";

            filteredItems = filteredItems.OrderByDescending(item =>
            {
                var matchCount = 0;
                foreach (var word in searchWords)
                {
                    if (item.Title.ToLower().Contains(word.ToLower())) matchCount++;
                    if (item.AuthorName.ToLower().Contains(word.ToLower())) matchCount++;
                }

                return matchCount;
            });

            var index = 0;
            foreach (Item item in filteredItems)
            {
                Button button = Helper.CreateButton(item.ImagePath, item.Title, "作者: " + item.AuthorName);
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    CurrentPath.CurrentSelectedItem = item;
                    GenerateItemCategoryList();
                    PathTextBox.Text = GeneratePath();
                };

                ContextMenuStrip contextMenuStrip = new();
                ToolStripMenuItem toolStripMenuItem = new("Boothリンクのコピー");
                toolStripMenuItem.Click += (_, _) =>
                {
                    Clipboard.SetText("https://booth.pm/ja/items/" + item.BoothId);
                };
                ToolStripMenuItem toolStripMenuItem2 = new("削除");
                toolStripMenuItem2.Click += (_, _) =>
                {
                    Items = Items.Where(i => i.Title != item.Title).ToArray();
                    GenerateItems();
                };
                ToolStripMenuItem toolStripMenuItem3 = new("編集");
                toolStripMenuItem3.Click += (_, _) =>
                {
                    AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, true, item, null);
                    addItem.ShowDialog();
                    GenerateAvatarList();
                };
                contextMenuStrip.Items.Add(toolStripMenuItem);
                contextMenuStrip.Items.Add(toolStripMenuItem2);
                contextMenuStrip.Items.Add(toolStripMenuItem3);
                button.ContextMenuStrip = contextMenuStrip;
                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        private void ResetAvatarList(bool startLabelVisible = false)
        {
            for (int i = AvatarItemExplorer.Controls.Count - 1; i >= 0; i--)
            {
                if (AvatarItemExplorer.Controls[i].Name != "StartLabel")
                {
                    AvatarItemExplorer.Controls.RemoveAt(i);
                }
                else
                {
                    AvatarItemExplorer.Controls[i].Visible = startLabelVisible;
                }
            }
        }

        private void AvatarItemExplorer_DragEnter(object sender, DragEventArgs e) => e.Effect = DragDropEffects.All;

        private void AvatarItemExplorer_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[]? dragFilePathArr = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
            if (dragFilePathArr == null) return;
            var folderPath = dragFilePathArr[0];

            AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, false, null, folderPath);
            addItem.ShowDialog();
        }

        private void AvatarPage_DragEnter(object sender, DragEventArgs e) => e.Effect = DragDropEffects.All;

        private void AvatarPage_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[]? dragFilePathArr = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
            if (dragFilePathArr == null) return;
            var folderPath = dragFilePathArr[0];

            AddItem addItem = new(this, ItemType.Avatar, false, null, folderPath);
            addItem.ShowDialog();
        }
    }
}