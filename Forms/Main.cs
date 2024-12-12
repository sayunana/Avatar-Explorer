using System.Diagnostics;
using System.Text.Json;
using Avatar_Explorer.Classes;

namespace Avatar_Explorer.Forms
{
    public sealed partial class Main : Form
    {
        private const string CurrentVersion = "v1.0.0";
        public Item[] Items = Array.Empty<Item>();

        public CurrentPath CurrentPath = new();

        private bool _authorMode;

        public Main()
        {
            LoadItemsData();
            InitializeComponent();
            GenerateAvatarList();
            GenerateAuthorList();

            Text = $"Avatar Explorer {CurrentVersion} by ぷこるふ";
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
                    CurrentPath.CurrentSelectedItemCategory = "";
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
                    CurrentPath.CurrentSelectedAvatar = "";
                    CurrentPath.CurrentSelectedCategory = ItemType.Unknown;
                    CurrentPath.CurrentSelectedItemCategory = "";
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
            for (int i = AvatarItemExplorer.Controls.Count - 1; i >= 0; i--)
            {
                if (AvatarItemExplorer.Controls[i].Name != "StartLabel")
                {
                    AvatarItemExplorer.Controls.RemoveAt(i);
                }
                else
                {
                    AvatarItemExplorer.Controls[i].Visible = false;
                }
            }

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
            for (int i = AvatarItemExplorer.Controls.Count - 1; i >= 0; i--)
            {
                if (AvatarItemExplorer.Controls[i].Name != "StartLabel")
                {
                    AvatarItemExplorer.Controls.RemoveAt(i);
                }
                else
                {
                    AvatarItemExplorer.Controls[i].Visible = false;
                }
            }

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
                    CurrentPath.CurrentSelectedCategory = CurrentPath.CurrentSelectedCategory;
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
                    AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, true, item);
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

            for (int i = AvatarItemExplorer.Controls.Count - 1; i >= 0; i--)
            {
                if (AvatarItemExplorer.Controls[i].Name != "StartLabel")
                {
                    AvatarItemExplorer.Controls.RemoveAt(i);
                }
                else
                {
                    AvatarItemExplorer.Controls[i].Visible = false;
                }
            }

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
            for (int i = AvatarItemExplorer.Controls.Count - 1; i >= 0; i--)
            {
                if (AvatarItemExplorer.Controls[i].Name != "StartLabel")
                {
                    AvatarItemExplorer.Controls.RemoveAt(i);
                }
                else
                {
                    AvatarItemExplorer.Controls[i].Visible = false;
                }
            }

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
            AddItem addItem = new AddItem(this, CurrentPath.CurrentSelectedCategory, false, null);
            addItem.ShowDialog();
            GenerateAvatarList();
        }

        // Generate Path
        private string GeneratePath()
        {
            if (!_authorMode)
            {
                if (CurrentPath.CurrentSelectedAvatar == "") return "";
                if (CurrentPath.CurrentSelectedCategory == ItemType.Unknown)
                    return Helper.RemoveFormat(CurrentPath.CurrentSelectedAvatar);
                if (CurrentPath.CurrentSelectedItem == null)
                    return Helper.RemoveFormat(CurrentPath.CurrentSelectedAvatar) + "/" +
                           Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory);
                if (CurrentPath.CurrentSelectedItemCategory == "")
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
            if (CurrentPath.CurrentSelectedItemCategory == "")
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
            if (CurrentPath.CurrentSelectedItemCategory != "")
            {
                CurrentPath.CurrentSelectedItemCategory = "";
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

        private void SaveItemsData()
        {
            using var sw = new StreamWriter("./Datas/ItemsData.json");
            sw.Write(JsonSerializer.Serialize(Items, new JsonSerializerOptions { WriteIndented = true }));
        }

        private void LoadItemsData()
        {
            if (!File.Exists("./Datas/ItemsData.json")) return;
            using var sr = new StreamReader("./Datas/ItemsData.json");
            var data = JsonSerializer.Deserialize<Item[]>(sr.ReadToEnd());
            if (data == null) return;
            Items = data;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveItemsData();
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            if (SearchBox.Text == "")
            {
                if (CurrentPath.CurrentSelectedItemCategory != "")
                {
                    Debug.WriteLine("GenerateItemFiles");
                    GenerateItemFiles();
                    return;
                }

                if (CurrentPath.CurrentSelectedItem != null)
                {
                    Debug.WriteLine("GenerateItemCategoryList");
                    GenerateItemCategoryList();
                    return;
                }

                if (CurrentPath.CurrentSelectedCategory != ItemType.Unknown)
                {
                    Debug.WriteLine("GenerateItems");
                    GenerateItems();
                    return;
                }

                if (CurrentPath.CurrentSelectedAvatar != "" || CurrentPath.CurrentSelectedAuthor != null)
                {
                    Debug.WriteLine("GenerateCategoryList");
                    GenerateCategoryList();
                    return;
                }

                for (int i = AvatarItemExplorer.Controls.Count - 1; i >= 0; i--)
                {
                    if (AvatarItemExplorer.Controls[i].Name != "StartLabel")
                    {
                        AvatarItemExplorer.Controls.RemoveAt(i);
                    }
                    else
                    {
                        Debug.WriteLine("Visible");
                        AvatarItemExplorer.Controls[i].Visible = true;
                    }
                }
                return;
            }

            string[] searchWords = SearchBox.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            GenerateFilteredItem(searchWords);
        }

        private void GenerateFilteredItem(string[] searchWords)
        {
            for (int i = AvatarItemExplorer.Controls.Count - 1; i >= 0; i--)
            {
                if (AvatarItemExplorer.Controls[i].Name != "StartLabel")
                {
                    AvatarItemExplorer.Controls.RemoveAt(i);
                }
                else
                {
                    AvatarItemExplorer.Controls[i].Visible = false;
                }
            }

            var filteredItems = Items.Where(item =>
                searchWords.Any(word => item.Title.ToLower().Contains(word.ToLower())) ||
                searchWords.Any(word => item.AuthorName.ToLower().Contains(word.ToLower())));

            filteredItems = filteredItems.OrderByDescending(item =>
            {
                var matchCount = 0;
                foreach (var word in searchWords)
                {
                    if (item.Title.ToLower().Contains(word.ToLower())) matchCount += 2;
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
                    CurrentPath.CurrentSelectedCategory = CurrentPath.CurrentSelectedCategory;
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
                    AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, true, item);
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
    }
}