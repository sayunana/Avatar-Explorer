using System.Diagnostics;
using System.Text;
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

        private readonly Image _copyImage = Image.FromFile("./Datas/CopyIcon.png");
        private readonly Image _trashImage = Image.FromFile("./Datas/TrashIcon.png");
        private readonly Image _editImage = Image.FromFile("./Datas/EditIcon.png");

        private Window _openingWindow = Window.Nothing;

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
                Button button = Helper.CreateButton(item.ImagePath, item.Title, "作者: " + item.AuthorName, true,
                    item.Title);
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
                    Items.Count(item => item.AuthorName == author.AuthorName) + "個の項目", true, author.AuthorName);
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
            _openingWindow = Window.ItemCategoryList;
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
            _openingWindow = Window.ItemList;
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
                Button button = Helper.CreateButton(item.ImagePath, item.Title, "作者: " + item.AuthorName, false,
                    item.Title);
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    CurrentPath.CurrentSelectedItem = item;
                    GenerateItemCategoryList();
                    PathTextBox.Text = GeneratePath();
                };

                ContextMenuStrip contextMenuStrip = new();

                ToolStripMenuItem toolStripMenuItem = new("Boothリンクのコピー", _copyImage);
                toolStripMenuItem.Click += (_, _) =>
                {
                    if (item.BoothId == -1)
                    {
                        MessageBox.Show("BoothIDが存在しません", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Clipboard.SetText("https://booth.pm/ja/items/" + item.BoothId);
                };

                ToolStripMenuItem toolStripMenuItem2 = new("削除", _trashImage);
                toolStripMenuItem2.Click += (_, _) =>
                {
                    Items = Items.Where(i => i.Title != item.Title).ToArray();
                    GenerateItems();
                };

                ToolStripMenuItem toolStripMenuItem3 = new("編集", _editImage);
                toolStripMenuItem3.Click += (_, _) =>
                {
                    AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, true, item, null);
                    addItem.ShowDialog();
                    GenerateAvatarList();
                    GenerateAuthorList();
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
            _openingWindow = Window.ItemFolderCategoryList;
            var types = new[]
            {
                "改変用データ",
                "テクスチャ",
                "ドキュメント",
                "Unityパッケージ",
                "不明"
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
            _openingWindow = Window.ItemFolderItemsList;
            ResetAvatarList();

            var index = 0;
            foreach (var file in CurrentPath.CurrentSelectedItemFolderInfo.GetItems(CurrentPath
                         .CurrentSelectedItemCategory))
            {
                var imagePath = file.FileExtension is ".png" or ".jpg" ? file.FilePath : "./Datas/FileIcon.png";
                Button button = Helper.CreateButton(imagePath, file.FileName,
                    file.FileExtension.Replace(".", "") + "ファイル", false, "開くファイルのパス: " + file.FilePath);
                button.Location = new Point(0, (70 * index) + 2);

                ContextMenuStrip contextMenuStrip = new();
                ToolStripMenuItem toolStripMenuItem = new("ファイルのパスを開く", _copyImage);
                toolStripMenuItem.Click += (_, _) => { Process.Start("explorer.exe", "/select," + file.FilePath); };
                contextMenuStrip.Items.Add(toolStripMenuItem);
                button.ContextMenuStrip = contextMenuStrip;

                button.Click += (_, _) =>
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = file.FilePath,
                            UseShellExecute = true
                        });
                    }
                    catch
                    {
                        Process.Start("explorer.exe", "/select," + file.FilePath);
                    }
                };
                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        private void GenerateFilteredItem(string[] searchWords)
        {
            ResetAvatarList();
            _openingWindow = Window.SearchItemList;

            var filteredItems = Items
                .Where(item =>
                    searchWords.All(word =>
                        item.Title.ToLower().Contains(word.ToLower()) ||
                        item.AuthorName.ToLower().Contains(word.ToLower()) ||
                        item.SupportedAvatar.Any(avatar => avatar.ToLower().Contains(word.ToLower())) ||
                        item.BoothId.ToString().Contains(word.ToLower())
                    )
                )
                .OrderByDescending(item =>
                {
                    var matchCount = 0;
                    foreach (var word in searchWords)
                    {
                        if (item.Title.ToLower().Contains(word.ToLower())) matchCount++;
                        if (item.AuthorName.ToLower().Contains(word.ToLower())) matchCount++;
                    }

                    return matchCount;
                })
                .ToList();

            SearchResultLabel.Text = "検索結果: " + filteredItems.Count + "件" + " (全" + Items.Length + "件)";

            var index = 0;
            foreach (Item item in filteredItems)
            {
                Button button = Helper.CreateButton(item.ImagePath, item.Title, "作者: " + item.AuthorName, false,
                    item.Title);
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    _authorMode = false;
                    GeneratePathFromItem(item);
                    SearchBox.Text = "";
                    GenerateItemCategoryList();
                    PathTextBox.Text = GeneratePath();
                };

                ContextMenuStrip contextMenuStrip = new();

                ToolStripMenuItem toolStripMenuItem = new("Boothリンクのコピー", _copyImage);
                toolStripMenuItem.Click += (_, _) =>
                {
                    if (item.BoothId == -1)
                    {
                        MessageBox.Show("BoothIDが存在しません", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Clipboard.SetText("https://booth.pm/ja/items/" + item.BoothId);
                };

                ToolStripMenuItem toolStripMenuItem2 = new("削除", _trashImage);
                toolStripMenuItem2.Click += (_, _) =>
                {
                    Items = Items.Where(i => i.Title != item.Title).ToArray();
                    GenerateItems();
                };

                ToolStripMenuItem toolStripMenuItem3 = new("編集", _editImage);
                toolStripMenuItem3.Click += (_, _) =>
                {
                    AddItem addItem = new(this, item.Type, true, item, null);
                    addItem.ShowDialog();
                    GenerateAvatarList();
                    GenerateAuthorList();
                };

                contextMenuStrip.Items.Add(toolStripMenuItem);
                contextMenuStrip.Items.Add(toolStripMenuItem2);
                contextMenuStrip.Items.Add(toolStripMenuItem3);
                button.ContextMenuStrip = contextMenuStrip;
                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        private void GenerateFilteredFolderItems(string[] searchWords)
        {
            ResetAvatarList();

            var fileDatas = _openingWindow == Window.ItemFolderItemsList
                ? CurrentPath.CurrentSelectedItemFolderInfo.GetItems(CurrentPath.CurrentSelectedItemCategory)
                : CurrentPath.CurrentSelectedItemFolderInfo.GetAllItem();

            _openingWindow = Window.SearchItemList;

            var filteredItems = fileDatas
                .Where(file =>
                    searchWords.All(word =>
                        file.FileName.ToLower().Contains(word.ToLower())
                    )
                )
                .OrderByDescending(file =>
                {
                    return searchWords.Count(word => file.FileName.ToLower().Contains(word.ToLower()));
                })
                .ToList();

            SearchResultLabel.Text = "フォルダー内検索結果: " + filteredItems.Count + "件" + " (全" +
                                     fileDatas.Length + "件)";

            var index = 0;
            foreach (var file in filteredItems)
            {
                var imagePath = file.FileExtension is ".png" or ".jpg" ? file.FilePath : "./Datas/FileIcon.png";
                Button button = Helper.CreateButton(imagePath, file.FileName,
                    file.FileExtension.Replace(".", "") + "ファイル", false, "開くファイルのパス: " + file.FilePath);
                button.Location = new Point(0, (70 * index) + 2);

                ContextMenuStrip contextMenuStrip = new();
                ToolStripMenuItem toolStripMenuItem = new("ファイルのパスを開く", _copyImage);
                toolStripMenuItem.Click += (_, _) => { Process.Start("explorer.exe", "/select," + file.FilePath); };
                contextMenuStrip.Items.Add(toolStripMenuItem);
                button.ContextMenuStrip = contextMenuStrip;

                button.Click += (_, _) =>
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = file.FilePath,
                            UseShellExecute = true
                        });
                    }
                    catch
                    {
                        Process.Start("explorer.exe", "/select," + file.FilePath);
                    }
                };
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
            GenerateAuthorList();
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
                   Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title) + "/" +
                   CurrentPath.CurrentSelectedItemCategory;
        }

        private void GeneratePathFromItem(Item item)
        {
            Debug.WriteLine(item.SupportedAvatar.FirstOrDefault());
            CurrentPath.CurrentSelectedAvatar = item.SupportedAvatar.FirstOrDefault() ?? "*";
            CurrentPath.CurrentSelectedCategory = item.Type;
            CurrentPath.CurrentSelectedItem = item;
        }

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

            if (CurrentPath.CurrentSelectedAvatar == "*")
            {
                CurrentPath.CurrentSelectedAvatar = null;
                ResetAvatarList(true);
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

            if (_openingWindow is Window.ItemFolderCategoryList or Window.ItemFolderItemsList)
            {
                GenerateFilteredFolderItems(searchWords);
            }
            else
            {
                GenerateFilteredItem(searchWords);
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
            GenerateAvatarList();
            GenerateAuthorList();
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists("./Output"))
            {
                Directory.CreateDirectory("./Output");
            }

            var fileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv";

            using var sw = new StreamWriter("./Output/" + fileName, false, Encoding.UTF8);
            sw.WriteLine("Title,AuthorName,AuthorImageFilePath,ImagePath,Type,SupportedAvatar,BoothId,ItemPath");
            foreach (var item in Items)
            {
                sw.WriteLine(
                    $"{item.Title},{item.AuthorName},{item.AuthorImageFilePath},{item.ImagePath},{item.Type},{string.Join(";", item.SupportedAvatar)},{item.BoothId},{item.ItemPath}");
            }

            MessageBox.Show("Outputフォルダにエクスポートが完了しました！\nファイル名: " + fileName, "完了", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}